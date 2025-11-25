using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ECO
{
    [CustomEditor(typeof(ConfigSOBase<>), true)]
    public class ConfigSOInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var so = target as ScriptableObject;
            if (so == null)
                return;

            var type = so.GetType();
            var methodRefresh = type.BaseType.GetMethod("Refresh", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            GUILayout.Space(10);

            if (GUILayout.Button("▶ Refresh Data"))
            {
                methodRefresh?.Invoke(so, null);
                EditorUtility.SetDirty(so);
            }

        }
    }
}