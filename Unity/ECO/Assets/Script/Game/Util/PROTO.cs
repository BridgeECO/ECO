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

        public static void GenerateAllCsFile()
        {
            string csvFolderPath = PATH.GetProtoCsvFolderPath();
            string templatePath = PATH.GetProtoTemplatePath();

            foreach (var csvName in PATH.GetAllFileName(csvFolderPath, ".csv"))
            {
                string csvPath = PATH.GetProtoCsvPath(csvName);
                string csPath = PATH.GetProtoCsPath(csvName);


                try
                {
                    _prtTool.GenerateProto(csvName, csPath, csvPath, templatePath);
                    LOG.I($"GENERATE_CS_SUCCESS, PrtName({csvName})");
                }
                catch (ProtoException exc)
                {
                    LOG.I($"GENERATE_CS_FAILED, PrtName({csvName}), Message({exc.Message})");
                }
            }
        }
    }
}