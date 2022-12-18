using Game.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class UIElementData
    {
        public enum ElementType { Text, Button, Image, Tooltip, TextInput, NumberInput, SliderInput, Toggle }

        //Basic data
        public ElementType Type { get; private set; }
        public string GameObjectName { get; private set; }
        public string ElementText { get; private set; }
        public Color? TextColor { get; set; }
        public CustomButton.OnClick OnClick { get; private set; }
        public UISpriteData SpriteData { get; private set; }

        //Input data
        public string DefaultStringValue { get; private set; } = null;
        public float? DefaultNumberValue { get; private set; } = null;
        public string PlaceholderText { get; private set; } = null;
        public float MinValue { get; private set; } = float.MinValue;
        public float MaxValue { get; private set; } = float.MaxValue;
        public float IncrementAmount { get; private set; } = 1.0f;
        public bool AllowDecimals { get; private set; } = false;
        public int MaxLenght { get; private set; } = int.MaxValue;
        public bool IsPercentage { get; protected set; } = false;
        public bool IsDisabled { get; protected set; } = false;
        public CustomInputField.OnChange OnTextInputChange { get; private set; } = null;
        public CustomNumberInputField.OnNumberChange OnNumberInputChange { get; private set; } = null;
        public CustomToggleInput.OnChange OnToggleInputChange { get; private set; } = null;
        public CustomSliderInput.InputFieldType SliderInputType { get; private set; } = CustomSliderInput.InputFieldType.Normal;

        /// <summary>
        /// Basic ui element constructor
        /// </summary>
        private UIElementData(ElementType type, string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, UISpriteData spriteData)
        {
            Type = type;
            GameObjectName = gameObjectName;
            ElementText = text;
            TextColor = textColor;
            OnClick = onClick;
            SpriteData = new UISpriteData(spriteData);
        }

        /// <summary>
        /// Input element constructor
        /// </summary>
        private UIElementData(ElementType type, string gameObjectName, string defaultStringValue, float? defaultNumberValue, string placeholderText, float minValue, float maxValue,
            float incrementAmount, bool allowDecimals, bool isPercentage, bool isDisabled, int maxLenght, CustomInputField.OnChange onTextInputChange, CustomNumberInputField.OnNumberChange onNumberInputChange,
            CustomToggleInput.OnChange onToggleInputChange, CustomSliderInput.InputFieldType sliderInputType = CustomSliderInput.InputFieldType.Normal)
        {
            Type = type;
            GameObjectName = gameObjectName;
            DefaultStringValue = defaultStringValue;
            DefaultNumberValue = defaultNumberValue;
            ElementText = null;
            TextColor = null;
            OnClick = null;
            SpriteData = new UISpriteData();

            PlaceholderText = placeholderText;
            MinValue = minValue;
            MaxValue = maxValue;
            IncrementAmount = incrementAmount;
            AllowDecimals = allowDecimals;
            IsPercentage = isPercentage;
            IsDisabled = isDisabled;
            MaxLenght = maxLenght;
            OnTextInputChange = onTextInputChange;
            OnNumberInputChange = onNumberInputChange;
            OnToggleInputChange = onToggleInputChange;
            SliderInputType = sliderInputType;
        }

        public static UIElementData Text(string gameObjectName, string text, Color? textColor)
        {
            return new UIElementData(ElementType.Text, gameObjectName, text, textColor, null, new UISpriteData());
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, new UISpriteData());
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, UISpriteData spriteData)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, spriteData);
        }

        public static UIElementData Image(string gameObjectName, UISpriteData spriteData)
        {
            return new UIElementData(ElementType.Image, gameObjectName, null, null, null, spriteData);
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, SpriteData spriteData)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, new UISpriteData(spriteData));
        }

        public static UIElementData Image(string gameObjectName, SpriteData spriteData)
        {
            return new UIElementData(ElementType.Image, gameObjectName, null, null, null, new UISpriteData(spriteData));
        }

        public static UIElementData Tooltip(string gameObjectName, LString tooltip)
        {
            return new UIElementData(ElementType.Tooltip, gameObjectName, tooltip, null, null, new UISpriteData());
        }

        public static UIElementData TextInput(string gameObjectName, CustomInputField.OnChange onChange, string defaultValue, LString placeholder = null, int maxLenght = int.MaxValue,
            bool isDisabled = false)
        {
            return new UIElementData(ElementType.TextInput, gameObjectName, defaultValue, null, placeholder, float.MinValue, float.MaxValue, 1.0f, false, false, isDisabled, maxLenght, onChange, null, null);
        }

        public static UIElementData NumberInput(string gameObjectName, CustomNumberInputField.OnNumberChange onChange, float defaultValue, LString placeholder = null,
            float minValue = float.MinValue, float maxValue = float.MaxValue, float increment = 1.0f, bool allowDecimals = true, bool isPercentage = false, bool isDisabled = false)
        {
            return new UIElementData(ElementType.NumberInput, gameObjectName, null, defaultValue, placeholder, minValue, maxValue, increment, allowDecimals, isPercentage, isDisabled, int.MaxValue, null, onChange, null);
        }

        public static UIElementData SliderInput(string gameObjectName, CustomNumberInputField.OnNumberChange onChange, float defaultValue, float minValue, float maxValue, bool allowDecimals, bool isPercentage,
            CustomSliderInput.InputFieldType sliderInputType = CustomSliderInput.InputFieldType.Normal)
        {
            return new UIElementData(ElementType.SliderInput, gameObjectName, null, defaultValue, null, minValue, maxValue, 0.0f, allowDecimals, isPercentage, false, int.MaxValue, null, onChange, null, sliderInputType);
        }

        public static UIElementData ToggleInput(string gameObjectName, CustomToggleInput.OnChange onChange, bool defaultValue, LString label)
        {
            return new UIElementData(ElementType.Toggle, gameObjectName, label, defaultValue ? 1.0f : 0.0f, null, float.MinValue, float.MaxValue, 0.0f, false, false, false, int.MaxValue, null, null, onChange);
        }

        public void Set(GameObject parentGameObject)
        {
            switch (Type) {
                case ElementType.Text:
                    UIHelper.SetText(parentGameObject, GameObjectName, ElementText, TextColor);
                    break;
                case ElementType.Button:
                    UIHelper.SetButton(parentGameObject, GameObjectName, ElementText, OnClick);
                    if (!SpriteData.IsEmpty) {
                        //TODO: Implement this
                        //Needs name for image GameObject?
                        throw new NotImplementedException("Buttons with images is not implemented");
                    }
                    break;
                case ElementType.Image:
                    if (SpriteData.IsEmpty) {
                        GameObjectHelper.Find(parentGameObject, GameObjectName).SetActive(false);
                    } else {
                        UIHelper.SetImage(parentGameObject, GameObjectName, SpriteData);
                    }
                    break;
                case ElementType.Tooltip:
                    GameObject gameObject = GameObjectHelper.Find(parentGameObject, GameObjectName);
                    TooltipManager.Instance.UnregisterTooltip(gameObject);
                    if (!string.IsNullOrEmpty(ElementText)) {
                        TooltipManager.Instance.RegisterTooltip(new Tooltip(gameObject, ElementText));
                    }
                    break;
                case ElementType.TextInput:
                    CustomInputField textInput = new CustomInputField(GameObjectHelper.Find(parentGameObject, GameObjectName).GetComponent<TMPro.TMP_InputField>(), PlaceholderText,
                        OnTextInputChange, MaxLenght);
                    textInput.Text = DefaultStringValue;
                    break;
                case ElementType.NumberInput:
                    CustomNumberInputField numberInput = new CustomNumberInputField(GameObjectHelper.Find(parentGameObject, GameObjectName).GetComponent<TMPro.TMP_InputField>(),
                        PlaceholderText, OnNumberInputChange, MinValue, MaxValue, IncrementAmount, AllowDecimals, IsPercentage);
                    numberInput.Number = DefaultNumberValue;
                    break;
                case ElementType.SliderInput:
                    CustomSliderInput sliderInput = new CustomSliderInput(GameObjectHelper.Find(parentGameObject, GameObjectName), OnNumberInputChange, MinValue, MaxValue, AllowDecimals, IsPercentage, SliderInputType);
                    sliderInput.Number = DefaultNumberValue;
                    break;
                case ElementType.Toggle:
                    CustomToggleInput toggleInput = new CustomToggleInput(GameObjectHelper.Find(parentGameObject, GameObjectName).GetComponent<Toggle>(), DefaultStringValue, OnToggleInputChange);
                    toggleInput.Value = DefaultNumberValue == 1.0f;
                    break;
                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }
    }
}
