using Game.UI.Components;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenu : WindowBase
    {
        public static MainMenu Instance;

        public Button PrototypeButton;

        private Dictionary<string, CustomButton> buttons;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;
            Tags.Add(Tag.MainMenu);

            //Create buttons
            //Localized string key, action delegate
            Dictionary<string, CustomButton.OnClick> buttonActions = new Dictionary<string, CustomButton.OnClick>() {
                { "{NewGame}", NewGame },
                { "{Load}", Load },
                { "{Save}", Save },
                { "{Controls}", KeyBindings },
                { "{Quit}", Quit }
            };

            float buttonSpacing = 30.0f;
            buttons = new Dictionary<string, CustomButton>();
            foreach(KeyValuePair<string, CustomButton.OnClick> pair in buttonActions) {
                buttons.Add(pair.Key, new CustomButton(
                    PrototypeButton,
                    Panel.gameObject,
                    new Vector2(0.0f, (-1.0f * buttons.Count * buttonSpacing)),
                    pair.Key,
                    pair.Value
                ));
            }

            //Hide prototype button
            PrototypeButton.gameObject.SetActive(false);

            //Resize panel
            Height = buttons.Count * buttonSpacing + 5.0f;

            //Set event priority
            WindowEventPriority = (int)WindowEventPriorityDefaults.VeryLow;

            //Show on game start
            Active = true;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        { }

        public override bool HandleWindowEvent(WindowEvent windowEvent)
        {
            if(windowEvent == WindowEvent.Close && Main.Instance.State != State.MainMenu) {
                Active = !Active;
                return true;
            }
            return false;
        }

        private void NewGame()
        {
            Main.Instance.NewGame();
        }

        private void Load()
        {
            SavesWindowManager.Instance.OpenLoadingWindow();
        }

        private void Save()
        {
            SavesWindowManager.Instance.OpenSavingWindow();
        }

        private void KeyBindings()
        {
            KeyBindingsWindowManager.Instance.Active = true;
        }

        private void Quit()
        {
            CustomLogger.Debug("Quit game");
        }

        public override void UpdateUI()
        {
            buttons["{Save}"].Interactable = Main.Instance != null && Main.Instance.State == State.Running;
        }
    }
}