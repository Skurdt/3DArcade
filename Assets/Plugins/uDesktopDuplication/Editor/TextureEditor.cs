using UnityEngine;
using UnityEditor;
using System.Linq;

namespace uDesktopDuplication
{
    [CustomEditor(typeof(Texture))]
    public class TextureEditor : Editor
    {
        private Texture Texture => target as Texture;

        private Monitor Monitor => Texture.Monitor;

        private bool IsAvailable => Monitor == null || !Application.isPlaying;

        private bool _basicsFolded = true;
        private bool _invertFolded = true;
        private bool _clipFolded = true;
        private bool _matFolded = true;
        private SerializedProperty _invertX;
        private SerializedProperty _invertY;
        private SerializedProperty _useClip;
        private SerializedProperty _clipPos;
        private SerializedProperty _clipScale;

        private void OnEnable()
        {
            _invertX = serializedObject.FindProperty("_invertX");
            _invertY = serializedObject.FindProperty("_invertY");
            _useClip = serializedObject.FindProperty("_useClip");
            _clipPos = serializedObject.FindProperty("ClipPos");
            _clipScale = serializedObject.FindProperty("ClipScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            DrawMonitor();
            DrawInvert();
            DrawClip();
            DrawMaterial();

            _ = serializedObject.ApplyModifiedProperties();
        }

        private void Fold(string name, ref bool folded, System.Action func)
        {
            folded = Utils.Foldout(name, folded);
            if (folded)
            {
                ++EditorGUI.indentLevel;
                func();
                --EditorGUI.indentLevel;
            }
        }

        private void DrawMonitor() => Fold("Monitor", ref _basicsFolded, () =>
        {
            if (IsAvailable)
            {
                EditorGUILayout.HelpBox("Monitor information is available only in runtime.", MessageType.Info);
                return;
            }

            int id = EditorGUILayout.Popup("Monitor", Monitor.Id, Manager.Monitors.Select(x => x.Name).ToArray());
            if (id != Monitor.Id)
                Texture.MonitorId = id;

            _ = EditorGUILayout.IntField("ID", Monitor.Id);
            _ = EditorGUILayout.Toggle("Is Primary", Monitor.IsPrimary);
            _ = EditorGUILayout.EnumPopup("Rotation", Monitor.Rotation);
            _ = EditorGUILayout.Vector2Field("Resolution", new Vector2(Monitor.Width, Monitor.Height));
            _ = EditorGUILayout.Vector2Field("DPI", new Vector2(Monitor.DpiX, Monitor.DpiY));
        });

        private void DrawInvert() => Fold("Invert", ref _invertFolded, () =>
        {
            Texture.InvertX = EditorGUILayout.Toggle("Invert X", _invertX.boolValue);
            Texture.InvertY = EditorGUILayout.Toggle("Invert Y", _invertY.boolValue);
        });

        private void DrawClip() => Fold("Clip", ref _clipFolded, () =>
        {
            Texture.UseClip = EditorGUILayout.Toggle("Use Clip", _useClip.boolValue);
            _ = EditorGUILayout.PropertyField(_clipPos);
            _ = EditorGUILayout.PropertyField(_clipScale);
        });

        private void DrawMaterial() => Fold("Material", ref _matFolded, () =>
        {
            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("These parameters are applied to the shared material when not playing.", MessageType.Info);
            Texture.MeshForwardDirection = (MeshForwardDirection)EditorGUILayout.EnumPopup("Mesh Forward Direction", Texture.MeshForwardDirection);
            Texture.Bend = EditorGUILayout.Toggle("Use Bend", Texture.Bend);
            Texture.Width = EditorGUILayout.FloatField("Bend Width", Texture.Width);
            Texture.Radius = EditorGUILayout.Slider("Bend Radius", Texture.Radius, Texture.WorldWidth / (2 * Mathf.PI), 100f);
            Texture.Thickness = EditorGUILayout.Slider("Thickness", Texture.Thickness, 0f, 30f);
            Texture.Culling = (Culling)EditorGUILayout.EnumPopup("Culling", Texture.Culling);
        });
    }
}
