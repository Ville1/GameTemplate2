using Game.UI.Components;
using Game.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class WindowBase : MonoBehaviour
    {
        private static readonly Image.Type DEFAULT_BACKGROUND_IMAGE_TYPE = Image.Type.Tiled;
        private static readonly string DEFAULT_CLOSE_BUTTON_NAME = "Close Button";

        public enum Tag {
            /// <summary>
            /// Does not get closed with UIManager.CloseWindows if Main.Instance.State == State.MainMenu
            /// </summary>
            MainMenu,
            /// <summary>
            /// Does not get closed by UIManager.CloseWindows unless specificly targeted by with include-parameter or
            /// WindowEvent.Close in default event handling (in virtual base.HandleWindowEvent)
            /// </summary>
            ProgressBar,
            /// <summary>
            /// Does not get closed by UIManager.CloseWindows unless specificly targeted by with include-parameter
            /// </summary>
            StaysOpen,
            /// <summary>
            /// Closes other windows then opened (if base.Active gets called)
            /// </summary>
            ClosesOthers,
            /// <summary>
            /// Alternative to combining StaysOpen-tag with BlockKeyboardInputs = false and BlockMouseEvents = false
            /// </summary>
            HUD,
            Console
        }

        public enum WindowEventPriorityDefaults
        {
            VeryLow = -100000,
            Low = -10000,
            Normal = 0,
            Hight = 10000,
            VeryHight = 100000
        }

        public GameObject Panel;
        public List<Tag> Tags { get; private set; } = new List<Tag>();
        public List<KeyEventTag> AllowedKeyEvents { get; private set; } = new List<KeyEventTag>() { KeyEventTag.IgnoreUI };
        public List<MouseEventTag> AllowedMouseEvents { get; private set; } = new List<MouseEventTag> { MouseEventTag.IgnoreUI };
        /// <summary>
        /// Window's priority, when it comes to receiving WindowEvents. If multiple windows can be open at once, and stacked on top of one another,
        /// top most ones should have higher priority than ones in the bottom. Otherwise can be left as 0.
        /// </summary>
        public int WindowEventPriority { get; protected set; } = 0;
        /// <summary>
        /// If button with this name is found as a child of Panel, it gets assigned close action automatically during Start().
        /// </summary>
        protected string AutoAssignCloseButtonName { get; set; } = null;

        private bool baseIsInitialized = false;
        private CustomButton autoAssignedCloseButton = null;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected virtual void Start()
        {
            UIManager.Instance.RegisterWindow(this);
            Active = false;

            if (AutoAssignCloseButton) {
                GameObject buttonGameObject = GameObjectHelper.Find(Panel.transform, AutoAssignCloseButtonName);
                if(buttonGameObject == null) {
                    CustomLogger.Warning("{GameObjectNotFound}", AutoAssignCloseButtonName, Panel.name);
                } else {
                    Button button = buttonGameObject.GetComponent<Button>();
                    if (button == null) {
                        CustomLogger.Warning("{ComponentNotFound}", AutoAssignCloseButtonName, "Button");
                    } else {
                        autoAssignedCloseButton = new CustomButton(button, null, () => { Active = false; });
                    }
                }
            }

            baseIsInitialized = true;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected virtual void Update()
        { }

        public virtual bool Active
        {
            get {
                return Panel.activeSelf;
            }
            set {
                if(Panel.activeSelf == value) {
                    return;
                }
                if (value && Tags.Contains(Tag.ClosesOthers) && UIManager.Instance != null) {
                    UIManager.Instance.CloseAllWindows();
                }
                Panel.SetActive(value);
                if (value) {
                    OnOpen();
                    UpdateUI();
                } else {
                    OnClose();
                }
            }
        }

        public void ToggleActive()
        {
            Active = !Active;
        }

        public RectTransform RectTransform
        {
            get {
                return Panel.GetComponent<RectTransform>();
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

        public UISpriteData SpriteData
        {
            set {
                Image image = Panel.GetComponent<Image>();
                if(image != null) {
                    image.sprite = TextureManager.GetSprite(value);
                    if (value.PixelsPerUnitMultiplier.HasValue) {
                        image.pixelsPerUnitMultiplier = value.PixelsPerUnitMultiplier.Value;
                    }
                    image.type = value.ImageType.HasValue ? value.ImageType.Value : DEFAULT_BACKGROUND_IMAGE_TYPE;
                }
            }
        }

        public bool BlockKeyboardInputs
        {
            get {
                return AllowedKeyEvents != null;
            }
            set {
                if (value) {
                    AllowedKeyEvents = new List<KeyEventTag>() { KeyEventTag.IgnoreUI };
                } else {
                    AllowedKeyEvents = null;
                }
            }
        }

        public bool BlockMouseEvents
        {
            get {
                return AllowedMouseEvents != null;
            }
            set {
                if (value) {
                    AllowedMouseEvents = new List<MouseEventTag>() { MouseEventTag.IgnoreUI };
                } else {
                    AllowedMouseEvents = null;
                }
            }
        }

        protected bool AutoAssignCloseButton
        {
            get {
                return !string.IsNullOrEmpty(AutoAssignCloseButtonName);
            }
            set {
                if (baseIsInitialized) {
                    throw new Exception("AutoAssignCloseButton should be called before base.Start();");
                }
                if (value) {
                    AutoAssignCloseButtonName = DEFAULT_CLOSE_BUTTON_NAME;
                } else {
                    AutoAssignCloseButtonName = null;
                }
            }
        }

        /// <summary>
        /// Return true, if window has "consumed" this event. (Prevents other windows for also having it fire)
        /// This default nonoverriden functionality just closes window on WindowEvent.Close, thus closing the top most window
        /// </summary>
        public virtual bool HandleWindowEvent(WindowEvent windowEvent)
        {
            if(windowEvent == WindowEvent.Close && !Tags.Contains(Tag.ProgressBar)) {
                Active = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called then window opens, can be used to refresh ui
        /// </summary>
        public virtual void UpdateUI() { }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
    }
}