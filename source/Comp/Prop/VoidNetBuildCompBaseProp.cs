using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public abstract class VoidNetBuildCompBaseProp:CompProperties
    {

        //是否注册为常驻耗能组件,只有注册为耗能组件的才会消耗能量
        public bool registerToNetPort = true;
        //忽略最低储能限制
        public bool ignoreSavingEnergy = false;
        //待机时每秒耗能
        public float energyCostPerSec = 0f;
        

       
        
    }
}
