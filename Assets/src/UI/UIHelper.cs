using Game.UI.Components;
using Game.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIHelper
    {
        private static Image.Type DEFAULT_IMAGE_TYPE = Image.Type.Simple;

        public static void SetText(GameObject parent, string textGameObjectName, LString text, Color? color = null)
        {
            SetTextObject(parent, null, textGameObjectName, text, color);
        }

        public static void SetText(string parentGameObjectName, string textGameObjectName, LString text, Color? color = null)
        {
            SetTextObject(null, parentGameObjectName, textGameObjectName, text, color);
        }

        private static void SetTextObject(GameObject parent, string parentGameObjectName, string textGameObjectName, LString text, Color? color)
        {
            //Find GameObject
            GameObject textGameObject = parent != null ? GameObjectHelper.Find(parent, textGameObjectName) : GameObjectHelper.Find(parentGameObjectName, textGameObjectName);
            parentGameObjectName = parent != null ? parent.name : parentGameObjectName;
            if (textGameObject == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' not found", parentGameObjectName, textGameObjectName));
            }

            //Get component
            TMP_Text textComponent = textGameObject.GetComponent<TMP_Text>();
            if (textComponent == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' does not contain a TMP_Text component", parentGameObjectName, textGameObjectName));
            }

            //Set text
            textComponent.text = text;
            if (color.HasValue) {
                textComponent.color = color.Value;
            }
        }

        public static CustomButton SetButton(GameObject parentGameObject, string buttonGameObjectName, LString text, CustomButton.OnClick onClick)
        {
            return SetButtonObject(parentGameObject, null, buttonGameObjectName, text, onClick);
        }

        public static CustomButton SetButton(string parentGameObjectName, string buttonGameObjectName, LString text, CustomButton.OnClick onClick)
        {
            return SetButtonObject(null, parentGameObjectName, buttonGameObjectName, text, onClick);
        }

        public static CustomButton SetButtonObject(GameObject parent, string parentGameObjectName, string buttonGameObjectName, LString text, CustomButton.OnClick onClick)
        {
            //Find GameObject
            GameObject buttonGameObject = parent != null ? GameObjectHelper.Find(parent, buttonGameObjectName) : GameObjectHelper.Find(parentGameObjectName, buttonGameObjectName);
            parentGameObjectName = parent != null ? parent.name : parentGameObjectName;
            if (buttonGameObject == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' not found", parentGameObjectName, buttonGameObjectName));
            }

            //Get component
            Button buttonComponent = buttonGameObject.GetComponent<Button>();
            if (buttonComponent == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' does not contain a Button component", parentGameObjectName, buttonGameObjectName));
            }

            //Wrap with custom button
            CustomButton button = new CustomButton(buttonComponent, null, onClick);
            button.Text = text;
            return button;
        }

        public static void SetImage(GameObject parentGameObject, string imageGameObjectName, UISpriteData spriteData)
        {
            SetImageObject(parentGameObject, null, imageGameObjectName, spriteData);
        }

        public static void SetImage(string parentGameObjectName, string imageGameObjectName, UISpriteData spriteData)
        {
            SetImageObject(null, parentGameObjectName, imageGameObjectName, spriteData);
        }

        public static void SetImage(Image image, UISpriteData spriteData)
        {
            SetImageObject(image.gameObject, image, spriteData);
        }

        private static void SetImageObject(GameObject parent, string parentGameObjectName, string imageGameObjectName, UISpriteData spriteData)
        {
            //Find GameObject
            GameObject imageGameObject = parent != null ? GameObjectHelper.Find(parent, imageGameObjectName) : GameObjectHelper.Find(parentGameObjectName, imageGameObjectName);
            parentGameObjectName = parent != null ? parent.name : parentGameObjectName;
            if (imageGameObject == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' not found", parentGameObjectName, imageGameObjectName));
            }

            //Get component
            Image imageComponent = imageGameObject.GetComponent<Image>();
            if (imageComponent == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' does not contain a Image component", parentGameObjectName, imageGameObjectName));
            }

            SetImageObject(imageGameObject, imageComponent, spriteData);
        }

        private static void SetImageObject(GameObject imageGameObject, Image imageComponent, UISpriteData spriteData)
        {
            if (spriteData.IsEmpty) {
                //Hide image
                imageGameObject.SetActive(false);
                return;
            }

            //Make GameObject active
            if (!imageGameObject.activeSelf) {
                imageGameObject.SetActive(true);
            }

            //Set sprite
            imageComponent.sprite = TextureManager.GetSprite(spriteData);

            //Flips
            RectTransform rectTransform = imageGameObject.GetComponent<RectTransform>();
            if (spriteData.FlipX) {
                rectTransform.localScale = new Vector3(-1.0f * rectTransform.localScale.x, rectTransform.localScale.y, rectTransform.localScale.z);
            }
            if (spriteData.FlipY) {
                rectTransform.localScale = new Vector3(rectTransform.localScale.x, -1.0f * rectTransform.localScale.y, rectTransform.localScale.z);
            }

            if (spriteData.PixelsPerUnitMultiplier.HasValue) {
                imageComponent.pixelsPerUnitMultiplier = spriteData.PixelsPerUnitMultiplier.Value;
            }
            imageComponent.type = spriteData.ImageType.HasValue ? spriteData.ImageType.Value : DEFAULT_IMAGE_TYPE;
        }
    }
}
