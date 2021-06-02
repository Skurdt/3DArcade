using UnityEngine;
using UnityEngine.UI;

namespace SG.UnityEditor
{
    internal static class SGDefaultControls
    {
        public static GameObject CreateLoopHorizontalScrollRect()
        {
            GameObject root         = CreateUIElementRoot("Loop Horizontal Scroll Rect", new Vector2(200f, 200f));
            GameObject content      = CreateUIObject("Content", root);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin     = new Vector2(0f, 0.5f);
            contentRT.anchorMax     = new Vector2(0f, 0.5f);
            contentRT.sizeDelta     = new Vector2(0f, 200f);
            contentRT.pivot         = new Vector2(0f, 0.5f);

            LoopHorizontalScrollRect scrollRect      = root.AddComponent<LoopHorizontalScrollRect>();
            scrollRect.Content                       = contentRT;
            scrollRect.Viewport                      = null;
            scrollRect.HorizontalScrollbar           = null;
            scrollRect.VerticalScrollbar             = null;
            scrollRect.Horizontal                    = true;
            scrollRect.Vertical                      = false;
            scrollRect.HorizontalScrollbarVisibility = LoopScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.VerticalScrollbarVisibility   = LoopScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.HorizontalScrollbarSpacing    = 0f;
            scrollRect.VerticalScrollbarSpacing      = 0f;

            _ = root.AddComponent<RectMask2D>();

            HorizontalLayoutGroup layoutGroup  = content.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment         = TextAnchor.MiddleLeft;
            layoutGroup.childForceExpandWidth  = false;
            layoutGroup.childForceExpandHeight = true;

            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit     = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit       = ContentSizeFitter.FitMode.Unconstrained;

            return root;
        }

        public static GameObject CreateLoopVerticalScrollRect()
        {
            GameObject root         = CreateUIElementRoot("Loop Vertical Scroll Rect", new Vector2(200f, 200f));
            GameObject content      = CreateUIObject("Content", root);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin     = new Vector2(0.5f, 1f);
            contentRT.anchorMax     = new Vector2(0.5f, 1f);
            contentRT.sizeDelta     = new Vector2(200f, 0f);
            contentRT.pivot         = new Vector2(0.5f, 1f);

            LoopVerticalScrollRect scrollRect        = root.AddComponent<LoopVerticalScrollRect>();
            scrollRect.Content                       = contentRT;
            scrollRect.Viewport                      = null;
            scrollRect.HorizontalScrollbar           = null;
            scrollRect.VerticalScrollbar             = null;
            scrollRect.Horizontal                    = false;
            scrollRect.Vertical                      = true;
            scrollRect.HorizontalScrollbarVisibility = LoopScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.VerticalScrollbarVisibility   = LoopScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.HorizontalScrollbarSpacing    = 0f;
            scrollRect.VerticalScrollbarSpacing      = 0f;

            _ = root.AddComponent<RectMask2D>();

            VerticalLayoutGroup layoutGroup    = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment         = TextAnchor.UpperCenter;
            layoutGroup.childForceExpandWidth  = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit     = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit       = ContentSizeFitter.FitMode.PreferredSize;

            return root;
        }

        #region code from DefaultControls.cs
        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child            = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta     = size;
            return child;
        }

        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            _ = go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
        #endregion
    }
}
