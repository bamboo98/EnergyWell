using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class VoidEnergyConverter: zzLib.Comp.CompStatPower
    {
        private VoidNetPort compVoidNetPort;
        private Prop.VoidEnergyConverterProp prop;

        private float convertRate = 100000;

        //private
        private float energyCost = 0;
        protected override float PowerConsumption
        {
            get
            {
                return -convertRate*energyCost;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.VoidEnergyConverterProp)props;

            convertRate = prop.convertRate;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compVoidNetPort = parent.TryGetComp<VoidNetPort>();

            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override string CompInspectStringExtra()
        {
            if (PowerNet == null)
            {
                return "PowerNotConnected".Translate();
            }
            StringBuilder str = new StringBuilder();
            str.AppendLine("电力转换: " + (-PowerConsumption).ToString("f0") + "W");
            string value = (PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick).ToString("F0");
            string value2 = PowerNet.CurrentStoredEnergy().ToString("F0");
            str.Append("PowerConnectedRateStored".Translate(value, value2));
            if (Prefs.DevMode)
            {
                str.Append("\n幽能输入: " + energyCost + " 转换率: " + (convertRate * 100).ToString("F0")+"%");
            }
            return str.ToString();
        }
        public override void CompTick()
        {
            base.CompTick();
            if (!compVoidNetPort.PowerOn)
            {
                if (energyCost != 0)
                {
                    energyCost = 0;
                    SetUpPowerVars();
                }
            }
            else
            {
                if (energyCost != compVoidNetPort.EnergyCostPerTick * 60f) 
                {
                    energyCost = compVoidNetPort.EnergyCostPerTick * 60f;
                    SetUpPowerVars();
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref convertRate, "convertRate", 100000);
            Scribe_Values.Look(ref energyCost, "energyCost", 0);
            base.PostExposeData();
        }


    }
}
