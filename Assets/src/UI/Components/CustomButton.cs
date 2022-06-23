using Game.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class CustomButton
    {
        public delegate void OnClick();

        public Button ButtonBase { get; private set; }
        public TMPro.TMP_Text TmpText { get; private set; }

        /// <summary>
        /// Instantiate a new button based on prototype
        /// </summary>
        public CustomButton(Button prototype, GameObject parent, Vector2 positionDelta, string textKey, OnClick onClick)
        {
            ButtonBase = GameObject.Instantiate(
                prototype,
                new Vector3(
                    prototype.transform.position.x + positionDelta.x,
                    prototype.transform.position.y + positionDelta.y,
                    prototype.transform.position.z
                ),
                Quaternion.identity,
                parent.transform
            );

            //Find a next unique name (ideally this loop should only run once)
            int index = 0;
            string name = null;
            while (name == null) {
                string newName = string.Format("{0}{1}Button", textKey, index == 0 ? string.Empty : index);
                if(GameObject.Find(string.Format("{0}/{1}", parent.name, newName)) == null) {
                    name = newName;
                }
                index++;
                if(index == 10000) {
                    //This should not happen, but lest be sure to avoid infinite loops
                    CustomLogger.Error("FailedToGenerateGameObjectName");
                    break;
                }
            }
            if(name != null) {
                ButtonBase.gameObject.name = name;
            }

            InitializeTextAndEventListener(textKey, onClick);
        }

        /// <summary>
        /// Wrap a pre-existing button as a CustomButton
        /// </summary>
        public CustomButton(Button button, string textKey, OnClick onClick)
        {
            ButtonBase = button;
            InitializeTextAndEventListener(textKey, onClick);
        }

        public string Text
        {
            get {
                return TmpText == null ? null : TmpText.text;
            }
            set {
                if(TmpText != null) {
                    TmpText.text = value;
                }
            }
        }

        public bool Active
        {
            get {
                return ButtonBase.gameObject.activeSelf;
            }
            set {
                ButtonBase.gameObject.SetActive(value);
            }
        }

        public RectTransform RectTransform
        {
            get {
                return ButtonBase.GetComponent<RectTransform>();
            }
        }

        public float Width
        {
            get {
                return RectTransform.rect.width;
            }
            set {
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
            }
        }

        public float Height
        {
            get {
                return RectTransform.rect.height;
            }
            set {
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
            }
        }

        public bool Interactable
        {
            get {
                return ButtonBase.interactable;
            }
            set {
                ButtonBase.interactable = value;
            }
        }

        public override string ToString()
        {
            return ButtonBase.name;
        }

        private void InitializeTextAndEventListener(string textKey, OnClick onClick)
        {
            //Set localized text
            TmpText = ButtonBase.GetComponentInChildren<TMPro.TMP_Text>();
            if (TmpText != null && !string.IsNullOrEmpty(textKey)) {
                TmpText.text = Localization.Game.Get(textKey);
            }

            //Set event listener
            Button.ButtonClickedEvent buttonClickedEvent = new Button.ButtonClickedEvent();
            buttonClickedEvent.AddListener(new UnityAction(onClick));
            ButtonBase.onClick = buttonClickedEvent;
        }
    }
}
