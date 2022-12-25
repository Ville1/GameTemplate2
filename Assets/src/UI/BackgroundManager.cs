using Game.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class BackgroundManager : MonoBehaviour
    {
        public static BackgroundManager Instance;

        public Image Image;

        private UISpriteData spriteData;

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

            Image.gameObject.SetActive(false);
            spriteData = new UISpriteData(Image.sprite.name, TextureDirectory.UI);
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
                return Image.gameObject.activeSelf;
            }
            set {
                Image.gameObject.SetActive(value);
            }
        }

        public UISpriteData SpriteData
        {
            get {
                return spriteData;
            }
            set {
                spriteData = value;
                UIHelper.SetImage(Image, spriteData);
            }
        }
    }
}
