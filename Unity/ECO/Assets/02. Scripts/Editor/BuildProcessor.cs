using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildProcessor
{
    private const string ArgName_OutputPath = "outputPath";
    private const string ArgName_BuildVersion = "buildVersion";
    private const string ArgName_EnableDev = "enableDev";
    private const string ArgName_EnableDeepProfiling = "enableDeepProfiling";
    private const string ArgName_OutputFileName = "outputFileName";

    private static string GetCommandLineArgument(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == $"-{name}" && i + 1 < args.Length)
                return args[i + 1];
        }
        return null;
    }

    public static void BuildPC()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        var outputPath = GetCommandLineArgument(ArgName_OutputPath);
        var version = GetCommandLineArgument(ArgName_BuildVersion);
        var enableDev = GetCommandLineArgument(ArgName_EnableDev) == "true";
        var enableDeepProfiling = GetCommandLineArgument(ArgName_EnableDeepProfiling) == "true";
        var outputFileName = GetCommandLineArgument(ArgName_OutputFileName);

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = FindEnabledEditorScenes(),
            locationPathName = $"{outputPath}/{outputFileName}.exe",
            target = BuildTarget.StandaloneWindows64
        };

        EditorUserBuildSettings.development = enableDev;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = enableDeepProfiling;

        PlayerSettings.bundleVersion = version;

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        switch (report.summary.result)
        {
            case BuildResult.Succeeded:
            case BuildResult.Failed:
            case BuildResult.Unknown:
            case BuildResult.Cancelled:
                Debug.Log($"ºôµå °á°ú : {report.summary.result}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
#endif
    }

    private static string[] FindEnabledEditorScenes()
    {
        var editorScenes = new List<string>();

        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            editorScenes.Add(scene.path);
        }

        return editorScenes.ToArray();
    }
}