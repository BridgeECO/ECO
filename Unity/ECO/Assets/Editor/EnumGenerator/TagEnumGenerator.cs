using UnityEditor;

public class TagEnumGenerator : EnumGeneratorBase
{
    [MenuItem("Tools/Generate Enum/ETags")]
    public static void GenerateTagEnum()
    {
        Generate("ETags.cs", "ETags", (writer) =>
        {
            string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
            foreach (string tag in tags)
            {
                writer.WriteLine($"    {tag.Replace(" ", "_")},");
            }
        });
    }
}