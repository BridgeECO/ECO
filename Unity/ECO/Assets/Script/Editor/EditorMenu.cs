using UnityEditor;

namespace ECO.Editor
{
    public class EditorMenu
    {
        [MenuItem("ECO/Proto/Convert Excel To Csv")]
        public static void ConvertExcelToCsv()
        {
            PROTO.ConvertAllExcelToCsv();
        }

        [MenuItem("ECO/Proto/Generate Cs")]
        public static void GenerateCs()
        {
            PROTO.GenerateAllCsFile();
        }
    }
}