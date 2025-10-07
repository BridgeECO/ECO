using ECO.Tool.Proto;

namespace ECO
{
    public static partial class PROTO
    {
        private static EnumReigster _enumRigster = new EnumReigster();
        private static ProtoTool _prtTool = new ProtoTool(_enumRigster);

        public static void ConvertAllExcelToCsv()
        {
            string excelFolderPath = PATH.GetProtoExcelFolderPath();

            foreach (var excelName in PATH.GetAllFileName(excelFolderPath, ".xlsx"))
            {
                if (excelName.StartsWith("~"))
                    continue;

                string excelPath = PATH.JoinPath(excelFolderPath, $"{excelName}.xlsx");
                string csvPath = PATH.GetProtoCsvPath(excelName);
                _prtTool.ConvertExcelToCsv(excelPath, csvPath);
                LOG.I($"CONVERT_EXCEL_TO_CSV_SUCCESS, ExcelPath({excelPath}), CsvPath({csvPath}");
            }
        }
    }
}