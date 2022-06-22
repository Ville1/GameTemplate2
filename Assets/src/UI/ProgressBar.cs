using Game.Utils;
using System;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ProgressBar : WindowBase
    {
        public static ProgressBar Instance;

        public TMP_Text DescriptionText;
        public GameObject BarPanel;

        private string description = string.Empty;
        private float progress = 0.0f;

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
            Tags.Add(Tag.ProgressBar);
            BlockKeyboardInputs = true;
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        public override bool Active
        {
            get {
                return Panel.activeSelf;
            }
            set {
                if (value) {
                    UIManager.Instance.CloseAllWindows();
                }
                Panel.SetActive(value);
                Progress = 0.0f;
                Description = string.Empty;
            }
        }

        public void Show(string description)
        {
            UIManager.Instance.CloseAllWindows();
            Panel.SetActive(true);
            this.description = description;
            progress = 0.0f;
            UpdateText();
        }

        public string Description
        {
            get {
                return description;
            }
            set {
                description = value;
                UpdateText();
            }
        }

        public float Progress
        {
            get {
                return progress;
            }
            set {
                if(value < 0.0f || value > 1.0f) {
                    throw new ArgumentException("Value must be in range: 0.0 - 1.0");
                }
                progress = value;
                BarPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Panel.GetComponent<RectTransform>().rect.width * value);
                UpdateText();
            }
        }

        private void UpdateText()
        {
            DescriptionText.text = string.IsNullOrEmpty(description) ? progress.ToPercentage() : string.Format("{0} {1}", description, progress.ToPercentage());
        }
    }
}