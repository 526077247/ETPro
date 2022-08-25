﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ET
{
    internal class OpcodeInfo
    {
        public string Name;
        public int Opcode;
    }

    public static class Proto2CS
    {
        public static void Export()
        {
            Console.WriteLine("Proto2CS 开始");
            InnerProto2CS.Proto2CS();
            Console.WriteLine("Proto2CS 成功");
        }
    }

    public static class InnerProto2CS
    {
        private const string protoPath = ".";
        private const string clientMessagePath = "../Unity/Codes/Model/Generate/Message/";
        private const string serverMessagePath = "../Server/Model/Generate/Message/";
        private static readonly char[] splitChars = { ' ', '\t' };
        private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();

        public static void Proto2CS()
        {
            msgOpcode.Clear();
            if(Directory.Exists(serverMessagePath))
                Directory.Delete(serverMessagePath, true);
            if(Directory.Exists(clientMessagePath))
                Directory.Delete(clientMessagePath, true);
            DirectoryInfo folder = new DirectoryInfo("../Proto/");
            int innercode = OpcodeRangeDefine.InnerMinOpcode;
            int outercode = OpcodeRangeDefine.OuterMinOpcode;
            int mongocode = OpcodeRangeDefine.MongoMinOpcode;
            foreach (FileInfo file in folder.GetFiles("*.proto"))
            {
                var name = Path.GetFileNameWithoutExtension(file.FullName);
                if (name.StartsWith("Outer"))
                {
                    var endcode = Proto2CS("ET", file.FullName, serverMessagePath, "OuterOpcode", outercode);
                    GenerateOpcode("ET", name + "Opcode", serverMessagePath,"OuterOpcode");
                    Proto2CS("ET", file.FullName, clientMessagePath, "OuterOpcode", outercode);
                    GenerateOpcode("ET", name + "Opcode", clientMessagePath,"OuterOpcode");
                    outercode = endcode;
                }
                else if (name.StartsWith("Inner"))
                {
                    var endcode = Proto2CS("ET", file.FullName, serverMessagePath, "InnerOpcode", innercode);
                    GenerateOpcode("ET", name + "Opcode", serverMessagePath,"InnerOpcode");
                    innercode = endcode;
                }
                else if (name.StartsWith("Mongo"))
                {
                    var endcode = Proto2CS("ET", file.FullName, serverMessagePath, "MongoOpcode", mongocode);
                    GenerateOpcode("ET", name + "Opcode", serverMessagePath,"MongoOpcode");
                    mongocode = endcode;
                }
            }
        }

        public static int Proto2CS(string ns, string protoName, string outputPath, string opcodeClassName, int startOpcode)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            msgOpcode.Clear();
            string proto = Path.Combine(protoPath, protoName);
            string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(proto) + ".cs");

            string s = File.ReadAllText(proto);

            StringBuilder sb = new StringBuilder();
            sb.Append("using ET;\n");
            sb.Append("using ProtoBuf;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");

            bool isMsgStart = false;
            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();

                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//ResponseType"))
                {
                    string responseType = line.Split(" ")[1].TrimEnd('\r', '\n');
                    sb.AppendLine($"\t[ResponseType(nameof({responseType}))]");
                    continue;
                }

                if (newline.StartsWith("//"))
                {
                    sb.Append($"{newline}\n");
                    continue;
                }

                if (newline.StartsWith("message"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

                    sb.Append($"\t[Message({opcodeClassName}.{msgName})]\n");
                    sb.Append($"\t[ProtoContract]\n");
                    sb.Append($"\tpublic partial class {msgName}: Object");
                    if (parentClass == "IActorMessage" || parentClass == "IActorRequest" || parentClass == "IActorResponse")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else if (parentClass != "")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else
                    {
                        sb.Append("\n");
                    }

                    continue;
                }

                if (isMsgStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");
                        continue;
                    }

                    if (newline == "}")
                    {
                        isMsgStart = false;
                        sb.Append("\t}\n\n");
                        continue;
                    }

                    if (newline.Trim().StartsWith("//"))
                    {
                        sb.AppendLine(newline);
                        continue;
                    }

                    if (newline.Trim() != "" && newline != "}")
                    {
                        if (newline.StartsWith("repeated"))
                        {
                            Repeated(sb, ns, newline);
                        }
                        else
                        {
                            Members(sb, newline, true);
                        }
                    }
                }
            }

            sb.Append("}\n");
            using FileStream txt = new FileStream(csPath, FileMode.Create, FileAccess.ReadWrite);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
            return startOpcode;
        }

        private static void GenerateOpcode(string ns, string outputFileName, string outputPath,string className)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static partial class {className}");
            sb.AppendLine("\t{");
            foreach (OpcodeInfo info in msgOpcode)
            {
                sb.AppendLine($"\t\t public const ushort {info.Name} = {info.Opcode};");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string csPath = Path.Combine(outputPath, outputFileName + ".cs");

            using FileStream txt = new FileStream(csPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }

        private static void Repeated(StringBuilder sb, string ns, string newline)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                string name = ss[2];
                int n = int.Parse(ss[4]);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }

        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                default:
                    typeCs = type;
                    break;
            }

            return typeCs;
        }

        private static void Members(StringBuilder sb, string newline, bool isRequired)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[0];
                string name = ss[1];
                int n = int.Parse(ss[3]);
                string typeCs = ConvertType(type);

                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic {typeCs} {name} {{ get; set; }}\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }
    }
}