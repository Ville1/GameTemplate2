using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Version of FloatingTextManager that creates texts that are in the UI instead of the game world
    /// </summary>
    public class UIFloatingTextManager : MonoBehaviour
    {
        public static UIFloatingTextManager Instance;

        public GameObject Prototype;

        public List<UIFloatingText> CurrentTexts { get; private set; }
        public List<UIFloatingText> TextsInQueue { get; private set; }
        public List<Guid> TextIdHistory { get; private set; }

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;
            CurrentTexts = new List<UIFloatingText>();
            TextsInQueue = new List<UIFloatingText>();
            TextIdHistory = new List<Guid>();

            Prototype.SetActive(false);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            //Process queue
            List<UIFloatingText> newTexts = new List<UIFloatingText>();
            foreach (UIFloatingText floatingText in TextsInQueue) {
                if (floatingText.TryStart()) {
                    newTexts.Add(floatingText);
                    CurrentTexts.Add(floatingText);
                    TextIdHistory.Add(floatingText.Id);
                }
            }
            TextsInQueue = TextsInQueue.Where(queuedText => !newTexts.Any(newText => newText.Id == queuedText.Id)).ToList();

            //Process active texts
            foreach (UIFloatingText floatingText in CurrentTexts) {
                floatingText.Update();
            }

            //Remove destroyed texts from list
            CurrentTexts = CurrentTexts.Where(text => !text.IsDestroyed).ToList();
        }

        public void Show(UIFloatingText text)
        {
            if (CurrentTexts.Any(t => t.Id == text.Id)) {
                //This text is already being displayed
                throw new Exception(string.Format("UIFloatingText \"{0}\" is already being displayed", text.Id));
            }
            text.Start();
            if (text.IsInQueue) {
                TextsInQueue.Add(text);
            } else {
                CurrentTexts.Add(text);
                TextIdHistory.Add(text.Id);
            }
        }

        public void RemoveAll()
        {
            TextsInQueue.Clear();
            foreach (UIFloatingText floatingText in CurrentTexts) {
                floatingText.DestroyGameObject();
            }
            CurrentTexts.Clear();
        }
    }

    public class UIFloatingText
    {
        private static readonly int DEFAULT_FONT_SIZE = 24;
        private static readonly float DEFAULT_PADDING = 10.0f;
        private static readonly float OVERLAP_MARGIN = 1.0f;

        public Guid Id { get; private set; }
        public LString Text { get; private set; }
        public GameObject GameWorldTarget { get; private set; }
        /// <summary>
        /// Seconds
        /// </summary>
        public float Time { get; set; } = 5.0f;
        public Vector2 Movement { get; set; } = new Vector3(0.0f, 50.0f);
        public Color? BackgroundColor { get; set; } = null;
        public int FontSize { get; set; } = DEFAULT_FONT_SIZE;
        public Vector2 Padding { get; set; } = new Vector2(DEFAULT_PADDING, DEFAULT_PADDING);
        public bool CanOverlap { get; set; } = false;
        /// <summary>
        /// Id of a UIFloatingText that needs to be displayed before this one
        /// </summary>
        public Guid? LinkedTextId { get; set; } = null;

        public float TimeLeft { get; private set; }
        public bool IsInQueue { get; private set; }
        public bool IsDestroyed { get; private set; } = false;
        public RectTransform RectTransform { get { return gameObject == null ? null : gameObject.GetComponent<RectTransform>(); } }

        private GameObject gameObject;

        public UIFloatingText(GameObject gameWorldTarget, LString text)
        {
            Text = text;
            GameWorldTarget = gameWorldTarget;
            Id = Guid.NewGuid();
        }

        public void Start()
        {
            //Set time left
            TimeLeft = Time;

            //Initialize GameObject
            gameObject = GameObject.Instantiate(
                UIFloatingTextManager.Instance.Prototype,
                CameraManager.Instance.CurrentCamera.WorldToScreenPoint(GameWorldTarget.transform.position),
                Quaternion.identity,
                UIFloatingTextManager.Instance.gameObject.transform
            );
            gameObject.SetActive(true);
            gameObject.name = string.Format("UI Floating Text {0}", Id);

            if (BackgroundColor.HasValue) {
                //Set background color
                gameObject.GetComponentInChildren<Image>().color = BackgroundColor.Value;
            }

            //Set text
            TMP_Text tmp_Text = gameObject.GetComponentInChildren<TMP_Text>();
            tmp_Text.text = Text;
            tmp_Text.fontSize = FontSize;

            //Resize
            RectTransform rectTransform = RectTransform;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tmp_Text.preferredWidth + Padding.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tmp_Text.preferredHeight + Padding.y);

            if (LinkedTextId.HasValue) {
                //Check that a text with this id has already been displayed
                if (!FloatingTextManager.Instance.TextIdHistory.Contains(LinkedTextId.Value)) {
                    //We need to wait for text with this id to be displayed first
                    IsInQueue = true;
                    gameObject.SetActive(false);
                }
            }

            if (!CanOverlap && !IsInQueue) {
                //Check if this text overlaps with any texts that are currently visible
                if (IsOverlapping()) {
                    IsInQueue = true;
                    gameObject.SetActive(false);
                }
            }
        }

        public bool TryStart()
        {
            if (!IsOverlapping() && (!LinkedTextId.HasValue || FloatingTextManager.Instance.TextIdHistory.Contains(LinkedTextId.Value))) {
                IsInQueue = false;
                gameObject.SetActive(true);
                return true;
            }
            return false;
        }

        public void Update()
        {
            if (IsInQueue) {
                //Update should not get called when text is in queue, as it's GameObject would not be active, but just in case this gets called return here
                //Could also put an exception here?
                return;
            }
            if (TimeLeft > 0.0f) {
                float deltaTime = UnityEngine.Time.deltaTime;

                //Update TimeLeft
                TimeLeft = Mathf.Max(0.0f, TimeLeft - deltaTime);

                //Update position
                Vector3 oldPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
                gameObject.transform.position = new Vector3(
                    oldPosition.x + Movement.x * deltaTime,
                    oldPosition.y + Movement.y * deltaTime,
                    oldPosition.z
                );
            } else {
                //Destroy
                DestroyGameObject();
            }
        }

        public void DestroyGameObject()
        {
            GameObject.Destroy(gameObject);
            IsDestroyed = true;
        }

        /// <summary>
        /// Check if this text overlaps with any currently visile texts
        /// </summary>
        /// <returns></returns>
        private bool IsOverlapping()
        {
            RectTransform rectTransform = RectTransform;
            Rect thisRect = new Rect(
                rectTransform.localPosition.x - OVERLAP_MARGIN,
                rectTransform.localPosition.y - OVERLAP_MARGIN,
                rectTransform.rect.width + OVERLAP_MARGIN * 2.0f,
                rectTransform.rect.height + OVERLAP_MARGIN * 2.0f
            );
            foreach (UIFloatingText floatingText in UIFloatingTextManager.Instance.CurrentTexts) {
                Rect otherRect = new Rect(
                    floatingText.RectTransform.localPosition.x,
                    floatingText.RectTransform.localPosition.y,
                    floatingText.RectTransform.rect.width,
                    floatingText.RectTransform.rect.height
                );
                if (otherRect.Overlaps(thisRect)) {
                    return true;
                }
            }
            return false;
        }
    }
}
