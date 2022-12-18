using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class CustomToggleInput
    {
        protected readonly string DEFAULT_LABEL_NAME = "Label";

        public delegate void OnChange(bool value);

        public Toggle ToggleBase { get; protected set; }
        public OnChange ChangeCallback { get; protected set; }
        public GameObject LabelGameObject { get; protected set; }
        public Text LabelText { get; protected set; }
        public TMP_Text TMPLabelText { get; protected set; }

        protected LString label;

        public CustomToggleInput(Toggle toggle, OnChange changeCallback)
        {
            Initialize(toggle, DEFAULT_LABEL_NAME, null, changeCallback);
        }

        public CustomToggleInput(Toggle toggle, LString label, OnChange changeCallback)
        {
            Initialize(toggle, DEFAULT_LABEL_NAME, label, changeCallback);
        }

        public CustomToggleInput(Toggle toggle, string labelGameObjectName, LString label, OnChange changeCallback)
        {
            Initialize(toggle, labelGameObjectName, label, changeCallback);
        }

        private void Initialize(Toggle toggleBase, string labelGameObjectName, LString label, OnChange changeCallback)
        {
            ToggleBase = toggleBase;
            ChangeCallback = changeCallback;
            LabelGameObject = null;
            LabelText = null;
            TMPLabelText = null;

            Toggle.ToggleEvent toggleEvent = new Toggle.ToggleEvent();
            toggleEvent.AddListener(new UnityEngine.Events.UnityAction<bool>(HandleChange));
            toggleBase.onValueChanged = toggleEvent;

            if (!string.IsNullOrEmpty(labelGameObjectName)) {
                LabelGameObject = GameObjectHelper.Find(toggleBase.gameObject, labelGameObjectName);
                if(LabelGameObject == null) {
                    CustomLogger.Error("{UIElementError}", string.Format("Label GameObject not found: {0}/{1}", toggleBase.gameObject.name, labelGameObjectName));
                } else {
                    LabelText = LabelGameObject.GetComponent<Text>();
                    if(LabelText == null) {
                        TMPLabelText = LabelGameObject.GetComponent<TMP_Text>();
                        if(TMPLabelText == null) {
                            CustomLogger.Error("{UIElementError}", string.Format("Label GameObject {0}/{1} has no Text or TMP_Text component", toggleBase.gameObject.name, labelGameObjectName));
                        }
                    }
                }
            }
            Label = label;
        }

        public bool Value
        {
            get {
                return ToggleBase.isOn;
            }
            set {
                ToggleBase.isOn = value;
            }
        }

        public LString Label
        {
            get {
                return label;
            }
            set {
                label = value;
                if(LabelText != null) {
                    LabelText.text = label;
                }
                if (TMPLabelText != null) {
                    TMPLabelText.text = label;
                }
            }
        }

        protected void HandleChange(bool value)
        {
            if(ChangeCallback != null) {
                ChangeCallback(value);
            }
        }
    }
}
