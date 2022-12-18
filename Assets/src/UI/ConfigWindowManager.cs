using Game.UI.Components;
using Game.Utils;
using Game.Utils.Config;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ConfigWindowManager : WindowBase
    {
        private static readonly string TITLE_ROW = "TitleRow";
        private static readonly string TEXT_ROW = "TextRow";
        private static readonly string NUMBER_ROW = "NumberRow";
        private static readonly string SLIDER_ROW = "SliderRow";
        private static readonly string TOGGLE_ROW = "ToggleRow";

        public static ConfigWindowManager Instance;

        public Button CloseButton;
        public Button SaveButton;
        public Button CancelButton;
        public TMP_InputField SearchInputField;
        public GameObject TabMenuContainer;
        public Button TabMenuButtonPrototype;
        public GameObject TabContainer;
        public GameObject TabPrototype;

        public GameObject TitleRowPrototype;
        public GameObject TextInputRowPrototype;
        public GameObject NumberInputRowPrototype;
        public GameObject SliderInputRowPrototype;
        public GameObject ToggleInputRowPrototype;

        private bool isInitialized = false;
        private CustomButton closeButton = null;
        private CustomButton saveButton = null;
        private CustomButton cancelButton = null;
        private CustomInputField searchInputField = null;
        private TabContainer tabContainer = null;
        private Dictionary<ConfigCategory, TabData> tabs = new Dictionary<ConfigCategory, TabData>();
        private Dictionary<string, string> oldTextValues = new Dictionary<string, string>();
        private Dictionary<string, float> oldNumberValues = new Dictionary<string, float>();
        private Dictionary<string, bool> oldBooleanValues = new Dictionary<string, bool>();
        private Config config = null;

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

            closeButton = new CustomButton(CloseButton, null, Close);
            saveButton = new CustomButton(SaveButton, "{Save}", Save);
            cancelButton = new CustomButton(CancelButton, "{Cancel}", Close);
            searchInputField = new CustomInputField(SearchInputField, "{FilterSettings}", HandleSearchInputChange);

            saveButton.Interactable = false;
            TitleRowPrototype.SetActive(false);
            TextInputRowPrototype.SetActive(false);
            NumberInputRowPrototype.SetActive(false);
            SliderInputRowPrototype.SetActive(false);
            ToggleInputRowPrototype.SetActive(false);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
        }

        public override bool Active {
            get {
                return base.Active;
            }
            set {
                base.Active = value;
                if (value) {
                    if (isInitialized) {
                        GetValuesFromConfig();
                    } else {
                        Initialize();
                    }
                }
            }
        }

        private void Initialize()
        {
            if (isInitialized) {
                return;
            }

            config = ConfigManager.Config.Clone();
            tabContainer = new EnumTabContainer<ConfigCategory>(TabMenuContainer, TabContainer, TabMenuButtonPrototype, TabPrototype);

            Dictionary<string, GameObject> rowPrototypes = new Dictionary<string, GameObject>() {
                { TITLE_ROW, TitleRowPrototype },
                { TEXT_ROW, TextInputRowPrototype },
                { NUMBER_ROW, NumberInputRowPrototype },
                { SLIDER_ROW, SliderInputRowPrototype },
                { TOGGLE_ROW, ToggleInputRowPrototype }
            };
            AddTab("{GeneralSettings}", ConfigCategory.General, rowPrototypes);
            AddTab("{AudioSettings}", ConfigCategory.Audio, rowPrototypes);

            isInitialized = true;
        }

        private void HandleSearchInputChange(string inputText)
        {
            UpdateUI();
        }

        private void Close()
        {
            Active = false;
        }

        private void Save()
        {
            ConfigManager.Save(config);
            Active = false;
        }

        private void AddTab(LString title, ConfigCategory category, Dictionary<string, GameObject> rowPrototypes)
        {
            GameObject tab = tabContainer.AddTab(title);
            bool wasActive = tab.activeSelf;
            tab.SetActive(true);

            Dictionary<string, Guid> textRows = new Dictionary<string, Guid>();
            Dictionary<string, Guid> numberRows = new Dictionary<string, Guid>();
            Dictionary<string, Guid> booleanRows = new Dictionary<string, Guid>();

            ScrollableList list = new ScrollableList(rowPrototypes, GameObjectHelper.Find(tab, "Scroll View"));
            foreach (FieldInfo fieldInfo in config.GetType().GetFields()) {
                ConfigUIDataAttribute uiData = fieldInfo.GetCustomAttribute<ConfigUIDataAttribute>();
                if (uiData != null && uiData.Category == category) {
                    object value = fieldInfo.GetValue(config);
                    Guid fieldId = Guid.NewGuid();
                    switch (uiData.Type) {
                        case InputType.Text:
                            list.AddRow(fieldId, new List<UIElementData>() {
                                UIElementData.Text("Label Text", uiData.Label, null),
                                GetTextInput(category, (string)value, fieldInfo, uiData.MaxLenght)
                            }, TEXT_ROW);
                            oldTextValues.Add(fieldInfo.Name, (string)value);
                            textRows.Add(fieldInfo.Name, fieldId);
                            break;
                        case InputType.Number:
                            list.AddRow(fieldId, new List<UIElementData>() {
                                UIElementData.Text("Label Text", uiData.Label, null),
                                GetNumberInput(category, (float)value, fieldInfo, uiData.MinValue, uiData.MaxValue, uiData.IsPercentage ? 0.1f : 1.0f, uiData.AllowDecimals, uiData.IsPercentage)
                            }, NUMBER_ROW);
                            oldNumberValues.Add(fieldInfo.Name, (float)value);
                            numberRows.Add(fieldInfo.Name, fieldId);
                            break;
                        case InputType.Slider:
                            list.AddRow(fieldId, new List<UIElementData>() {
                                UIElementData.Text("Label Text", uiData.Label, null),
                                GetSliderInput(category, (float)value, fieldInfo, uiData.MinValue, uiData.MaxValue, uiData.AllowDecimals, uiData.IsPercentage)
                            }, SLIDER_ROW);
                            oldNumberValues.Add(fieldInfo.Name, (float)value);
                            numberRows.Add(fieldInfo.Name, fieldId);
                            break;
                        case InputType.Toggle:
                            list.AddRow(fieldId, new List<UIElementData>() {
                                UIElementData.Text("Label Text", uiData.Label, null),
                                GetToggleInput(category, (bool)value, fieldInfo)
                            }, TOGGLE_ROW);
                            oldBooleanValues.Add(fieldInfo.Name, (bool)value);
                            booleanRows.Add(fieldInfo.Name, fieldId);
                            break;
                    }
                }
            }

            tab.SetActive(wasActive);
            tabs.Add(category, new TabData() {
                GameObject = tab,
                List = list,
                TextRows = textRows,
                NumberRows = numberRows,
                BooleanRows = booleanRows
            });

            config = ConfigManager.Config.Clone();
        }

        private void GetValuesFromConfig()
        {
            config = ConfigManager.Config.Clone();
            oldTextValues.Clear();
            oldNumberValues.Clear();
            oldBooleanValues.Clear();

            foreach (KeyValuePair<ConfigCategory, TabData> pair in tabs) {
                foreach (FieldInfo fieldInfo in config.GetType().GetFields()) {
                    ConfigUIDataAttribute uiData = fieldInfo.GetCustomAttribute<ConfigUIDataAttribute>();
                    if (uiData != null && uiData.Category == pair.Key) {
                        object value = fieldInfo.GetValue(config);
                        switch (uiData.Type) {
                            case InputType.Text:
                                pair.Value.List.SetRow(pair.Value.TextRows[fieldInfo.Name], new List<UIElementData>() { GetTextInput(pair.Key, (string)value, fieldInfo, uiData.MaxLenght) });
                                oldTextValues.Add(fieldInfo.Name, (string)value);
                                break;
                            case InputType.Number:
                                pair.Value.List.SetRow(pair.Value.NumberRows[fieldInfo.Name], new List<UIElementData>() { GetNumberInput(pair.Key, (float)value, fieldInfo, uiData.MinValue, uiData.MaxValue, uiData.IsPercentage ? 0.1f : 1.0f, uiData.AllowDecimals, uiData.IsPercentage) });
                                oldNumberValues.Add(fieldInfo.Name, (float)value);
                                break;
                            case InputType.Slider:
                                pair.Value.List.SetRow(pair.Value.NumberRows[fieldInfo.Name], new List<UIElementData>() { GetSliderInput(pair.Key, (float)value, fieldInfo, uiData.MinValue, uiData.MaxValue, uiData.AllowDecimals, uiData.IsPercentage) });
                                oldNumberValues.Add(fieldInfo.Name, (float)value);
                                break;
                            case InputType.Toggle:
                                pair.Value.List.SetRow(pair.Value.BooleanRows[fieldInfo.Name], new List<UIElementData>() { GetToggleInput(pair.Key, (bool)value, fieldInfo) });
                                oldBooleanValues.Add(fieldInfo.Name, (bool)value);
                                break;
                        }
                    }
                }
            }

            saveButton.Interactable = false;
        }

        private void HandleTextChange(ConfigCategory category, string text, FieldInfo fieldInfo)
        {
            if (!oldTextValues.ContainsKey(fieldInfo.Name)) {
                //Initialization is not finished
                return;
            }
            saveButton.Interactable = oldTextValues[fieldInfo.Name] != text;
            fieldInfo.SetValue(config, text);
        }

        private void HandleNumberChange(ConfigCategory category, float? number, FieldInfo fieldInfo)
        {
            if (!oldNumberValues.ContainsKey(fieldInfo.Name)) {
                //Initialization is not finished
                return;
            }
            saveButton.Interactable = oldNumberValues[fieldInfo.Name] != number;
            fieldInfo.SetValue(config, number);
        }

        private void HandleToggleChange(ConfigCategory category, bool value, FieldInfo fieldInfo)
        {
            if (!oldBooleanValues.ContainsKey(fieldInfo.Name)) {
                //Initialization is not finished
                return;
            }
            saveButton.Interactable = oldBooleanValues[fieldInfo.Name] != value;
            fieldInfo.SetValue(config, value);
        }

        private UIElementData GetTextInput(ConfigCategory category, string value, FieldInfo fieldInfo, int maxLenght)
        {
            return UIElementData.TextInput("Input Field", (string text) => { HandleTextChange(category, text, fieldInfo); }, value, string.Empty, maxLenght);
        }

        private UIElementData GetNumberInput(ConfigCategory category, float value, FieldInfo fieldInfo, float minValue, float maxValue, float increment, bool allowDecimals, bool isPercentage)
        {
            return UIElementData.NumberInput("Input Field", (float? number) => { HandleNumberChange(category, number, fieldInfo); }, value, null, minValue, maxValue, increment, allowDecimals, isPercentage);
        }

        private UIElementData GetSliderInput(ConfigCategory category, float value, FieldInfo fieldInfo, float minValue, float maxValue, bool allowDecimals, bool isPercentage)
        {
            return UIElementData.SliderInput("Slider Container", (float? number) => { HandleNumberChange(category, number, fieldInfo); }, value, minValue, maxValue, allowDecimals, isPercentage);
        }

        private UIElementData GetToggleInput(ConfigCategory category, bool value, FieldInfo fieldInfo)
        {
            return UIElementData.ToggleInput("Toggle", (bool value) => { HandleToggleChange(category, value, fieldInfo); }, value, null);
        }

        private class TabData
        {
            public GameObject GameObject { get; set; }
            public ScrollableList List { get; set; }
            public Dictionary<string, Guid> TextRows { get; set; }
            public Dictionary<string, Guid> NumberRows { get; set; }
            public Dictionary<string, Guid> BooleanRows { get; set; }
        }
    }
}