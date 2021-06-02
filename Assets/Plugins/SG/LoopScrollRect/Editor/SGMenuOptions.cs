using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SG.UnityEditor
{
    internal static class SGMenuOptions
    {
        [MenuItem("GameObject/UI/Loop Horizontal Scroll Rect", false, 2151)]
        public static void AddLoopHorizontalScrollRect(MenuCommand menuCommand)
        {
            GameObject go = SGDefaultControls.CreateLoopHorizontalScrollRect();
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/Loop Vertical Scroll Rect", false, 2152)]
        public static void AddLoopVerticalScrollRect(MenuCommand menuCommand)
        {
            GameObject go = SGDefaultControls.CreateLoopVerticalScrollRect();
            PlaceUIElementRoot(go, menuCommand);
        }

        #region code from MenuOptions.cs
        private const string UI_LAYERNAME = "UI";

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
                parent = GetOrCreateCanvasGameObject();

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name      = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, $"Create {element.name}");
            Undo.SetTransformParent(element.transform, parent.transform, $"Parent {element.name}");
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        private static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = selectedGo != null ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        private static GameObject CreateNewUI()
        {
            // Root for the UI
            GameObject root = new GameObject("Canvas")
            {
                layer = LayerMask.NameToLayer(UI_LAYERNAME)
            };
            Canvas canvas     = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _ = root.AddComponent<CanvasScaler>();
            _ = root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, $"Create {root.name}");

            // if there is no event system add one...
            // CreateEventSystem(false);
            return root;
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Camera camera    = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f), camera, out Vector2 localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x += canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y += canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0f, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0f, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - (canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x);
                position.y = localPlanePosition.y - (canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y);

                Vector3 minLocalPosition;
                minLocalPosition.x = (canvasRTransform.sizeDelta.x * (0f - canvasRTransform.pivot.x)) + (itemTransform.sizeDelta.x * itemTransform.pivot.x);
                minLocalPosition.y = (canvasRTransform.sizeDelta.y * (0f - canvasRTransform.pivot.y)) + (itemTransform.sizeDelta.y * itemTransform.pivot.y);

                Vector3 maxLocalPosition;
                maxLocalPosition.x = (canvasRTransform.sizeDelta.x * (1f - canvasRTransform.pivot.x)) - (itemTransform.sizeDelta.x * itemTransform.pivot.x);
                maxLocalPosition.y = (canvasRTransform.sizeDelta.y * (1f - canvasRTransform.pivot.y)) - (itemTransform.sizeDelta.y * itemTransform.pivot.y);

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation    = Quaternion.identity;
            itemTransform.localScale       = Vector3.one;
        }
        #endregion
    }
}
