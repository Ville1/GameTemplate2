using Game.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class CustomButton
    {
        public delegate void OnClick();

        public Button ButtonBase { get; private set; }
        public TMPro.TMP_Text TmpText { get; private set; }

        public CustomButton(Button prototype, GameObject parent, Vector2 positionDelta, string textKey, OnClick onClick)
        {
            //Instantiate a new button based on prototype
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

            //Set localized text
            TmpText = ButtonBase.GetComponentInChildren<TMPro.TMP_Text>();
            if(TmpText == null) {
                CustomLogger.Error("UIElementError", "Text object not found");
            } else {
                TmpText.text = Localization.Game.Get(textKey);
            }

            //Set event listener
            Button.ButtonClickedEvent buttonClickedEvent = new Button.ButtonClickedEvent();
            buttonClickedEvent.AddListener(new UnityEngine.Events.UnityAction(onClick));
            ButtonBase.onClick = buttonClickedEvent;
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

        public string ToString()
        {
            return ButtonBase.name;
        }
    }
}
