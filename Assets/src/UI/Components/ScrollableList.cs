using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class ScrollableList : UIList
    {
        private static readonly string DEFAULT_CONTENT_NAME = "Content";
        private static float DEFAULT_SENSITIVITY = 10.0f;

        public Scrollbar HorizontalScrollbar { get; private set; }
        public Scrollbar VerticalScrollbar { get; private set; }
        public ScrollRect ScrollRect { get; private set; }

        public ScrollableList(GameObject rowPrototype, GameObject scrollView, GameObject scrollViewContent, float? rowSpacing = null) : base(rowPrototype, scrollViewContent, rowSpacing)
        {
            Initialize(scrollView);
        }

        public ScrollableList(GameObject rowPrototype, GameObject scrollView, float? rowSpacing = null) : base(rowPrototype, FindContent(scrollView, DEFAULT_CONTENT_NAME), rowSpacing)
        {
            Initialize(scrollView);
        }

        public ScrollableList(string rowPrototypeName, GameObject scrollView, float? rowSpacing = null) : base(rowPrototypeName, FindContent(scrollView, DEFAULT_CONTENT_NAME), rowSpacing)
        {
            Initialize(scrollView);
        }

        public ScrollableList(GameObject scrollView, float? rowSpacing = null) : base(FindContent(scrollView, DEFAULT_CONTENT_NAME), rowSpacing)
        {
            Initialize(scrollView);
        }

        private void Initialize(GameObject scrollView)
        {
            //Find scroll bars
            Scrollbar[] scrollbars = scrollView.GetComponentsInChildren<Scrollbar>();
            HorizontalScrollbar = scrollbars.FirstOrDefault(bar => bar.direction == Scrollbar.Direction.LeftToRight || bar.direction == Scrollbar.Direction.RightToLeft);
            VerticalScrollbar = scrollbars.FirstOrDefault(bar => bar.direction == Scrollbar.Direction.TopToBottom || bar.direction == Scrollbar.Direction.BottomToTop);

            //Set sensitivity
            ScrollRect = scrollView.GetComponent<ScrollRect>();
            ScrollRect.scrollSensitivity = DEFAULT_SENSITIVITY;
        }

        private static GameObject FindContent(GameObject scrollView, string name)
        {
            GameObject viewport = scrollView.GetComponentInChildren<Mask>().gameObject;
            RectTransform[] rectTransforms = viewport.GetComponentsInChildren<RectTransform>();
            foreach (RectTransform rectTransform in rectTransforms) {
                if(rectTransform.gameObject.name == name) {
                    return rectTransform.gameObject;
                }
            }
            return null;
        }
    }
}
