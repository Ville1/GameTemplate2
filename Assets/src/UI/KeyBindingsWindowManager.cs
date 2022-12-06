using Game.Input;
using Game.UI.Components;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class KeyBindingsWindowManager : WindowBase
    {
        private static readonly List<KeyCode> NOT_ALLOWED_KEYS = new List<KeyCode>() { KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2 };

        public static KeyBindingsWindowManager Instance;

        public Button CloseButton;
        public GameObject ScrollView;
        public TMP_InputField SearchInputField;
        public Button SaveButton;
        public Button CancelButton;
        public GameObject RebindPanel;
        public TMP_Text RebindPanelText;
        public GameObject RebindAlreadyUsedPanel;
        public TMP_Text RebindAlreadyUsedPanelText;

        private List<KeyBinding> keyBindings = new List<KeyBinding>();
        private List<string> changedBindings = new List<string>();
        private ScrollableList list = null;
        private CustomButton closeButton = null;
        private CustomButton saveButton = null;
        private CustomButton cancelButton = null;
        private CustomInputField searchInputField = null;
        private KeyBinding currentKeyBinding = null;
        private KeyBinding conflictKeyBinding = null;

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
            BlockKeyboardInputs = true;

            list = new ScrollableList(ScrollView);
            closeButton = new CustomButton(CloseButton, null, Close);
            saveButton = new CustomButton(SaveButton, "{Save}", Save);
            cancelButton = new CustomButton(CancelButton, "{Cancel}", Close);
            searchInputField = new CustomInputField(SearchInputField, "{FilterKeyBindings}", HandleSearchInputChange);

            RebindPanel.SetActive(false);
            RebindAlreadyUsedPanel.SetActive(false);
            RebindAlreadyUsedPanelText.faceColor = Color.red;

            MouseManager.Instance.AddEventListener(MouseButton.Left, new MouseEvent((GameObject target) => { ClosePanels(); }, 0, new List<MouseEventTag>() { MouseEventTag.IgnoreUI }, false));
            MouseManager.Instance.AddEventListener(MouseButton.Middle, new MouseEvent((GameObject target) => { ClosePanels(); }, 0, new List<MouseEventTag>() { MouseEventTag.IgnoreUI }, false));
            MouseManager.Instance.AddEventListener(MouseButton.Right, new MouseEvent((GameObject target) => { ClosePanels(); }, 0, new List<MouseEventTag>() { MouseEventTag.IgnoreUI }, false));
            MouseManager.Instance.AddEventListener(MouseButton.Left, new MouseNothingClickEvent(() => { ClosePanels(); }, 0, new List<MouseEventTag>() { MouseEventTag.IgnoreUI }, false));
            MouseManager.Instance.AddEventListener(MouseButton.Middle, new MouseNothingClickEvent(() => { ClosePanels(); }, 0, new List<MouseEventTag>() { MouseEventTag.IgnoreUI }, false));
            MouseManager.Instance.AddEventListener(MouseButton.Right, new MouseNothingClickEvent(() => { ClosePanels(); }, 0, new List<MouseEventTag>() { MouseEventTag.IgnoreUI }, false));
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if(currentKeyBinding != null) {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                    if (!NOT_ALLOWED_KEYS.Contains(keyCode) && UnityEngine.Input.GetKeyDown(keyCode)) {
                        TryChangeKeyBinding(keyCode);
                        break;
                    }
                }
            }
        }

        public override bool Active {
            get {
                return base.Active;
            }
            set {
                if (value) {
                    searchInputField.Text = string.Empty;
                    SetRebindKeyBinding(null);
                    SetConflictingKeyBinding(null);
                    keyBindings = KeyBindings.All.Select(keyBinding => new KeyBinding(keyBinding)).ToList();
                    changedBindings.Clear();
                }
                base.Active = value;
            }
        }

        public override void UpdateUI()
        {
            list.Clear();
            foreach(KeyBinding keyBinding in keyBindings.Where(b => b.Name.ToString().ToLower().Contains(searchInputField.Text.ToLower()))) {
                list.AddRow(keyBinding.InternalName, new List<UIElementData>() {
                    UIElementData.Text("Binding Name Text", keyBinding.Name + (changedBindings.Contains(keyBinding.InternalName) ? "*" : ""), null),
                    UIElementData.Text("Key Text", keyBinding.KeyCode.ToString(), null),
                    UIElementData.Button("Hidden Button", null, null, () => { SetRebindKeyBinding(keyBinding); })
                });
            }
            saveButton.Interactable = changedBindings.Count != 0;
        }

        private void SetRebindKeyBinding(KeyBinding keyBinding)
        {
            if(conflictKeyBinding != null && keyBinding != null) {
                SetConflictingKeyBinding(null);
                return;
            }

            currentKeyBinding = currentKeyBinding == null ? keyBinding : null;
            if(currentKeyBinding == null) {
                RebindPanel.SetActive(false);
            } else {
                RebindPanel.SetActive(true);
                RebindPanelText.text = keyBinding.Name;
            }
        }

        private void SetConflictingKeyBinding(KeyBinding keyBinding)
        {
            conflictKeyBinding = keyBinding;
            if(conflictKeyBinding == null) {
                RebindAlreadyUsedPanel.SetActive(false);
            } else {
                RebindAlreadyUsedPanel.SetActive(true);
                RebindAlreadyUsedPanelText.text = conflictKeyBinding.Name;
                SetRebindKeyBinding(null);
            }
        }

        private void TryChangeKeyBinding(KeyCode keyCode)
        {
            SetConflictingKeyBinding(keyBindings.FirstOrDefault(keyBinding => keyBinding.KeyCode == keyCode && keyBinding.InternalName != currentKeyBinding.InternalName));
            if(conflictKeyBinding == null) {
                if(currentKeyBinding.KeyCode != keyCode) {
                    changedBindings.Add(currentKeyBinding.InternalName);
                }
                currentKeyBinding.KeyCode = keyCode;
                SetRebindKeyBinding(null);
                UpdateUI();
            }
        }

        private void ClosePanels()
        {
            if (Active) {
                SetRebindKeyBinding(null);
                SetConflictingKeyBinding(null);
            }
        }

        private void HandleSearchInputChange(string inputText)
        {
            UpdateUI();
        }

        private void Save()
        {
            foreach(KeyBinding keyBinding in keyBindings) {
                KeyBindings.Get(keyBinding.InternalName).Rebind(keyBinding.KeyCode);
            }
            Active = false;
        }

        private void Close()
        {
            Active = false;
        }
    }
}
