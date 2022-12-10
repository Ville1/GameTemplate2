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
        private Dictionary<string, KeyCode> changedBindings = new Dictionary<string, KeyCode>();//Key binding name / old key code
        private List<KeyBindingCategory> closedCategories = new List<KeyBindingCategory>();
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

            MouseManager.Instance.AddEventListener(new MouseEvent((GameObject target) => { ClosePanels(); }, 0, MouseEventTag.IgnoreUI, false));
            MouseManager.Instance.AddEventListener(new MouseNothingClickEvent(() => { ClosePanels(); }, 0, MouseEventTag.IgnoreUI, false));
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
                    closedCategories.Clear();
                }
                base.Active = value;
            }
        }

        public override void UpdateUI()
        {
            list.Clear();

            IEnumerable<KeyBinding> bindings = keyBindings.Where(b => (string.IsNullOrEmpty(searchInputField.Text) || b.Name.ToString().ToLower().Contains(searchInputField.Text.ToLower()))
                && !b.Category.IsInternal);
            IEnumerable<IGrouping<KeyBindingCategory, KeyBinding>> groupings = bindings.GroupBy(b => b.Category).OrderBy(g => g.Key.Order);

            foreach (IGrouping<KeyBindingCategory, KeyBinding> grouping in groupings) {
                list.AddRow(string.Format("Category_{0}", grouping.Key.InternalName), new List<UIElementData>() {
                    UIElementData.Text("Binding Name Text", "    " + grouping.Key.Name + (closedCategories.Contains(grouping.Key) ? string.Format(" ({0})", grouping.Count()) : string.Empty), null),
                    UIElementData.Text("Key Text", string.Empty, null),
                    UIElementData.Image("Chevron Image", new SpriteData("icon chevron down", TextureDirectory.UI, 0, false, closedCategories.Contains(grouping.Key))),
                    UIElementData.Button("Hidden Button", null, null, () => {
                        if (closedCategories.Contains(grouping.Key)) {
                            closedCategories.Remove(grouping.Key);
                        } else {
                            closedCategories.Add(grouping.Key);
                        }
                        UpdateUI();
                    })
                });

                if (!closedCategories.Contains(grouping.Key)) {
                    foreach (KeyBinding keyBinding in grouping) {
                        list.AddRow(string.Format("Binding_{0}", keyBinding.InternalName), new List<UIElementData>() {
                            UIElementData.Text("Binding Name Text", keyBinding.Name + (changedBindings.ContainsKey(keyBinding.InternalName) ? "*" : ""), null),
                            UIElementData.Text("Key Text", keyBinding.KeyCode.ToString(), null),
                            UIElementData.Image("Chevron Image", new SpriteData()),
                            UIElementData.Button("Hidden Button", null, null, () => { SetRebindKeyBinding(keyBinding); })
                        });
                    }
                }
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

        private void FindConflictingKeyBinding(KeyCode keyCode)
        {
            KeyBinding conflictingKeyBinding = null;
            if (currentKeyBinding.HasConflictingCategories) {
                List<KeyBindingCategory> bindingCategories = currentKeyBinding.ConflictingCategories.Contains(KeyBindingCategories.Any) ? KeyBindingCategories.All : currentKeyBinding.ConflictingCategories;
                foreach (KeyBindingCategory category in bindingCategories) {
                    conflictingKeyBinding = keyBindings.FirstOrDefault(binding => binding.Category == category && binding.KeyCode == keyCode && binding.InternalName != currentKeyBinding.InternalName);
                    if(conflictingKeyBinding != null) {
                        break;
                    }
                }
            }
            SetConflictingKeyBinding(conflictingKeyBinding);
        }

        private void TryChangeKeyBinding(KeyCode keyCode)
        {
            FindConflictingKeyBinding(keyCode);
            if (conflictKeyBinding == null) {
                if(currentKeyBinding.KeyCode != keyCode) {
                    if (changedBindings.ContainsKey(currentKeyBinding.InternalName) && changedBindings[currentKeyBinding.InternalName] == keyCode) {
                        //Key binding was changed back to it's old key code
                        changedBindings.Remove(currentKeyBinding.InternalName);
                    } else if(!changedBindings.ContainsKey(currentKeyBinding.InternalName)) {
                        //Newly changed key binding
                        changedBindings.Add(currentKeyBinding.InternalName, currentKeyBinding.KeyCode);
                    }
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
            KeyBindings.SaveChanges();
            Active = false;
        }

        private void Close()
        {
            Active = false;
        }
    }
}
