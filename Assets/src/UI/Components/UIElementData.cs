using System;
using UnityEngine;

namespace Game.UI.Components
{
    public class UIElementData
    {
        public enum ElementType { Text, Button, Image }

        public ElementType Type { get; private set; }
        public string GameObjectName { get; private set; }
        public string Text { get; private set; }
        public Color? TextColor { get; set; }
        public CustomButton.OnClick OnClick { get; private set; }

        /// <summary>
        /// Text constructor
        /// </summary>
        public UIElementData(string gameObjectName, string text, Color? textColor)
        {
            Type = ElementType.Text;
            GameObjectName = gameObjectName;
            Text = text;
            TextColor = textColor;
        }

        /// <summary>
        /// Button constructor
        /// </summary>
        public UIElementData(string gameObjectName, string text, Color? textColor, CustomButton.OnClick onClick)
        {
            if(onClick == null) {
                throw new ArgumentNullException();
            }
            Type = ElementType.Button;
            GameObjectName = gameObjectName;
            Text = text;
            TextColor = textColor;
            OnClick = onClick;
        }
    }
}
