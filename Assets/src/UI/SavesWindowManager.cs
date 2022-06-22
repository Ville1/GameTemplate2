using Game.UI.Components;
using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Window for saving and loading
    /// </summary>
    public class SavesWindowManager : WindowBase
    {
        public enum WindowState { Uninitialized, Saving, Loading }

        public static SavesWindowManager Instance;

        public TMP_Text TitleText;
        public Button CloseButton;
        public GameObject ScrollViewContent;
        public GameObject ScrollViewRowPrototype;
        public Button ConfirmButton;
        public Button CancelButton;

        private WindowState windowState = WindowState.Uninitialized;
        private ScrollableList list = null;
        private CustomButton closeButton = null;
        private CustomButton confirmButton = null;
        private CustomButton cancelButton = null;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("AttemptingToCreateMultipleInstances");
                return;
            }
            Instance = this;
            Tags.Add(Tag.ClosesOthers);

            list = new ScrollableList(ScrollViewRowPrototype, ScrollViewContent);
            closeButton = new CustomButton(CloseButton, null, Close);
            confirmButton = new CustomButton(ConfirmButton, "Save", Confirm);
            cancelButton = new CustomButton(CancelButton, "Cancel", Close);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        public void OpenSavingWindow()
        {
            Active = true;
            State = WindowState.Saving;
        }

        public void OpenLoadingWindow()
        {
            Active = true;
            State = WindowState.Loading;
        }

        public WindowState State
        {
            get {
                return windowState;
            }
            set {
                if(windowState != value) {
                    windowState = value;
                    UpdateUI();
                }
            }
        }

        private void UpdateUI()
        {
            TitleText.text = Localization.Game.Get(State == WindowState.Saving ? "SaveGameTitle" : "LoadGameTitle");
        }

        private void Close()
        {
            Active = false;
        }

        private void Confirm()
        {

        }
    }
}
