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
    }
}