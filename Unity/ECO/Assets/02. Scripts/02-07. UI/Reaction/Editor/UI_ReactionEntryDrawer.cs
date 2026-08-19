using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>리액션 항목을 접었다 펴며 그린다. 트리거 종류에 쓰이지 않는 필드는 아예 그리지 않는다.</summary>
[CustomPropertyDrawer(typeof(UI_ReactionEntry))]
public class UI_ReactionEntryDrawer : PropertyDrawer
{
    private static readonly List<string> _fieldPaths = new List<string>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect headerRect = new Rect(position.x, position.y, position.width, lineHeight);

        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded,
            UI_ReactionEntrySummary.Build(property), true);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;
        DrawFields(position, property);
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded)
        {
            return height;
        }

        CollectFieldPaths(property);
        for (int i = 0; i < _fieldPaths.Count; i++)
        {
            SerializedProperty field = property.FindPropertyRelative(_fieldPaths[i]);
            if (field == null)
            {
                continue;
            }

            height += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    private void DrawFields(Rect position, SerializedProperty property)
    {
        float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        CollectFieldPaths(property);
        for (int i = 0; i < _fieldPaths.Count; i++)
        {
            SerializedProperty field = property.FindPropertyRelative(_fieldPaths[i]);
            if (field == null)
            {
                continue;
            }

            float fieldHeight = EditorGUI.GetPropertyHeight(field, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, fieldHeight), field, true);
            y += fieldHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private static void CollectFieldPaths(SerializedProperty property)
    {
        _fieldPaths.Clear();
        _fieldPaths.Add("_label");
        _fieldPaths.Add("_isMuted");
        _fieldPaths.Add("_kind");

        EUIReactionTriggerKind kind =
            (EUIReactionTriggerKind)property.FindPropertyRelative("_kind").enumValueIndex;

        switch (kind)
        {
            case EUIReactionTriggerKind.Event:
                _fieldPaths.Add("_eventTrigger");
                break;

            case EUIReactionTriggerKind.Signal:
                _fieldPaths.Add("_signalTrigger");
                break;

            default:
                // 중단·복귀 정책은 상태가 이어지는 동안에만 의미가 있다.
                _fieldPaths.Add("_stateTrigger");
                _fieldPaths.Add("_interruptPolicy");
                _fieldPaths.Add("_exitPolicy");
                break;
        }

        _fieldPaths.Add("_reactions");
    }
}
