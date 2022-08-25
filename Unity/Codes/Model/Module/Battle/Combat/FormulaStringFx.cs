﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ET;

/// 公式解析类，将公式的基础单元与运算符分割出来//
public class FormulaStringFx
{
    public static FormulaStringFx GetInstance(string formatString)
    {
        if (!Instances.ContainsKey(formatString))
        {
            Instances[formatString] = new FormulaStringFx(formatString);
        }

        return Instances[formatString];
    }
   
    private static Dictionary<string, FormulaStringFx> Instances = new Dictionary<string, FormulaStringFx>();
    public class FormulaNode
    {
        public int Key;
        public string Value;
        public bool IsSelf;
        public FormulaNode(int Key, string Value,bool IsSelf = true)
        {
            this.Key = Key;
            this.Value = Value;
            this.IsSelf = IsSelf;
        }
    }

    /// 获得字符类型//
    protected int GetPriority(char c)
    {
        if (c == '+' || c == '-') return 1;
        if (c == '*' || c == '/') return 2;
        if (c == '^') return 3;
        if (c == '(' || c == ')') return 4;
        return 0;
    }
    protected int GetPriority(string c)
    {
        if (c == "+" || c == "-") return 1;
        if (c == "*" || c == "/") return 2;
        if (c == "^") return 3;
        if (c == "(" || c == ")") return 4;
        return 0;
    }
    protected const int priorityMax = 4;

    protected List<FormulaNode> formulaNodeList = new List<FormulaNode>();

    protected string lastFormatString = "";

    /// 解析字符串获得公式结构//
    private FormulaStringFx(string formatString)
    {
        if (formatString == null)
            formatString = "";
        lastFormatString = (string)formatString.Clone();
        formatString = formatString.Replace(" ", "").Replace("\n", "").Replace("\t", "");

        if (formatString.Length > 0)
        {
            formatString = formatString.Replace("(-", "(0-");
            formatString = formatString.Replace("(+", "(0+");
            if (formatString[0] == '-' || formatString[0] == '+')
                formatString = "0" + formatString;
        }

        //第一层 用来分类//
        int laststate = -1;
        string temp = "";
        for (int i = 0; i < formatString.Length; i++)
        {
            char c = formatString[i];
            if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '.' ||
                c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '(' || c == ')' || c == '_'|| c == '$')
            {
            }
            else
                continue;
            int state = GetPriority(c);

            if (state == 0)
            {//数值//
                temp += c;
            }
            else
            {
                if (laststate == 0)
                {
                    AddNode(laststate, temp);
                    temp = "";
                }
                temp += c;
                AddNode(state, temp);
                temp = "";
            }

            laststate = state;
        }

        if (!string.IsNullOrEmpty(temp))
        {
            AddNode(laststate, temp);
        }
            
        
        
        List<FormulaNode> templist = new List<FormulaNode>();

        int operadd = 0;
        for (int i = 0; i < formulaNodeList.Count; i++)
        {
            if (formulaNodeList[i].Value == "(")
            {
                operadd += priorityMax;
            }
            else if (formulaNodeList[i].Value == ")")
            {
                operadd -= priorityMax;
            }
            formulaNodeList[i].Key += operadd;
            if (formulaNodeList[i].Value != "(" && formulaNodeList[i].Value != ")")
                templist.Add(formulaNodeList[i]);
        }
        formulaNodeList = templist;
        if(Define.Debug)
            Print();
    }

    public void AddNode(int Key, string Value)
    {
        if (Value.StartsWith("$"))
        {
            Value = Value.Substring(1, Value.Length - 1);
            formulaNodeList.Add(new FormulaNode(Key, Value,false));
            return;
        }
        formulaNodeList.Add(new FormulaNode(Key, Value));
    }
    public void Print()
    {
        string outstring = "";
        for (int i = 0; i < formulaNodeList.Count; i++)
        {
            outstring += string.Format("[{0}-({1}):({2}) ]",formulaNodeList[i].IsSelf?"Slef":"Other", formulaNodeList[i].Key, formulaNodeList[i].Value);
        }

        Log.Info(outstring);

    }

    /// 计算结果,传入的Dictionary中必须包含所有信息//
    public float GetData(NumericComponent comp,NumericComponent other)
    {
        ListComponent<int> priorityList = ListComponent<int>.Create();

        ListComponent<FormulaNode> tempList = ListComponent<FormulaNode>.Create();

        for (int i = 0; i < formulaNodeList.Count; i++)
        {
            FormulaNode node = new FormulaNode(formulaNodeList[i].Key, formulaNodeList[i].Value,formulaNodeList[i].IsSelf);
            tempList.Add(node);
        }

        
        for (int i = 0; i < tempList.Count; i++)
        {
            if(!float.TryParse(tempList[i].Value,out _))
            {
                if (NumericType.Map.TryGetValue(tempList[i].Value,out int type))
                {
                    if(tempList[i].IsSelf)
                        tempList[i].Value = comp.GetAsFloat(type).ToString();
                    else if(other!=null)
                        tempList[i].Value = other.GetAsFloat(type).ToString();
                    else
                    {
                        tempList[i].Value = "0";
                        Log.Error("计算伤害未传入目标");
                    }
                }
                
            }

            if (tempList[i].Key != 0 && !priorityList.Contains(tempList[i].Key))
            {
                priorityList.Add(tempList[i].Key);
            }
        }
        priorityList.Sort();
    
        while (priorityList.Count > 0)
        {
            int currentpri = priorityList[priorityList.Count - 1];
            bool hasfind = false;
            do
            {
                hasfind = false;
                for (int i = 0; i < tempList.Count && tempList.Count >= 3; i++)
                {
                    if (tempList[i].Key == currentpri && GetPriority(tempList[i].Value) != 0)
                    {
                        float final = GetOperactorFinal(tempList[i - 1].Value, tempList[i].Value, tempList[i + 1].Value);
                        var newformula = new FormulaNode(tempList[i - 1].Key, final.ToString());
                        tempList.RemoveRange(i - 1, 3);
                        tempList.Insert(i - 1, newformula);
                        hasfind = true;
                        break;
                    }
                }
            } while (hasfind);

            priorityList.RemoveAt(priorityList.Count - 1);
            priorityList.Sort();
        }
        
        if (tempList.Count == 0)
        {
#if UNITY_EDITOR
            //			UnityEngine.Debug.LogError("FormulaString is Error string = "+ lastFormatString);
#endif
            return 0;
        }
        string outstring = tempList[0].Value;// + formulaNodeList.Count.ToString();
        float finalnum = 0;
        if (!float.TryParse(outstring, out finalnum))
        {
#if UNITY_EDITOR
            //			UnityEngine.Debug.Log(outstring);
#endif
            finalnum = 0;
        }
        priorityList.Dispose();
        tempList.Dispose();
        return finalnum;
    }
    /// s1 ope s2格式的计算式 例如 5 + 6
    protected float GetOperactorFinal(string s1, string ope, string s2)
    {

        float num1 = 0;
        float num2 = 0;
        if (!float.TryParse(s1, out num1) || !float.TryParse(s2, out num2))
            return 0;
        if (ope == "+")
        {
            return num1 + num2;
        }
        if (ope == "-")
        {
            return num1 - num2;
        }
        if (ope == "*")
        {
            return num1 * num2;
        }
        if (ope == "/")
        {
            if (num2 == 0)
                return 0;
            return num1 / num2;
        }
        if (ope == "^")
        {
            return (float)Math.Pow(num1, num2);
        }
        return 0;
    }
}


