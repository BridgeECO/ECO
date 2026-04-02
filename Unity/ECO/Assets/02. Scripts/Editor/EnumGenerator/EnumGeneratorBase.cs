using System;
using System.IO;
using UnityEditor;

public abstract class EnumGeneratorBase
{
    protected const string DIRECTORY_PATH = "Assets/02. Scripts/02-01. Common/Enum";

    protected static void Generate(string fileName, string enumName, Action<StreamWriter> writeAction)
    {
        if (!Directory.Exists(DIRECTORY_PATH))
        {
            Directory.CreateDirectory(DIRECTORY_PATH);
        }

        string filePath = Path.Combine(DIRECTORY_PATH, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"public enum {enumName}");
            writer.WriteLine("{");

            writeAction?.Invoke(writer);

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }
}