using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class VoidEnergyConverterProp : CompProperties_Power
    {
        public VoidEnergyConverterProp()
        {
            compClass = typeof(VoidEnergyConverter);
        }

        public float convertRate = 100000;

    }
}
