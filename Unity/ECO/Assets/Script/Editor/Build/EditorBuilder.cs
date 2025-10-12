using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using IOComp = System.IO.Compression;
using System.Collections.Generic;
using UnityEditor.Build;

namespace ECO
{
    public class EditorBuilder : EditorWindow
    {
        private enum EBuildMode { DEBUG, RELEASE }

        private EBuildMode _buildMode = EBuildMode.DEBUG;
        private string _customVersion = "";
        private bool _isAppendVerToFolder = true;
        private bool _isAppendDateToFolder = true; // yyMMdd-HHmmss
        private bool _isCreateZipArchive = true;

        private static readonly bool _isOpenFolderOnSuccess = true;
        private static bool _isBuilding = false;

        [MenuItem("ECO/Build/Editor Builder")]
        public static void ShowWindow()
        {
            var win = GetWindow<EditorBuilder>("Builder");
            win.minSize = new Vector2(460, 240);
        }

        private void OnGUI()
        {
            GUILayout.Label("Windows Build Settings", EditorStyles.boldLabel);
            _buildMode = (EBuildMode)EditorGUILayout.EnumPopup("Build Mode", _buildMode);

            GUILayout.Space(6);
            GUILayout.Label("Versioning", EditorStyles.boldLabel);
            _customVersion = EditorGUILayout.TextField("Custom Version", _customVersion);
            _isAppendVerToFolder = EditorGUILayout.ToggleLeft("Append version to folder name", _isAppendVerToFolder);
            _isAppendDateToFolder = EditorGUILayout.ToggleLeft("Append date (yyMMdd-HHmmss) to folder name", _isAppendDateToFolder);

            GUILayout.Space(6);
            GUILayout.Label("Archive", EditorStyles.boldLabel);
            _isCreateZipArchive = EditorGUILayout.ToggleLeft("Create .zip archive of the build folder", _isCreateZipArchive);

            GUILayout.Space(10);
            using (new EditorGUI.DisabledScope(_isBuilding))
            {
                var btnText = _isBuilding ? "Building..." : "Build EXE";
                if (GUILayout.Button(btnText))
                {
                    if (_isBuilding) return;
                    _isBuilding = true;

                    var mode = _buildMode;
                    var ver = _customVersion;
                    var isAppendVer = _isAppendVerToFolder;
                    var isAppendDate = _isAppendDateToFolder;
                    var isZip = _isCreateZipArchive;

                    EditorApplication.delayCall += () =>
                    {
                        try
                        {
                            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                            BuildForWindows(mode, ver, isAppendVer, isAppendDate, isZip);
                        }
                        finally
                        {
                            _isBuilding = false;
                            Repaint();
                        }
                    };

                    GUIUtility.ExitGUI();
                }
            }
        }

        private static void BuildForWindows(EBuildMode mode, string versionInput, bool isAppendVerToFolder, bool isAppendDateToFolder, bool isMakeZip)
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) throw new Exception("Build Settings에 활성화된 씬이 없습니다.");

            var group = BuildTargetGroup.Standalone;
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);

            var buildStartStamp = DateTime.Now.ToString("yyMMdd-HHmmss");

            string prevSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            var prevStripping = PlayerSettings.GetManagedStrippingLevel(namedTarget);
            bool prevStripEngine = PlayerSettings.stripEngineCode;
            string prevBundleVersion = PlayerSettings.bundleVersion;

            try
            {
                if (!string.IsNullOrWhiteSpace(versionInput))
                    PlayerSettings.bundleVersion = SanitizeVer(versionInput);
                string effectiveVersion = PlayerSettings.bundleVersion;

                string modeName = mode.ToString();
                string folderStem = PlayerSettings.productName;
                if (isAppendVerToFolder && !string.IsNullOrEmpty(effectiveVersion))
                    folderStem += "_v" + SanitizeForFile(effectiveVersion);
                if (isAppendDateToFolder)
                    folderStem += "_" + buildStartStamp;

                string runDir = Path.Combine("Builds", modeName, folderStem);
                string exeName = PlayerSettings.productName + ".exe";
                string fullPath = Path.Combine(runDir, exeName);
                Directory.CreateDirectory(runDir);

                string applySymbol = (mode == EBuildMode.DEBUG) ? "DEBUG_BUILD" : "RELEASE_BUILD";
                string removeSymbol = (mode == EBuildMode.DEBUG) ? "RELEASE_BUILD" : "DEBUG_BUILD";
                string mergedSymbols = MergeSymbols(prevSymbols, applySymbol, removeSymbol);
                PlayerSettings.SetScriptingDefineSymbols(namedTarget, mergedSymbols);

                PlayerSettings.stripEngineCode = (mode == EBuildMode.RELEASE);
                PlayerSettings.SetManagedStrippingLevel(
                    namedTarget,
                    (mode == EBuildMode.RELEASE) ? ManagedStrippingLevel.Medium : ManagedStrippingLevel.Disabled
                );

                BuildOptions opts = BuildOptions.CompressWithLz4;
                if (mode == EBuildMode.DEBUG) opts |= BuildOptions.Development | BuildOptions.AllowDebugging;

                var buildOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = fullPath,
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = group,
                    options = opts
                };

                var started = DateTime.Now;
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
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

                    if (isMakeZip)
                    {
                        string zipPath = Path.Combine(Path.GetDirectoryName(runDir) ?? "", Path.GetFileName(runDir) + ".zip");
                        CreateZipFromDir(runDir, zipPath);
                        var zipInfo = new FileInfo(zipPath);
                        LOG.Info("압축 생성: " + zipPath + "  크기: " + (zipInfo.Length / (1024f * 1024f)).ToString("0.0") + " MB");
                    }

                    if (_isOpenFolderOnSuccess) EditorUtility.RevealInFinder(runDir);
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
                PlayerSettings.SetScriptingDefineSymbols(namedTarget, prevSymbols);
                PlayerSettings.SetManagedStrippingLevel(namedTarget, prevStripping);
                PlayerSettings.stripEngineCode = prevStripEngine;
                PlayerSettings.bundleVersion = prevBundleVersion;
            }
        }

        private static string MergeSymbols(string previousSymbols, string addSymbol, string removeSymbol)
        {
            var set = new HashSet<string>(
                (previousSymbols ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
            );
            set.RemoveWhere(s => string.Equals(s, removeSymbol, StringComparison.OrdinalIgnoreCase));
            set.Add(addSymbol);
            return string.Join(";", set);
        }

        private static string SanitizeVer(string version) => (version ?? "").Trim();

        private static string SanitizeForFile(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            return new string(name.Where(c => !invalid.Contains(c)).ToArray());
        }

        private static void CreateZipFromDir(string sourceDir, string zipPath)
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
                        fileStream.CopyTo(entryStream);
                }
            }
        }
    }
}
