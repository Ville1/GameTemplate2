using Game.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class CustomButton
    {
        private static readonly float DEFAULT_PIXELS_PER_UNIT_MULTIPLIER = 3.0f;

        public delegate void OnClick();

        public Button ButtonBase { get; private set; }
        public TMPro.TMP_Text TmpText { get; private set; }

        private LString text;

        /// <summary>
        /// Instantiate a new button based on prototype
        /// </summary>
        public CustomButton(Button prototype, GameObject parent, Vector2 positionDelta, LString text, OnClick onClick, UISpriteData spriteData = null)
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
                string newName = string.Format("{0}{1}Button", text.IsLocalized ? text.Key : string.Empty, index == 0 ? string.Empty : index);
                if(GameObject.Find(string.Format("{0}/{1}", parent.name, newName)) == null) {
                    name = newName;
                }
                index++;
                if(index == 10000) {
                    //This should not happen, but lest be sure to avoid infinite loops
                    CustomLogger.Error("{FailedToGenerateGameObjectName}");
                    break;
                }
            }
            if(name != null) {
                ButtonBase.gameObject.name = name;
            }

            InitializeTextEventListenerAndSprite(text, onClick, spriteData);
        }

        /// <summary>
        /// Wrap a pre-existing button as a CustomButton
        /// </summary>
        public CustomButton(Button button, LString text, OnClick onClick, UISpriteData spriteData = null)
        {
            ButtonBase = button;
            InitializeTextEventListenerAndSprite(text, onClick, spriteData);
        }

        public LString Text
        {
            get {
                return text;
            }
            set {
                if(TmpText != null) {
                    text = value;
                    TmpText.text = text;
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

        private void InitializeTextEventListenerAndSprite(LString text, OnClick onClick, UISpriteData spriteData)
        {
            //Set text
            TmpText = ButtonBase.GetComponentInChildren<TMPro.TMP_Text>();
            this.text = text;
            if (TmpText != null) {
                if (string.IsNullOrEmpty(text)) {
                    this.text = TmpText.text;
                } else {
                    TmpText.text = text;
                }
            }

            //Set event listener
            Button.ButtonClickedEvent buttonClickedEvent = new Button.ButtonClickedEvent();
            buttonClickedEvent.AddListener(new UnityAction(onClick));
            ButtonBase.onClick = buttonClickedEvent;

            if(spriteData != null) {
                //Set sprite
                ButtonBase.image.sprite = TextureManager.GetSprite(spriteData);
                ButtonBase.image.pixelsPerUnitMultiplier = spriteData.PixelsPerUnitMultiplier.HasValue ? spriteData.PixelsPerUnitMultiplier.Value : DEFAULT_PIXELS_PER_UNIT_MULTIPLIER;
            }
        }
    }
}
