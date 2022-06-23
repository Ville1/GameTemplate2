using TMPro;
using UnityEngine;

namespace Game.UI.Components
{
    public class CustomInputField
    {
        private static readonly string DEFAULT_PLACEHOLDER_GAMEOBJECT_NAME = "Placeholder";

        public delegate void OnChange(string val);

        public TMP_InputField InputBase { get; private set; }
        public TMP_Text PlaceholderText { get; private set; }

        private string placeholderLocalizationKey;

        public CustomInputField(TMP_InputField input, OnChange onChange)
        {
            Initialize(input, null, null, null, onChange);
        }

        public CustomInputField(TMP_InputField input, string placeholderLocalizationKey, OnChange onChange)
        {
            Initialize(input, null, null, placeholderLocalizationKey, onChange);
        }

        public CustomInputField(TMP_InputField input, TMP_Text placeholderText, string placeholderLocalizationKey, OnChange onChange)
        {
            Initialize(input, null, placeholderText, placeholderLocalizationKey, onChange);
        }

        public CustomInputField(TMP_InputField input, string placeholderGameObjectName, string placeholderLocalizationKey, OnChange onChange)
        {
            Initialize(input, placeholderGameObjectName, null, placeholderLocalizationKey, onChange);
        }

        private void Initialize(TMP_InputField input, string placeholderGameObjectName, TMP_Text placeholderText, string placeholderTextLocalizationKey, OnChange onChange)
        {
            InputBase = input;
            PlaceholderText = null;
            placeholderLocalizationKey = placeholderTextLocalizationKey;

            if (placeholderText != null) {
                //Direct reference to placeholder was provided
                PlaceholderText = placeholderText;
            } else {
                //Try to find placeholder
                string placeholderName = string.IsNullOrEmpty(placeholderGameObjectName) ? DEFAULT_PLACEHOLDER_GAMEOBJECT_NAME : placeholderGameObjectName;
                for (int i = 0; i < input.gameObject.transform.childCount && PlaceholderText == null; i++) {
                    GameObject child = input.gameObject.transform.GetChild(i).gameObject;
                    if (child.name == placeholderName) {
                        PlaceholderText = child.GetComponent<TMP_Text>();
                    }
                }
            }

            if(PlaceholderText != null && !string.IsNullOrEmpty(placeholderTextLocalizationKey)) {
                //Localize placeholder
                PlaceholderText.text = Localization.Game.Get(placeholderTextLocalizationKey);
            }

            if(onChange != null) {
                //Add event listener
                TMP_InputField.OnChangeEvent onChangeEvent = new TMP_InputField.OnChangeEvent();
                onChangeEvent.AddListener(new UnityEngine.Events.UnityAction<string>(onChange));
                InputBase.onValueChanged = onChangeEvent;
            }
        }

        public string PlaceholderLocalizationKey
        {
            get {
                return placeholderLocalizationKey;
            }
            set {
                if(PlaceholderText != null && placeholderLocalizationKey != value) {
                    placeholderLocalizationKey = value;
                    if (string.IsNullOrEmpty(placeholderLocalizationKey)) {
                        PlaceholderText.text = string.Empty;
                    } else {
                        PlaceholderText.text = Localization.Game.Get(placeholderLocalizationKey);
                    }
                }
            }
        }

        public bool Interactable
        {
            get {
                return InputBase.interactable;
            }
            set {
                InputBase.interactable = value;
            }
        }

        public string Text
        {
            get {
                return InputBase.text;
            }
            set {
                InputBase.text = value;
            }
        }
    }
}
