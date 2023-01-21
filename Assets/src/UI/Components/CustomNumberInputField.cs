using Game.Utils;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class CustomNumberInputField : CustomInputField
    {
        protected static readonly string DEFAULT_INCREASE_BUTTON_NAME = "Increase Button";
        protected static readonly string DEFAULT_DECREASE_BUTTON_NAME = "Decrease Button";

        public delegate void OnNumberChange(float? val);

        public float MinValue { get; protected set; }
        public float MaxValue { get; protected set; }
        public float IncrementAmount { get; protected set; }
        public bool AllowDecimals { get; protected set; }
        public bool IsPercentage { get; protected set; }
        public OnNumberChange NumberChangeCallback { get; protected set; }

        protected bool ShowIncrementButtons { get { return IncrementAmount != 0.0f; } }

        protected string lastText = string.Empty;
        protected IFormatProvider formatProvider = CultureInfo.CurrentCulture.NumberFormat;

        protected CustomButton increaseButton = null;
        protected CustomButton decreaseButton = null;

        public CustomNumberInputField(TMP_InputField input, OnNumberChange onChange, float minValue = float.MinValue, float maxValue = float.MaxValue, float incrementAmount = 0.0f,
            bool allowDecimals = true, bool isPercentage = false, bool isDisabled = false) : base(input, null, int.MaxValue, isDisabled)
        {
            InitializeNumberInput(onChange, minValue, maxValue, incrementAmount, allowDecimals, null, null, isPercentage, isDisabled);
        }

        public CustomNumberInputField(TMP_InputField input, LString placeholderText, OnNumberChange onChange, float minValue = float.MinValue, float maxValue = float.MaxValue,
            float incrementAmount = 0.0f, bool allowDecimals = true, bool isPercentage = false, bool isDisabled = false) : base(input, placeholderText, null, int.MaxValue)
        {
            InitializeNumberInput(onChange, minValue, maxValue, incrementAmount, allowDecimals, null, null, isPercentage, isDisabled);
        }

        public CustomNumberInputField(TMP_InputField input, TMP_Text placeholder, LString placeholderText, OnNumberChange onChange, float minValue = float.MinValue,
            float maxValue = float.MaxValue, float incrementAmount = 0.0f, bool allowDecimals = true, bool isPercentage = false, bool isDisabled = false) :
            base(input, placeholder, placeholderText, null, int.MaxValue, isDisabled)
        {
            InitializeNumberInput(onChange, minValue, maxValue, incrementAmount, allowDecimals, null, null, isPercentage, isDisabled);
        }

        public CustomNumberInputField(TMP_InputField input, string placeholderGameObjectName, LString placeholderText, OnNumberChange onChange, float minValue = float.MinValue,
            float maxValue = float.MaxValue, float incrementAmount = 0.0f, bool allowDecimals = true, string increaseButtonName = null, string decreaseButtonName = null,
            bool isPercentage = false, bool isDisabled = false) : base(input, placeholderGameObjectName, placeholderText, null, int.MaxValue, isDisabled)
        {
            InitializeNumberInput(onChange, minValue, maxValue, incrementAmount, allowDecimals, increaseButtonName, decreaseButtonName, isPercentage, isDisabled);
        }

        protected void InitializeNumberInput(OnNumberChange onChange, float minValue, float maxValue, float incrementAmount, bool allowDecimals, string increaseButtonName, string decreaseButtonName,
            bool isPercentage, bool isDisabled)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            IncrementAmount = incrementAmount;
            AllowDecimals = allowDecimals;
            NumberChangeCallback = onChange;
            IsPercentage = isPercentage;
            IsDisabled = isDisabled;

            //Find increase and decrease buttons
            increaseButton = InitializeIncrementButton(increaseButtonName, DEFAULT_INCREASE_BUTTON_NAME, Increase);
            decreaseButton = InitializeIncrementButton(decreaseButtonName, DEFAULT_DECREASE_BUTTON_NAME, Decrease);
        }

        protected CustomButton InitializeIncrementButton(string buttonName, string defaultName, CustomButton.OnClick onClick)
        {
            bool useDefaultName = string.IsNullOrEmpty(buttonName);
            buttonName = useDefaultName ? defaultName : buttonName;
            GameObject increaseButtonGameObject = GameObjectHelper.Find(InputBase.gameObject, buttonName);
            if (increaseButtonGameObject == null) {
                //Failed to find the game object
                if (!ShowIncrementButtons) {
                    //No need to make new buttons
                    return null;
                }
                if (!useDefaultName) {
                    CustomLogger.Error("{UIElementError}", string.Format("Increment button \"{0}\" not found", buttonName));
                    return null;
                } else {
                    //Create a new button
                    bool increase = defaultName == DEFAULT_DECREASE_BUTTON_NAME;
                    increaseButtonGameObject = new GameObject(defaultName);
                    increaseButtonGameObject.transform.parent = InputBase.gameObject.transform;
                    RectTransform rectTransform = increaseButtonGameObject.AddComponent<RectTransform>();
                    rectTransform.anchorMin = new Vector2(1.0f, increase ? 1.0f : 0.0f);
                    rectTransform.anchorMax = new Vector2(1.0f, increase ? 1.0f : 0.0f);
                    rectTransform.anchoredPosition = new Vector2(-15.0f, increase ? -9.0f : 9.0f);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 25.0f);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 12.0f);
                    increaseButtonGameObject.AddComponent<CanvasRenderer>();
                    Image image = increaseButtonGameObject.AddComponent<Image>();
                    image.sprite = TextureManager.Instance.DefaultUISprite;
                    increaseButtonGameObject.AddComponent<Button>();
                }
            }
            Button button = increaseButtonGameObject.GetComponent<Button>();
            if(button == null) {
                CustomLogger.Error("{UIElementError}", "Increment button has no Button component");
                return null;
            }
            button.gameObject.SetActive(ShowIncrementButtons);
            return new CustomButton(button, null, onClick);
        }

        public virtual float? Number
        {
            get {
                if (string.IsNullOrEmpty(Text) || Text == "-") {
                    return null;
                }
                string text = Text;
                if (IsPercentage && text.EndsWith("%")) {
                    if(text.Length == 1) {
                        return null;
                    }
                    text = text[..^1];
                }
                float number;
                if(float.TryParse(text, NumberStyle, FormatProvider, out number)) {
                    if (IsPercentage) {
                        number /= 100.0f;
                    }
                    return number;
                }
                return null;
            }
            set {
                if (!value.HasValue) {
                    SetText(string.Empty);
                    lastText = string.Empty;
                    return;
                }
                float number = Mathf.Clamp(value.Value, MinValue, MaxValue);
                if (IsPercentage) {
                    number *= 100.0f;
                }
                if (!AllowDecimals) {
                    number = Mathf.Round(number);
                }
                SetText(number.ToString(FormatProvider));
                if (IsPercentage) {
                    SetText(InputBase.text + "%");
                }
                lastText = InputBase.text;
            }
        }

        public IFormatProvider FormatProvider
        {
            get {
                return formatProvider;
            }
            set {
                float? number = Number;
                formatProvider = value;
                Number = number;
            }
        }

        protected void Increase()
        {
            Number = Number.HasValue ? Number.Value + IncrementAmount : IncrementAmount;
            HandleChange(InputBase.text);
        }

        protected void Decrease()
        {
            Number = Number.HasValue ? Number.Value - IncrementAmount : (-1.0f) * IncrementAmount;
            HandleChange(InputBase.text);
        }

        protected NumberStyles NumberStyle
        {
            get {
                return AllowDecimals ? NumberStyles.Number : NumberStyles.None;
            }
        }

        protected override void HandleChange(string value)
        {
            if (skipCallbacks) {
                skipCallbacks = false;
                return;
            }

            if (string.IsNullOrEmpty(value)) {
                //Empty field
                lastText = string.Empty;
            } else {
                if(IsPercentage && value == "%") {
                    //User is starting to type a percent number
                    lastText = value;
                } else if(value == "-") {
                    //User is starting to type a negative number
                    if(MinValue >= 0.0f) {
                        //Negative values are not allowed
                        SetText(lastText);
                    } else {
                        lastText = value;
                    }
                } else {
                    float parsedNumber;
                    string parseValue = value;
                    if (IsPercentage && parseValue.EndsWith("%")) {
                        parseValue = parseValue[..^1];
                    }
                    if (float.TryParse(parseValue, NumberStyle, FormatProvider, out parsedNumber)) {
                        //Input can be parsed to float
                        if (IsPercentage) {
                            parsedNumber /= 100.0f;
                        }
                        if (parsedNumber < MinValue) {
                            //Value is too low
                            SetText((IsPercentage ? MinValue * 100.0f : MinValue).ToString(FormatProvider));
                            if (IsPercentage) {
                                SetText(InputBase.text + "%");
                            }
                            lastText = Text;
                        } else if (parsedNumber > MaxValue) {
                            //Value is too high
                            SetText((IsPercentage ? MaxValue * 100.0f : MaxValue).ToString(FormatProvider));
                            if (IsPercentage) {
                                SetText(InputBase.text + "%");
                            }
                            lastText = Text;
                        } else {
                            //Valid input
                            if (IsPercentage && !value.EndsWith("%")) {
                                value += "%";
                                Text += "%";
                            }
                            lastText = value;
                        }
                    } else {
                        //Invalid input
                        SetText(lastText);
                    }
                }
            }

            if(NumberChangeCallback != null) {
                NumberChangeCallback(Number);
            }
        }
    }
}
