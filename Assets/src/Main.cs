using Game.Utils;
using Game.Utils.Config;
using Game.Maps;
using UnityEngine;
using Game.UI;

namespace Game
{
    public class Main : MonoBehaviour
    {
        public static Main Instance;

        public State State { get; private set; }
        public Map WorldMap { get; private set; }
        public float CurrentFrameRate { get; private set; }

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
            CustomLogger.LogRaw("LOGGER - " + Localization.Log.Get("LoggerSettingsLoaded"));

            //Show main menu
            MainMenu.Instance.Active = true;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            CurrentFrameRate = 1.0f / Time.deltaTime;
        }

        public void NewGame()
        {
            State = State.GeneratingMap;
            ProgressBar.Instance.Show("Generation map...");
            WorldMap = Map.Instantiate("WorldMap", 25, 25);
            WorldMap.StartGeneration(() => { EndMapGeneration(); });
        }

        public void SaveGame()
        {

        }

        private void EndMapGeneration()
        {
            State = State.Running;
            ProgressBar.Instance.Active = false;
        }
    }
}
