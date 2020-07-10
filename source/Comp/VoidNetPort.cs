using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;
namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class VoidNetPort:ThingComp
    {

        MapVoidEnergyNet VoidNet;
        protected CompFlickable flickableComp;

        private float energyCacheMax = 1f;
        private float energyCost = 0.03f;

        private int initTicks = 60;

        //save
        private float energyCache = 0;
        private bool online = false;
        private int initCountdown = 60;

        public bool PowerOn
        {
            get {
                return online && energyCache >= energyCost/60f;

            }
        }
        public float EnergyCost
        {
            get
            {
                return energyCost/60f;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            VoidNet = parent.Map.GetComponent<MapVoidEnergyNet>();
            flickableComp = parent.GetComp<CompFlickable>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
        {
            float energtCostPerTick = energyCost / 60f;
            //cost
            if (online)
            {
                if(energtCostPerTick > energyCache)
                { 
                    online = false;
                    initCountdown = initTicks;
                }
                else
                {
                    energyCache -= energtCostPerTick;
                }
            }
            else
            {
                if (energtCostPerTick > energyCache)
                    initCountdown = initTicks;
                else
                {
                    energyCache -= energtCostPerTick;
                    if (initCountdown-- <= 0)
                        online = true;
                }
            }
            //recharge
            energyCache += VoidNet.GetEnergy(energyCacheMax - energyCache);
            base.CompTick();
        }


        public override string CompInspectStringExtra()
        {
            StringBuilder str = new StringBuilder();
            
            if (!PowerOn)
                str.AppendLine("启动进度: " +((float)(initTicks-initCountdown)/(float)initTicks*100).ToString("f1")+"%");
            else
                str.AppendLine("幽能缓存: " + energyCache.ToString("f1") + "/" + energyCacheMax.ToString("f2"));
            str.Append("幽能需求: " + energyCost.ToString("f3") + "/秒");



            return str.ToString().Trim();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate ()
                    {
                        energyCache = energyCacheMax;
                        initCountdown = 0;
                        online = true;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Offline",
                    action = delegate ()
                    {
                        energyCache = 0;
                        initCountdown = initTicks;
                        online = false;
                    }
                };
            }
            yield break;
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (!parent.IsBrokenDown())
            {
                if (flickableComp != null && !flickableComp.SwitchIsOn)
                {
                    parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.PowerOff);
                    return;
                }
                if (!PowerOn)
                {
                    parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.NeedsPower);
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref energyCache, "energyCache", 0);
            Scribe_Values.Look(ref online, "online", false);
            Scribe_Values.Look(ref initCountdown, "initCountdown", 60);
            base.PostExposeData();
        }

    }
}
