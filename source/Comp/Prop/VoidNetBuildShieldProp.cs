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
    class VoidNetBuildShieldProp: VoidNetBuildCompBaseProp
    {
        public VoidNetBuildShieldProp()
        {
            compClass = typeof(VoidNetBuildShield);
        }
        public float shieldMax = 200f;
        public float shieldRecharge = 5f;
        public float shieldConvertRate = 0.023f;
        public float shieldDamagedRate = 1f;
        public int shieldInitTick = 600;
        public float drawSizeScale = 1f;
    }
}
