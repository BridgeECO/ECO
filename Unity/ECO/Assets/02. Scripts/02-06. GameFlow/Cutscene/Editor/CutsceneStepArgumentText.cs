using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 접힌 스텝 줄에 붙일 타입별 핵심 인자 문자열.
///
/// 런타임 스텝 클래스에 표시용 문자열을 두지 않는다. asmdef이 없어 전부 Assembly-CSharp이므로
/// 런타임에 두면 한글 요약이 통째로 플레이어 빌드에 들어가고, 스텝마다 줄 예산을 연출이 아닌
/// 에디터 문자열에 쓰게 된다.
/// </summary>
public static class CutsceneStepArgumentText
{
    private const int MAX_CHILD_NAMES = 3;
    private const string MISSING = "⚠ 대상 없음";

    private static readonly StringBuilder _builder = new StringBuilder();

    public static string Build(SerializedProperty step, string typeName)
    {
        switch (typeName)
        {
            case "CutsceneWaitStep":
                return Seconds(step, "_duration");

            case "CutsceneParallelGroupStep":
            case "CutsceneSequenceGroupStep":
                return BuildChildren(step);

            case "CutscenePlayerControlStep":
                return Bool(step, "_isControlBlocked") ? "정지" : "조작 반환";

            case "CutsceneCameraMoveStep":
                return Join(ObjectName(step, "_viewPoint", "플레이어 복귀"), Seconds(step, "_duration"));

            case "CutsceneCameraShakeStep":
                return Join(Seconds(step, "_duration"), "세기 " + Float(step, "_strength"));

            case "CutsceneActorActiveStep":
                return Join(ListSummary(step, "_targets"), Bool(step, "_isActive") ? "켜기" : "끄기");

            case "CutsceneActorSpriteStep":
                return BuildSprite(step);

            case "CutsceneTerrainEnergyStep":
                return Join(Count(step, "_targets"), Bool(step, "_isEnergyActive") ? "에너지 ON" : "에너지 OFF");

            case "CutsceneEnergyDeviceStep":
                return Join(Count(step, "_devices"), Bool(step, "_isDeviceActive") ? "가동" : "중단");

            case "CutsceneWaitTerrainSettledStep":
                return Join(Count(step, "_targets"), "정지 대기");

            case "CutsceneFadeStep":
                return Join(Bool(step, "_isFadeOut") ? "덮기" : "걷기", Seconds(step, "_duration"));

            case "CutsceneSfxStep":
                return EnumName(step, "_clip");

            case "CutsceneBossCinematicStep":
                return ObjectName(step, "_cinematic", MISSING);

            default:
                return string.Empty;
        }
    }

    private static string BuildSprite(SerializedProperty step)
    {
        SerializedProperty sprites = step.FindPropertyRelative("_sprites");
        string frames = sprites == null || sprites.arraySize == 0 ? MISSING : sprites.arraySize + "장";
        string loop = Bool(step, "_isLoop") ? "반복" : string.Empty;
        return Join(Join(ObjectName(step, "_spriteRenderer", MISSING), frames), loop);
    }

    private static string BuildChildren(SerializedProperty step)
    {
        SerializedProperty children = step.FindPropertyRelative("_steps");
        if (children == null || children.arraySize == 0)
        {
            return "(비어 있음)";
        }

        _builder.Clear();
        int shown = 0;
        int assigned = 0;

        // 숨은 개수는 arraySize가 아니라 타입이 지정된 것만 센다. 빈 줄까지 세면
        // "외 N개"가 있지도 않은 스텝을 가리킨다.
        for (int i = 0; i < children.arraySize; i++)
        {
            string childType = SerializeReferenceTypeMenu.GetShortTypeName(children.GetArrayElementAtIndex(i));
            if (childType == null)
            {
                continue;
            }

            assigned++;
            if (MAX_CHILD_NAMES <= shown)
            {
                continue;
            }

            if (0 < shown)
            {
                _builder.Append(", ");
            }

            _builder.Append(CutsceneStepSummary.GetDisplayName(childType));
            shown++;
        }

        if (shown == 0)
        {
            return "(타입 미지정)";
        }

        if (MAX_CHILD_NAMES < assigned)
        {
            _builder.Append(" 외 ").Append(assigned - MAX_CHILD_NAMES).Append("개");
        }

        return _builder.ToString();
    }

    private static string Join(string left, string right)
    {
        if (string.IsNullOrEmpty(left))
        {
            return right;
        }

        return string.IsNullOrEmpty(right) ? left : left + " " + right;
    }

    private static string Seconds(SerializedProperty step, string relativePath)
    {
        return "(" + Float(step, relativePath) + "초)";
    }

    private static string Float(SerializedProperty step, string relativePath)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        return property == null ? "?" : property.floatValue.ToString("0.##");
    }

    private static bool Bool(SerializedProperty step, string relativePath)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        return property != null && property.boolValue;
    }

    private static string Count(SerializedProperty step, string relativePath)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        if (property == null || property.arraySize == 0)
        {
            return MISSING;
        }

        return property.arraySize + "개";
    }

    private static string ListSummary(SerializedProperty step, string relativePath)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        if (property == null || property.arraySize == 0)
        {
            return MISSING;
        }

        Object first = property.GetArrayElementAtIndex(0).objectReferenceValue;
        string name = first == null ? MISSING : first.name;
        return property.arraySize == 1 ? name : name + " 외 " + (property.arraySize - 1) + "개";
    }

    private static string ObjectName(SerializedProperty step, string relativePath, string emptyText)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        Object value = property == null ? null : property.objectReferenceValue;
        return value == null ? emptyText : value.name;
    }

    private static string EnumName(SerializedProperty step, string relativePath)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        if (property == null)
        {
            return string.Empty;
        }

        int index = property.enumValueIndex;
        return 0 <= index && index < property.enumDisplayNames.Length ? property.enumDisplayNames[index] : "?";
    }
}
