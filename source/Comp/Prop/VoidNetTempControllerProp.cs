using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp.Prop
{
    public class VoidNetTempControllerProp:CompProperties
    {
        public VoidNetTempControllerProp()
        {
            compClass = typeof(VoidNetTempController);
        }

        public float energyPerSecond = 60f;


        public float defaultTargetTemperature = 21f;


        public float minTargetTemperature = -1000f;


        public float maxTargetTemperature = 1000f;


        public float lowPowerConsumptionFactor = 0.1f;

        public float energyCostPerSec = 0.01f;
    }
}
