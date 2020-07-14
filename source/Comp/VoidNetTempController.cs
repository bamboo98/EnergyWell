using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class VoidNetTempController:ThingComp
    {

        public Prop.VoidNetTempControllerProp prop;



        public float energyPerSecond = 60f;
        public float defaultTargetTemperature = 21f;
        public float minTargetTemperature = -500f;
        public float maxTargetTemperature = 500f;
        public float lowPowerConsumptionFactor = 0.1f;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.VoidNetTempControllerProp)props;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.targetTemperature < -2000f)
            {
                this.targetTemperature = prop.defaultTargetTemperature;
            }
        }
        private float RoundedToCurrentTempModeOffset(float celsiusTemp)
        {
            return GenTemperature.ConvertTemperatureOffset((float)Mathf.RoundToInt(GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode)), Prefs.TemperatureMode, TemperatureDisplayMode.Celsius);
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("TargetTemperature".Translate() + ": ");
            stringBuilder.Append(this.targetTemperature.ToStringTemperature("F0"));
            return stringBuilder.ToString();
        }
        private void ThrowCurrentTemperatureText()
        {
            MoteMaker.ThrowText(this.parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), this.parent.Map, this.targetTemperature.ToStringTemperature("F0"), Color.white, -1f);
        }
        private void InterfaceChangeTargetTemperature(float offset)
        {
            SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            this.targetTemperature += offset;
            this.targetTemperature = Mathf.Clamp(this.targetTemperature, -273.15f, 1000f);
            this.ThrowCurrentTemperatureText();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            float offset2 = this.RoundedToCurrentTempModeOffset(-10f);
            yield return new Command_Action
            {
                action = delegate ()
                {
                    this.InterfaceChangeTargetTemperature(offset2);
                },
                defaultLabel = offset2.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandLowerTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc5,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower", true)
            };
            float offset3 = this.RoundedToCurrentTempModeOffset(-1f);
            yield return new Command_Action
            {
                action = delegate ()
                {
                    this.InterfaceChangeTargetTemperature(offset3);
                },
                defaultLabel = offset3.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandLowerTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc4,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower", true)
            };
            yield return new Command_Action
            {
                action = delegate ()
                {
                    this.targetTemperature = DefaultTargetTemperature;
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
                    this.ThrowCurrentTemperatureText();
                },
                defaultLabel = "CommandResetTemp".Translate(),
                defaultDesc = "CommandResetTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempReset", true)
            };
            float offset4 = this.RoundedToCurrentTempModeOffset(1f);
            yield return new Command_Action
            {
                action = delegate ()
                {
                    this.InterfaceChangeTargetTemperature(offset4);
                },
                defaultLabel = "+" + offset4.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandRaiseTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc2,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise", true)
            };
            float offset = this.RoundedToCurrentTempModeOffset(10f);
            yield return new Command_Action
            {
                action = delegate ()
                {
                    this.InterfaceChangeTargetTemperature(offset);
                },
                defaultLabel = "+" + offset.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandRaiseTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc3,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise", true)
            };
            yield break;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.targetTemperature, "targetTemperature", 0f, false);
        }
        [Unsaved(false)]
        public bool operatingAtHighPower;

        // Token: 0x04002E2C RID: 11820
        public float targetTemperature = -99999f;

        // Token: 0x04002E2D RID: 11821
        private const float DefaultTargetTemperature = 21f;
    }
}
