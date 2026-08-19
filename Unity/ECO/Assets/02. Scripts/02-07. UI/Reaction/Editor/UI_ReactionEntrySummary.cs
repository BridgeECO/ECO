using System.Text;
using UnityEditor;

/// <summary>접힌 항목 한 줄에 "무엇에 반응해 무엇을 하는지"를 적는다.</summary>
public static class UI_ReactionEntrySummary
{
    private const int MAX_REACTION_NAMES = 3;

    private static readonly StringBuilder _builder = new StringBuilder();

    public static string Build(SerializedProperty entry)
    {
        _builder.Clear();

        string label = entry.FindPropertyRelative("_label").stringValue;
        _builder.Append(string.IsNullOrEmpty(label) ? BuildTriggerName(entry) : label);

        SerializedProperty reactions = entry.FindPropertyRelative("_reactions");
        if (reactions == null || reactions.arraySize == 0)
        {
            _builder.Append("  —  (리액션 없음)");
            return _builder.ToString();
        }

        _builder.Append("  —  ");
        AppendReactionNames(reactions);

        if (entry.FindPropertyRelative("_isMuted").boolValue)
        {
            _builder.Append("   [꺼짐]");
        }

        return _builder.ToString();
    }

    public static string BuildTriggerName(SerializedProperty entry)
    {
        EUIReactionTriggerKind kind = (EUIReactionTriggerKind)entry.FindPropertyRelative("_kind").enumValueIndex;
        switch (kind)
        {
            case EUIReactionTriggerKind.Event:
                return "이벤트 " + GetEnumName(entry, "_eventTrigger");

            case EUIReactionTriggerKind.Signal:
                return "신호 " + GetEnumName(entry, "_signalTrigger");

            default:
                return "상태 " + GetEnumName(entry, "_stateTrigger");
        }
    }

    private static void AppendReactionNames(SerializedProperty reactions)
    {
        int shown = 0;
        int assigned = 0;

        // 숨은 개수는 arraySize가 아니라 타입이 지정된 것만 센다. 타입을 아직 안 고른 빈 줄까지
        // 세면 "외 N개"가 있지도 않은 리액션을 가리킨다.
        for (int i = 0; i < reactions.arraySize; i++)
        {
            string typeName = GetShortTypeName(reactions.GetArrayElementAtIndex(i));
            if (string.IsNullOrEmpty(typeName))
            {
                continue;
            }

            assigned++;
            if (MAX_REACTION_NAMES <= shown)
            {
                continue;
            }

            if (0 < shown)
            {
                _builder.Append(", ");
            }

            _builder.Append(typeName);
            shown++;
        }

        if (shown == 0)
        {
            _builder.Append("(타입 미지정)");
            return;
        }

        if (MAX_REACTION_NAMES < assigned)
        {
            _builder.Append(" 외 ").Append(assigned - MAX_REACTION_NAMES).Append("개");
        }
    }

    private static string GetEnumName(SerializedProperty entry, string relativePath)
    {
        SerializedProperty property = entry.FindPropertyRelative(relativePath);
        int index = property.enumValueIndex;
        return 0 <= index && index < property.enumDisplayNames.Length ? property.enumDisplayNames[index] : "?";
    }

    // "UI_ScaleReaction" 에서 접두사와 접미사를 떼어 "Scale"만 남긴다.
    private static string GetShortTypeName(SerializedProperty element)
    {
        string fullTypename = element.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(fullTypename))
        {
            return null;
        }

        // "<어셈블리> <네임스페이스>.<타입>" 형식이라 어셈블리를 먼저 떼야 한다.
        // 어셈블리명에도 점이 들어갈 수 있어 순서를 뒤집으면 엉뚱하게 잘린다.
        int split = fullTypename.LastIndexOf(' ');
        string typeName = split < 0 ? fullTypename : fullTypename.Substring(split + 1);

        split = typeName.LastIndexOf('.');
        if (0 <= split)
        {
            typeName = typeName.Substring(split + 1);
        }

        if (typeName.StartsWith("UI_"))
        {
            typeName = typeName.Substring(3);
        }

        if (typeName.EndsWith("Reaction"))
        {
            typeName = typeName.Substring(0, typeName.Length - "Reaction".Length);
        }

        return typeName;
    }
}
