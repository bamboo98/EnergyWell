using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class VoidNetWeaponPortProp : CompProperties
    {
        public VoidNetWeaponPortProp()
        {
            compClass = typeof(VoidNetEquipmentPort);
        }
    }
    public class VoidNetTurretEquipmentPortProp : CompProperties
    {
        public VoidNetTurretEquipmentPortProp()
        {
            compClass = typeof(VoidNetTurretEquipmentPort);
        }
    }
}
