using Game.UI.Components;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIHelper
    {
        public static GameObject Find(Transform parent, string name)
        {
            return Find(parent.gameObject.name, name);
        }

        public static GameObject Find(GameObject parent, string name)
        {
            return Find(parent.name, name);
        }

        public static GameObject Find(string parent, string name)
        {
            return GameObject.Find(string.Format("{0}/{1}", parent, name));
        }

        public static void SetText(GameObject parent, string textGameObjectName, string text, Color? color = null)
        {
            SetText(parent.name, textGameObjectName, text, color);
        }

        public static void SetText(string parentGameObjectName, string textGameObjectName, string text, Color? color = null)
        {
            //Find GameObject
            GameObject textGameObject = Find(parentGameObjectName, textGameObjectName);
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
        public static CustomButton SetButton(GameObject parentGameObject, string buttonGameObjectName, string text, CustomButton.OnClick onClick)
        {
            return SetButton(parentGameObject.name, buttonGameObjectName, text, onClick);
        }

        public static CustomButton SetButton(string parentGameObjectName, string buttonGameObjectName, string text, CustomButton.OnClick onClick)
        {
            //Find GameObject
            GameObject buttonGameObject = UIHelper.Find(parentGameObjectName, buttonGameObjectName);
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
    }
}
