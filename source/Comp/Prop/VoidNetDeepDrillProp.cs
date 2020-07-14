using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class VoidNetDeepDrillProp : VoidNetBuildCompBaseProp
    {
        public VoidNetDeepDrillProp()
        {
            compClass = typeof(VoidNetDeepDrill);
        }

        public float WorkPerPortionBase = 10000f;
    }
}
