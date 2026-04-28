using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TerrainGimmickEntry))]
public class TerrainGimmickEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty gimmickDataProperty = property.FindPropertyRelative("_gimmickData");
        SerializedProperty changeSpriteProperty = property.FindPropertyRelative("_changeSprite");
        SerializedProperty moveSpeedProperty = property.FindPropertyRelative("_moveSpeed");
        SerializedProperty waypointsProperty = property.FindPropertyRelative("_waypoints");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(currentRect, gimmickDataProperty, label);

        if (IsImageChangeGimmick(gimmickDataProperty))
        {
            currentRect.y += lineHeight + spacing;
            EditorGUI.PropertyField(currentRect, changeSpriteProperty, new GUIContent("Change Sprite"));
        }

        if (IsMoveTerrainGimmick(gimmickDataProperty) || IsPatrolTerrainGimmick(gimmickDataProperty))
        {
            currentRect.y += lineHeight + spacing;
            EditorGUI.PropertyField(currentRect, moveSpeedProperty, new GUIContent("Move Speed"));
        }

        if (IsMoveTerrainGimmick(gimmickDataProperty) || IsPatrolTerrainGimmick(gimmickDataProperty))
        {
            currentRect.y += lineHeight + spacing;
            float waypointsHeight = EditorGUI.GetPropertyHeight(waypointsProperty, true);
            currentRect.height = waypointsHeight;
            EditorGUI.PropertyField(currentRect, waypointsProperty, new GUIContent("Waypoints"), true);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty gimmickDataProperty = property.FindPropertyRelative("_gimmickData");
        SerializedProperty waypointsProperty = property.FindPropertyRelative("_waypoints");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        float height = lineHeight;

        if (IsImageChangeGimmick(gimmickDataProperty))
        {
            height += lineHeight + spacing;
        }

        if (IsMoveTerrainGimmick(gimmickDataProperty) || IsPatrolTerrainGimmick(gimmickDataProperty))
        {
            height += lineHeight + spacing;
        }

        if (IsMoveTerrainGimmick(gimmickDataProperty) || IsPatrolTerrainGimmick(gimmickDataProperty))
        {
            height += EditorGUI.GetPropertyHeight(waypointsProperty, true) + spacing;
        }

        return height;
    }

    private bool IsImageChangeGimmick(SerializedProperty gimmickDataProperty)
    {
        if (gimmickDataProperty.objectReferenceValue == null)
        {
            return false;
        }

        return gimmickDataProperty.objectReferenceValue is ImageChangeGimmickSO;
    }

    private bool IsMoveTerrainGimmick(SerializedProperty gimmickDataProperty)
    {
        if (gimmickDataProperty.objectReferenceValue == null)
        {
            return false;
        }

        return gimmickDataProperty.objectReferenceValue is MoveTerrainGimmickSO;
    }

    private bool IsPatrolTerrainGimmick(SerializedProperty gimmickDataProperty)
    {
        if (gimmickDataProperty.objectReferenceValue == null)
        {
            return false;
        }

        return gimmickDataProperty.objectReferenceValue is PatrolTerrainGimmickSO;
    }
}
