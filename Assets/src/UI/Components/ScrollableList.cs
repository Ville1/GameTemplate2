using System.Collections.Generic;
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
        public ScrollRect.ScrollbarVisibility VerticalScrollbarVisibility { get; private set; }
        public ScrollRect.ScrollbarVisibility HorizontalScrollbarVisibility { get; private set; }

        public ScrollableList(GameObject rowPrototype, GameObject scrollView, GameObject scrollViewContent, float? rowSpacing = null,
            ScrollRect.ScrollbarVisibility? verticalScrollbarVisibility = null, ScrollRect.ScrollbarVisibility? horizontalScrollbarVisibility = null) : base(rowPrototype, scrollViewContent, rowSpacing)
        {
            Initialize(scrollView, verticalScrollbarVisibility, horizontalScrollbarVisibility);
        }

        public ScrollableList(GameObject rowPrototype, GameObject scrollView, float? rowSpacing = null) : base(rowPrototype, FindContent(scrollView, DEFAULT_CONTENT_NAME), rowSpacing)
        {
            Initialize(scrollView, null, null);
        }

        public ScrollableList(string rowPrototypeName, GameObject scrollView, float? rowSpacing = null) : base(rowPrototypeName, FindContent(scrollView, DEFAULT_CONTENT_NAME), rowSpacing)
        {
            Initialize(scrollView, null, null);
        }

        public ScrollableList(GameObject scrollView, float? rowSpacing = null) : base(FindContent(scrollView, DEFAULT_CONTENT_NAME), rowSpacing)
        {
            Initialize(scrollView, null, null);
        }

        public ScrollableList(GameObject scrollView, float? rowSpacing, ScrollRect.ScrollbarVisibility? verticalScrollbarVisibility, ScrollRect.ScrollbarVisibility? horizontalScrollbarVisibility) : base(FindContent(scrollView, DEFAULT_CONTENT_NAME), rowSpacing)
        {
            Initialize(scrollView, verticalScrollbarVisibility, horizontalScrollbarVisibility);
        }

        public ScrollableList(Dictionary<string, GameObject> rowPrototypes, GameObject scrollView, GameObject scrollViewContent,
            ScrollRect.ScrollbarVisibility? verticalScrollbarVisibility = null, ScrollRect.ScrollbarVisibility? horizontalScrollbarVisibility = null) : base(rowPrototypes, scrollViewContent)
        {
            Initialize(scrollView, verticalScrollbarVisibility, horizontalScrollbarVisibility);
        }

        public ScrollableList(Dictionary<string, GameObject> rowPrototypes, GameObject scrollView) : base(rowPrototypes, FindContent(scrollView, DEFAULT_CONTENT_NAME))
        {
            Initialize(scrollView, null, null);
        }

        public ScrollableList(Dictionary<string, GameObject> rowPrototypes, GameObject scrollView, ScrollRect.ScrollbarVisibility? verticalScrollbarVisibility, ScrollRect.ScrollbarVisibility? horizontalScrollbarVisibility) : base(rowPrototypes, FindContent(scrollView, DEFAULT_CONTENT_NAME))
        {
            Initialize(scrollView, verticalScrollbarVisibility, horizontalScrollbarVisibility);
        }

        private void Initialize(GameObject scrollView, ScrollRect.ScrollbarVisibility? verticalScrollbarVisibility, ScrollRect.ScrollbarVisibility? horizontalScrollbarVisibility)
        {
            //Find scroll bars
            Scrollbar[] scrollbars = scrollView.GetComponentsInChildren<Scrollbar>();
            HorizontalScrollbar = scrollbars.FirstOrDefault(bar => bar.direction == Scrollbar.Direction.LeftToRight || bar.direction == Scrollbar.Direction.RightToLeft);
            VerticalScrollbar = scrollbars.FirstOrDefault(bar => bar.direction == Scrollbar.Direction.TopToBottom || bar.direction == Scrollbar.Direction.BottomToTop);

            //Set sensitivity
            ScrollRect = scrollView.GetComponent<ScrollRect>();
            ScrollRect.scrollSensitivity = DEFAULT_SENSITIVITY;

            //Visibility
            ScrollRect.verticalScrollbarVisibility = verticalScrollbarVisibility.HasValue ? verticalScrollbarVisibility.Value : ScrollRect.verticalScrollbarVisibility;
            ScrollRect.horizontalScrollbarVisibility = horizontalScrollbarVisibility.HasValue ? horizontalScrollbarVisibility.Value : ScrollRect.horizontalScrollbarVisibility;
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
