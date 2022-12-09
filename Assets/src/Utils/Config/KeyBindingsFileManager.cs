using Game.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Game.Utils.Config
{
    public class KeyBindingsFileManager
    {
        private static readonly string FILE_PATH = "/Resources/Config/key bindings.json";
        private enum ManagerState { Uninitialized, Ready, Error }

        private static KeyBindingListData data = null;
        private static ManagerState state = ManagerState.Uninitialized;

        private static void Initialize()
        {
            if(state != ManagerState.Uninitialized) {
                return;
            }
            try {
                data = JsonUtility.FromJson<KeyBindingListData>(File.ReadAllText(Application.dataPath + FILE_PATH));
                CustomLogger.Debug("{KeyBindingsLoaded}", FILE_PATH);
                data.Bindings = data.Bindings ?? new List<KeyBindingData>();
                state = ManagerState.Ready;
            } catch (Exception exception) {
                CustomLogger.Warning("{FailedToLoadKeyBindings}", exception.Message);
                state = ManagerState.Error;
            }
        }

        /// <summary>
        /// Sets key binding's key code to match that of current key binding data loaded from the file. If file has no entry for this binding, new one is added with with default key code.
        /// </summary>
        /// <param name="keyBinding"></param>
        public static void Load(KeyBinding keyBinding)
        {
            Initialize();
            if(state == ManagerState.Error) {
                //Load error, use default bindings
                return;
            }

            KeyBindingData currentData = data.Bindings.FirstOrDefault(bindingData => bindingData.Name == keyBinding.InternalName);
            if(currentData == null) {
                //Binding is not saved, save default binding
                data.Bindings.Add(new KeyBindingData() {
                    Name = keyBinding.InternalName,
                    Value = (int)keyBinding.KeyCode
                });
                Save();
            } else {
                keyBinding.KeyCode = (KeyCode)currentData.Value;
            }
        }

        /// <summary>
        /// Saves current keybinding data
        /// </summary>
        /// <param name="keyBindings">If not null, updates current keybinding data to match this list.</param>
        public static void Save(List<KeyBinding> keyBindings = null)
        {
            Initialize();
            if(state == ManagerState.Error) {
                //Load failed, no reason to try and save as it will also likely fail
                return;
            }

            if(keyBindings != null) {
                //Update bindings
                foreach(KeyBinding keyBinding in keyBindings) {
                    KeyBindingData currentData = data.Bindings.FirstOrDefault(bindingData => bindingData.Name == keyBinding.InternalName);
                    if(currentData != null) {
                        currentData.Value = (int)keyBinding.KeyCode;
                    } else {
                        data.Bindings.Add(new KeyBindingData() {
                            Name = keyBinding.InternalName,
                            Value = (int)keyBinding.KeyCode
                        });
                    }
                }
            }

            try {
                File.WriteAllText(Application.dataPath + FILE_PATH, JsonUtility.ToJson(data, true));
                CustomLogger.Debug("{KeyBindingsSaved}", FILE_PATH);
            } catch (Exception exception) {
                CustomLogger.Error("{FailedToSaveKeyBindings}", exception.Message);
                state = ManagerState.Error;
            }
        }

        [Serializable]
        private class KeyBindingListData
        {
            public List<KeyBindingData> Bindings;
        }


        [Serializable]
        private class KeyBindingData
        {
            public string Name;
            public int Value;
        }
    }
}