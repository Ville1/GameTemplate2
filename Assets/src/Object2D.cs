using Game.Input;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// TODO: allow spriteless objects
    /// </summary>
    public class Object2D : IClickListener, IHasSprite
    {
        public delegate void EventListenerDelegate();

        public GameObject GameObject { get { return gameObject; } }
        public RectTransform RectTransform { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        public BoxCollider Collider { get; private set; }
        public bool IsPrototype { get { return gameObject == null; } }
        public MouseEventData MouseEventData { get { return clickListenerData; } }
        public bool IsClickable { get { return clickListenerData != null; } }
        public float MovementSpeed { get; set; }
        public bool CanMove { get { return MovementSpeed != 0.0f; } }
        public List<EventListenerDelegate> OnMovementStart { get; set; } = new List<EventListenerDelegate>();
        public List<EventListenerDelegate> OnMovement { get; set; } = new List<EventListenerDelegate>();
        public List<EventListenerDelegate> OnMovementEnd { get; set; } = new List<EventListenerDelegate>();

        protected bool IsMoving { get { return movementTarget.HasValue; } }

        protected Object2DListener updateListener = null;
        private GameObject gameObject = null;
        private string prefabName = null;
        private string name = null;
        private SpriteData spriteData = null;
        private bool isDestroyed = false;
        private bool spriteDirectoryChanged = false;
        protected MouseEventData clickListenerData;
        protected Vector3? oldPosition = null;
        protected Vector3? movementTarget = null;
        protected float movementDistanceTotal = -1.0f;
        protected float movementDistanceCurrent = -1.0f;

        /// <summary>
        /// GameObject constructor (prototype)
        /// </summary>
        public Object2D(Object2D prototype, string objectName, bool active, Vector3 position, Transform parent)
        {
            Initialize(prototype.prefabName, objectName, active, position, parent, prototype.spriteData, prototype.clickListenerData, prototype.MovementSpeed);
            OnMovementStart = prototype.OnMovementStart.Copy();
            OnMovement = prototype.OnMovement.Copy();
            OnMovementEnd = prototype.OnMovementEnd.Copy();
        }

        /// <summary>
        /// GameObject constructor
        /// </summary>
        public Object2D(string objectName, bool active, Vector3 position, Transform parent, SpriteData spriteData, MouseEventData clickListenerData = null, float movementSpeed = 0.0f)
        {
            Initialize(null, objectName, active, position, parent, spriteData, clickListenerData, movementSpeed);
        }

        /// <summary>
        /// GameObject constructor (prefab)
        /// </summary>
        public Object2D(string prefabName, string objectName, bool active, Vector3 position, Transform parent, SpriteData spriteData, MouseEventData clickListenerData = null,
            float movementSpeed = 0.0f)
        {
            Initialize(prefabName, objectName, active, position, parent, spriteData, clickListenerData, movementSpeed);
        }

        /// <summary>
        /// Prototype constructor
        /// </summary>
        public Object2D(string prefabName, string name, SpriteData spriteData, MouseEventData clickListenerData = null, float movementSpeed = 0.0f)
        {
            this.prefabName = prefabName;
            this.name = name;
            this.spriteData = spriteData.Copy;
            this.clickListenerData = clickListenerData;
            MovementSpeed = movementSpeed;
        }

        public virtual bool Active
        {
            get {
                return gameObject == null ? false : gameObject.activeSelf;
            }
            set {
                if (IsPrototype) {
                    CustomLogger.Error("{ObjectIsPrototype}", string.IsNullOrEmpty(name) ? "Unnamed" : name);
                } else {
                    gameObject.SetActive(value);
                }
            }
        }

        public float Width
        {
            get {
                return RectTransform == null ? -1.0f : RectTransform.rect.width;
            }
            set {
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
            }
        }

        public float Height
        {
            get {
                return RectTransform == null ? -1.0f : RectTransform.rect.height;
            }
            set {
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
            }
        }

        public string Sprite
        {
            get {
                return spriteData.Sprite;
            }
            set {
                spriteData.Sprite = value;
                if(Renderer != null) {
                    UpdateSprite();
                }
            }
        }

        public TextureDirectory SpriteDirectory
        {
            get {
                return spriteData.SpriteDirectory;
            }
            set {
                if(spriteData.SpriteDirectory != value) {
                    spriteData.SpriteDirectory = value;
                    spriteDirectoryChanged = true;
                    if (Renderer != null) {
                        UpdateSprite();
                    }
                }
            }
        }

        public int RenderingOrder
        {
            get {
                return spriteData.Order;
            }
            set {
                spriteData.Order = value;
                if (Renderer != null) {
                    Renderer.sortingOrder = value;
                }
            }
        }

        public Vector3 Position
        {
            get {
                return new Vector3(GameObject.transform.position.x, GameObject.transform.position.y, GameObject.transform.position.z);
            }
            set {
                GameObject.transform.position = new Vector3(value.x, value.y, value.z);
            }
        }

        public virtual void OnClick(MouseButton button)
        { }

        public virtual void Update() {
            if (IsMoving) {
                if (CanMove) {
                    movementDistanceCurrent = Mathf.Clamp(movementDistanceCurrent + Time.deltaTime * MovementSpeed, 0.0f, movementDistanceTotal);
                    float progress = movementDistanceCurrent / movementDistanceTotal;
                    Position = Vector3.Lerp(oldPosition.Value, movementTarget.Value, progress);
                    foreach (EventListenerDelegate eventListener in OnMovement) {
                        eventListener();
                    }
                    if (progress == 1.0f) {
                        EndMovement();
                    }
                } else {
                    //This object can no longer move
                    EndMovement();
                }
            }
        }

        public void Destroy()
        {
            if (isDestroyed) {
                CustomLogger.Error("{ObjectIsDestroyed}", name);
            } else {
                GameObject.Destroy(gameObject);
                isDestroyed = true;
            }
        }

        public override string ToString()
        {
            return gameObject.name;
        }

        protected bool StartMoving(Vector3 target)
        {
            if (IsMoving || !CanMove) {
                //Already moving or can't move
                return false;
            }
            if(Position.x == target.x && Position.y == target.y && Position.z == target.z) {
                //Same position
                return false;
            }
            movementTarget = target;
            oldPosition = Position;
            movementDistanceTotal = Vector3.Distance(oldPosition.Value, movementTarget.Value);
            movementDistanceCurrent = 0.0f;
            foreach(EventListenerDelegate eventListener in OnMovementStart) {
                eventListener();
            }
            return true;
        }

        protected bool EndMovement()
        {
            bool wasMoving = IsMoving;
            movementTarget = null;
            oldPosition = null;
            movementDistanceTotal = -1.0f;
            movementDistanceCurrent = -1.0f;
            if (wasMoving) {
                foreach (EventListenerDelegate eventListener in OnMovementEnd) {
                    eventListener();
                }
            }
            return wasMoving;
        }

        private void Initialize(string prefabName, string objectName, bool active, Vector3 position, Transform parent, SpriteData spriteData, MouseEventData clickListenerData,
            float movementSpeed)
        {
            this.prefabName = prefabName;
            name = objectName;
            this.spriteData = spriteData.Copy;
            this.clickListenerData = clickListenerData;
            MovementSpeed = movementSpeed;

            //Instantiate GameObject
            if (string.IsNullOrEmpty(prefabName)) {
                gameObject = new GameObject();
                gameObject.transform.position = position;
                gameObject.transform.parent = parent;
            } else {
                //Prefab
                gameObject = GameObject.Instantiate(
                    PrefabManager.Instance.Get(prefabName),
                    position,
                    Quaternion.identity,
                    parent
                );
            }
            gameObject.name = objectName;
            gameObject.SetActive(active);

            //Get UpdateListener, or add if missing
            updateListener = gameObject.GetComponent<Object2DListener>();
            if (updateListener == null) {
                updateListener = gameObject.AddComponent<Object2DListener>();
            }
            updateListener.Object2D = this;

            //Get RectTransform, or add if missing
            RectTransform = gameObject.GetComponent<RectTransform>();
            if (RectTransform == null) {
                RectTransform = gameObject.AddComponent<RectTransform>();
            }

            //Get SpriteRenderer, or add if missing
            Renderer = gameObject.GetComponent<SpriteRenderer>();
            if (Renderer == null) {
                Renderer = gameObject.AddComponent<SpriteRenderer>();
            }
            if (string.IsNullOrEmpty(this.spriteData.Sprite)) {
                //Use default sprite from prefab
                if(Renderer.sprite == null) {
                    //Sprite name was not provided with spriteName - parameter and Renderer is missing or is lacking a sprite
                    CustomLogger.Error("{Object2DNoSprite}");
                    this.spriteData.Sprite = string.Empty;
                } else {
                    this.spriteData.Sprite = Renderer.sprite.name;
                }
            }

            UpdateSprite();

            if (IsClickable) {
                //Get BoxCollider, or add if missing
                Collider = gameObject.GetComponent<BoxCollider>();
                if (Collider == null) {
                    Collider = gameObject.AddComponent<BoxCollider>();
                }
            }

            if (string.IsNullOrEmpty(prefabName)) {
                //Not prefab, set size based on sprite
                Width = 100 / Renderer.sprite.pixelsPerUnit;
                Height = 100 / Renderer.sprite.pixelsPerUnit;
            }
        }

        private void UpdateSprite()
        {
            if((Renderer.sprite != null && Renderer.sprite.name == spriteData.Sprite) && Renderer.sortingOrder == spriteData.Order && !spriteDirectoryChanged) {
                //No change
                return;
            }
            Renderer.sprite = TextureManager.GetSprite(spriteData);
            Renderer.sortingOrder = spriteData.Order;
            spriteDirectoryChanged = false;
        }
    }
}