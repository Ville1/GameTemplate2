using Game.Utils;
using Game.Utils.Config;
using UnityEngine;

namespace Game
{
    public class Main : MonoBehaviour
    {
        public static Main Instance;

        public State State { get; private set; }

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("AttemptingToCreateMultipleInstances");
                return;
            }
            //Initialize Main
            Instance = this;
            State = State.MainMenu;
            CustomLogger.Debug("GameStart");

            //Load config
            ConfigManager.Load();

            //Apply logger settings
            CustomLogger.MinLevel = ConfigManager.Config.LogLevel;
            CustomLogger.LogPrefix = ConfigManager.Config.LogPrefix;
            CustomLogger.LogMethod = ConfigManager.Config.LogMethod;
            CustomLogger.LogRaw(Localization.Log.Get("LoggerSettingsLoaded"));
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }
    }
}
