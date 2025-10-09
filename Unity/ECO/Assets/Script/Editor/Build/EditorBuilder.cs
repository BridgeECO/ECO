#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using IOComp = System.IO.Compression;
using System.Collections.Generic;
using ECO;

public class EditorBuilder : EditorWindow
{
    // 빌드 모드 정의
    private enum BuildMode { Debug, Release }
    private BuildMode buildMode = BuildMode.Debug;

    // 버전 커스텀 입력값
    private string customVersion = "";

    // 폴더명에 버전과 날짜를 포함할지 여부
    private bool appendVersionToFolder = true;
    private bool appendDateStampToFolder = true; // yyMMdd 형식

    // 빌드 산출물 ZIP 생성 옵션
    private bool createZipArchive = true;

    // 빌드 성공 시 탐색기 열기
    private static readonly bool OpenFolderOnSuccess = true;

    [MenuItem("ECO/Build/Editor Builder")]
    public static void ShowWindow()
    {
        var win = GetWindow<EditorBuilder>("Builder");
        win.minSize = new Vector2(460, 220);
    }

    private void OnGUI()
    {
        // 기본 빌드 설정 UI
        GUILayout.Label("Windows Build Settings", EditorStyles.boldLabel);
        buildMode = (BuildMode)EditorGUILayout.EnumPopup("Build Mode", buildMode);

        // 버전 관리 UI
        GUILayout.Space(6);
        GUILayout.Label("Versioning", EditorStyles.boldLabel);
        customVersion = EditorGUILayout.TextField("Custom Version", customVersion);
        appendVersionToFolder = EditorGUILayout.ToggleLeft("Append version to folder name", appendVersionToFolder);
        appendDateStampToFolder = EditorGUILayout.ToggleLeft("Append date (yyMMdd) to folder name", appendDateStampToFolder);

        // ZIP 옵션 UI
        GUILayout.Space(6);
        GUILayout.Label("Archive", EditorStyles.boldLabel);
        createZipArchive = EditorGUILayout.ToggleLeft("Create .zip archive of the build folder", createZipArchive);

        // 실행 버튼
        GUILayout.Space(10);
        if (GUILayout.Button("Build exe"))
        {
            // 안전을 위해 빌드 타겟 전환
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

            // 빌드 실행
            BuildForWindows(buildMode, customVersion, appendVersionToFolder, appendDateStampToFolder, createZipArchive);
        }
    }

    // CLI 진입점
    public static void BuildCLI()
    {
        var args = Environment.GetCommandLineArgs();
        string modeStr = GetArg(args, "buildMode", "Release");
        string version = GetArg(args, "version", "");
        bool appendVer = GetArgBool(args, "appendVersionToFolder", true);
        bool appendDate = GetArgBool(args, "appendDateStampToFolder", true);
        bool zip = GetArgBool(args, "zip", true);

        var mode = string.Equals(modeStr, "Debug", StringComparison.OrdinalIgnoreCase) ? BuildMode.Debug : BuildMode.Release;

        // 안전을 위해 빌드 타겟 전환
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        // 빌드 실행
        BuildForWindows(mode, version, appendVer, appendDate, zip);
    }

    // CLI 인자 파싱 유틸
    private static string GetArg(string[] args, string key, string def)
    {
        string prefix = key + "=";
        foreach (var a in args)
            if (a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return a.Substring(prefix.Length);
        return def;
    }

    private static bool GetArgBool(string[] args, string key, bool def)
    {
        var s = GetArg(args, key, def.ToString());
        return bool.TryParse(s, out var b) ? b : def;
    }

    // 실제 빌드 로직
    private static void BuildForWindows(BuildMode mode, string versionInput, bool appendVerToFolder, bool appendDateToFolder, bool makeZip)
    {
        // 빌드에 포함된 활성화된 씬 수집
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) throw new Exception("Build Settings에 활성화된 씬이 없습니다.");

        var group = BuildTargetGroup.Standalone;

        // PlayerSettings 관련 기존값 백업
        string prevSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        var prevStripping = PlayerSettings.GetManagedStrippingLevel(group);
        bool prevStripEngine = PlayerSettings.stripEngineCode;
        string prevBundleVersion = PlayerSettings.bundleVersion;

        try
        {
            // 커스텀 버전이 입력된 경우에만 bundleVersion 덮어쓰기
            if (!string.IsNullOrWhiteSpace(versionInput))
                PlayerSettings.bundleVersion = SanitizeVersion(versionInput);
            string effectiveVersion = PlayerSettings.bundleVersion;

            // 폴더명 구성
            string modeName = mode.ToString();
            string folderStem = PlayerSettings.productName;
            if (appendVerToFolder && !string.IsNullOrEmpty(effectiveVersion))
                folderStem += "_v" + SanitizeForFile(effectiveVersion);
            if (appendDateToFolder)
                folderStem += "_" + DateTime.Now.ToString("yyMMdd"); // 예시: 251009

            // 최종 폴더 및 exe 경로
            string runDir = Path.Combine("Builds", modeName, folderStem);
            string exeName = PlayerSettings.productName + ".exe";
            string fullPath = Path.Combine(runDir, exeName);
            Directory.CreateDirectory(runDir);

            // 심볼 관리
            string applySymbol = (mode == BuildMode.Debug) ? "DEBUG_BUILD" : "RELEASE_BUILD";
            string removeSymbol = (mode == BuildMode.Debug) ? "RELEASE_BUILD" : "DEBUG_BUILD";
            string mergedSymbols = MergeSymbols(prevSymbols, applySymbol, removeSymbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, mergedSymbols);

            // 스트리핑 설정
            PlayerSettings.stripEngineCode = (mode == BuildMode.Release);
            PlayerSettings.SetManagedStrippingLevel(group, (mode == BuildMode.Release) ? ManagedStrippingLevel.Medium : ManagedStrippingLevel.Disabled);

            // 빌드 옵션 구성
            BuildOptions opts = BuildOptions.CompressWithLz4;
            if (mode == BuildMode.Debug) opts |= BuildOptions.Development | BuildOptions.AllowDebugging;

            // 빌드 실행
            var bpo = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = fullPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = group,
                options = opts
            };

            var started = DateTime.Now;
            BuildReport report = BuildPipeline.BuildPlayer(bpo);
            var summary = report.summary;
            var elapsed = DateTime.Now - started;

            if (summary.result == BuildResult.Succeeded)
            {
                LOG.Info(
                    mode + " 빌드 성공\n" +
                    "버전: " + PlayerSettings.bundleVersion + "\n" +
                    "출력: " + fullPath + "\n" +
                    "크기: " + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB  소요: " + elapsed.ToString("mm':'ss") + "\n" +
                    "경고: " + summary.totalWarnings + ", 오류: " + summary.totalErrors
                );

                // ZIP 생성 옵션 처리
                if (makeZip)
                {
                    string zipPath = Path.Combine(Path.GetDirectoryName(runDir) ?? "", Path.GetFileName(runDir) + ".zip");
                    CreateZipFromDirectory(runDir, zipPath);
                    var zipInfo = new FileInfo(zipPath);
                    LOG.Info("압축 생성: " + zipPath + "  크기: " + (zipInfo.Length / (1024f * 1024f)).ToString("0.0") + " MB");
                }

                // 탐색기 열기
                if (OpenFolderOnSuccess) EditorUtility.RevealInFinder(runDir);
            }
            else
            {
                LOG.Error(
                    "빌드 실패 (" + summary.result + ")\n" +
                    "경고: " + summary.totalWarnings + ", 오류: " + summary.totalErrors
                );
                throw new Exception("Build failed: " + summary.result);
            }
        }
        catch (Exception ex)
        {
            LOG.Error("빌드 중 오류: " + ex.Message + "\n" + ex.StackTrace);
            throw;
        }
        finally
        {
            // PlayerSettings 원복
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, prevSymbols);
            PlayerSettings.SetManagedStrippingLevel(group, prevStripping);
            PlayerSettings.stripEngineCode = prevStripEngine;
            PlayerSettings.bundleVersion = prevBundleVersion;
        }
    }

    // 기존 심볼 보존, 현재 모드 심볼 추가, 반대 모드 심볼 제거
    private static string MergeSymbols(string prev, string add, string remove)
    {
        HashSet<string> set = new HashSet<string>(
            (prev ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())
        );
        set.RemoveWhere(s => string.Equals(s, remove, StringComparison.OrdinalIgnoreCase));
        set.Add(add);
        return string.Join(";", set);
    }

    // 버전 문자열 정리
    private static string SanitizeVersion(string v) => (v ?? "").Trim();

    // 파일명에 허용되지 않는 문자 제거
    private static string SanitizeForFile(string s)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return new string(s.Where(c => !invalid.Contains(c)).ToArray());
    }

    // 디렉터리를 ZIP으로 압축
    private static void CreateZipFromDirectory(string sourceDir, string zipPath)
    {
        if (File.Exists(zipPath)) File.Delete(zipPath);

        using (var fs = new FileStream(zipPath, FileMode.CreateNew))
        using (var archive = new IOComp.ZipArchive(fs, IOComp.ZipArchiveMode.Create))
        {
            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string entryName = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var entry = archive.CreateEntry(entryName, IOComp.CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                using (var fileStream = File.OpenRead(file))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}
#endif
