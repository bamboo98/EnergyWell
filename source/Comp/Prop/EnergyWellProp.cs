using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class EnergyWellProp :CompProperties
    {
        public EnergyWellProp()
        {
            compClass = typeof(EnergyWell);
        }
        public float energyStorageMax = 500f;
        public float produceEnergyPerSec = 0.1f;
        public float heatAccumulationRate = 1f;
        public float activePower = 10000f;
        public float permanentPower = 1000f;
    }
}
