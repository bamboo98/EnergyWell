using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;
using zhuzi.AdvancedEnergy.EnergyWell.Resources;
using Verse.Sound;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    public class VoidNetPort:ThingComp
    {

        MapVoidEnergyNet VoidNet;
        protected CompFlickable flickableComp;

        private float energyCacheMax = 1f;
        private float energyCostPerSec = 0.03f;
        private float energyRechargePerSec = 0.1f;


        private int initTicks = 60;

        //save
        private float energyCache = 0;
        private bool online = false;
        private int initCountdown = 60;
        public bool showInfo = true;
        public float savingRate = 0.25f;


        public float EnergyCache
        {
            get
            {
                return energyCache;
            }
        }
        public float EnergyCacheMax
        {
            get
            {
                return energyCacheMax;
            }
        }

        public bool PowerOn
        {
            get {
                return online && energyCache >= energyCostPerSec/60f;

            }
        }
        public float EnergyCost
        {
            get
            {
                return energyCostPerSec/60f;
            }
        }

        public bool IsSavingEnergy
        {
            get
            {
                return energyCache / energyCacheMax <= savingRate;
            }
        }

        public virtual bool CanCostEnergy(float count)
        {
            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return false;
            return energyCache >= count;
        }
        public virtual bool CostEnergy(float count)
        {
            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return false;
            if (count <= 0)
                return true;
            if (energyCache < count)
                return false;
            energyCache -= count;
            return true;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            VoidNet = parent.Map.GetComponent<MapVoidEnergyNet>();
            flickableComp = parent.GetComp<CompFlickable>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
        {
            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return;
            float energtCostPerTick = energyCostPerSec / 60f;
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
            energyCache += VoidNet.GetEnergy(Mathf.Min(energyRechargePerSec/60f,energyCacheMax - energyCache));
            if (DebugSettings.godMode)
                energyCache = energyCacheMax;
            base.CompTick();
        }


        public override string CompInspectStringExtra()
        {
            if (!showInfo) return "";
            StringBuilder str = new StringBuilder();
            if (!PowerOn)
                str.AppendLine("启动进度: " +((float)(initTicks-initCountdown)/(float)initTicks*100).ToString("f1")+"%");
            else
                str.AppendLine("幽能缓存: " + energyCache.ToString("f1") + "/" + energyCacheMax.ToString("f2"));
            str.Append("幽能需求: " + energyCostPerSec.ToString("f3") + "/秒");



            return str.ToString().Trim();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            yield return new Gizmo_VoidNetPortStatus
            {
                voidNetPort = this,
                voidNetBuildShield=parent.TryGetComp<VoidNetBuildShield>()
            };

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
            Scribe_Values.Look(ref showInfo, "showInfo", true);
            Scribe_Values.Look(ref savingRate, "savingRate",0.25f);
            base.PostExposeData();
        }



    }


    [StaticConstructorOnStartup]
    public class Gizmo_VoidNetPortStatus : Gizmo
    {
        public Gizmo_VoidNetPortStatus()
        {
            order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }
        private void DrowLimitLine(Rect rect, float percent)
        {

            float num = Mathf.Round((rect.width - 8f) * percent);
            GUI.DrawTexture(new Rect
            {
                x = rect.x + 3f + num,
                y = rect.y,
                width = 2f,
                height = rect.height
            }, Resources.Texture2Ds.PsyfocusTargetTex);
            float num2 = Widgets.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
            float xMax = Widgets.AdjustCoordToUIScalingCeil(num2 + 4f);
            Rect rect2 = new Rect
            {
                y = rect.y - 3f,
                height = 5f,
                xMin = num2,
                xMax = xMax
            };
            GUI.DrawTexture(rect2, Resources.Texture2Ds.PsyfocusTargetTex);
            Rect position = rect2;
            position.y = rect.yMax - 2f;
            GUI.DrawTexture(position, Resources.Texture2Ds.PsyfocusTargetTex);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y - 40, GetWidth(maxWidth), 75f + 40);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 3f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, "幽能接收终端");

            if (voidNetBuildShield==null || voidNetBuildShield.ShieldMax <= 0)
            {

                Rect rect4 = rect2;
                rect4.yMin = rect2.y + rect2.height / 3f;
                float fillPercent = voidNetPort.EnergyCache / voidNetPort.EnergyCacheMax;
                Widgets.FillableBar(rect4, fillPercent, Texture2Ds.FullVoidBarTex, Texture2Ds.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, (voidNetPort.EnergyCache * 100f).ToString("F0") + " / " + (voidNetPort.EnergyCacheMax * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;

            }
            else
            {

                Rect rect4 = rect2;
                rect4.yMin = rect2.y + rect2.height / 3f;
                rect4.height = rect4.height / 2f - 2f;
                float fillPercent = voidNetBuildShield.ShieldCur / voidNetBuildShield.ShieldMax;
                Widgets.FillableBar(rect4, fillPercent, Texture2Ds.FullShieldBarTex, Texture2Ds.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                if (voidNetBuildShield.ShieldInit > 0)
                    Widgets.Label(rect4, "启动中:" + (voidNetBuildShield.ShieldInit / 60f).ToString("f1") + "秒");
                else
                    Widgets.Label(rect4, (voidNetBuildShield.ShieldCur).ToString("F0") + " / " + (voidNetBuildShield.ShieldMax).ToString("F0"));

                Text.Anchor = TextAnchor.UpperLeft;




                Rect rect5 = rect4;
                rect5.yMin = rect4.yMax + 4f;
                rect5.height = rect4.height;
                float fillPercent2 = voidNetPort.EnergyCache / voidNetPort.EnergyCacheMax;
                Widgets.FillableBar(rect5, fillPercent2, Texture2Ds.FullVoidBarTex, Texture2Ds.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect5, (voidNetPort.EnergyCache * 100f).ToString("F0") + " / " + (voidNetPort.EnergyCacheMax * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;


                Thing thing = voidNetPort.parent;

                if (thing.Faction!=null && thing.Faction==Faction.OfPlayer)
                {
                    DrowLimitLine(rect5, voidNetPort.savingRate);
                    bool flag = Mouse.IsOver(rect5);
                    if (flag && Input.GetMouseButton(0))
                    {
                        Vector2 mousePosition = Event.current.mousePosition;
                        voidNetPort.savingRate = (mousePosition.x - (rect5.x + 3f)) / (rect5.width - 8f);
                        zzLib.Log.Message("savingRate: " + voidNetPort.savingRate);
                        SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
                    }
                    TooltipHandler.TipRegion(rect5, "ToolTip_SetVoidNetPortSavingRate".Translate((voidNetPort.savingRate * 100f).ToString("f0")));
                }




            }

            return new GizmoResult(GizmoState.Clear);
        }

        public VoidNetPort voidNetPort;
        public VoidNetBuildShield voidNetBuildShield;

    }
}
