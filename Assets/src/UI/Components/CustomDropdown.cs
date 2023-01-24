using Game.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class CustomDropdown
    {
        private static readonly string DEFAULT_CAPTION_IMAGE_NAME = "Caption Image";
        private static readonly string DEFAULT_ITEM_IMAGE_NAME = "Item Image";
        private static readonly float GENERATED_CAPTION_IMAGE_SIZE = 0.867f;//1.0f = dropdown height
        private static readonly float GENERATED_ITEM_IMAGE_SIZE = 0.867f;//1.0f = dropdown height
        private static readonly List<string> ITEM_PATH = new List<string>() { "Template", "Viewport", "Content", "Item" };

        public delegate void CustomDropdownCallback(int value);

        public CustomDropdownCallback OnChange { get; set; }

        private TMP_Dropdown baseDropdown;
        private RectTransform rectTransform;

        public CustomDropdown(TMP_Dropdown baseDropdown, CustomDropdownCallback onChange)
        {
            Initialize(baseDropdown, onChange, null, null, false, null, null, false);
        }

        public CustomDropdown(TMP_Dropdown baseDropdown, CustomDropdownCallback onChange, Image captionImage = null, Image itemImage = null)
        {
            Initialize(baseDropdown, onChange, captionImage, null, false, itemImage, null, false);
        }

        public CustomDropdown(TMP_Dropdown baseDropdown, CustomDropdownCallback onChange, string captionImageName = null, string itemImageName = null)
        {
            Initialize(baseDropdown, onChange, null, captionImageName, false, null, itemImageName, false);
        }

        public CustomDropdown(TMP_Dropdown baseDropdown, CustomDropdownCallback onChange, bool generateCaptionImage = false, bool generateItemImage = false)
        {
            Initialize(baseDropdown, onChange, null, null, generateCaptionImage, null, null, generateItemImage);
        }

        private void Initialize(TMP_Dropdown baseDropdown, CustomDropdownCallback onChange, Image captionImage, string captionImageName, bool generateCaptionImage,
            Image itemImage, string itemImageName, bool generateItemImage)
        {
            //Set baseDropdown
            this.baseDropdown = baseDropdown;
            this.baseDropdown.ClearOptions();
            rectTransform = this.baseDropdown.gameObject.GetComponent<RectTransform>();

            //Set event listener
            TMP_Dropdown.DropdownEvent onChangeEvent = new TMP_Dropdown.DropdownEvent();
            onChangeEvent.AddListener(HandleChange);
            this.baseDropdown.onValueChanged = onChangeEvent;
            OnChange = onChange;

            //Caption image
            if(captionImage != null) {
                this.baseDropdown.captionImage = captionImage;
            } else if(!string.IsNullOrEmpty(captionImageName)) {
                //Find caption image based on parameter
                this.baseDropdown.captionImage = GetImage(this.baseDropdown.gameObject, captionImageName, null, true);
            } else if (generateCaptionImage) {
                //Create a new caption image
                string captionImageGameObjectName = GetNewGameObjectName(this.baseDropdown.gameObject, DEFAULT_CAPTION_IMAGE_NAME);

                //Create GameObject
                GameObject captionImageGameObject = new GameObject();
                captionImageGameObject.name = captionImageGameObjectName;
                captionImageGameObject.transform.parent = this.baseDropdown.gameObject.transform;

                //Create a new component
                this.baseDropdown.captionImage = captionImageGameObject.AddComponent<Image>();

                //Set a proper size
                SetImageSizeAndPosition(captionImageGameObject, GENERATED_CAPTION_IMAGE_SIZE, rectTransform.rect.height * 0.75f);
            } else {
                //Check if there is a caption image with the default name
                this.baseDropdown.captionImage = GetImage(this.baseDropdown.gameObject, DEFAULT_CAPTION_IMAGE_NAME, null, false);
            }

            //Item image
            if(itemImage != null) {
                this.baseDropdown.itemImage = itemImage;
            } else if (!string.IsNullOrEmpty(itemImageName)) {
                //Find item image based on parameter
                this.baseDropdown.itemImage = GetImage(this.baseDropdown.gameObject, itemImageName, ITEM_PATH, true);
            } else if (generateItemImage) {
                //Create a new item image
                //Find the parent GameObject
                GameObject itemImageParent;
                string lastParentName, lastChildName;
                bool parentFound = GameObjectHelper.FindWithPath(this.baseDropdown.gameObject, out itemImageParent, out lastParentName, out lastChildName, ITEM_PATH);
                if (!parentFound) {
                    //Could not find the item template
                    CustomLogger.Error("{GameObjectNotFound}", lastParentName, lastChildName);
                } else {
                    //Create GameObject
                    string itemImageGameObjectName = GetNewGameObjectName(itemImageParent, DEFAULT_ITEM_IMAGE_NAME);

                    GameObject itemImageGameObject = new GameObject();
                    itemImageGameObject.name = itemImageGameObjectName;
                    itemImageGameObject.transform.parent = itemImageParent.transform;

                    //Create a new component
                    this.baseDropdown.itemImage = itemImageGameObject.AddComponent<Image>();

                    //Set a proper size
                    SetImageSizeAndPosition(itemImageGameObject, GENERATED_ITEM_IMAGE_SIZE, 5.0f);
                }
            } else {
                //Check if there is a item image with the default name
                this.baseDropdown.itemImage = GetImage(this.baseDropdown.gameObject, DEFAULT_ITEM_IMAGE_NAME, ITEM_PATH, false);
            }
        }

        private Image GetImage(GameObject parent, string name, List<string> path, bool logErrors)
        {
            path = path ?? new List<string>();
            path.Add(name);
            GameObject imageGameObject;
            string lastParentName, lastChildName;

            bool found = GameObjectHelper.FindWithPath(parent, out imageGameObject, out lastParentName, out lastChildName, path);

            if (!found) {
                if (logErrors) {
                    CustomLogger.Error("{GameObjectNotFound}", lastParentName, lastChildName);
                }
                return null;
            } else {
                Image imageComponent = imageGameObject.GetComponent<Image>();
                if (imageComponent == null) {
                    if (logErrors) {
                        CustomLogger.Warning("{ComponentNotFound}", imageComponent.name, "Image");
                    }
                    return null;
                } else {
                    return imageComponent;
                }
            }
        }

        private void SetImageSizeAndPosition(GameObject imageGameObject, float sizeMultiplier, float positionDelta)
        {
            //Anchors and pivot
            RectTransform rectTransform = imageGameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1.0f, 0.5f);
            rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            //Size
            float size = Mathf.Round(this.rectTransform.rect.height * sizeMultiplier);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

            //Position
            rectTransform.anchoredPosition = new Vector2(-1.0f * Mathf.Round(size * 0.5f + positionDelta), 0.0f);
        }

        private string GetNewGameObjectName(GameObject parent, string name)
        {
            //Check if there is already a GameObject with same name
            string newName = name;
            GameObject gameObjectWithSameName = GameObjectHelper.Find(parent, newName);
            int findNameCounter = 2;
            int maxIteration = findNameCounter + 999999;
            while (gameObjectWithSameName != null) {
                newName = string.Format("{0} ({1})", name, findNameCounter);
                gameObjectWithSameName = GameObjectHelper.Find(parent, newName);
                findNameCounter++;
                if (findNameCounter == maxIteration) {
                    //This should not realistically happen
                    return string.Format("{0} ({1})", name, Guid.NewGuid());
                }
            }

            return newName;
        }

        public int Value
        {
            get {
                return baseDropdown.value;
            }
            set {
                baseDropdown.value = value;
            }
        }

        public bool Interactable
        {
            get {
                return baseDropdown.interactable;
            }
            set {
                baseDropdown.interactable = value;
            }
        }

        public void AddOption(LString text, IHasSprite objectWithSprite = null)
        {
            AddOption(new TMP_Dropdown.OptionData() {
                text = text,
                image = objectWithSprite == null ? null : TextureManager.GetSprite(objectWithSprite)
            });
        }

        public void AddOption(LString text, SpriteData spriteData)
        {
            AddOption(new TMP_Dropdown.OptionData() {
                text = text,
                image = spriteData == null || spriteData.IsEmpty ? null : TextureManager.GetSprite(spriteData)
            });
        }

        public void AddOption(LString text, string spriteName)
        {
            AddOption(new TMP_Dropdown.OptionData() {
                text = text,
                image = string.IsNullOrEmpty(spriteName) ? null : TextureManager.GetSprite(TextureDirectory.UI, spriteName)
            });
        }

        public void AddOption(TMP_Dropdown.OptionData optionData)
        {
            if(optionData.image != null && baseDropdown.captionImage == null && baseDropdown.itemImage == null) {
                CustomLogger.Warning("{UIElementError}", "Dropdown has neither caption image nor item image, parameter image will not be visible");
            }
            baseDropdown.AddOptions(new List<TMP_Dropdown.OptionData>() { optionData });
        }

        public void Clear()
        {
            baseDropdown.ClearOptions();
        }

        private void HandleChange(int value)
        {
            if(OnChange != null) {
                OnChange(value);
            }
        }
    }
}
