using System.IO;
using System.Linq;
using UnityEngine;

namespace ECO
{
    public class PATH
    {
        public static string GetProtoCsvAddressableKey(string csvName) => $"Assets/Data/Proto/{csvName}.csv";


        public static string GetProtoCsvFolderPath() => JoinPath(GetAssetPath(true), $"Data/Csv");
        public static string GetProtoCsvPath(string csvName) => JoinPath(GetProtoCsvFolderPath(), $"{csvName}.csv");
        public static string GetProtoExcelFolderPath() => JoinPath(GetAssetPath(true), $"..\\..\\..\\Excel\\Proto");
        public static string GetProtoCsPath(string prtName) => JoinPath(GetAssetPath(true), $"Script\\Game\\Core\\Proto\\{prtName}.cs");
        public static string GetProtoTemplatePath() => JoinPath(GetAssetPath(true), $"..\\..\\..\\Excel\\Template\\ProtoTemplate.txt");

        public static string[] GetAllFileName(string folderPath, string ext)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            if (!dirInfo.Exists)
                return new string[0];

            return dirInfo.GetFiles().Where(x => x.Extension == ext).Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToArray();
        }

        public static string JoinPath(string path1, string path2) => Path.Join(path1, path2);
        private static string GetAssetPath(bool isFullPath = false)
        {
            if (isFullPath)
                return Path.GetFullPath(Application.dataPath);

            return Application.dataPath;
        }
    }
}