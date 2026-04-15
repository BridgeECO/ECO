using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnergyPathNode))]
public class EnergyPathNodeDrawer : PropertyDrawer
{
    private const float LINE_SPACING = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            height += EditorGUIUtility.singleLineHeight + LINE_SPACING;
            height += EditorGUIUtility.singleLineHeight + LINE_SPACING;
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            SerializedProperty nodeTypeProp = property.FindPropertyRelative("_nodeType");
            SerializedProperty terrainProp = property.FindPropertyRelative("_terrain");
            SerializedProperty waypointProp = property.FindPropertyRelative("_waypoint");

            float y = position.y + EditorGUIUtility.singleLineHeight + LINE_SPACING;

            Rect nodeTypeRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(nodeTypeRect, nodeTypeProp);

            y += EditorGUIUtility.singleLineHeight + LINE_SPACING;

            Rect valueRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

            EEnergyPathNodeType nodeType = (EEnergyPathNodeType)nodeTypeProp.enumValueIndex;
            if (nodeType == EEnergyPathNodeType.Terrain)
            {
                EditorGUI.PropertyField(valueRect, terrainProp);
            }
            else
            {
                EditorGUI.PropertyField(valueRect, waypointProp);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}
