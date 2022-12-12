using Game.Saving;
using Game.Saving.Data;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Game
{
    public class NameManager
    {
        private static readonly string FILE_PATH = "/Resources/names/";

        public enum ManagerState { Uninitialized, Ready, Error }
        public enum OutOfNamesBehaviour { Reset, AddNumbers }

        public static ManagerState State = ManagerState.Uninitialized;

        //List of names of json files and types they are deserialized to. These deserialized classes should have List<string> fields with NameListAttribute attributes, so they can
        //be matched with right name lists
        private static readonly Dictionary<string, Type> files = new Dictionary<string, Type>() {
            { "cities.json", typeof(CityNameData) }
        };

        private static Dictionary<NameType, List<string>> names;
        private static Dictionary<NameType, List<UsedName>> namesUsed;
        private static Dictionary<NameType, long> erroredNamesCount;
        private static NameManagerSaveHelper saveHelper = new NameManagerSaveHelper();

        private static void Initialize()
        {
            if(State != ManagerState.Uninitialized) {
                return;
            }
            
            names = DictionaryHelper.CreateNewFromEnum((NameType type) => { return new List<string>(); });
            namesUsed = DictionaryHelper.CreateNewFromEnum((NameType type) => { return new List<UsedName>(); });
            erroredNamesCount = DictionaryHelper.CreateNewFromEnum<NameType, long>(0);

            try {
                foreach(KeyValuePair<string, System.Type> file in files) {
                    //Deserialize file file.Key as file.Value type object
                    object deserializedFile = JsonUtility.FromJson(File.ReadAllText(Application.dataPath + FILE_PATH + file.Key), file.Value);

                    //Loop each field in the object
                    foreach(FieldInfo fieldInfo in file.Value.GetFields()) {
                        NameListAttribute nameListAttribute = fieldInfo.GetCustomAttribute<NameListAttribute>();
                        if (nameListAttribute != null) {
                            //Fields has NameListAttribute, it's value should be List<string>
                            if(fieldInfo.FieldType != typeof(List<string>)) {
                                CustomLogger.Warning("{NameListDeserializationAttributeError}", file.Value.Name, fieldInfo.Name);
                            } else {
                                List<string> fieldValue = (List<string>)fieldInfo.GetValue(deserializedFile);
                                if (fieldValue != null) {
                                    //Add each name from the field value to the correct name list
                                    foreach (string name in fieldValue) {
                                        if (!names[nameListAttribute.Type].Contains(name)) {
                                            names[nameListAttribute.Type].Add(name);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Write log
                    CustomLogger.Debug("{NameListLoaded}", file.Key);
                }
            } catch(Exception exception) {
                CustomLogger.Error("{FailedToLoadNameList}", "Cities", exception.Message);
                State = ManagerState.Error;
                return;
            }

            State = ManagerState.Ready;
        }

        public static string Get(NameType type, bool allowDuplicates = true, OutOfNamesBehaviour outOfNamesBehaviour = OutOfNamesBehaviour.Reset)
        {
            Initialize();

            if(State == ManagerState.Error) {
                //Failed to load name lists
                erroredNamesCount[type] = erroredNamesCount[type] == long.MaxValue ? 0 : erroredNamesCount[type] + 1;
                return string.Format("{0}{1}", type.ToString(), erroredNamesCount[type]);
            }

            long timesUsed;
            if (allowDuplicates) {
                //Duplicates allowed, return a random name
                return PickName(type, names[type], out timesUsed);
            }

            List<string> availableNames = names[type].Where((name) => !namesUsed[type].Any(used => used.Name == name)).ToList();
            if(availableNames.Count > 0) {
                //Pick a random name that has not yet been used
                return PickName(type, availableNames, out timesUsed);
            }

            //We have run out of names
            if(outOfNamesBehaviour == OutOfNamesBehaviour.Reset) {
                //Reset used names list
                namesUsed[type].Clear();
                return PickName(type, names[type], out timesUsed);
            }

            //Add a number after used name
            string name = PickName(type, names[type], out timesUsed);
            return string.Format("{0} {1}", name, timesUsed);
        }

        /// <summary>
        /// Returns an object that saves and loads name manager save data
        /// </summary>
        public static NameManagerSaveHelper SaveHelper
        {
            get {
                return saveHelper;
            }
        }

        private static void ResetUsedNames()
        {
            Initialize();
            foreach(NameType type in Enum.GetValues(typeof(NameType))) {
                namesUsed[type].Clear();
            }
        }

        private static string PickName(NameType type, List<string> names, out long timesUsed)
        {
            string name = RNG.Item(names);
            UsedName usedName = namesUsed[type].FirstOrDefault(used => used.Name == name);
            if (usedName == null) {
                namesUsed[type].Add(new UsedName() { Name = name, TimesUsed = 1 });
                timesUsed = 1;
            } else {
                usedName.TimesUsed = usedName.TimesUsed == long.MaxValue ? 0 : usedName.TimesUsed + 1;
                timesUsed = usedName.TimesUsed;
            }
            return name;
        }

        private class CityNameData
        {
            [NameList(NameType.City)]
            public List<string> Cities;

            [NameList(NameType.Village)]
            public List<string> Villages;

            [NameList(NameType.Test)]
            public List<string> Test;
        }

        private class UsedName
        {
            public string Name { get; set; }
            public long TimesUsed { get; set; }
        }

        private class NameListAttribute : Attribute
        {
            public NameType Type { get; private set; }

            public NameListAttribute(NameType type)
            {
                Type = type;
            }
        }

        public class NameManagerSaveHelper : ISaveable
        {
            public float Load(ISaveData data)
            {
                ResetUsedNames();
                NameManagerData saveData = data as NameManagerData;
                foreach(NameManagerUsedNameListData list in saveData.UsedNames) {
                    if(list.Names == null) {
                        continue;
                    }
                    foreach(NameManagerUsedNameData usedNameData in list.Names) {
                        namesUsed[(NameType)list.Type].Add(new UsedName() {
                            Name = usedNameData.Name,
                            TimesUsed = usedNameData.Times
                        });
                    }
                }
                return 1.0f;
            }

            public float Save(ref ISaveData data)
            {
                NameManagerData saveData = data as NameManagerData;
                foreach (KeyValuePair<NameType, List<UsedName>> pair in namesUsed) {
                    NameManagerUsedNameListData list = saveData.UsedNames.First(l => l.Type == (int)pair.Key);
                    foreach(UsedName usedName in pair.Value) {
                        list.Names.Add(new NameManagerUsedNameData() {
                            Name = usedName.Name,
                            Times = usedName.TimesUsed
                        });
                    }
                }
                return 1.0f;
            }

            public void StartLoading(ISaveData data)
            {
                NameManagerData saveData = data as NameManagerData;
                saveData.UsedNames = saveData.UsedNames ?? new List<NameManagerUsedNameListData>();
            }

            public void StartSaving(ref ISaveData data)
            {
                NameManagerData saveData = data as NameManagerData;
                saveData.UsedNames = new List<NameManagerUsedNameListData>();
                foreach (NameType type in Enum.GetValues(typeof(NameType))) {
                    saveData.UsedNames.Add(new NameManagerUsedNameListData() {
                        Type = (int)type,
                        Names = new List<NameManagerUsedNameData>()
                    });
                }
            }
        }
    }
}
