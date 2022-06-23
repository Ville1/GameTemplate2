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

        public enum ManagerState { Ready, Saving, Done, Error }

        public float Progress { get; private set; }
        public string Description { get; private set; }
        public Exception SaveException { get; private set; }

        private string saveFolder = null;
        private string saveName = null;
        private bool saving = false;
        private bool startSavingWarningSent = false;
        private TSaveData data = default(TSaveData);
        private List<FieldInfo> dataFields = null;
        private FieldInfo currentField = null;
        private DataPropertyAttribute currentAttribute = null;
        private int fieldIndex = 0;
        private List<ISaveable> saveables = null;
        private ISaveable currentSaveable = null;
        private ISaveData currentSaveData = null;
        private int callsPerFrame = 1;
        private float targetFrameRate = 60.0f;

        public SaveManager(string saveFolder, List<ISaveable> saveables)
        {
            this.saveFolder = saveFolder;
            this.saveables = saveables;
        }

        /// <summary>
        /// TODO: add more return types for different errors
        /// </summary>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public bool StartSaving(string saveName)
        {
            if (!Directory.Exists(saveFolder)) {
                //Folder for save files is missing, show error message to user
                return false;
            }

            this.saveName = saveName.Contains(".json") ? saveName : saveName + ".json";
            saving = true;
            callsPerFrame = DEFAULT_CALLS_PER_FRAME;
            targetFrameRate = ConfigManager.Config.SavingTargetFPS;
            SaveException = null;

            //Create a new save data object
            data = new TSaveData();

            //Get a list of fields to be looped through 
            dataFields = data.GetType().GetFields().Where(field =>
                field.IsPublic &&
                field.GetCustomAttribute<DataPropertyAttribute>() != null &&
                field.FieldType.GetInterfaces().Contains(typeof(ISaveData))
            ).ToList();
            if(dataFields.Count == 0) {
                //No properties with DataPropertyAttribute
                CustomLogger.Error("InvalidSaveDataClass");
                return false;
            }

            //Check that all saveables are found in saveables - list
            foreach(FieldInfo field in dataFields) {
                if (!saveables.Any(saveable => saveable.GetType().Name == field.GetCustomAttribute<DataPropertyAttribute>().SaveableName)) {
                    CustomLogger.Error("SaveableIsMissing", field.GetCustomAttribute<DataPropertyAttribute>().SaveableName);
                    return false;
                }
            }

            //Fill in current values
            fieldIndex = -1;
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
            if(State != ManagerState.Saving) {
                if (!startSavingWarningSent) {
                    //If Update() gets called before StartSaving() log a warning, but only once. Update() is intended to be called repeatedly and we don't want spam log
                    //with same warning a million times
                    CustomLogger.Warning("CallStartSavingBeforeUpdate");
                    startSavingWarningSent = true;
                }
                return;
            }

            //Adjust saving speed
            //TODO: Add some kind of weight property to ISaveable and if it is large -> slower calls per frame delta
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

            //Save data
            for (int i = 0; i < callsPerFrame; i++) {
                if (!currentSaveable.Save(ref currentSaveData)) {
                    if (!NextSaveable()) {
                        saving = false;
                    }
                    break;//TODO: Should this break be with saving = false;?
                }
            }

            //Finish saving
            if (!saving) {
                try {
                    File.WriteAllText(Path.Combine(saveFolder, saveName), JsonUtility.ToJson(data, PRETTY_JSON));
                } catch (Exception exception) {
                    SaveException = exception;
                    CustomLogger.Error("SaveException", exception.Message);
                }
            }
        }

        public ManagerState State
        {
            get {
                if(data == null) {
                    return ManagerState.Ready;
                }
                if(SaveException != null) {
                    return ManagerState.Error;
                }
                return saving ? ManagerState.Saving : ManagerState.Done;
            }
        }

        private void UpdateDescription()
        {
            Description = currentAttribute.LocalizeDescription ? Localization.Game.Get(currentAttribute.Description) : currentAttribute.Description;
        }

        private bool NextSaveable()
        {
            fieldIndex++;
            if(dataFields.Count == fieldIndex) {
                return false;
            }

            currentField = dataFields[fieldIndex];
            currentAttribute = currentField.GetCustomAttribute<DataPropertyAttribute>();
            currentSaveData = (ISaveData)Activator.CreateInstance(currentField.FieldType);
            currentField.SetValue(data, currentSaveData);
            currentSaveable = saveables.First(saveable => saveable.GetType().Name == currentAttribute.SaveableName);
            UpdateDescription();
            return true;
        }
    }
}
