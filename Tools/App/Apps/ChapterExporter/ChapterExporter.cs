using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MongoDB.Bson.Serialization;
using OfficeOpenXml;
using ProtoBuf;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace ET
{
    public static partial class ExcelExporter
    {
        
        public static void ExportChapter()
        {
            Console.WriteLine("ChapterExporter 开始");
            Table table = null;
            foreach (string excelPath in ExportHelper.FindFile(excelDir))
            {
                string dir = Path.GetDirectoryName(excelPath);
                string relativePath = Path.GetRelativePath(excelDir, dir);
                string fileName = Path.GetFileName(excelPath);
                if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                {
                    continue;
                }

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string fileNameWithoutCS = fileNameWithoutExtension;
                string cs = "cs";
                if (fileNameWithoutExtension.Contains("@"))
                {
                    string[] ss = fileNameWithoutExtension.Split("@");
                    fileNameWithoutCS = ss[0];
                    cs = ss[1];
                }

                if (cs!="p")
                {
                    continue;
                }

                ExcelPackage p = GetPackage(Path.GetFullPath(excelPath));
                if (table == null)
                {
                    table = GetTable("Chapter");
                    ExportExcelClass(p,"Chapter",table);
                }
                ExportExcelChapter(p, fileNameWithoutCS,table,ConfigType.p, relativePath);
                ExportExcelProtobuf(ConfigType.p, typeof(ChapterCategory),typeof(Chapter),fileNameWithoutCS , relativePath);

                
            }
            Console.WriteLine("ChapterExporter 成功");
        }
        
        static void ExportExcelChapter(ExcelPackage p, string name,Table table, ConfigType configType, string relativeDir)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{\"list\":[");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }
                if(worksheet.Dimension==null||worksheet.Dimension.End==null) continue;
                Console.WriteLine("ExportExcelJson "+name);
                ExportSheetChapter(worksheet, name,table.HeadInfos,configType, sb);
            }

            sb.AppendLine("]}");

            string dir = string.Format(jsonDir, configType.ToString(), relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string jsonPath = Path.Combine(dir, $"{name}.txt");
            using FileStream txt = new FileStream(jsonPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }
        static void ExportSheetChapter(ExcelWorksheet worksheet, string name, 
        Dictionary<string, HeadInfo> classField, ConfigType configType, StringBuilder sb)
        {
            string configTypeStr = configType.ToString();
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                string prefix = worksheet.Cells[row, 2].Text.Trim();
                if (prefix.Contains("#"))
                {
                    continue;
                }

                if (worksheet.Cells[row, 3].Text.Trim() == "")
                {
                    continue;
                }

                sb.Append("{");
                sb.Append($"\"_t\":\"{name}\"");
                for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
                {
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!classField.ContainsKey(fieldName))
                    {
                        continue;
                    }

                    HeadInfo headInfo = classField[fieldName];

                    if (headInfo == null)
                    {
                        continue;
                    }

                    if (headInfo.FieldType == "json")
                    {
                        continue;
                    }

                    string fieldN = headInfo.FieldName;
                    if (fieldN == "Id")
                    {
                        fieldN = "_id";
                    }

                    sb.Append($",\"{fieldN}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
                }

                sb.Append("},\n");
            }
        }
        
        // 根据生成的类，把json转成protobuf
        private static void ExportExcelProtobuf(ConfigType configType, Type type,Type subType,string protoName, string relativeDir)
        {
            string dir = GetProtoDir(configType, relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Serializer.NonGeneric.PrepareSerializer(type);
            Serializer.NonGeneric.PrepareSerializer(subType);


            string json = File.ReadAllText(Path.Combine(string.Format(jsonDir, configType, relativeDir), $"{protoName}.txt"));
            object deserialize = JsonHelper.FromJson(type,json);

            string path = Path.Combine(dir, $"{protoName}Category.bytes");

            using FileStream file = File.Create(path);
            Serializer.Serialize(file, deserialize);
        }
    }
}