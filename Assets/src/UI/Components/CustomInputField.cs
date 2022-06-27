using TMPro;
using UnityEngine;

namespace Game.UI.Components
{
    public class CustomInputField
    {
        private static readonly string DEFAULT_PLACEHOLDER_GAMEOBJECT_NAME = "Placeholder";

        public delegate void OnChange(string val);

        public TMP_InputField InputBase { get; private set; }
        public TMP_Text Placeholder { get; private set; }

        private LString placeholderText;

        public CustomInputField(TMP_InputField input, OnChange onChange)
        {
            Initialize(input, null, null, null, onChange);
        }

        public CustomInputField(TMP_InputField input, LString placeholderText, OnChange onChange)
        {
            Initialize(input, null, null, placeholderText, onChange);
        }

        public CustomInputField(TMP_InputField input, TMP_Text placeholder, LString placeholderText, OnChange onChange)
        {
            Initialize(input, null, placeholder, placeholderText, onChange);
        }

        public CustomInputField(TMP_InputField input, string placeholderGameObjectName, LString placeholderText, OnChange onChange)
        {
            Initialize(input, placeholderGameObjectName, null, placeholderText, onChange);
        }

        private void Initialize(TMP_InputField input, string placeholderGameObjectName, TMP_Text placeholder, LString placeholderText, OnChange onChange)
        {
            InputBase = input;
            Placeholder = null;
            this.placeholderText = placeholderText;

            if (placeholder != null) {
                //Direct reference to placeholder was provided
                Placeholder = placeholder;
            } else {
                //Try to find placeholder
                string placeholderName = string.IsNullOrEmpty(placeholderGameObjectName) ? DEFAULT_PLACEHOLDER_GAMEOBJECT_NAME : placeholderGameObjectName;
                for (int i = 0; i < input.gameObject.transform.childCount && Placeholder == null; i++) {
                    GameObject child = input.gameObject.transform.GetChild(i).gameObject;
                    if (child.name == placeholderName) {
                        Placeholder = child.GetComponent<TMP_Text>();
                    }
                }
            }

            if(Placeholder != null) {
                //Update placeholder text
                if (string.IsNullOrEmpty(placeholderText)) {
                    this.placeholderText = Placeholder.text;
                } else {
                    Placeholder.text = placeholderText;
                }
            }

            if(onChange != null) {
                //Add event listener
                TMP_InputField.OnChangeEvent onChangeEvent = new TMP_InputField.OnChangeEvent();
                onChangeEvent.AddListener(new UnityEngine.Events.UnityAction<string>(onChange));
                InputBase.onValueChanged = onChangeEvent;
            }
        }

        public LString PlaceholderText
        {
            get {
                return placeholderText;
            }
            set {
                if(Placeholder != null) {
                    placeholderText = value;
                    if (string.IsNullOrEmpty(placeholderText)) {
                        Placeholder.text = string.Empty;
                    } else {
                        Placeholder.text = placeholderText;
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
