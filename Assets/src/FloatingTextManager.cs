using Game.Objects;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Game
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance;

        private List<FloatingText> currentTexts;

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
            currentTexts = new List<FloatingText>();
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {

        }

        public void Show(FloatingText text)
        {
            if (currentTexts.Any(t => t.Id == text.Id)) {
                //This text is already being displayed
                throw new Exception(string.Format("FloatingText \"{0}\" is already being displayed", text.Id));
            }
            text.Start(OnTextDestroyed);
            currentTexts.Add(text);
        }

        private void OnTextDestroyed(Guid id)
        {
            //Remove text from currentTexts
            currentTexts = currentTexts.Where(text => text.Id != id).ToList();
        }
    }

    public class FloatingText : Object2D
    {
        private static readonly string PREFAB_NAME = "Floating Text";
        private static readonly string SPRITE_NAME = "floating text background";
        private static readonly int SPRITE_SORTING_ORDER = 100;
        private static readonly int DEFAULT_FONT_SIZE = 2;
        private static readonly float DEFAULT_PADDING = 0.1f;

        public Guid Id { get; private set; }
        public LString Text { get; private set; }
        public GameObject GameWorldTarget { get; private set; }
        /// <summary>
        /// Seconds
        /// </summary>
        public float Time { get; set; } = 5.0f;
        public Vector3 Movement { get; set; } = new Vector3(0.0f, 1.0f, 0.0f);
        public Color? BackgroundColor { get; set; } = null;
        public int FontSize { get; set; } = DEFAULT_FONT_SIZE;
        public Vector2 Padding { get; set; } = new Vector2(DEFAULT_PADDING, DEFAULT_PADDING);

        public float TimeLeft { get; private set; }

        private Action<Guid> onDestroy;

        public FloatingText(GameObject gameWorldTarget, LString text) : base(PREFAB_NAME, "Floating Text", new SpriteData(SPRITE_NAME, TextureDirectory.UI, SPRITE_SORTING_ORDER), null, 1.0f)
        {
            Text = text;
            GameWorldTarget = gameWorldTarget;
            Id = Guid.NewGuid();
        }

        public void Start(Action<Guid> onDestroy)
        {
            //Set time left
            TimeLeft = Time;

            //Set callback
            this.onDestroy = onDestroy;

            //Initialize GameObject
            InitializeObject(true, GameWorldTarget.transform.position, FloatingTextManager.Instance.gameObject.transform);
            GameObject.name = string.Format("Floating Text {0}", Id);

            if (BackgroundColor.HasValue) {
                //Set background color
                Renderer.color = BackgroundColor.Value;
            }

            //Set text
            GameObject textGameObject = GameObjectHelper.Find(GameObject, "Text");
            TMP_Text text = textGameObject.GetComponent<TMP_Text>();
            RectTransform textRectTransform = textGameObject.GetComponent<RectTransform>();
            textGameObject.GetComponent<MeshRenderer>().sortingOrder = SPRITE_SORTING_ORDER + 1;
            text.text = Text;
            text.fontSize = FontSize;

            //Resize
            Vector2 newSize = new Vector2(text.preferredWidth + Padding.x, text.preferredHeight + Padding.y);
            Width = newSize.x;
            Height = newSize.y;
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
            Renderer.drawMode = SpriteDrawMode.Sliced;
            Renderer.size = new Vector2(Width, Height);
        }

        public override void Update()
        {
            base.Update();
            if (TimeLeft > 0.0f) {
                float deltaTime = UnityEngine.Time.deltaTime;

                //Update TimeLeft
                TimeLeft = Mathf.Max(0.0f, TimeLeft - deltaTime);

                //Update position
                Position = new Vector3(
                    Position.x + Movement.x * deltaTime,
                    Position.y + Movement.y * deltaTime,
                    Position.z + Movement.z * deltaTime
                );
            } else {
                //Destroy
                DestroyGameObject();
                onDestroy(Id);
            }
        }
    }
}
