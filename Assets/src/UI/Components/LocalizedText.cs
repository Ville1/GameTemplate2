using UnityEditor;
using UnityEngine;
using TMPro;
using Game.Utils;

namespace Game.UI
{
    public class LocalizedText : TextMeshProUGUI
    {
        protected override void Start()
        {
            base.Start();
            if (Application.isPlaying) {
                text = new LString(text);
            }
        }

        [MenuItem("GameObject/UI/Localized Text", false, 10)]
        private static void CreateFromMenu(MenuCommand menuCommand)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Localized Text";
            gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            gameObject.transform.parent = (menuCommand.context as GameObject).transform;

            LocalizedText text = gameObject.AddComponent<LocalizedText>();
            text.text = "New Text";
            text.fontSize = 20.0f;

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            GameObjectHelper.SetAnchorAndPivot(rectTransform, new Vector2(0.0f, 1.0f));
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160.0f);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30.0f);
            rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);

            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = text;
        }
    }
}
