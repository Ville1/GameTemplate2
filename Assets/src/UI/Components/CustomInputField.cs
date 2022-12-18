using TMPro;
using UnityEngine;

namespace Game.UI.Components
{
    public class CustomInputField
    {
        protected static readonly string DEFAULT_PLACEHOLDER_GAMEOBJECT_NAME = "Placeholder";

        public delegate void OnChange(string val);

        public TMP_InputField InputBase { get; protected set; }
        public TMP_Text Placeholder { get; protected set; }
        public int MaxLenght { get; protected set; }
        public bool IsDisabled { get; protected set; }
        public OnChange ChangeCallback { get; protected set; }

        protected LString placeholderText;
        protected bool skipCallbacks = false;

        public CustomInputField(TMP_InputField input, OnChange onChange, int maxLenght = int.MaxValue, bool isDisabled = false)
        {
            Initialize(input, null, null, null, onChange, maxLenght, isDisabled);
        }

        public CustomInputField(TMP_InputField input, LString placeholderText, OnChange onChange, int maxLenght = int.MaxValue, bool isDisabled = false)
        {
            Initialize(input, null, null, placeholderText, onChange, maxLenght, isDisabled);
        }

        public CustomInputField(TMP_InputField input, TMP_Text placeholder, LString placeholderText, OnChange onChange, int maxLenght = int.MaxValue, bool isDisabled = false)
        {
            Initialize(input, null, placeholder, placeholderText, onChange, maxLenght, isDisabled);
        }

        public CustomInputField(TMP_InputField input, string placeholderGameObjectName, LString placeholderText, OnChange onChange, int maxLenght = int.MaxValue, bool isDisabled = false)
        {
            Initialize(input, placeholderGameObjectName, null, placeholderText, onChange, maxLenght, isDisabled);
        }

        protected void Initialize(TMP_InputField input, string placeholderGameObjectName, TMP_Text placeholder, LString placeholderText, OnChange onChange, int maxLenght, bool isDisabled)
        {
            InputBase = input;
            Placeholder = null;
            this.placeholderText = placeholderText;
            MaxLenght = maxLenght;
            ChangeCallback = onChange;

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
                    for (int j = 0; j < child.transform.childCount && Placeholder == null; j++) {
                        GameObject child2 = child.transform.GetChild(j).gameObject;
                        if (child2.name == placeholderName) {
                            Placeholder = child2.GetComponent<TMP_Text>();
                        }
                    }
                }
            }

            if(Placeholder != null) {
                //Update placeholder text
                Placeholder.text = placeholderText;
            }

            //Add event listener
            TMP_InputField.OnChangeEvent onChangeEvent = new TMP_InputField.OnChangeEvent();
            onChangeEvent.AddListener(new UnityEngine.Events.UnityAction<string>(HandleChange));
            InputBase.onValueChanged = onChangeEvent;

            InputBase.interactable = !isDisabled;
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

        /// <summary>
        /// Set text and ignore callbacks and event listeners
        /// </summary>
        /// <param name="text"></param>
        protected void SetText(string text)
        {
            skipCallbacks = true;
            InputBase.text = text;
        }

        protected virtual void HandleChange(string value)
        {
            if(value != null && value.Length > MaxLenght) {
                value = value.Substring(0, MaxLenght);
                Text = value;
            }
            if (skipCallbacks) {
                skipCallbacks = false;
                return;
            }
            if(ChangeCallback != null) {
                ChangeCallback(value);
            }
        }
    }
}
