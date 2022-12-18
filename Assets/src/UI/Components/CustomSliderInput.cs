using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class CustomSliderInput : CustomNumberInputField
    {
        public enum InputFieldType { Hidden, ReadOnly, Normal }

        private static readonly string DEFAULT_SLIDER_NAME = "Slider";
        private static readonly string DEFAULT_INPUT_NAME = "Input Field";

        public InputFieldType InputField { get; protected set; }

        protected GameObject Container = null;
        protected Slider slider = null;

        public CustomSliderInput(GameObject container, OnNumberChange onChange, float minValue, float maxValue, bool allowDecimals, bool isPercentage, InputFieldType inputField = InputFieldType.Normal) :
            base(FindInput(container, DEFAULT_INPUT_NAME), onChange, minValue, maxValue, 0.0f, allowDecimals, isPercentage, inputField == InputFieldType.ReadOnly)
        {
            InitializeSlider(container, DEFAULT_SLIDER_NAME, minValue, maxValue, inputField);
        }

        public CustomSliderInput(GameObject container, string sliderGameObjectName, string inputGameObjectName, OnNumberChange onChange, float minValue, float maxValue, bool allowDecimals,
            bool isPercentage, InputFieldType inputField = InputFieldType.Normal) : base(FindInput(container, inputGameObjectName), onChange, minValue, maxValue, 0.0f, allowDecimals,
                isPercentage, inputField == InputFieldType.ReadOnly)
        {
            InitializeSlider(container, sliderGameObjectName, minValue, maxValue, inputField);
        }

        protected void InitializeSlider(GameObject container, string sliderName, float minValue, float maxValue, InputFieldType inputField)
        {
            Container = container;
            InputField = inputField;
            sliderName ??= DEFAULT_SLIDER_NAME;

            //Find slider
            GameObject gameObject = GameObjectHelper.Find(container, sliderName);
            if (gameObject == null) {
                CustomLogger.Error("{UIElementError}", string.Format("Slider GameObject not found: {0}/{1}", container.name, sliderName));
            } else {
                slider = gameObject.GetComponent<Slider>();
                if(slider == null) {
                    CustomLogger.Error("{UIElementError}", string.Format("GameObject {0}/{1} has no Slider component", container.name, sliderName));
                } else {
                    //Apply slider settings
                    slider.minValue = (IsPercentage ? 100.0f : 1.0f) * minValue;
                    slider.maxValue = (IsPercentage ? 100.0f : 1.0f) * maxValue;
                    slider.wholeNumbers = !AllowDecimals;
                    //Add event listener
                    Slider.SliderEvent onChangeEvent = new Slider.SliderEvent();
                    onChangeEvent.AddListener(new UnityEngine.Events.UnityAction<float>(HandleSliderChange));
                    slider.onValueChanged = onChangeEvent;
                }
            }

            if(inputField == InputFieldType.Hidden) {
                //Hide input
                InputBase.gameObject.SetActive(false);
            }
        }

        protected static TMP_InputField FindInput(GameObject container, string name)
        {
            name ??= DEFAULT_INPUT_NAME;
            GameObject gameObject = GameObjectHelper.Find(container, name);
            if(gameObject == null) {
                CustomLogger.Error("{UIElementError}", string.Format("Input GameObject not found: {0}/{1}", container.name, name));
                return null;
            }
            TMP_InputField inputField = gameObject.GetComponent<TMP_InputField>();
            if(inputField == null) {
                CustomLogger.Error("{UIElementError}", string.Format("GameObject {0}/{1} has no TMP_InputField component", container.name, name));
            }
            return inputField;
        }

        protected void HandleSliderChange(float value)
        {
            SetText(value.ToString(FormatProvider));
            if (IsPercentage) {
                SetText(Text + "%");
            }
            lastText = Text;
            if(NumberChangeCallback != null) {
                NumberChangeCallback(IsPercentage ? value / 100.0f : value);
            }
        }

        protected override void HandleChange(string value)
        {
            base.HandleChange(value);
            slider.value = (Number.HasValue ? Number.Value : MinValue) * (IsPercentage ? 100.0f : 1.0f);
        }

        public override float? Number
        {
            get {
                return base.Number;
            }
            set {
                base.Number = value;
                slider.value = (value.HasValue ? value.Value : MinValue) * (IsPercentage ? 100.0f : 1.0f);
            }
        }
    }
}
