using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ECO.Tool.Proto
{
    public class ProtoTool
    {
        public ProtoTool(IProtoEnumRegister enumRegister)
        {
            _enumRegister = enumRegister;
        }

        private readonly IProtoEnumRegister _enumRegister = null;

        public List<PRT> ParseDataTable<PRT>(DataTable dt) where PRT : IProto, new()
        {
            List<PRT> prtList = new List<PRT>();
            List<ProtoScheme> protoSchemeList = ParseScehemeList<PRT>(dt);

            for (int i = 2; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];

                if (row.ItemArray.Length != protoSchemeList.Count)
                    throw new ProtoException<PRT>($"Not Match Column Count And SchemeCnt. RowNum({i + 1}) ColCnt({row.ItemArray.Length}), SchemeCnt({protoSchemeList.Count})");

                PRT prt = new PRT();
                PropertyInfo[] propertyInfoArr = prt.GetType().GetProperties(BindingFlags.Public);
                Dictionary<ProtoScheme, List<object>> arrProtoSchemeDict = new Dictionary<ProtoScheme, List<object>>();

                for (int j = 0; j < row.ItemArray.Length; j++)
                {
                    object value = row.ItemArray[j];
                    ProtoScheme scheme = protoSchemeList[j];
                    PropertyInfo propertyInfo = prt.GetType().GetProperty(scheme.Name);
                    if (propertyInfo == null)
                        throw new ProtoException<PRT>($"Not Found Property Falied. ProtoScheme({scheme}), RowNum({i + 1}), Value({value})");

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
                        throw new ProtoException<PRT>($"Set Value Falied. ProtoScheme({scheme}), RowNum({i + 1}), Value({value}), Message({exc.Message})");
                    }
                }

                foreach (var pair in arrProtoSchemeDict)
                {
                    PropertyInfo propertyInfo = prt.GetType().GetProperty(pair.Key.Name);
                    if (propertyInfo == null)
                        throw new ProtoException<PRT>($"Not Found Property Falied. ProtoScheme({pair.Key}), RowNum({i + 1}))");

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

        //public void GenerateProto(string protoName, string protoFilePath)
        //{
        //    string csvTxt = FILE.ReadFile(PATH.GetProtoCsvPath(protoName));
        //    string templateTxt = FILE.ReadFile(PATH.GetProtoTemplatePath());

        //    Template template = Template.Parse(templateTxt);
        //    if (template.HasErrors)
        //    {
        //        LOG.E($"GenerateProto Failed. ProtoName({protoName}), Message({template.Messages})");
        //        return;
        //    }

        //    DataTable dt = MakeDataTable(csvTxt);

        //    try
        //    {
        //        List<ProtoScheme> schemeList = ParseScehemeList<ProtoBase>(dt, false);
        //        string txt = template.Render(new { ProtoName = protoName, Propertys = schemeList }, memebr => memebr.Name);

        //        using (FileStream fileStream = new FileStream(protoFilePath, FileMode.OpenOrCreate, FileAccess.Write))
        //        {
        //            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
        //            {
        //                writer.WriteLine(txt);
        //                LOG.I($"Generate Proto Success. ProtoName({protoName})");
        //            }
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        LOG.E($"GenerateProto Failed. ProtoName({protoName}), Message({exc.Message})");
        //        return;
        //    }
        //}


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

        private List<ProtoScheme> ParseScehemeList<PRT>(DataTable dt, bool allowSameScheme = true) where PRT : IProto
        {
            if (dt.Rows.Count < 2)
            {
                throw new ProtoException<PRT>($"Not Enough Row Count({dt.Rows.Count}), It Must Be Greater Than Equal 2");
            }

            DataRow headerRow = dt.Rows[0];
            DataRow typeRow = dt.Rows[1];

            int headerLength = headerRow.ItemArray.Length;
            int typeLength = typeRow.ItemArray.Length;

            if (headerLength != typeLength)
                throw new ProtoException<PRT>($"Not Match Header and Scheme Length, HeaderCnt({headerLength}), TypeCnt({typeLength})");

            if (headerLength <= 0)
                throw new ProtoException<PRT>($"Invalid Header Count");

            List<ProtoScheme> list = new List<ProtoScheme>();

            for (int i = 0; i < headerLength; i++)
            {
                string name = headerRow[i] as string;
                string typeStr = typeRow[i] as string;

                (Type, Type) typeTuple = ParseType(typeStr);

                if (typeTuple.Item1 == null)
                    throw new ProtoException<PRT>($"Invalid TypeString({typeStr}), ColNum({i + 1})");

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