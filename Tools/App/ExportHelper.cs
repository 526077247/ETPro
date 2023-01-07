using System;
using System.IO;
using System.Text;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Runtime;
namespace ET
{
    public static class ExportHelper
    {
        public static List<string> FindFile(string dirPath) //参数dirPath为指定的目录
        {
            List<string> res = new List<string>();
            DirectoryInfo Dir = new DirectoryInfo(dirPath);
            try
            {
                foreach (DirectoryInfo d in Dir.GetDirectories()) //查找子目录
                {
                    res.AddRange(FindFile(dirPath + "/" + d.Name));
                }
                foreach (var f in Directory.GetFiles(dirPath)) //查找文件
                {
                    res.Add(f); 
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return res;
        }
    }
}