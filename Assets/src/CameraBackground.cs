using Game.Utils;
using System.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Background that follows camera in the game world
    /// </summary>
    public class CameraBackground : MonoBehaviour
    {
        public static readonly int DISTANCE = 10;
        private static readonly int SORTING_ORDER = -100;

        public static CameraBackground Instance;

        private SpriteData spriteData;

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

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {

        }

        public bool Active
        {
            get {
                return gameObject.activeSelf;
            }
            set {
                gameObject.SetActive(value);
            }
        }

        public SpriteData SpriteData
        {
            get {
                return spriteData;
            }
            set {
                spriteData = value;
                UpdateSprite();
            }
        }

        private SpriteRenderer Renderer { get { return gameObject.GetComponent<SpriteRenderer>(); } }
        private RectTransform RectTransform { get { return gameObject.GetComponent<RectTransform>(); } }

        public void UpdateSprite()
        {
            Renderer.sprite = spriteData.IsEmpty || spriteData == null ? null : TextureManager.GetSprite(spriteData);
            Renderer.sortingOrder = SORTING_ORDER;
            if (spriteData != null && !spriteData.IsEmpty) {
                Renderer.flipX = spriteData.FlipX;
                Renderer.flipY = spriteData.FlipY;

                //Resize image
                //Calculate how much space image takes on the screen
                float pixelsPerUnit = Renderer.sprite.pixelsPerUnit;
                float spriteWidth = Renderer.sprite.rect.width;
                float spriteHeight = Renderer.sprite.rect.height;
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;

                Vector3[] corners = new Vector3[4];
                RectTransform.GetWorldCorners(corners);
                corners = corners.Select(c => CameraManager.Instance.CurrentCamera.WorldToScreenPoint(c)).ToArray();

                float rectScreenWidth = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x) - Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
                float rectScreenHeight = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y) - Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

                float realSpriteWidth = rectScreenWidth * (spriteWidth / pixelsPerUnit) / RectTransform.localScale.x;
                float realSpriteHeight = rectScreenHeight * (spriteHeight / pixelsPerUnit) / RectTransform.localScale.y;

                float widthRatio = realSpriteWidth / screenWidth;
                float widthMultiplier = 1.0f / widthRatio;
                float heightRatio = realSpriteHeight / screenHeight;
                float heightMultiplier = 1.0f / heightRatio;

                //Determine if image needs to be scaled to fit the screen based on width or height 
                bool resizeWidth = false;
                bool resizeHeight = false;

                float minMultiplier = Mathf.Min(widthRatio, heightRatio);
                float maxMultiplier = Mathf.Max(widthRatio, heightRatio);

                if (minMultiplier < 1.0f) {
                    //Image needs to be made larger
                    if (widthRatio < heightRatio) {
                        resizeWidth = true;
                    } else {
                        resizeHeight = true;
                    }
                } else if (minMultiplier > 1.0f) {
                    //Image needs to be made smaller
                    if (widthRatio < heightRatio) {
                        resizeHeight = true;
                    } else {
                        resizeWidth = true;
                    }
                }

                if (resizeWidth) {
                    RectTransform.localScale = new Vector3(widthMultiplier, widthMultiplier, 1.0f);
                } else if (resizeHeight) {
                    RectTransform.localScale = new Vector3(heightMultiplier, heightMultiplier, 1.0f);
                }
            }
        }
    }
}
