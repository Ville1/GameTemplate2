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
        private static readonly string DEFAULT_ACCEPT_BUTTON_NAME = "Ok Button";
        private static readonly string DEFAULT_CANCEL_BUTTON_NAME = "Cancel Button";

        public enum Tag
        {
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
        protected string AutoAssignAcceptButtonName { get; set; } = null;
        protected string AutoAssignCancelButtonName { get; set; } = null;
        protected bool AcceptEnabled { get { return acceptEnabled; } set { acceptEnabled = value; if (autoAssignedAcceptButton != null) { autoAssignedAcceptButton.Interactable = value; } } }

        private bool baseIsInitialized = false;
        private bool acceptEnabled = false;
        protected CustomButton autoAssignedCloseButton = null;
        protected CustomButton autoAssignedAcceptButton = null;
        protected CustomButton autoAssignedCancelButton = null;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected virtual void Start()
        {
            UIManager.Instance.RegisterWindow(this);
            Panel.SetActive(false);

            autoAssignedCloseButton = InitializeAutoAssingButton(AutoAssignCloseButtonName, null, () => { HandleWindowEvent(WindowEvent.Close); });
            autoAssignedAcceptButton = InitializeAutoAssingButton(AutoAssignAcceptButtonName, "{Ok}", () => { HandleWindowEvent(WindowEvent.Accept); });
            autoAssignedCancelButton = InitializeAutoAssingButton(AutoAssignCancelButtonName, "{Cancel}", () => { HandleWindowEvent(WindowEvent.Cancel); });

            baseIsInitialized = true;
        }

        private CustomButton InitializeAutoAssingButton(string name, LString text, CustomButton.OnClick onClick)
        {
            if (!string.IsNullOrEmpty(name)) {
                GameObject buttonGameObject = GameObjectHelper.Find(Panel.transform, name);
                if (buttonGameObject == null) {
                    CustomLogger.Warning("{GameObjectNotFound}", name, Panel.name);
                } else {
                    Button button = buttonGameObject.GetComponent<Button>();
                    if (button == null) {
                        CustomLogger.Warning("{ComponentNotFound}", name, "Button");
                    } else {
                        return new CustomButton(button, text, onClick);
                    }
                }
            }
            return null;
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
                if (Panel.activeSelf == value) {
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
                if (image != null) {
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

        protected bool AutoAssignAcceptButton
        {
            get {
                return !string.IsNullOrEmpty(AutoAssignAcceptButtonName);
            }
            set {
                if (baseIsInitialized) {
                    throw new Exception("AutoAssignAcceptButton should be called before base.Start();");
                }
                if (value) {
                    AutoAssignAcceptButtonName = DEFAULT_ACCEPT_BUTTON_NAME;
                } else {
                    AutoAssignAcceptButtonName = null;
                }
            }
        }

        protected bool AutoAssignCancelButton
        {
            get {
                return !string.IsNullOrEmpty(AutoAssignCancelButtonName);
            }
            set {
                if (baseIsInitialized) {
                    throw new Exception("AutoAssignCancelButton should be called before base.Start();");
                }
                if (value) {
                    AutoAssignCancelButtonName = DEFAULT_CANCEL_BUTTON_NAME;
                } else {
                    AutoAssignCancelButtonName = null;
                }
            }
        }

        /// <summary>
        /// Return true, if window has "consumed" this event. (Prevents other windows for also having it fire)
        /// </summary>
        public virtual bool HandleWindowEvent(WindowEvent windowEvent)
        {
            if ((windowEvent == WindowEvent.Close || windowEvent == WindowEvent.Accept || windowEvent == WindowEvent.Cancel) && !Tags.Contains(Tag.ProgressBar)) {
                switch (windowEvent) {
                    case WindowEvent.Close:
                        OnClose();
                        Panel.SetActive(false);
                        return true;
                    case WindowEvent.Cancel:
                        OnCancel();
                        Panel.SetActive(false);
                        return true;
                    case WindowEvent.Accept:
                        if (!AcceptEnabled) {
                            return false;
                        }
                        OnAccept();
                        Panel.SetActive(false);
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called then window opens, can be used to refresh ui
        /// </summary>
        public virtual void UpdateUI() { }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnAccept() { }
        protected virtual void OnCancel() { }
    }
}