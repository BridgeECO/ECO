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
            SerializedProperty waypointProp = property.FindPropertyRelative("_waypoint");
            SerializedProperty energyCoreProp = property.FindPropertyRelative("_energyCore");

            float y = position.y + EditorGUIUtility.singleLineHeight + LINE_SPACING;

            Rect nodeTypeRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EEnergyPathNodeType nodeType = DrawNodeType(nodeTypeRect, nodeTypeProp);

            y += EditorGUIUtility.singleLineHeight + LINE_SPACING;

            Rect valueRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            if (nodeType == EEnergyPathNodeType.EnergyCore)
            {
                EditorGUI.PropertyField(valueRect, energyCoreProp);
            }
            else
            {
                EditorGUI.PropertyField(valueRect, waypointProp);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    // 열거형 값이 1부터 시작해 선언 순서(enumValueIndex)와 값이 다르므로 값(intValue)으로 읽고 쓴다.
    // 정의되지 않은 값(0)은 기본 칸에서 빈칸으로 보이므로, 경로 계산과 같이 Waypoint로 보여 주고 사용자가 고를 때만 값을 쓴다.
    private EEnergyPathNodeType DrawNodeType(Rect rect, SerializedProperty nodeTypeProp)
    {
        EEnergyPathNodeType nodeType = EnergyPathNode.ToDefinedType((EEnergyPathNodeType)nodeTypeProp.intValue);
        GUIContent label = EditorGUI.BeginProperty(rect, new GUIContent(nodeTypeProp.displayName), nodeTypeProp);
        EditorGUI.BeginChangeCheck();
        nodeType = (EEnergyPathNodeType)EditorGUI.EnumPopup(rect, label, nodeType);
        if (EditorGUI.EndChangeCheck())
        {
            nodeTypeProp.intValue = (int)nodeType;
        }
        EditorGUI.EndProperty();
        return nodeType;
    }
}
