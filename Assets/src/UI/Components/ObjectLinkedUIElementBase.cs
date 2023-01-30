using Game.Utils;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// UI element linked to a ObjectType object. Linked object can then update it's UI element when needed.
    /// </summary>
    /// <typeparam name="ObjectType"></typeparam>
    public abstract class ObjectLinkedUIElementBase<ObjectType> : MonoBehaviour, IObjectLinkedUIElement where ObjectType : IUILinkableObject
    {
        protected ObjectType linkedObject;

        protected virtual void Start()
        { }

        protected virtual void Update()
        { }

        public void Link(ObjectType target)
        {
            linkedObject = target;
            linkedObject.UIElement = this;
            UpdateUI();
        }

        public abstract void UpdateUI();

        protected static ComponentType InitializeGameObject<ComponentType>(MenuCommand menuCommand, float width = 50.0f, float height = 50.0f, string name = null) where ComponentType : Component
        {
            GameObject gameObject = new GameObject();
            gameObject.name = string.IsNullOrEmpty(name) ? typeof(ObjectType).Name.SplitCamelCase() : name;
            gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            gameObject.transform.parent = (menuCommand.context as GameObject).transform;

            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            GameObjectHelper.SetAnchorAndPivot(rectTransform, new Vector2(0.0f, 1.0f));
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);

            ComponentType component = (ComponentType)gameObject.AddComponent(typeof(ComponentType));

            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = component;

            return component;
        }

        protected static TMP_Text CreateText(GameObject parent, string name, Vector2 position, float width, float height, LString text, float fontSize)
        {
            GameObject textGameObject = new GameObject();
            textGameObject.name = name;
            textGameObject.transform.parent = parent.transform;

            TMP_Text textComponent = textGameObject.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = fontSize;
            textComponent.text = text;

            RectTransform textRectTransform = textGameObject.GetComponent<RectTransform>();
            GameObjectHelper.SetAnchorAndPivot(textRectTransform, new Vector2(0.0f, 1.0f));
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            textRectTransform.anchoredPosition = position.Clone();

            return textComponent;
        }

        protected static Image CreateImage(GameObject parent, string name, Vector2 position, float width, float height, SpriteData sprite)
        {
            GameObject imageGameObject = new GameObject();
            imageGameObject.name = name;
            imageGameObject.transform.parent = parent.transform;

            Image image = imageGameObject.AddComponent<Image>();
            image.sprite = sprite == null || sprite.IsEmpty ? null : TextureManager.GetSprite(sprite);

            RectTransform textRectTransform = imageGameObject.GetComponent<RectTransform>();
            GameObjectHelper.SetAnchorAndPivot(textRectTransform, new Vector2(0.0f, 1.0f));
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            textRectTransform.anchoredPosition = position.Clone();

            return image;
        }
    }

    public interface IUILinkableObject
    {
        IObjectLinkedUIElement UIElement { get; set; }
    }

    public interface IObjectLinkedUIElement
    {
        void UpdateUI();
    }
}
