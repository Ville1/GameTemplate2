using Game.Utils;
using Game.Utils.Config;
using Game.Maps;
using UnityEngine;
using Game.UI;
using Game.Saving;
using System.Collections.Generic;
using System;

namespace Game
{
    public class Main : MonoBehaviour
    {
        public static Main Instance;

        public Maps.Map WorldMap { get; private set; }
        public float CurrentFrameRate { get; private set; }
        public Character PlayerCharacter { get; private set; }

        private SaveManager<Saving.Data.SaveData> saveManager = null;
        private State state;
        private float? lastScreenWidth = null;
        private float? lastScreenHeight = null;

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            //Initialize Main
            Instance = this;
            State = State.MainMenu;
            CustomLogger.Debug("{GameStart}");

            //Load config
            ConfigManager.Load();

            //Apply logger settings
            CustomLogger.LoadSettings();

            //Enable/disable console
            ConsoleManager.Instance.Enabled = ConfigManager.Config.ConsoleEnabled;

            //Show main menu
            MainMenu.Instance.Active = true;

            //Show menu background
            BackgroundManager.Instance.Active = true;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            CurrentFrameRate = 1.0f / Time.deltaTime;
            DebugWindowManager.Instance.SetValue("FPS", CurrentFrameRate.Parse(2, true));
            if(saveManager != null && (State == State.Saving || State == State.Loading)) {
                switch (saveManager.State) {
                    case SaveManager<Saving.Data.SaveData>.ManagerState.Saving:
                    case SaveManager<Saving.Data.SaveData>.ManagerState.Loading:
                        saveManager.Update();
                        ProgressBar.Instance.Progress = saveManager.Progress;
                        ProgressBar.Instance.Description = saveManager.Description;
                        break;
                    case SaveManager<Saving.Data.SaveData>.ManagerState.Error:
                        ProgressBar.Instance.Active = false;
                        MessageWindowManager.Instance.ShowMessage("{FailedToSaveGame}");
                        State = State.Running;
                        saveManager = null;
                        break;
                    case SaveManager<Saving.Data.SaveData>.ManagerState.Done:
                        ProgressBar.Instance.Active = false;
                        if(State == State.Loading) {
                            Tile centerTile = WorldMap.Tiles[WorldMap.Width / 2][WorldMap.Height / 2];
                            CameraManager.Instance.Center(centerTile);
                            PlayerCharacter = new Character(centerTile);
                        }
                        State = State.Running;
                        saveManager = null;
                        break;
                    default:
                        throw new NotImplementedException(saveManager.State.ToString());
                }
            }

            //Check for screen size changes
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            if (lastScreenWidth.HasValue && lastScreenHeight.HasValue && (lastScreenWidth.Value != screenWidth || lastScreenHeight.Value != screenHeight) &&
                CameraBackground.Instance != null && CameraBackground.Instance.Active) {
                CameraBackground.Instance.UpdateSprite();
            }
            lastScreenWidth = screenWidth;
            lastScreenHeight = screenHeight;
        }

        public State State
        {
            get {
                return state;
            }
            private set {
                state = value;
                BackgroundManager.Instance.Active = state == State.MainMenu || state == State.GeneratingMap || state == State.Loading;
            }
        }

        public void NewGame()
        {
            State = State.GeneratingMap;
            ProgressBar.Instance.Show("Generation map...");
            if(WorldMap == null) {
                WorldMap = Map.Instantiate("WorldMap", 25, 25);
            }
            WorldMap.StartGeneration(() => { EndMapGeneration(); });
        }

        public void SaveGame(string folder, string fileName)
        {
            State = State.Saving;
            List<ISaveable> saveables = new List<ISaveable>() {
                WorldMap,
                NameManager.SaveHelper,
                NotificationManager.Instance
            };
            saveManager = new SaveManager<Saving.Data.SaveData>(folder, saveables);
            saveManager.StartSaving(fileName);
            ProgressBar.Instance.Show(saveManager.Description);
        }

        public void LoadGame(string folder, string fileName)
        {
            State = State.Loading;
            WorldMap = WorldMap ?? Map.Instantiate("WorldMap", 1, 1);
            if(PlayerCharacter != null) {
                PlayerCharacter.DestroyGameObject();
                PlayerCharacter = null;
            }
            List<ISaveable> saveables = new List<ISaveable>() {
                WorldMap,
                NameManager.SaveHelper,
                NotificationManager.Instance
            };
            saveManager = new SaveManager<Saving.Data.SaveData>(folder, saveables);
            saveManager.StartLoading(fileName);
            ProgressBar.Instance.Show(saveManager.Description);
        }

        private void EndMapGeneration()
        {
            State = State.Running;
            ProgressBar.Instance.Active = false;
            Tile centerTile = WorldMap.Tiles[WorldMap.Width / 2][WorldMap.Height / 2];
            CameraManager.Instance.Center(centerTile);
            PlayerCharacter = new Character(centerTile);
        }

        public void BackToMainMenu()
        {
            if(State == State.Running) {
                WorldMap.Clear();
                State = State.MainMenu;
                PlayerCharacter.DestroyGameObject();
                PlayerCharacter = null;
                MainMenu.Instance.Active = true;
            }
        }
    }
}
