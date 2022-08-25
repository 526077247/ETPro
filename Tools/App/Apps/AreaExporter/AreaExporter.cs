using System;
using System.IO;
using System.Text;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using MongoDB.Bson.Serialization;
using ProtoBuf;

namespace ET
{
    public static class AreaExporter
    {
        private const string excelDir = "../Excel";

        private const string protobuf = "../Config";
        public static void Export()
        {
            Console.WriteLine("AreaExporter 开始");
            var paths = ExportHelper.FindFile(excelDir);
            if (paths.Count <= 0)
            {
                Console.WriteLine("AreaExporter 文件未找到");
                return;
            }
            foreach (string excelPath in paths)
            {
                string fileName = Path.GetFileName(excelPath);
                if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                {
                    continue;
                }
                if(!excelPath.Contains("@m"))  continue;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string fileNameWithoutCS = fileNameWithoutExtension;
                if (fileNameWithoutExtension.Contains("@"))
                {
                    string[] ss = fileNameWithoutExtension.Split("@");
                    fileNameWithoutCS = ss[0];
                }
                using Stream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using ExcelPackage p = new ExcelPackage(stream);

                Dictionary<long, AreaConfig> mapInfo = new Dictionary<long, AreaConfig>();
                ExportExcelEnum(p, mapInfo);
                var list = mapInfo.Values.ToList();
                ExportExcelProtobuf(list,fileNameWithoutCS);
                Console.WriteLine(fileName);
            }
            Console.WriteLine("AreaExporter 成功");
        }
        
        static void ExportExcelEnum(ExcelPackage p,Dictionary<long, AreaConfig> map)
        {
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if(worksheet.Dimension==null||worksheet.Dimension.End==null) continue;
                ExportSheetEnum(worksheet,map);
            }
        }
        public static long CreateCellId(int x, int y)
        {
            return (long) ((ulong) x << 32) | (uint) y;
        }
        static void ExportSheetEnum(ExcelWorksheet worksheet, Dictionary<long, AreaConfig> sb)
        {
            int[] Row = new int[worksheet.Dimension.End.Column];
            int[] Col = new int[worksheet.Dimension.End.Row];
            for (int col = 1; col <= worksheet.Dimension.End.Column; ++col)
            {
                var area = worksheet.Cells[1, col].Text.Trim();
                if (area == "")
                {
                    continue;
                }
                
                Col[col-1] = int.Parse(area);
            }
            for (int row = 1; row <= worksheet.Dimension.End.Row; ++row)
            {
                var area = worksheet.Cells[row, 1].Text.Trim();
                if (area == "")
                {
                    continue;
                }
                Row[row-1] = int.Parse(area);
            }
            for (int col = 2; col <= worksheet.Dimension.End.Column; ++col)
            {
                for (int row = 2; row <= worksheet.Dimension.End.Row; ++row)
                {
                    var area = worksheet.Cells[row, col].Text.Trim();
                    if (area == "")
                    {
                        continue;
                    }

                    var sceneId = int.Parse(area);
                    int x = Col[col - 1];
                    int y = Row[row - 1];
                    var id = CreateCellId(x, y);
                    if (sb.TryGetValue(id, out var val))
                    {
                        if (val.SceneId != sceneId)
                        {
                            Log.Error("相同id sceneId不同 "+sceneId+"  "+val.SceneId);
                        }
                    }
                    else
                    {
                        sb.Add(id,new AreaConfig(){Id = id,SceneId = sceneId});
                    }
                }
            }
        }
        // 根据生成的类，转成protobuf
        private static void ExportExcelProtobuf(List<AreaConfig> areaConfigs, string protoName)
        {
            string dir = protobuf;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            string path = Path.Combine(dir, $"{protoName}Category.bytes");

            using FileStream file = File.Create(path);
            Serializer.Serialize(file, areaConfigs);
        }
    }
}