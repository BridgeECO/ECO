using UnityEditor;
using UnityEngine;

public class LayerEnumGenerator : EnumGeneratorBase
{
    private const int MAX_LAYERS = 32;

    [MenuItem("Tools/Generate Enum/ELayers")]
    public static void GenerateLayerEnum()
    {
        Generate("ELayers.cs", "ELayers", (writer) =>
        {
            for (int i = 0; i < MAX_LAYERS; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    writer.WriteLine($"    {layerName.Replace(" ", "_")} = {i},");
                }
            }
        });
    }
}