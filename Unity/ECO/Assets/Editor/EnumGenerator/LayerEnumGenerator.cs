using UnityEditor;
using UnityEngine;
using System.IO;

public class LayerEnumGenerator : EnumGeneratorBase
{
    [MenuItem("Tools/Generate Enum/ELayers")]
    public static void GenerateLayerEnum()
    {
        Generate("ELayers.cs", "ELayers", (writer) =>
        {
            for (int i = 0; i < 32; i++)
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