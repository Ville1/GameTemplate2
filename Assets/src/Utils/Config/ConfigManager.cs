using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

namespace Game.Utils.Config
{
    public class ConfigManager : MonoBehaviour
    {
        private static readonly string FILE_PATH = "/Resources/Config/config.json";

        public static Config Config { get; private set; }
        public static bool LoadFailed { get; private set; }

        private static List<IConfigListener> listeners;

        public static void Load()
        {
            listeners = new List<IConfigListener>();
            try {
                Config = JsonUtility.FromJson<Config>(File.ReadAllText(Application.dataPath + FILE_PATH));
                CustomLogger.Debug("{ConfigLoaded}", FILE_PATH);
            } catch (Exception exception) {
                CustomLogger.Warning("{FailedToLoadConfig}", exception.Message);
                Config = Config.Default;
                Save(Config);
            }
        }

        public static void RegisterListener(IConfigListener listener)
        {
            if (listeners.Contains(listener)) {
                CustomLogger.Warning("{ListenerIsAlreadyRegistered}");
                return;
            }
            listeners.Add(listener);
            listener.ReadConfig();
        }

        public static void UnregisterListener(IConfigListener listener)
        {
            if (!listeners.Contains(listener)) {
                CustomLogger.Warning("{ListenerIsNotRegistered}");
                return;
            }
            listeners.Remove(listener);
        }

        public static void Save(Config config)
        {
            Config = config;
            try {
                File.WriteAllText(Application.dataPath + FILE_PATH, JsonUtility.ToJson(Config, true));
                CustomLogger.Debug("{ConfigSaved}", FILE_PATH);
                foreach(IConfigListener listener in listeners) {
                    listener.ReadConfig();
                }
            } catch (Exception exception) {
                CustomLogger.Error("{FailedToSaveConfig}", exception.Message);
            }
        }
    }
}
