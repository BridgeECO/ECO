using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/09. SFXs 폴더의 AudioClip 파일명을 스캔하여 ESfxClip.cs를 자동 생성하는 에디터 툴.
/// 메뉴: Tools > Sound > Generate ESfxClip Enum
///
/// 항목에는 항상 명시적 정수값을 붙인다. 값을 생략하면 선언 순서가 곧 값이 되어,
/// 클립이 하나만 추가돼도 그 뒤 항목이 전부 밀리며 이미 직렬화된 참조가 다른 클립을 가리킨다.
///
/// 클립이 사라져도 항목은 지우지 않고 경고만 남긴다. 지워버리면 그 항목을 쓰던 코드가
/// 컴파일되지 않고 직렬화된 값도 갈 곳을 잃는다. 정리하려면 참조를 먼저 없앤 뒤
/// 해당 줄을 손으로 지운다(번호는 nextId가 지켜주므로 재사용되지 않는다).
/// </summary>
public static class SfxEnumGenerator
{
    private const string SFX_FOLDER = "Assets/09. SFXs";
    private const string OUTPUT_PATH = "Assets/02. Scripts/02-09. SFXs/Enum/ESfxClip.cs";

    [MenuItem("Tools/Sound/Generate ESfxClip Enum")]
    public static void GenerateEnum()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { SFX_FOLDER });

        if (guids.Length == 0)
        {
            Debug.LogWarning($"[SfxEnumGenerator] '{SFX_FOLDER}' 폴더에 AudioClip이 없습니다. 음악 파일을 추가 후 다시 실행하세요.");
            return;
        }

        List<string> clipNames = CollectSortedClipNames(guids);
        string absolutePath = GetAbsoluteOutputPath();

        SfxClipIdAllocator allocator = new SfxClipIdAllocator();
        allocator.LoadFrom(ReadExistingSource(absolutePath));

        int previousNextId = allocator.NextId;
        Dictionary<string, int> assignedIds = AssignIds(clipNames, allocator);
        List<string> orphanNames = CollectOrphanNames(clipNames, allocator, assignedIds);
        List<string> memberNames = clipNames.Concat(orphanNames)
            .OrderBy(n => n, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

        WriteEnumFile(absolutePath, memberNames, assignedIds, orphanNames, allocator.NextId);

        AssetDatabase.Refresh();
        Debug.Log($"[SfxEnumGenerator] ESfxClip.cs 생성 완료. 클립 {clipNames.Count}개, 신규 부여 {allocator.NextId - previousNextId}개");
        WarnOrphanNames(orphanNames);

        ValidateSoundManagerArray(memberNames.Count);
    }

    /// <summary>
    /// 값은 부여돼 있으나 폴더에 클립이 없는 항목. 지우면 참조하던 코드가 깨지므로 그대로 유지한다.
    /// </summary>
    private static List<string> CollectOrphanNames(List<string> clipNames, SfxClipIdAllocator allocator,
        Dictionary<string, int> assignedIds)
    {
        HashSet<string> presentNames = new HashSet<string>(clipNames);
        List<string> orphanNames = new List<string>();

        foreach (string knownName in allocator.KnownNames)
        {
            if (presentNames.Contains(knownName))
            {
                continue;
            }

            orphanNames.Add(knownName);
            assignedIds[knownName] = allocator.GetOrAssign(knownName);
        }

        return orphanNames;
    }

    private static void WarnOrphanNames(List<string> orphanNames)
    {
        if (orphanNames.Count == 0)
        {
            return;
        }

        Debug.LogWarning($"[SfxEnumGenerator] 오디오 클립이 없는 항목 {orphanNames.Count}개를 그대로 두었습니다. " +
            $"클립을 복구하거나, 참조를 없앤 뒤 ESfxClip.cs에서 해당 줄을 직접 지우세요.\n" +
            string.Join(", ", orphanNames));
    }

    /// <summary>guids에서 AudioClip 파일명을 수집하고 알파벳 오름차순으로 정렬한다.</summary>
    private static List<string> CollectSortedClipNames(string[] guids)
    {
        return guids
            .Select(g => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)))
            .OrderBy(n => n, System.StringComparer.OrdinalIgnoreCase)
            .Distinct()
            .ToList();
    }

    // 정렬된 순서로 훑어야 신규 항목의 값 부여가 실행할 때마다 같아진다.
    private static Dictionary<string, int> AssignIds(List<string> clipNames, SfxClipIdAllocator allocator)
    {
        Dictionary<string, int> assignedIds = new Dictionary<string, int>(clipNames.Count);
        for (int i = 0; i < clipNames.Count; i++)
        {
            assignedIds[clipNames[i]] = allocator.GetOrAssign(clipNames[i]);
        }

        return assignedIds;
    }

    private static string ReadExistingSource(string absolutePath)
    {
        return File.Exists(absolutePath) ? File.ReadAllText(absolutePath) : string.Empty;
    }

    private static string GetAbsoluteOutputPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", OUTPUT_PATH));
    }

    private static void WriteEnumFile(string absolutePath, List<string> memberNames,
        Dictionary<string, int> assignedIds, List<string> orphanNames, int nextId)
    {
        HashSet<string> orphans = new HashSet<string>(orphanNames);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// 09. SFXs 폴더의 AudioClip 에셋명과 1:1 대응하는 SFX 식별자 Enum.");
        sb.AppendLine("/// ⚠️ 이 파일은 SfxEnumGenerator 에디터 툴이 자동 생성합니다.");
        sb.AppendLine("///    직접 수정하지 마세요. (Tools > Sound > Generate ESfxClip Enum)");
        sb.AppendLine("///");
        sb.AppendLine("/// 각 항목의 정수값은 프리팹·씬에 직렬화되므로 한 번 부여하면 바뀌지 않습니다.");
        sb.AppendLine("/// 아래 값은 다음에 부여할 번호이며, 삭제된 클립의 번호를 재사용하지 않기 위해 보존합니다.");
        sb.AppendLine($"/// {SfxClipIdAllocator.NEXT_ID_MARKER}{nextId}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public enum ESfxClip");
        sb.AppendLine("{");

        for (int i = 0; i < memberNames.Count; i++)
        {
            string name = memberNames[i];
            string note = orphans.Contains(name) ? "   // 오디오 클립 없음" : string.Empty;
            sb.AppendLine($"    {name} = {assignedIds[name]},{note}");
        }

        sb.AppendLine("}");

        File.WriteAllText(absolutePath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 자동 생성된 Enum 개수 정보를 로그에 남긴다.
    /// </summary>
    private static void ValidateSoundManagerArray(int enumCount)
    {
        Debug.Log($"[SfxEnumGenerator] ESfxClip 항목 수: {enumCount}개. SoundManager 인스펙터의 카테고리별 SFX Clips 리스트에 필요한 오디오 클립을 추가해 주세요.");
    }
}
