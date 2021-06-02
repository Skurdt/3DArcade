using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace SG.UnityEditor
{
    [CustomEditor(typeof(LoopScrollRect), true)]
    internal sealed class LoopScrollRectInspector : Editor
    {
        private LoopScrollRect _loopScrollRect;
        private int _index   = 0;
        private float _speed = 1000f;

        private void OnEnable() => _loopScrollRect = target as LoopScrollRect;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear"))
                    _loopScrollRect.ClearCells();

                if (GUILayout.Button("Refresh"))
                    _loopScrollRect.RefreshCells();

                if (GUILayout.Button("Refill"))
                    _loopScrollRect.RefillCells();

                if (GUILayout.Button("RefillFromEnd"))
                    _loopScrollRect.RefillCellsFromEnd();
            }

            EditorGUIUtility.labelWidth = 45f;
            using (new EditorGUILayout.HorizontalScope())
            {
                float width = (EditorGUIUtility.currentViewWidth - 100f) / 2f;
                _index = EditorGUILayout.IntField("Index", _index, GUILayout.Width(width));
                _speed = EditorGUILayout.FloatField("Speed", _speed, GUILayout.Width(width));
                if (GUILayout.Button("Scroll", GUILayout.Width(45f)))
                    _loopScrollRect.SrollToCell(_index, _speed);
            }
        }
    }
}
