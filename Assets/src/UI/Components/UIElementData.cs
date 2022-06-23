using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class UIElementData : IHasSprite
    {
        public enum ElementType { Text, Button, Image }

        public ElementType Type { get; private set; }
        public string GameObjectName { get; private set; }
        public string ElementText { get; private set; }
        public Color? TextColor { get; set; }
        public CustomButton.OnClick OnClick { get; private set; }
        public string Sprite { get; private set; }
        public TextureDirectory SpriteDirectory { get; private set; }

        private UIElementData(ElementType type, string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, string sprite, TextureDirectory spriteDirectory)
        {
            Type = type;
            GameObjectName = gameObjectName;
            ElementText = text;
            TextColor = textColor;
            OnClick = onClick;
            Sprite = sprite;
            SpriteDirectory = spriteDirectory;
        }

        public static UIElementData Text(string gameObjectName, string text, Color? textColor)
        {
            return new UIElementData(ElementType.Text, gameObjectName, text, textColor, null, null, TextureDirectory.Sprites);
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, null, TextureDirectory.Sprites);
        }

        public static UIElementData Button(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick, string sprite, TextureDirectory spriteDirectory)
        {
            return new UIElementData(ElementType.Button, gameObjectName, text, textColor, onClick, sprite, spriteDirectory);
        }

        public static UIElementData Image(string gameObjectName, string sprite, TextureDirectory spriteDirectory)
        {
            return new UIElementData(ElementType.Image, gameObjectName, null, null, null, sprite, spriteDirectory);
        }

        public void Set(GameObject parentGameObject)
        {
            switch (Type) {
                case ElementType.Text:
                    UIHelper.SetText(parentGameObject, GameObjectName, ElementText, TextColor);
                    break;
                case ElementType.Button:
                    UIHelper.SetButton(parentGameObject, GameObjectName, ElementText, OnClick);
                    break;
                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }
    }
}
