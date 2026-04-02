using System.IO;
using UnityEditor;

public class SceneEnumGenerator : EnumGeneratorBase
{
    [MenuItem("ECO/Tools/Generate Enum/ESceneNames")]
    public static void GenerateSceneEnum()
    {
        Generate("ESceneNames.cs", "ESceneNames", (writer) =>
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                if (!scene.enabled) continue;

                string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                writer.WriteLine($"    {sceneName.Replace(" ", "_")},");
            }
        });
    }
}