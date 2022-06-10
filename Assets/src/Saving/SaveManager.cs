using Game.Saving.Data;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Game.Saving
{
    public class SaveManager<TSaveData> where TSaveData : new()
    {
        public enum ManagerState { Ready, Saving, Done }

        public float Progress { get; private set; }
        public string Description { get; private set; }

        private string saveFolder = null;
        private bool saving = false;
        private bool startSavingWarningSent = false;
        private TSaveData data = default(TSaveData);
        private List<FieldInfo> dataFields = null;
        private FieldInfo currentField = null;
        private DataPropertyAttribute currentAttribute = null;
        private int propertyIndex = 0;
        private List<ISaveable> saveables = null;
        private ISaveable currentSaveable = null;
        private ISaveData currentSaveData = null;

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

            saving = true;
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
            currentField = dataFields[0];
            currentAttribute = currentField.GetCustomAttribute<DataPropertyAttribute>();
            currentSaveData = (ISaveData)Activator.CreateInstance(currentField.FieldType);
            currentField.SetValue(data, currentSaveData);
            currentSaveable = saveables.First(saveable => saveable.GetType().Name == currentAttribute.SaveableName);
            UpdateDescription();

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
            if(State != ManagerState.Saving) {
                if (!startSavingWarningSent) {
                    //If Update() gets called before StartSaving() log a warning, but only once. Update() is intended to be called repeatedly and we don't want spam log
                    //with same warning a million times
                    CustomLogger.Warning("CallStartSavingBeforeUpdate");
                    startSavingWarningSent = true;
                }
                return;
            }

            currentSaveable.Save(ref currentSaveData);
        }

        public ManagerState State
        {
            get {
                if(data == null) {
                    return ManagerState.Ready;
                }
                return saving ? ManagerState.Saving : ManagerState.Done;
            }
        }

        private void UpdateDescription()
        {
            Description = currentAttribute.LocalizeDescription ? Localization.Game.Get(currentAttribute.Description) : currentAttribute.Description;
        }
    }
}
