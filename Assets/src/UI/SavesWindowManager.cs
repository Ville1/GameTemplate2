using Game.UI.Components;
using Game.Utils;
using Game.Utils.Config;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Window for saving and loading
    /// </summary>
    public class SavesWindowManager : WindowBase
    {
        private static readonly string FILE_PATTERN = "*.json";

        public enum WindowState { Uninitialized, Saving, Loading }

        public static SavesWindowManager Instance;

        public TMP_Text TitleText;
        public Button CloseButton;
        public GameObject ScrollViewContent;
        public GameObject ScrollViewRowPrototype;
        public TMP_InputField InputField;
        public Button ConfirmButton;
        public Button CancelButton;

        private WindowState windowState = WindowState.Uninitialized;
        private ScrollableList list = null;
        private CustomButton closeButton = null;
        private CustomButton confirmButton = null;
        private CustomButton cancelButton = null;
        private CustomInputField inputField = null;
        private List<string> fileNames = new List<string>();
        private bool validSaveName = false;

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
            Tags.Add(Tag.ClosesOthers);

            list = new ScrollableList(ScrollViewRowPrototype, ScrollViewContent);
            closeButton = new CustomButton(CloseButton, null, Close);
            confirmButton = new CustomButton(ConfirmButton, "{Save}", Confirm);
            cancelButton = new CustomButton(CancelButton, "{Cancel}", Close);
            inputField = new CustomInputField(InputField, "{EnterSaveName}", HandleInputChange);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        public void OpenSavingWindow()
        {
            Open(WindowState.Saving);
        }

        public void OpenLoadingWindow()
        {
            Open(WindowState.Loading);
        }

        private void Open(WindowState state)
        {
            validSaveName = false;
            InputField.text = string.Empty;
            State = state;
            Active = true;
        }

        public WindowState State
        {
            get {
                return windowState;
            }
            set {
                //if(windowState != value) {
                    windowState = value;
                  //  UpdateUI();
                //}
            }
        }

        public override void UpdateUI()
        {
            TitleText.text = new LString(State == WindowState.Saving ? "{SaveGameTitle}" : "{LoadGameTitle}");
            confirmButton.Text = State == WindowState.Saving ? "{Save}" : "{Load}";
            inputField.PlaceholderText = State == WindowState.Saving ? "{EnterSaveName}" : null;
            inputField.Interactable = State == WindowState.Saving;
            list.Clear();
            fileNames.Clear();

            //Find all saves and add rows to the list
            foreach(string fullFileName in Directory.GetFiles(ConfigManager.Config.SaveFolder, FILE_PATTERN)) {
                FileInfo fileInfo = new FileInfo(fullFileName);
                string saveName = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.'));
                list.AddRow(saveName, new List<UIElementData>() {
                    UIElementData.Text("File Name Text", saveName, null),
                    UIElementData.Button("Hidden Button", null, null, () => { HandleFileClick(saveName); })
                });
                fileNames.Add(saveName);
            }

            confirmButton.Interactable = State == WindowState.Saving && validSaveName || State == WindowState.Loading && !string.IsNullOrEmpty(InputField.text);
        }

        private void HandleFileClick(string fileName)
        {
            inputField.Text = fileName;
            confirmButton.Interactable = true;
        }

        private void HandleInputChange(string inputText)
        {
            validSaveName = !string.IsNullOrEmpty(inputText);
            confirmButton.Interactable = validSaveName;
        }

        private void Close()
        {
            Active = false;
        }

        private void Confirm()
        {
            if(State == WindowState.Uninitialized) {
                throw new Exception("Uninitialized");
            }

            if(State == WindowState.Saving) {
                if (fileNames.Contains(inputField.Text)) {
                    //Overwrite save?
                    //TODO: Disable windows that are under other windows
                    ConfirmationDialogManager.Instance.ShowDialog(
                        new LString("OverwriteSaveConfirmationDialog", LTables.Game, inputField.Text),
                        "{OverwriteSaveFile}",
                        "{Cancel}",
                        StartSaving,
                        () => {}
                    );
                } else {
                    StartSaving();
                }
            } else {
                StartLoading();
            }
        }

        private void StartSaving()
        {
            Active = false;
            Main.Instance.SaveGame(ConfigManager.Config.SaveFolder, inputField.Text);
        }

        private void StartLoading()
        {
            Active = false;
            Main.Instance.LoadGame(ConfigManager.Config.SaveFolder, inputField.Text);
        }
    }
}
