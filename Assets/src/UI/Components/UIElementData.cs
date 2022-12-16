using Game.Utils;
using System;
using UnityEngine;

namespace Game.UI.Components
{
    public class UIElementData
    {
        public enum ElementType { Text, Button, Image, Tooltip }

        public ElementType Type { get; private set; }
        public string GameObjectName { get; private set; }
        public string ElementText { get; private set; }
        public Color? TextColor { get; set; }
        public CustomButton.OnClick OnClick { get; private set; }
        public UISpriteData SpriteData { get; private set; }

        private UIElementData(ElementType type, string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, UISpriteData spriteData)
        {
            Type = type;
            GameObjectName = gameObjectName;
            ElementText = text;
            TextColor = textColor;
            OnClick = onClick;
            SpriteData = new UISpriteData(spriteData);
        }

        public static UIElementData Text(string gameObjectName, string text, Color? textColor)
        {
            return new UIElementData(ElementType.Text, gameObjectName, text, textColor, null, new UISpriteData());
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, new UISpriteData());
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, UISpriteData spriteData)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, spriteData);
        }

        public static UIElementData Image(string gameObjectName, UISpriteData spriteData)
        {
            return new UIElementData(ElementType.Image, gameObjectName, null, null, null, spriteData);
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, SpriteData spriteData)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, new UISpriteData(spriteData));
        }

        public static UIElementData Image(string gameObjectName, SpriteData spriteData)
        {
            return new UIElementData(ElementType.Image, gameObjectName, null, null, null, new UISpriteData(spriteData));
        }

        public static UIElementData Tooltip(string gameObjectName, LString tooltip)
        {
            return new UIElementData(ElementType.Tooltip, gameObjectName, tooltip, null, null, new UISpriteData());
        }

        public void Set(GameObject parentGameObject)
        {
            switch (Type) {
                case ElementType.Text:
                    UIHelper.SetText(parentGameObject, GameObjectName, ElementText, TextColor);
                    break;
                case ElementType.Button:
                    UIHelper.SetButton(parentGameObject, GameObjectName, ElementText, OnClick);
                    if (!SpriteData.IsEmpty) {
                        //TODO: Implement this
                        //Needs name for image GameObject?
                        throw new NotImplementedException("Buttons with images is not implemented");
                    }
                    break;
                case ElementType.Image:
                    if (SpriteData.IsEmpty) {
                        GameObjectHelper.Find(parentGameObject, GameObjectName).SetActive(false);
                    } else {
                        UIHelper.SetImage(parentGameObject, GameObjectName, SpriteData);
                    }
                    break;
                case ElementType.Tooltip:
                    GameObject gameObject = GameObjectHelper.Find(parentGameObject, GameObjectName);
                    TooltipManager.Instance.UnregisterTooltip(gameObject);
                    if (!string.IsNullOrEmpty(ElementText)) {
                        TooltipManager.Instance.RegisterTooltip(new Tooltip(gameObject, ElementText));
                    }
                    break;
                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }
    }
}
