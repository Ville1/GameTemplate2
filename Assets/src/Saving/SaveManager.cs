using Game.Saving.Data;
using Game.Utils;
using Game.Utils.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Game.Saving
{
    public class SaveManager<TSaveData> where TSaveData : new()
    {
        private int DEFAULT_CALLS_PER_FRAME = 1;
        private int CALLS_PER_FRAME_DELTA = 1;
        private int MIN_CALLS_PER_FRAME = 1;
        private int MAX_CALLS_PER_FRAME = 1000;
        private bool PRETTY_JSON = true;

        public enum ManagerState { Ready, Saving, Loading, Done, Error }
        private enum Task { None, Save, Load }

        public ManagerState State { get; private set; }
        public float Progress { get; private set; }
        public string Description { get; private set; }
        public Exception SaveException { get; private set; }

        private string saveFolder = null;
        private string saveName = null;
        private Task task = Task.None;
        private bool stateStartWarningSent = false;
        private TSaveData data = default(TSaveData);
        private List<SaveableStepData> saveableSteps = null;
        private SaveableStepData currentSaveableStep = null;
        private ISaveData currentSaveData = null;
        private int stepIndex = 0;
        private float previousProgress = 0.0f;
        private List<ISaveable> saveables = null;
        private int callsPerFrame = 1;
        private float targetFrameRate = 60.0f;

        public SaveManager(string saveFolder, List<ISaveable> saveables)
        {
            this.saveFolder = saveFolder;
            this.saveables = saveables;
            task = Task.None;
            State = ManagerState.Ready;
        }

        /// <summary>
        /// TODO: add more return types for different errors
        /// </summary>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public bool StartSaving(string saveName)
        {
            return Start(saveName, Task.Save, ManagerState.Saving);
        }

        public bool StartLoading(string saveName)
        {
            return Start(saveName, Task.Load, ManagerState.Loading);
        }

        private bool Start(string saveName, Task task, ManagerState state)
        {
            if (!Directory.Exists(saveFolder)) {
                //Folder for save files is missing, show error message to user
                return false;
            }

            //Set state
            this.task = task;
            State = state;

            //Initialize variables
            this.saveName = saveName.Contains(".json") ? saveName : saveName + ".json";
            callsPerFrame = DEFAULT_CALLS_PER_FRAME;
            targetFrameRate = ConfigManager.Config.SavingTargetFPS;
            SaveException = null;
            Progress = 0.0f;
            previousProgress = 0.0f;

            if(State == ManagerState.Saving) {
                //Create a new save data object
                data = new TSaveData();
            } else {
                //Load data from save file
                try {
                    data = JsonUtility.FromJson<TSaveData>(File.ReadAllText(Path.Combine(saveFolder, saveName + ".json")));
                } catch (Exception exception) {
                    SaveException = exception;
                    CustomLogger.Error("LoadException", exception.Message);
                    State = ManagerState.Error;
                    return false;
                }
            }

            //Get a list of fields to be looped through 
            List<FieldInfo> dataFields = data.GetType().GetFields().Where(field =>
                field.IsPublic &&
                field.GetCustomAttribute<DataPropertyAttribute>() != null &&
                field.FieldType.GetInterfaces().Contains(typeof(ISaveData))
            ).ToList();
            if (dataFields.Count == 0) {
                //No properties with DataPropertyAttribute
                CustomLogger.Error("InvalidSaveDataClass");
                return false;
            }

            //Check that all saveables are found in saveables - list
            foreach (FieldInfo field in dataFields) {
                if (!saveables.Any(saveable => saveable.GetType().Name == field.GetCustomAttribute<DataPropertyAttribute>().SaveableName)) {
                    CustomLogger.Error("SaveableIsMissing", field.GetCustomAttribute<DataPropertyAttribute>().SaveableName);
                    return false;
                }
            }

            //Create a list of necessary steps to save/load all saveables
            saveableSteps = new List<SaveableStepData>();
            foreach(FieldInfo field in dataFields) {
                SaveableStepData step = new SaveableStepData() {
                    FieldInfo = field,
                    Attribute = field.GetCustomAttribute<DataPropertyAttribute>(),
                    Saveable = saveables.First(saveable => saveable.GetType().Name == field.GetCustomAttribute<DataPropertyAttribute>().SaveableName),
                    //If we are saving, create a new instance of field.FieldType so we can fill it by calling ISaveable.Save
                    //If we are loading just add a reference to data's field, as data has been filled by loading from file
                    SaveData = task == Task.Save ? (ISaveData)Activator.CreateInstance(field.FieldType) : (ISaveData)field.GetValue(data)
                };
                saveableSteps.Add(step);
                if(task == Task.Save) {
                    //Set field value to our new instance
                    field.SetValue(data, step.SaveData);
                }
            }

            //Calculate progress multipliers
            float weightTotal = saveableSteps.Select(step => step.Attribute.Weight).Sum();
            foreach (SaveableStepData stepData in saveableSteps) {
                stepData.ProgressMultiplier = stepData.Attribute.Weight / weightTotal;
            }

            //Fill in current values
            stepIndex = -1;
            NextSaveable();

            return true;
        }

        /// <summary>
        /// TODO: Implement a simpler version of saving, which saves everything is one swoop. Could be better for saving smaller quantities of data, where a progress bar is unnecessary
        /// </summary>
        public bool Save()
        {
            return false;
        }

        public void Update()
        {
            //Check state
            if(State != ManagerState.Saving && State != ManagerState.Loading) {
                if (!stateStartWarningSent) {
                    //If Update() gets called before StartSaving() or StartLoading() log a warning, but only once. Update() is intended to be called repeatedly and we don't want
                    //spam log with same warning a million times, so lets use startSavingWarningSent variable to see if error has already been sent
                    CustomLogger.Warning("InvalidState", State.ToString());
                    stateStartWarningSent = true;
                }
                return;
            }

            if(task == Task.None) {
                //Task not defined, if State is corrent this should not happen
                throw new Exception("Task not defined");
            }

            //Adjust saving/loading speed
            //TODO: Use weight -> slower calls per frame delta?
            float minFrameRate = Math.Max(10.0f, targetFrameRate / 2.0f);
            if (Main.Instance.CurrentFrameRate > targetFrameRate && callsPerFrame < MAX_CALLS_PER_FRAME) {
                //Speed up
                int callsPerFrameDelta = Math.Min(1, Mathf.RoundToInt(((Main.Instance.CurrentFrameRate - targetFrameRate) / targetFrameRate) * CALLS_PER_FRAME_DELTA));
                callsPerFrame = Math.Min(callsPerFrame + callsPerFrameDelta, MAX_CALLS_PER_FRAME);
            } else if (Main.Instance.CurrentFrameRate < minFrameRate && callsPerFrame > MIN_CALLS_PER_FRAME) {
                //Speed down
                int callsPerFrameDelta = Math.Min(1, Mathf.RoundToInt(((Main.Instance.CurrentFrameRate - targetFrameRate) / targetFrameRate) * CALLS_PER_FRAME_DELTA));
                callsPerFrame = Math.Max(callsPerFrame - callsPerFrameDelta, MIN_CALLS_PER_FRAME);
            }

            //Save/load data
            bool done = false;
            for (int i = 0; i < callsPerFrame; i++) {
                float currentProgress = task == Task.Save ? currentSaveableStep.Saveable.Save(ref currentSaveData) : currentSaveableStep.Saveable.Load(currentSaveData);
                Progress = previousProgress + currentProgress * currentSaveableStep.ProgressMultiplier;
                if (currentProgress == 1.0f) {
                    if (!NextSaveable()) {
                        done = true;
                    }
                    break;//TODO: Should this break be with saving = false;?
                }
            }

            //Finished
            if (done) {
                State = ManagerState.Done;
                if(task == Task.Save) {
                    //Write file
                    //TODO: Split this, if json string is long?
                    try {
                        File.WriteAllText(Path.Combine(saveFolder, saveName), JsonUtility.ToJson(data, PRETTY_JSON));
                    } catch (Exception exception) {
                        SaveException = exception;
                        CustomLogger.Error("SaveException", exception.Message);
                        State = ManagerState.Error;
                    }
                }
            }
        }

        private void UpdateDescription()
        {
            Description = currentSaveableStep.Attribute.LocalizeDescription ? Localization.Game.Get(currentSaveableStep.Attribute.Description) : currentSaveableStep.Attribute.Description;
        }

        private bool NextSaveable()
        {
            //Increment index
            stepIndex++;
            if(saveableSteps.Count == stepIndex) {
                //Done saving
                return false;
            }

            //Add old steps progress to total
            if(currentSaveableStep != null) {
                //^This is null on the first call
                previousProgress += currentSaveableStep.ProgressMultiplier;
                Progress = previousProgress;
            }

            //Get next step
            currentSaveableStep = saveableSteps[stepIndex];
            currentSaveData = currentSaveableStep.SaveData;

            //Call saveable's start method
            if(task == Task.Save) {
                currentSaveableStep.Saveable.StartSaving(ref currentSaveData);
            } else {
                currentSaveableStep.Saveable.StartLoading(currentSaveData);
            }

            UpdateDescription();
            return true;
        }

        private class SaveableStepData
        {
            public FieldInfo FieldInfo { get; set; }
            public DataPropertyAttribute Attribute { get; set; }
            public ISaveable Saveable { get; set; }
            public ISaveData SaveData { get; set; }
            public float ProgressMultiplier { get; set; }
        }
    }
}
