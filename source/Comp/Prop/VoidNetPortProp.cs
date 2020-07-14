using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class VoidNetPortProp : VoidNetBuildCompBaseProp
    {
        public VoidNetPortProp()
        {
            compClass = typeof(VoidNetPort);
        }

        //最大能量缓存
        public float energyCacheMax = 1f;
        //每秒充能
        public float energyRechargePerSec = 0.1f;
        //初始化耗时
        public int initTicks = 60;
        //为武器保留能量比例
        public float savingRate = 0.25f;




    }
}
