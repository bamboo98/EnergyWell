using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    class VoidNetWeaponPortProp : CompProperties
    {
        public VoidNetWeaponPortProp()
        {
            compClass = typeof(VoidNetEquipmentPort);
        }
    }
    class VoidNetTurretEquipmentPortProp : CompProperties
    {
        public VoidNetTurretEquipmentPortProp()
        {
            compClass = typeof(VoidNetTurretEquipmentPort);
        }
    }
}
