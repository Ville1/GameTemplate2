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
            SetText(parent.name, textGameObjectName, text, color);
        }

        public static void SetText(string parentGameObjectName, string textGameObjectName, LString text, Color? color = null)
        {
            //Find GameObject
            GameObject textGameObject = GameObjectHelper.Find(parentGameObjectName, textGameObjectName);
            if(textGameObject == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' not found", parentGameObjectName, textGameObjectName));
            }

            //Get component
            TMP_Text textComponent = textGameObject.GetComponent<TMP_Text>();
            if(textComponent == null) {
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
            return SetButton(parentGameObject.name, buttonGameObjectName, text, onClick);
        }

        public static CustomButton SetButton(string parentGameObjectName, string buttonGameObjectName, LString text, CustomButton.OnClick onClick)
        {
            //Find GameObject
            GameObject buttonGameObject = GameObjectHelper.Find(parentGameObjectName, buttonGameObjectName);
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
            SetImage(parentGameObject.name, imageGameObjectName, spriteData);
        }

        public static void SetImage(string parentGameObjectName, string imageGameObjectName, UISpriteData spriteData)
        {
            //Find GameObject
            GameObject imageGameObject = GameObjectHelper.Find(parentGameObjectName, imageGameObjectName);
            if (imageGameObject == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' not found", parentGameObjectName, imageGameObjectName));
            }

            //Get component
            Image imageComponent = imageGameObject.GetComponent<Image>();
            if (imageComponent == null) {
                throw new ArgumentException(string.Format("GameObject '{0}/{1}' does not contain a Image component", parentGameObjectName, imageGameObjectName));
            }

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
