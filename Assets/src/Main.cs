using Game.Utils;
using Game.Utils.Config;
using Game.Maps;
using UnityEngine;
using Game.UI;
using Game.Saving;
using Game.Saving.Data;
using System.Collections.Generic;
using System;

namespace Game
{
    public class Main : MonoBehaviour
    {
        public static Main Instance;

        public State State { get; private set; }
        public Maps.Map WorldMap { get; private set; }
        public float CurrentFrameRate { get; private set; }

        private SaveManager<SaveData> saveManager = null;

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
            CustomLogger.LoadSettings();

            //Enable/disable console
            ConsoleManager.Instance.Enabled = ConfigManager.Config.ConsoleEnabled;

            //Show main menu
            MainMenu.Instance.Active = true;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            CurrentFrameRate = 1.0f / Time.deltaTime;
            if(saveManager != null && State == State.Saving) {
                switch (saveManager.State) {
                    case SaveManager<SaveData>.ManagerState.Saving:
                        saveManager.Update();
                        ProgressBar.Instance.Progress = saveManager.Progress;
                        ProgressBar.Instance.Description = saveManager.Description;
                        break;
                    case SaveManager<SaveData>.ManagerState.Error:
                    //TODO: Show message on screen
                    //Localization.Game.Get("FailedToSaveGame");
                    case SaveManager<SaveData>.ManagerState.Done:
                        ProgressBar.Instance.Active = false;
                        State = State.Running;
                        saveManager = null;
                        break;
                    default:
                        throw new NotImplementedException(saveManager.State.ToString());
                }
            }
        }

        public void NewGame()
        {
            State = State.GeneratingMap;
            ProgressBar.Instance.Show("Generation map...");
            WorldMap = Maps.Map.Instantiate("WorldMap", 25, 25);
            WorldMap.StartGeneration(() => { EndMapGeneration(); });
        }

        public void SaveGame(string folder, string fileName)
        {
            State = State.Saving;
            List<ISaveable> saveables = new List<ISaveable>() {
                WorldMap
            };
            saveManager = new SaveManager<SaveData>(folder, saveables);
            saveManager.StartSaving(fileName);
            ProgressBar.Instance.Show(saveManager.Description);
        }

        private void EndMapGeneration()
        {
            State = State.Running;
            ProgressBar.Instance.Active = false;
        }
    }
}
