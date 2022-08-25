using System.Collections.Generic;
namespace ET
{
    public static class NumericType
    {
        private static Dictionary<string, int> __Map;
        public static Dictionary<string, int> Map
        {
            get
            {
                if (__Map == null)
                {
                    __Map = new Dictionary<string, int>();
                    __Map.Add("AOI",1000);
                    __Map.Add("AOIBase",1000 * 10 + 1);
                    __Map.Add("Lv",1001);
                    __Map.Add("LvBase",1001 * 10 + 1);
                    __Map.Add("Exp",1002);
                    __Map.Add("ExpBase",1002 * 10 + 1);
                    __Map.Add("Hp",1003);
                    __Map.Add("HpBase",1003 * 10 + 1);
                    __Map.Add("MaxHp",1004);
                    __Map.Add("MaxHpBase",1004 * 10 + 1);
                    __Map.Add("MaxHpAdd",1004 * 10 + 2);
                    __Map.Add("MaxHpPct",1004 * 10 + 3);
                    __Map.Add("MaxHpFinalAdd",1004 * 10 + 4);
                    __Map.Add("MaxHpFinalPct",1004 * 10 + 5);
                    __Map.Add("HpReUp",1005);
                    __Map.Add("HpReUpBase",1005 * 10 + 1);
                    __Map.Add("HpReUpAdd",1005 * 10 + 2);
                    __Map.Add("HpReUpPct",1005 * 10 + 3);
                    __Map.Add("HpReUpFinalAdd",1005 * 10 + 4);
                    __Map.Add("HpReUpFinalPct",1005 * 10 + 5);
                    __Map.Add("Mp",1006);
                    __Map.Add("MpBase",1006 * 10 + 1);
                    __Map.Add("MaxMp",1007);
                    __Map.Add("MaxMpBase",1007 * 10 + 1);
                    __Map.Add("MaxMpAdd",1007 * 10 + 2);
                    __Map.Add("MaxMpPct",1007 * 10 + 3);
                    __Map.Add("MaxMpFinalAdd",1007 * 10 + 4);
                    __Map.Add("MaxMpFinalPct",1007 * 10 + 5);
                    __Map.Add("MpReUp",1008);
                    __Map.Add("MpReUpBase",1008 * 10 + 1);
                    __Map.Add("MpReUpAdd",1008 * 10 + 2);
                    __Map.Add("MpReUpPct",1008 * 10 + 3);
                    __Map.Add("MpReUpFinalAdd",1008 * 10 + 4);
                    __Map.Add("MpReUpFinalPct",1008 * 10 + 5);
                    __Map.Add("Speed",1009);
                    __Map.Add("SpeedBase",1009 * 10 + 1);
                    __Map.Add("SpeedAdd",1009 * 10 + 2);
                    __Map.Add("SpeedPct",1009 * 10 + 3);
                    __Map.Add("SpeedFinalAdd",1009 * 10 + 4);
                    __Map.Add("SpeedFinalPct",1009 * 10 + 5);
                    __Map.Add("ATK",1010);
                    __Map.Add("ATKBase",1010 * 10 + 1);
                    __Map.Add("ATKAdd",1010 * 10 + 2);
                    __Map.Add("ATKPct",1010 * 10 + 3);
                    __Map.Add("ATKFinalAdd",1010 * 10 + 4);
                    __Map.Add("ATKFinalPct",1010 * 10 + 5);
                    __Map.Add("DEF",1011);
                    __Map.Add("DEFBase",1011 * 10 + 1);
                    __Map.Add("DEFAdd",1011 * 10 + 2);
                    __Map.Add("DEFPct",1011 * 10 + 3);
                    __Map.Add("DEFFinalAdd",1011 * 10 + 4);
                    __Map.Add("DEFFinalPct",1011 * 10 + 5);
                }
                return __Map;
            }
        }
		public const int Max = 10000;

		public const int AOI = 1000; //AOI
		public const int AOIBase = 1000 * 10 + 1;

		public const int Lv = 1001; //等级
		public const int LvBase = 1001 * 10 + 1;

		public const int Exp = 1002; //Exp
		public const int ExpBase = 1002 * 10 + 1;

		public const int Hp = 1003; //当前血量
		public const int HpBase = 1003 * 10 + 1;

		public const int MaxHp = 1004; //最大血量上限
		public const int MaxHpBase = 1004 * 10 + 1;
		public const int MaxHpAdd = 1004 * 10 + 2;
		public const int MaxHpPct = 1004 * 10 + 3;
		public const int MaxHpFinalAdd = 1004 * 10 + 4;
		public const int MaxHpFinalPct = 1004 * 10 + 5;

		public const int HpReUp = 1005; //血量回复
		public const int HpReUpBase = 1005 * 10 + 1;
		public const int HpReUpAdd = 1005 * 10 + 2;
		public const int HpReUpPct = 1005 * 10 + 3;
		public const int HpReUpFinalAdd = 1005 * 10 + 4;
		public const int HpReUpFinalPct = 1005 * 10 + 5;

		public const int Mp = 1006; //当前法力
		public const int MpBase = 1006 * 10 + 1;

		public const int MaxMp = 1007; //最大法力上限
		public const int MaxMpBase = 1007 * 10 + 1;
		public const int MaxMpAdd = 1007 * 10 + 2;
		public const int MaxMpPct = 1007 * 10 + 3;
		public const int MaxMpFinalAdd = 1007 * 10 + 4;
		public const int MaxMpFinalPct = 1007 * 10 + 5;

		public const int MpReUp = 1008; //法力回复
		public const int MpReUpBase = 1008 * 10 + 1;
		public const int MpReUpAdd = 1008 * 10 + 2;
		public const int MpReUpPct = 1008 * 10 + 3;
		public const int MpReUpFinalAdd = 1008 * 10 + 4;
		public const int MpReUpFinalPct = 1008 * 10 + 5;

		public const int Speed = 1009; //移动速度
		public const int SpeedBase = 1009 * 10 + 1;
		public const int SpeedAdd = 1009 * 10 + 2;
		public const int SpeedPct = 1009 * 10 + 3;
		public const int SpeedFinalAdd = 1009 * 10 + 4;
		public const int SpeedFinalPct = 1009 * 10 + 5;

		public const int ATK = 1010; //攻击力
		public const int ATKBase = 1010 * 10 + 1;
		public const int ATKAdd = 1010 * 10 + 2;
		public const int ATKPct = 1010 * 10 + 3;
		public const int ATKFinalAdd = 1010 * 10 + 4;
		public const int ATKFinalPct = 1010 * 10 + 5;

		public const int DEF = 1011; //防御力
		public const int DEFBase = 1011 * 10 + 1;
		public const int DEFAdd = 1011 * 10 + 2;
		public const int DEFPct = 1011 * 10 + 3;
		public const int DEFFinalAdd = 1011 * 10 + 4;
		public const int DEFFinalPct = 1011 * 10 + 5;
    }
}
