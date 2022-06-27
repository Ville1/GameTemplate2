using Game.UI.Components;
using Game.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Template for ui panels
    /// </summary>
    public class ConfirmationDialogManager : WindowBase
    {
        public delegate void DialogAction();

        public static ConfirmationDialogManager Instance;

        public TMP_Text MessageText;
        public GameObject ButtonContainer;
        public Button AcceptButton;
        public Button DeclineButton;
        public Button CancelButton;

        private CustomButton acceptButton;
        private CustomButton declineButton;
        private CustomButton cancelButton;

        private bool showCancel;
        private DialogAction acceptCallback;
        private DialogAction declineCallback;
        private DialogAction cancelCallback;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;
            acceptButton = new CustomButton(AcceptButton, null, HandleAccept);
            declineButton = new CustomButton(DeclineButton, null, HandleDecline);
            cancelButton = new CustomButton(CancelButton, null, HandleCancel);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        public override bool HandleWindowEvent(WindowEvent windowEvent)
        {
            switch (windowEvent) {
                case WindowEvent.Accept:
                    HandleAccept();
                    return true;
                case WindowEvent.Close:
                    if (showCancel) {
                        HandleCancel();
                    } else {
                        HandleDecline();
                    }
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Show dialog with 2 options
        /// </summary>
        public void ShowDialog(LString message, LString acceptText, LString declineText, DialogAction acceptCallback, DialogAction declineCallback)
        {
            ShowDialog(message, acceptText, declineText, null, acceptCallback, declineCallback, null);
        }

        /// <summary>
        /// Show dialog with 3 options
        /// TODO: Allow use of nonlocalized text (in here and ui in general)
        /// Idea: Create some kind of LocalizedString-class for localized strings?
        /// Note: MessageText.text can't use localization because of params[], ^could that help? LocalizedString.FormatParams string[]
        /// </summary>
        public void ShowDialog(LString message, LString acceptText, LString declineText, LString cancelText, DialogAction acceptCallback, DialogAction declineCallback, DialogAction cancelCallback)
        {
            showCancel = cancelCallback != null;

            //Set texts
            MessageText.text = message;
            acceptButton.Text = acceptText;
            declineButton.Text = declineText;
            cancelButton.Text = showCancel ? cancelText : string.Empty;

            //Hide / show cancel
            cancelButton.Active = showCancel;
            ButtonContainer.transform.localPosition = new Vector3(
                showCancel ? 0.0f : 50.0f,
                ButtonContainer.transform.localPosition.y,
                ButtonContainer.transform.localPosition.z
            );

            //Set callbacks
            this.acceptCallback = acceptCallback;
            this.declineCallback = declineCallback;
            this.cancelCallback = cancelCallback;

            //Set width
            Width = Math.Max(MessageText.preferredWidth + 10.0f, acceptButton.Width + (declineButton.Width + 1.0f) + (showCancel ? (cancelButton.Width + 1.0f) : 0.0f));

            //TODO: use preferredWidth for buttons?

            //Show dialog
            Active = true;
        }

        private void HandleAccept()
        {
            Active = false;
            acceptCallback();
        }

        private void HandleDecline()
        {
            Active = false;
            declineCallback();
        }

        private void HandleCancel()
        {
            Active = false;
            cancelCallback();
        }
    }
}