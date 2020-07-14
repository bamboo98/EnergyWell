using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace zhuzi.AdvancedEnergy.EnergyWell.Builds
{
    // Token: 0x02000002 RID: 2
    public class PlaceWorker_VoidEnergyThermostat : PlaceWorker
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            RoomGroup roomGroup = center.GetRoomGroup(currentMap);
            if (roomGroup != null && !roomGroup.UsesOutdoorTemperature)
            {
                GenDraw.DrawFieldEdges(roomGroup.Cells.ToList<IntVec3>(), Color.magenta);
            }
        }
    }



    class VoidEnergyThermostat: Building
    {

        private Comp.MapVoidEnergyNet netPort;
        private Comp.VoidNetTempController compTempControl;
        private CompFlickable compFlickable;

        private float energyCostPerSec;
        private bool powerOn = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            netPort = map.GetComponent<Comp.MapVoidEnergyNet>();
            compTempControl = GetComp<Comp.VoidNetTempController>();
            compFlickable = GetComp<CompFlickable>();
            energyCostPerSec = compTempControl.prop.energyCostPerSec;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref energyCostPerSec, "energyCostPerSec", 0.01f);
            Scribe_Values.Look(ref powerOn, "powerOn");
        }

        public override string GetInspectStringLowPriority()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("VoidEnergyCostString".Translate(energyCostPerSec.ToString("f3")));

            return base.GetInspectStringLowPriority() + stringBuilder.ToString();
        }

        public override void Draw()
        {
            base.Draw();
            if (compFlickable != null && !compFlickable.SwitchIsOn)
            {
                Map.overlayDrawer.DrawOverlay(this, OverlayTypes.PowerOff);
                return;
            }
            if (!powerOn)
            {
                Map.overlayDrawer.DrawOverlay(this, OverlayTypes.NeedsPower);
            }
        }


        public override void TickRare()
        {
            if (this.compFlickable.SwitchIsOn)
            {

                float ambientTemperature = base.AmbientTemperature;
                float num;
                if (ambientTemperature > this.compTempControl.targetTemperature - 1f && ambientTemperature < this.compTempControl.targetTemperature + 1f)
                {
                    num = 0f;
                }
                else if (ambientTemperature < this.compTempControl.targetTemperature - 1f)
                {
                    if (ambientTemperature < 20f)
                    {
                        num = 1f;
                    }
                    else if (ambientTemperature > 1000f)
                    {
                        num = 0f;
                    }
                    else
                    {
                        num = Mathf.InverseLerp(1000f, 100f, ambientTemperature);
                    }
                }
                else if (ambientTemperature > this.compTempControl.targetTemperature + 1f)
                {
                    if (ambientTemperature < -50f)
                    {
                        num = -Mathf.InverseLerp(-273f, -50f, ambientTemperature);
                    }
                    else
                    {
                        num = -1f;
                    }
                }
                else
                {
                    num = 0f;
                }
                float energyLimit = this.compTempControl.prop.energyPerSecond * num * 4.16666651f;
                float num2 = GenTemperature.ControlTemperatureTempChange(base.Position, base.Map, energyLimit, this.compTempControl.targetTemperature);
                bool flag = !Mathf.Approximately(num2, 0f);

                if (flag)
                {
                    this.energyCostPerSec = compTempControl.prop.energyCostPerSec;
                    if (netPort.TryGetEnergy(energyCostPerSec))
                    {
                        this.GetRoomGroup().Temperature += num2;
                        powerOn = true;
                    }
                    else
                        powerOn = false;
                }
                else
                {
                    this.energyCostPerSec = compTempControl.prop.energyCostPerSec * this.compTempControl.prop.lowPowerConsumptionFactor;
                    if (netPort.TryGetEnergy(energyCostPerSec))
                    {
                        powerOn = true;
                    }
                    else
                        powerOn = false;
                }
                this.compTempControl.operatingAtHighPower = flag;
            }
        }
    }
}
