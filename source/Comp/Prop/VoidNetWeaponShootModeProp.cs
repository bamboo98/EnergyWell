using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class VoidNetWeaponShootModeProp:CompProperties
    {
        public VoidNetWeaponShootModeProp()
        {
            compClass = typeof(VoidNetWeaponShootMode);
        }
        public bool enableFullAuto = false;
        public float baseEnergyCost = 0.2f;
        public float baseAmount = 1f;
        public float baseWarmupTime = 1f;
        public float baseCooldown = 0;
        public int baseTicksBetweenBurstShots = 6;
        public float baseAccuracy = 1f;
    }
}
