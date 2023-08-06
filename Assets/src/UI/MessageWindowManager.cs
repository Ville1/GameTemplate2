using Game.Input;
using Game.Utils;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class MessageWindowManager : WindowBase
    {
        private static readonly float DEFAULT_TIME = 10.0f;//Seconds
        private static readonly int GRACE_FRAMES = 10;

        public static MessageWindowManager Instance;

        public TMP_Text Text;

        private float defaultWidth;
        private float defaultHeight;
        private float timeLeft;
        private int graceFramesLeft;

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
            BlockKeyboardInputs = false;
            BlockMouseEvents = false;

            defaultWidth = Width;
            defaultHeight = Height;

            MouseManager.Instance.AddEventListener(new MouseEvent((GameObject target) => { Close(); }, 0, MouseEventTag.IgnoreUI, false));
            MouseManager.Instance.AddEventListener(new MouseNothingClickEvent(() => { Close(); }, 0, MouseEventTag.IgnoreUI, false));
            KeyboardManager.Instance.AddOnKeyDownEventListener(() => { Close(); }, KeyEventTag.IgnoreUI);

            Active = false;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (Active) {
                if(graceFramesLeft > 0) {
                    graceFramesLeft--;
                }
                timeLeft -= Time.deltaTime;
                if(timeLeft <= 0.0f) {
                    Active = false;
                }
            }
        }

        public void ShowMessage(LString text, float? time = null, float? width = null, float? height = null)
        {
            timeLeft = time ?? DEFAULT_TIME;
            graceFramesLeft = GRACE_FRAMES;
            Active = true;
            Text.text = text;
            Width = width ?? defaultWidth;
            Height = height ?? Text.preferredHeight;
        }

        public void Close()
        {
            if (Active && graceFramesLeft == 0) {
                Active = false;
            }
        }
    }
}
