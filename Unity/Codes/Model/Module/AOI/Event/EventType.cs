using System.Collections.Generic;
namespace ET
{
    namespace EventType
    {
        #region AOI

        public struct AOIRemoveUnit
        {
            public AOIUnitComponent Receive;
            public List<AOIUnitComponent> Units;
        }

        public struct AOIRegisterUnit
        {
            public AOIUnitComponent Receive;
            public List<AOIUnitComponent> Units;
        }

        public struct ChangeGrid
        {
            public AOIUnitComponent Unit;
            public AOICell NewCell;
            public AOICell OldCell;
        }
        #endregion
    }
}