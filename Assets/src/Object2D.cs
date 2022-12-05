using Game.Input;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
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
        public bool IsMoving { get { return movementTarget.HasValue; } }
        public List<EventListenerDelegate> OnMovementStart { get; set; } = new List<EventListenerDelegate>();
        public List<EventListenerDelegate> OnMovement { get; set; } = new List<EventListenerDelegate>();
        public List<EventListenerDelegate> OnMovementEnd { get; set; } = new List<EventListenerDelegate>();
        public bool IsPlayingAnimation { get { return currentAnimation != null && currentAnimation.IsPlaying; } }
        public bool AnimationIsPaused { get { return currentAnimation != null && currentAnimation.IsPaused; } }
        public string CurrentAnimation { get { return IsPlayingAnimation ? currentAnimation.Name : null; } }

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
        protected bool movedThisFrame = false;
        protected bool movedLastFrame = false;
        protected List<SpriteAnimation> animations = new List<SpriteAnimation>();
        protected SpriteAnimation currentAnimation;
        protected bool hasMovementAnimation = false;
        protected bool lingeringMovementAnimation = false;
        protected List<QueuedAnimation> animationQueue = new List<QueuedAnimation>();

        /// <summary>
        /// GameObject constructor (prototype)
        /// </summary>
        public Object2D(Object2D prototype, string objectName, bool active, Vector3 position, Transform parent)
        {
            Initialize(prototype.prefabName, objectName, active, position, parent, prototype.spriteData, prototype.clickListenerData, prototype.MovementSpeed);
            OnMovementStart = prototype.OnMovementStart.Copy();
            OnMovement = prototype.OnMovement.Copy();
            OnMovementEnd = prototype.OnMovementEnd.Copy();
            animations = prototype.animations.Select(animation => new SpriteAnimation(animation)).ToList();
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

        public void AddAnimation(SpriteAnimation animation)
        {
            animations = animations.Where(a => a.Name != animation.Name).ToList();
            animations.Add(animation);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="queue">Determines what happens, if object is already playing an animation</param>
        /// <param name="callback">If provided, this gets called then animation ends</param>
        /// <param name="canStopMovementAnimation">If true, and queue = AnimationQueue.StopCurrent this can stop movement animation from playing</param>
        public void PlayAnimation(string name, AnimationQueue queue = AnimationQueue.StopCurrent, SpriteAnimation.AnimationDelegate callback = null, bool canStopMovementAnimation = true)
        {
            if (IsPrototype) {
                CustomLogger.Error("{ObjectIsPrototype}", name);
                return;
            }
            SpriteAnimation animation = animations.FirstOrDefault(animation => animation.Name == name);
            if (animation == null) {
                CustomLogger.Warning("{AnimationNotFound}", this.name, name);
                return;
            }
            if(currentAnimation != null) {
                switch (queue) {
                    case AnimationQueue.StopCurrent:
                        if(IsMoving && hasMovementAnimation && !canStopMovementAnimation) {
                            return;
                        }
                        currentAnimation.Stop();
                        animationQueue.Clear();
                        break;
                    case AnimationQueue.Skip:
                        return;
                    case AnimationQueue.QueueOne:
                        animationQueue.Clear();
                        animationQueue.Add(new QueuedAnimation() { Name = name, Callback = callback });
                        return;
                    case AnimationQueue.QueueUnlimited:
                        animationQueue.Add(new QueuedAnimation() { Name = name, Callback = callback });
                        return;
                }
            }
            hasMovementAnimation = false;
            currentAnimation = animation;
            currentAnimation.Start(spriteData, UpdateSprite, callback);
        }

        public void StopAnimation(bool clearQueue = true)
        {
            if (IsPlayingAnimation) {
                currentAnimation.Stop();
                currentAnimation = null;
                hasMovementAnimation = false;
                lingeringMovementAnimation = false;
                if (clearQueue) {
                    animationQueue.Clear();
                } else {
                    ProcessAnimationQueue();
                }
            }
        }

        public void PauseAnimation()
        {
            if (IsPlayingAnimation) {
                currentAnimation.IsPaused = true;
            }
        }

        public void ResumeAnimation()
        {
            if (IsPlayingAnimation) {
                currentAnimation.IsPaused = false;
            }
        }

        public float CurrentAnimationSpeed
        {
            get {
                return IsPlayingAnimation ? currentAnimation.SpeedMultiplier : 1.0f;
            }
            set {
                if (IsPlayingAnimation) {
                    currentAnimation.SpeedMultiplier = value;
                }
            }
        }

        public virtual void Update() {
            movedThisFrame = false;
            if (IsMoving) {
                if (CanMove) {
                    movementDistanceCurrent = Mathf.Clamp(movementDistanceCurrent + Time.deltaTime * MovementSpeed, 0.0f, movementDistanceTotal);
                    float progress = movementDistanceCurrent / movementDistanceTotal;
                    Position = Vector3.Lerp(oldPosition.Value, movementTarget.Value, progress);
                    movedThisFrame = true;
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
            } else if(lingeringMovementAnimation) {
                //Have movement animation linger for one frame for seamless animation
                StopAnimation(false);
            }

            if(currentAnimation != null && currentAnimation.IsPlaying) {
                if (currentAnimation.Update() && !currentAnimation.IsPlaying) {
                    //Animation has stopped
                    currentAnimation = null;
                    ProcessAnimationQueue();
                }
            }

            movedLastFrame = movedThisFrame;
        }

        public virtual void Destroy()
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
            return name;
        }

        protected void Move(Quaternion direction, bool rotateToFaceDirection = false)
        {
            if (rotateToFaceDirection) {
                RectTransform.rotation = direction;
                RectTransform.Translate(new Vector3(0.0f, MovementSpeed * Time.deltaTime, 0.0f));
            } else {
                Vector3 delta = new Vector3(0.0f, MovementSpeed * Time.deltaTime, 0.0f);
                delta = direction * delta;
                Position += delta;
            }
            movedThisFrame = true;
            foreach (EventListenerDelegate eventListener in OnMovement) {
                eventListener();
            }
        }

        protected bool StartMoving(Vector3 target, string animationName = null)
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
            lingeringMovementAnimation = false;

            if (!string.IsNullOrEmpty(animationName) && (!IsPlayingAnimation || currentAnimation.Name != animationName)) {
                PlayAnimation(animationName, AnimationQueue.StopCurrent);
                hasMovementAnimation = true;
            }

            foreach(EventListenerDelegate eventListener in OnMovementStart) {
                eventListener();
            }
            return true;
        }

        protected bool EndMovement(bool returnToStart = false)
        {
            bool wasMoving = IsMoving;
            movementTarget = null;
            movementDistanceTotal = -1.0f;
            movementDistanceCurrent = -1.0f;
            if (wasMoving) {
                lingeringMovementAnimation = hasMovementAnimation;
                if (returnToStart) {
                    Position = new Vector3(oldPosition.Value.x, oldPosition.Value.y, oldPosition.Value.z);
                    foreach (EventListenerDelegate eventListener in OnMovement) {
                        eventListener();
                    }
                }
                foreach (EventListenerDelegate eventListener in OnMovementEnd) {
                    eventListener();
                }
            }
            oldPosition = null;
            return wasMoving;
        }

        /// <summary>
        /// Playes the next animation in queue (if there is one)
        /// </summary>
        protected void ProcessAnimationQueue()
        {
            if (animationQueue.Count != 0) {
                //Pick next animation from queue
                PlayAnimation(animationQueue[0].Name, AnimationQueue.Skip /* <- This is irrelevant since currentAnimation was just set to null */, animationQueue[0].Callback);
                animationQueue.RemoveAt(0);
            }
        }

        private void Initialize(string prefabName, string objectName, bool active, Vector3 position, Transform parent, SpriteData spriteData, MouseEventData clickListenerData,
            float movementSpeed)
        {
            this.prefabName = prefabName;
            name = objectName;
            spriteData = spriteData ?? new SpriteData();
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
            if (this.spriteData.IsEmpty && Renderer.sprite != null) {
                //Default sprite from prefab
                this.spriteData.Sprite = Renderer.sprite.name;
            }

            UpdateSprite();

            if (IsClickable) {
                //Get BoxCollider, or add if missing
                Collider = gameObject.GetComponent<BoxCollider>();
                if (Collider == null) {
                    Collider = gameObject.AddComponent<BoxCollider>();
                }
            }

            if (string.IsNullOrEmpty(prefabName) && Renderer.sprite != null) {
                //Not prefab, set size based on sprite
                Width = 100 / Renderer.sprite.pixelsPerUnit;
                Height = 100 / Renderer.sprite.pixelsPerUnit;
            }
        }

        private void UpdateSprite()
        {
            if(((Renderer.sprite != null && Renderer.sprite.name == spriteData.Sprite) && Renderer.sortingOrder == spriteData.Order && !spriteDirectoryChanged &&
                spriteData.FlipX == Renderer.flipX && spriteData.FlipY == Renderer.flipY) || (Renderer.sprite == null && spriteData.IsEmpty)) {
                //No change
                return;
            }
            Renderer.sprite = spriteData.IsEmpty ? null : TextureManager.GetSprite(spriteData);
            Renderer.sortingOrder = spriteData.Order;
            Renderer.flipX = spriteData.FlipX;
            Renderer.flipY = spriteData.FlipY;
            spriteDirectoryChanged = false;
        }

        protected class QueuedAnimation
        {
            public string Name { get; set; }
            public SpriteAnimation.AnimationDelegate Callback { get; set; }
        }
    }
}