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

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect gimmickDataRect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(gimmickDataRect, gimmickDataProperty, new GUIContent("Gimmick Data"));

        if (IsImageChangeGimmick(gimmickDataProperty))
        {
            Rect changeSpriteRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
            EditorGUI.PropertyField(changeSpriteRect, changeSpriteProperty, new GUIContent("Change Sprite"));
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty gimmickDataProperty = property.FindPropertyRelative("_gimmickData");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        float height = lineHeight;

        if (IsImageChangeGimmick(gimmickDataProperty))
        {
            height += lineHeight + spacing;
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
}
