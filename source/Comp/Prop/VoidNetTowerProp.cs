using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class VoidNetTowerProp: CompProperties
    {
        public VoidNetTowerProp()
        {
            compClass = typeof(VoidNetTower);
        }
        public float energyTransportPerSec = 1f;
        public bool transportToWorld = false;
    }
}
