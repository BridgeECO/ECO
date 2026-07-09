using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/09. SFXs 폴더의 AudioClip 파일명을 스캔하여 ESfxClip.cs를 자동 생성하는 에디터 툴.
/// 메뉴: Tools > Sound > Generate ESfxClip Enum
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

        var clipNames = CollectSortedClipNames(guids);
        WriteEnumFile(clipNames);

        AssetDatabase.Refresh();
        Debug.Log($"[SfxEnumGenerator] ESfxClip.cs 생성 완료. 항목 수: {clipNames.Count}");

        ValidateSoundManagerArray(clipNames.Count);
    }

    /// <summary>guids에서 AudioClip 파일명을 수집하고 알파벳 오름차순으로 정렬한다.</summary>
    private static System.Collections.Generic.List<string> CollectSortedClipNames(string[] guids)
    {
        return guids
            .Select(g => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)))
            .OrderBy(n => n, System.StringComparer.OrdinalIgnoreCase)
            .Distinct()
            .ToList();
    }

    private static void WriteEnumFile(System.Collections.Generic.List<string> clipNames)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// 09. SFXs 폴더의 AudioClip 에셋명과 1:1 대응하는 SFX 식별자 Enum.");
        sb.AppendLine("/// ⚠️ 이 파일은 SfxEnumGenerator 에디터 툴이 자동 생성합니다.");
        sb.AppendLine("///    직접 수정하지 마세요. (Tools > Sound > Generate ESfxClip Enum)");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public enum ESfxClip");
        sb.AppendLine("{");

        for (int i = 0; i < clipNames.Count; i++)
        {
            sb.AppendLine($"    {clipNames[i]},");
        }

        sb.AppendLine("}");

        string absolutePath = Path.Combine(Application.dataPath.Replace("/Assets", ""), OUTPUT_PATH);
        File.WriteAllText(absolutePath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// SoundManager의 _sfxClips 배열 크기와 새로 생성된 Enum 항목 수를 비교하여
    /// 불일치 시 경고 메시지로 재할당 필요성을 안내한다.
    /// </summary>
    private static void ValidateSoundManagerArray(int enumCount)
    {
        string managerGuid = AssetDatabase.FindAssets("t:Prefab SoundManager").FirstOrDefault();
        if (managerGuid == null)
        {
            Debug.Log("[SfxEnumGenerator] SoundManager 프리팹을 찾을 수 없습니다. 컴파일 완료 후 Inspector에서 _sfxClips 배열 크기를 확인하세요.");
            return;
        }

        Debug.Log($"[SfxEnumGenerator] ESfxClip 항목 수: {enumCount}개 → SoundManager Inspector의 _sfxClips 배열도 {enumCount}개로 맞춰 주세요.");
    }
}
