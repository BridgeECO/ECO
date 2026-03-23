using CsvHelper;
using CsvHelper.Configuration;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Scriban;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace ECO.Tool.Proto
{
    public class ProtoTool
    {
        public ProtoTool(IProtoEnumRegister enumRegister)
        {
            _enumRegister = enumRegister;
        }

        public ProtoTool(IProtoEnumRegister enumRegister, ProtoConfig config)
        {
            _enumRegister = enumRegister;
            _prtConfig = config;
        }

        private readonly IProtoEnumRegister _enumRegister = null;
        private readonly ProtoConfig _prtConfig = new ProtoConfig();

        public void ConvertExcelToCsv(string excelPath, string csvPath)
        {
            //Excel 열고 있을때도 동작하도록 수정
            string tempExcelPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");
            File.Copy(excelPath, tempExcelPath, true);

            using (FileStream file = new FileStream(tempExcelPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file, true);
                XSSFFormulaEvaluator formula = new XSSFFormulaEvaluator(workbook);

                using (StreamWriter writer = new StreamWriter(csvPath))
                {
                    List<int> commentColIdx = new List<int>();

                    for (int i = 0; i < workbook.NumberOfSheets; i++)
                    {
                        var sheet = workbook.GetSheetAt(i);
                        if (!sheet.SheetName.StartsWith(_prtConfig.ExportExcelSheetSign))
                            continue;

                        // 행 루프
                        for (int row = 0; row <= sheet.LastRowNum; row++)
                        {
                            List<string> rowValueList = new List<string>();
                            IRow currentRow = sheet.GetRow(row);

                            if (currentRow == null)
                                continue;

                            for (int col = 0; col < currentRow.LastCellNum; col++)
                            {
                                if (row != 0 && commentColIdx.Contains(col))
                                    continue;

                                ICell cell = currentRow.GetCell(col);
                                if (cell == null)
                                    continue;

                                CellValue cellValue = formula.Evaluate(cell);
                                string value;

                                if (cellValue.CellType == CellType.String)
                                    value = cellValue.StringValue;
                                else if (cellValue.CellType == CellType.Numeric)
                                    value = cellValue.NumberValue.ToString();
                                else
                                    value = cell.ToString();

                                //첫번째 열 비면 빈 Row로 간주
                                if (col == 0 && string.IsNullOrEmpty(value))
                                    break;

                                //첫행이 주석 형태이면 Skip
                                if (col == 0 && value.StartsWith(_prtConfig.CommentSign))
                                    break;

                                if (row == 0 && value.StartsWith(_prtConfig.CommentSign))
                                {
                                    commentColIdx.Add(col);
                                    continue;
                                }

                                rowValueList.Add(value);
                            }

                            if (rowValueList.Count <= 0)
                                continue;

                            // CSV 형식으로 행을 작성
                            string line = string.Join(",", rowValueList);
                            writer.WriteLine(line);
                        }
                    }
                }
            }

            File.Delete(tempExcelPath);
        }

        public void GenerateProto(string prtName, string protoFilePath, string csvFilePath, string templateFilePath)
        {
            string csvTxt = File.ReadAllText(csvFilePath);
            string templateTxt = File.ReadAllText(templateFilePath);

            Template template = Template.Parse(templateTxt);
            if (template.HasErrors)
            {
                throw new ProtoException($"INVALID_TEMPLATE. Message({template.Messages}), TemplatePath({templateFilePath}), TemplateTxt({templateTxt})");
            }

            DataTable dt = MakeDataTable(csvTxt);

            List<ProtoScheme> schemeList = ParseScehemeList(dt, false);
            string txt = template.Render(new { ProtoName = prtName, Propertys = schemeList }, memebr => memebr.Name);

            using (FileStream fileStream = new FileStream(protoFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    writer.WriteLine(txt);
                }
            }
        }

        private static DataTable MakeDataTable(string csvTxt)
        {
            byte[] byteArr = Encoding.UTF8.GetBytes(csvTxt);
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IncludePrivateMembers = true,
                AllowComments = true,
                HasHeaderRecord = false
            };

            using (StreamReader txtReadeer = new StreamReader(new MemoryStream(byteArr)))
            {
                using (CsvReader csvReader = new CsvReader(txtReadeer, configuration))
                {
                    using (CsvDataReader csvDataReader = new CsvDataReader(csvReader))
                    {
                        DataTable dt = new DataTable();
                        dt.Load(csvDataReader);

                        return dt;
                    }
                }
            }
        }


        private List<PRT> ParseDataTable<PRT>(DataTable dt) where PRT : IProto, new()
        {
            List<PRT> prtList = new List<PRT>();
            List<ProtoScheme> protoSchemeList = ParseScehemeList(dt);

            for (int i = 2; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];

                if (row.ItemArray.Length != protoSchemeList.Count)
                    throw new ProtoException($"NOT_MATCH_COLUMN_AND_SCHEME_CNT. RowNum({i + 1}) ColCnt({row.ItemArray.Length}), SchemeCnt({protoSchemeList.Count})");

                PRT prt = new PRT();
                PropertyInfo[] propertyInfoArr = prt.GetType().GetProperties(BindingFlags.Public);
                Dictionary<ProtoScheme, List<object>> arrProtoSchemeDict = new Dictionary<ProtoScheme, List<object>>();

                for (int j = 0; j < row.ItemArray.Length; j++)
                {
                    object value = row.ItemArray[j];
                    ProtoScheme scheme = protoSchemeList[j];
                    PropertyInfo propertyInfo = prt.GetType().GetProperty(scheme.Name);
                    if (propertyInfo == null)
                        throw new ProtoException($"NOT_FOUND_PROPERTY. ProtoScheme({scheme}), RowNum({i + 1}), Value({value})");

                    Type type = scheme.Type;

                    try
                    {
                        object newValue;

                        if (scheme.Type == typeof(Array))
                        {
                            newValue = ConvertValue(scheme.SubType, value);

                            if (arrProtoSchemeDict.TryGetValue(scheme, out List<object> list))
                                list.Add(newValue);
                            else
                                arrProtoSchemeDict.Add(scheme, new List<object>() { newValue });
                            continue;
                        }

                        newValue = ConvertValue(scheme.Type, value);

                        propertyInfo.SetValue(prt, newValue);
                    }
                    catch (Exception exc)
                    {
                        throw new ProtoException($"FAILED_SET_VALUE. ProtoScheme({scheme}), RowNum({i + 1}), Value({value}), Message({exc.Message})");
                    }
                }

                foreach (var pair in arrProtoSchemeDict)
                {
                    PropertyInfo propertyInfo = prt.GetType().GetProperty(pair.Key.Name);
                    if (propertyInfo == null)
                        throw new ProtoException($"NOT_FOUND_PROPERTY. ProtoScheme({pair.Key}), RowNum({i + 1}))");

                    Type myType = typeof(string);
                    var arrayObj = pair.Value.ToArray();
                    Array newTypeArr = Array.CreateInstance(pair.Key.SubType, arrayObj.Length);
                    Array.Copy(arrayObj, newTypeArr, arrayObj.Length);

                    propertyInfo.SetValue(prt, newTypeArr);
                }


                prtList.Add(prt);
            }

            return prtList;
        }

        private object ConvertValue(Type type, object oldValue)
        {
            object newValue;

            if (type.IsEnum)
            {
                newValue = Enum.Parse(type, oldValue.ToString());
            }
            else
            {
                newValue = System.Convert.ChangeType(oldValue, type);
            }

            return newValue;
        }

        private List<ProtoScheme> ParseScehemeList(DataTable dt, bool allowSameScheme = true)
        {
            if (dt.Rows.Count < 2)
            {
                throw new ProtoException($"NOT_ENOUGH_ROW_CNT. Count({dt.Rows.Count}), It Must Be Greater Than Equal 2");
            }

            DataRow headerRow = dt.Rows[0];
            DataRow typeRow = dt.Rows[1];

            int headerLength = headerRow.ItemArray.Length;
            int typeLength = typeRow.ItemArray.Length;

            if (headerLength != typeLength)
                throw new ProtoException($"NOT_MATCH_HEADER_AND_SCHEME_CNT, HeaderCnt({headerLength}), TypeCnt({typeLength})");

            if (headerLength <= 0)
                throw new ProtoException($"INVALID_HEADER_CNT");

            List<ProtoScheme> list = new List<ProtoScheme>();

            for (int i = 0; i < headerLength; i++)
            {
                string name = headerRow[i] as string;
                string typeStr = typeRow[i] as string;

                (Type, Type) typeTuple = ParseType(typeStr);

                if (typeTuple.Item1 == null)
                    throw new ProtoException($"INVALD_TYPE_STRING. TypeStr({typeStr}), ColNum({i + 1})");

                ProtoScheme scheme = new ProtoScheme(name, typeTuple.Item1, typeTuple.Item2);

                if (!allowSameScheme && list.Contains(scheme))
                    continue;

                list.Add(scheme);
            }

            return list;
        }

        private (Type mainType, Type subType) ParseType(string typeStr)
        {
            string lowerStr = typeStr.ToLower();

            Type mainType = null;
            Type subType = null;
            string[] splitArr = lowerStr.Split(":");

            if (splitArr.Length <= 0)
                return (null, null);

            string mainTypeStr = splitArr[0];

            //일단 배열 처리만
            if (mainTypeStr.StartsWith("arr"))
                mainType = typeof(Array);

            if (mainTypeStr.StartsWith("int"))
                mainType = typeof(int);

            if (mainTypeStr.StartsWith("str"))
                mainType = typeof(string);

            if (mainTypeStr.StartsWith("bool"))
                mainType = typeof(bool);

            if (mainTypeStr.StartsWith("float"))
                mainType = typeof(float);

            if (mainTypeStr.StartsWith("double"))
                mainType = typeof(double);

            if (mainTypeStr.StartsWith("enum"))
            {
                if (splitArr.Length != 2)
                    return (null, null);

                return (_enumRegister.ConvertStrToType(splitArr[1]), null);
            }

            if (splitArr.Length > 1)
            {
                string subTypeStr = splitArr[1];

                if (subTypeStr == "enum")
                    subTypeStr = string.Join(":", splitArr[1], splitArr[2]);

                subType = ParseType(subTypeStr).mainType;
            }

            return (mainType, subType);
        }

        private static Type ParseEnumType(string enumStr)
        {
            Type type = Type.GetType($"{enumStr}, Assembly-CSharp");
            return type;
        }
    }
}