using UnityEditor;

public class TagEnumGenerator : EnumGeneratorBase
{
    [MenuItem("ECO/Enum Generator/Generate Tag Enums(ETags)", priority = 3)]
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