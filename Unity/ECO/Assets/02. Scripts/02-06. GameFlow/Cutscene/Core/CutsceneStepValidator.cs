#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 스텝별 참조 누락을 잡는다. 실행해 봐야 드러나고 원인이 인스펙터 안에 숨는 종류의 실패라
/// 자동 검출 가치가 높다.
///
/// 검사는 SerializedProperty로 읽는다. 스텝마다 검사용 공개 프로퍼티를 늘리면
/// 런타임 클래스가 에디터 사정으로 부푼다.
///
/// OnValidate가 부르므로 이 파일은 런타임 폴더에 있어야 한다. 그래서 Editor 폴더의
/// 요약·타입명 헬퍼를 쓸 수 없고 필요한 만큼만 여기서 직접 자른다.
/// </summary>
public static class CutsceneStepValidator
{
    public static void Validate(CutsceneDirector director)
    {
        if (director == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(director);
        ValidateList(director, serializedObject.FindProperty("_steps"), 1);
        serializedObject.Dispose();
    }

    private static void ValidateList(CutsceneDirector director, SerializedProperty steps, int depth)
    {
        if (steps == null || !steps.isArray || CutsceneGroupStepBase.MAX_NEST_DEPTH < depth)
        {
            return;
        }

        for (int i = 0; i < steps.arraySize; i++)
        {
            SerializedProperty step = steps.GetArrayElementAtIndex(i);
            string typeName = GetShortTypeName(step);

            if (typeName == null)
            {
                Report(director, i, "빈 항목", "타입을 고르지 않았습니다.");
                continue;
            }

            ValidateStep(director, step, i, typeName);
            ValidateList(director, step.FindPropertyRelative("_steps"), depth + 1);
        }
    }

    private static void ValidateStep(CutsceneDirector director, SerializedProperty step, int index, string typeName)
    {
        switch (typeName)
        {
            case "CutsceneActorSpriteStep":
                RequireObject(director, step, index, typeName, "_spriteRenderer");
                RequireList(director, step, index, typeName, "_sprites");
                break;

            case "CutsceneActorActiveStep":
            case "CutsceneTerrainEnergyStep":
            case "CutsceneWaitTerrainSettledStep":
                RequireList(director, step, index, typeName, "_targets");
                break;

            case "CutsceneEnergyDeviceStep":
                RequireList(director, step, index, typeName, "_devices");
                break;

            case "CutsceneBossCinematicStep":
                ValidateBossCinematic(director, step, index, typeName);
                break;
        }
    }

    /// <summary>
    /// 기존 보스 컷씬은 Start에서 카메라를 잡는다. 대상이 비활성이면 Start가 돌지 않아
    /// PlayCinematicAsync가 조용히 즉시 반환하고, 컷씬이 아무 일도 없이 지나간다.
    /// </summary>
    private static void ValidateBossCinematic(CutsceneDirector director, SerializedProperty step, int index,
        string typeName)
    {
        if (!RequireObject(director, step, index, typeName, "_cinematic"))
        {
            return;
        }

        Object value = step.FindPropertyRelative("_cinematic").objectReferenceValue;
        if (value is Component component && !component.gameObject.activeInHierarchy)
        {
            Report(director, index, typeName,
                $"보스 컷씬 '{component.name}'이(가) 비활성이라 재생해도 아무 일도 일어나지 않습니다.");
        }
    }

    private static bool RequireObject(CutsceneDirector director, SerializedProperty step, int index,
        string typeName, string relativePath)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        if (property != null && property.objectReferenceValue != null)
        {
            return true;
        }

        Report(director, index, typeName, $"'{ObjectNames.NicifyVariableName(relativePath)}'이(가) 비어 있습니다.");
        return false;
    }

    private static void RequireList(CutsceneDirector director, SerializedProperty step, int index,
        string typeName, string relativePath)
    {
        SerializedProperty property = step.FindPropertyRelative(relativePath);
        string label = ObjectNames.NicifyVariableName(relativePath);

        if (property == null || property.arraySize == 0)
        {
            Report(director, index, typeName, $"'{label}' 목록이 비어 있습니다.");
            return;
        }

        for (int i = 0; i < property.arraySize; i++)
        {
            if (property.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                Report(director, index, typeName, $"'{label}' 목록의 {i + 1}번이 비어 있습니다.");
                return;
            }
        }
    }

    // "<어셈블리> <네임스페이스>.<타입>" 형식이라 공백을 먼저 떼고 점을 뗀다.
    private static string GetShortTypeName(SerializedProperty step)
    {
        string fullTypename = step.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(fullTypename))
        {
            return null;
        }

        int assemblySplit = fullTypename.LastIndexOf(' ');
        string typeName = assemblySplit < 0 ? fullTypename : fullTypename.Substring(assemblySplit + 1);
        int namespaceSplit = typeName.LastIndexOf('.');
        return 0 <= namespaceSplit ? typeName.Substring(namespaceSplit + 1) : typeName;
    }

    private static void Report(CutsceneDirector director, int index, string typeName, string message)
    {
        Debug.LogError($"[CutsceneDirector] '{director.name}' {index + 1}번({typeName}) — {message}", director);
    }
}
#endif
