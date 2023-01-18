using Game.Utils;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Game.UI
{
    public class TemplateObjectElement : ObjectLinkedUIElementBase<ExampleObject>
    {
        public TMP_Text Text;

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void UpdateUI()
        {
            Text.text = string.Format("Counter: {0}", linkedObject.Counter);
        }

        [MenuItem("GameObject/UI/Example Object", false, 9)]
        private static void CreateFromMenu(MenuCommand menuCommand)
        {
            TemplateObjectElement templateElement = InitializeGameObject<TemplateObjectElement>(menuCommand, 100.0f, 25.0f);
            GameObject gameObject = templateElement.gameObject;

            GameObject textGameObject = new GameObject();
            textGameObject.name = "Text";
            textGameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            textGameObject.transform.parent = gameObject.transform;

            templateElement.Text = textGameObject.AddComponent<TextMeshProUGUI>();
            templateElement.Text.fontSize = 15.0f;

            RectTransform textRectTransform = textGameObject.GetComponent<RectTransform>();
            GameObjectHelper.SetAnchorAndPivot(textRectTransform, new Vector2(0.0f, 1.0f));
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100.0f);
            textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25.0f);
            textRectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
        }
    }

    public class ExampleObject : IUILinkableObject
    {
        public int Counter { get; set; }
        public IObjectLinkedUIElement UIElement { get; set; }

        public void Increment()
        {
            Counter++;
            if(Counter > 100) {
                Counter = 0;
            }
            UpdateUI();
        }

        public void UpdateUI()
        {
            if(UIElement != null) {
                UIElement.UpdateUI();
            }
        }
    }
}
