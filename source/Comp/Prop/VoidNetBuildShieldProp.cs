using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    class VoidNetBuildShieldProp:CompProperties
    {
        public VoidNetBuildShieldProp()
        {
            compClass = typeof(VoidNetBuildShield);
        }
    }
}
