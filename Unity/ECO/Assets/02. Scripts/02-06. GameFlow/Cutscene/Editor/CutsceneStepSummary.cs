using System;
using System.Text;
using UnityEditor;

/// <summary>
/// 접힌 스텝 한 줄에 "몇 번째가 무엇을 하는지"를 적는다.
/// 스텝이 스무 개로 늘어나도 "몇 번째가 문제야"를 말할 수 있게 되는 것 하나로 소통 비용이 크게 준다.
/// </summary>
public static class CutsceneStepSummary
{
    private const string NAME_PREFIX = "Cutscene";
    private const string NAME_SUFFIX = "Step";
    private const string EMPTY_TEXT = "(타입 미지정)";

    private static readonly StringBuilder _builder = new StringBuilder();

    public static string Build(SerializedProperty step)
    {
        _builder.Clear();
        AppendIndex(step);

        string typeName = SerializeReferenceTypeMenu.GetShortTypeName(step);
        if (typeName == null)
        {
            _builder.Append(EMPTY_TEXT);
            return _builder.ToString();
        }

        SerializedProperty label = step.FindPropertyRelative("_label");
        string labelText = label == null ? string.Empty : label.stringValue;
        _builder.Append(string.IsNullOrEmpty(labelText) ? GetDisplayName(typeName) : labelText);

        string argument = CutsceneStepArgumentText.Build(step, typeName);
        if (!string.IsNullOrEmpty(argument))
        {
            _builder.Append("  —  ").Append(argument);
        }

        SerializedProperty muted = step.FindPropertyRelative("_isMuted");
        if (muted != null && muted.boolValue)
        {
            _builder.Append("   [꺼짐]");
        }

        return _builder.ToString();
    }

    /// <summary>
    /// 타입 선택 드롭다운과 요약 줄에 함께 쓰는 이름.
    ///
    /// 기획자가 고르는 목록이므로 한글로 적는다. 이름을 등록하지 않은 새 스텝은
    /// Cutscene 접두사와 Step 접미사만 떼고 타입명을 그대로 쓴다.
    /// </summary>
    public static string GetDisplayName(string typeName)
    {
        switch (typeName)
        {
            case "CutsceneWaitStep": return "대기";
            case "CutsceneParallelGroupStep": return "동시 실행";
            case "CutsceneSequenceGroupStep": return "순차 실행";
            case "CutscenePlayerControlStep": return "플레이어 조작";
            case "CutsceneCameraMoveStep": return "카메라 이동";
            case "CutsceneCameraShakeStep": return "카메라 흔들기";
            case "CutsceneActorActiveStep": return "오브젝트 켜고 끄기";
            case "CutsceneActorSpriteStep": return "스프라이트 연출";
            case "CutsceneTerrainEnergyStep": return "지형 에너지";
            case "CutsceneEnergyDeviceStep": return "에너지 공급 장치";
            case "CutsceneWaitTerrainSettledStep": return "지형 정지 대기";
            case "CutsceneFadeStep": return "화면 페이드";
            case "CutsceneSfxStep": return "효과음";
            case "CutsceneBossCinematicStep": return "보스 컷씬";
        }

        if (typeName.StartsWith(NAME_PREFIX, StringComparison.Ordinal))
        {
            typeName = typeName.Substring(NAME_PREFIX.Length);
        }

        if (typeName.EndsWith(NAME_SUFFIX, StringComparison.Ordinal))
        {
            typeName = typeName.Substring(0, typeName.Length - NAME_SUFFIX.Length);
        }

        return ObjectNames.NicifyVariableName(typeName);
    }

    // "_steps.Array.data[2]" 의 마지막 대괄호에서 순서를 뽑는다.
    private static void AppendIndex(SerializedProperty step)
    {
        string path = step.propertyPath;
        int open = path.LastIndexOf('[');
        int close = path.LastIndexOf(']');
        if (open < 0 || close < open)
        {
            return;
        }

        if (int.TryParse(path.Substring(open + 1, close - open - 1), out int index))
        {
            _builder.Append((index + 1).ToString("00")).Append(" ▸ ");
        }
    }
}
