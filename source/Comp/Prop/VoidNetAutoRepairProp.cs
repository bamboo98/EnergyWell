using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    class VoidNetAutoRepairProp:CompProperties
    {
        public VoidNetAutoRepairProp()
        {
            compClass = typeof(VoidNetAutoRepair);
        }
        public float RepairRatePerSec = 0.01f;// 每秒修复*血量上限/转换率=消耗幽能
        public float VoidEnergyConvertRate = 500f;
    }
}
