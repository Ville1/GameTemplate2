using Game.Utils;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// TODO: allow spriteless objects
    /// </summary>
    public class Object2D
    {
        public RectTransform RectTransform { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        public bool IsPrototype { get { return gameObject == null; } }

        protected GameObject gameObject = null;
        private string prefabName = null;
        private string name = null;
        private string spriteName = null;
        private TextureDirectory spriteDirectory = TextureDirectory.Sprites;
        private UpdateListener updateListener = null;
        private bool isDestroyed = false;
        private bool spriteDirectoryChanged = false;

        /// <summary>
        /// GameObject constructor
        /// </summary>
        public Object2D(Object2D prototype, string objectName, bool active, Vector3 position, Transform parent)
        {
            Initialize(prototype.prefabName, objectName, active, position, parent, prototype.spriteName, prototype.spriteDirectory);
        }

        /// <summary>
        /// GameObject constructor
        /// </summary>
        public Object2D(string prefabName, string objectName, bool active, Vector3 position, Transform parent, string spriteName, TextureDirectory spriteDirectory)
        {
            Initialize(prefabName, objectName, active, position, parent, spriteName, spriteDirectory);
        }

        /// <summary>
        /// Prototype constructor
        /// </summary>
        public Object2D(string prefabName, string name, string spriteName, TextureDirectory spriteDirectory)
        {
            this.prefabName = prefabName;
            this.name = name;
            this.spriteName = spriteName;
            this.spriteDirectory = spriteDirectory;
        }

        public virtual bool Active
        {
            get {
                return gameObject == null ? false : gameObject.activeSelf;
            }
            set {
                if (IsPrototype) {
                    CustomLogger.Error("ObjectIsPrototype", string.IsNullOrEmpty(name) ? "Unnamed" : name);
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
                return spriteName;
            }
            set {
                spriteName = value;
                if(Renderer != null) {
                    UpdateSprite();
                }
            }
        }

        public TextureDirectory SpriteDirectory
        {
            get {
                return spriteDirectory;
            }
            set {
                if(spriteDirectory != value) {
                    spriteDirectory = value;
                    spriteDirectoryChanged = true;
                    if (Renderer != null) {
                        UpdateSprite();
                    }
                }
            }
        }

        public void Destroy()
        {
            if (isDestroyed) {
                CustomLogger.Error("ObjectIsDestroyed", name);
            } else {
                GameObject.Destroy(gameObject);
                isDestroyed = true;
            }
        }

        public override string ToString()
        {
            return gameObject.name;
        }

        private void Initialize(string prefabName, string objectName, bool active, Vector3 position, Transform parent, string spriteName, TextureDirectory spriteDirectory)
        {
            this.prefabName = prefabName;
            name = objectName;
            this.spriteName = spriteName;
            this.spriteDirectory = spriteDirectory;

            //Instantiate GameObject
            gameObject = GameObject.Instantiate(
                PrefabManager.Instance.Get(prefabName),
                position,
                Quaternion.identity,
                parent
            );
            gameObject.name = objectName;
            gameObject.SetActive(active);

            //Get UpdateListener, or add if missing
            updateListener = gameObject.GetComponent<UpdateListener>();
            if (updateListener == null) {
                updateListener = gameObject.AddComponent<UpdateListener>();
            }

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
            if (string.IsNullOrEmpty(this.spriteName)) {
                //Use default sprite from prefab
                if(Renderer.sprite == null) {
                    //Sprite name was not provided with spriteName - parameter and Renderer is missing or is lacking a sprite
                    CustomLogger.Error("Object2DNoSprite");
                    this.spriteName = string.Empty;
                } else {
                    this.spriteName = Renderer.sprite.name;
                }
            }

            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if((Renderer.sprite != null && Renderer.sprite.name == spriteName) && !spriteDirectoryChanged) {
                //No change
                return;
            }
            Renderer.sprite = TextureManager.GetSprite(spriteDirectory, spriteName);
            spriteDirectoryChanged = false;
        }
    }
}