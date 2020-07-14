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
    public class VoidNetPort: VoidNetBuildCompBase
    {
        //地图网络节点
        protected MapVoidEnergyNet VoidNet;
        //开关组件
        protected CompFlickable flickableComp;
        private Prop.VoidNetPortProp prop;

        //最大能量缓存
        protected float energyCacheMax = 1f;
        //每秒充能
        protected float energyRechargePerSec = 0.1f;
        //初始化耗时
        protected int initTicks = 60;

        //存档
        //当前能量缓存
        protected float energyCache = 0;
        //初始化倒计时
        protected int initCountdown = 60;
        //辅助信息的形式方式
        public ShowInfoMode ShowMode = ShowInfoMode.Both;
        //为武器保留能量比例
        public float savingRate = 0.25f;


        private readonly List<VoidNetBuildCompBase> voidNetBuildCompBases = new List<VoidNetBuildCompBase>();


        //所有已注册组件的耗能
        private float CompsEnergyCost
        {
            get
            {
                float sum = 0;
                foreach (VoidNetBuildCompBase compBase in voidNetBuildCompBases)
                {
                    sum += compBase.EnergyCostPerTick;
                }
                return sum;
            }
        }

        /// <summary>
        /// 是否初始化完毕
        /// </summary>
        public bool InitCompleted
        {
            get
            {
                return initCountdown <= 0;
            }
        }
        /// <summary>
        /// 当前能量缓存
        /// </summary>
        public float EnergyCache
        {
            get
            {
                return energyCache;
            }
        }
        /// <summary>
        /// 能量缓存上限
        /// </summary>
        public float EnergyCacheMax
        {
            get
            {
                return energyCacheMax;
            }
        }
        /// <summary>
        /// 开关是否打开
        /// </summary>
        public bool FlickOn
        {
            get
            {
                return flickableComp == null || flickableComp.SwitchIsOn;
            }
        }

        /// <summary>
        /// 是否可以维持基础供能
        /// </summary>
        public new bool PowerOn
        {
            get {
                return InitCompleted && energyCache >= CompsEnergyCost && FlickOn;

            }
        }
        /// <summary>
        /// 是否低于预留能量线
        /// </summary>
        public bool IsSavingEnergy
        {
            get
            {
                return energyCache / energyCacheMax <= savingRate;
            }
        }


        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.VoidNetPortProp)props;

            energyCacheMax = prop.energyCacheMax;
            energyRechargePerSec = prop.energyRechargePerSec;
            initTicks = prop.initTicks;
            savingRate = prop.savingRate;

        }

        /// <summary>
        /// 是否可以消耗指定数量的能量
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public new virtual bool CanCostEnergy(float count)
        {
            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return false;
            return energyCache >= count;
        }
        /// <summary>
        /// 消耗能量
        /// </summary>
        /// <param name="count"></param>
        /// <returns>是否成功</returns>
        public new virtual bool CostEnergy(float count)
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
        /// <summary>
        /// 注册耗能组件到节点
        /// </summary>
        /// <param name="comp"></param>
        public void RegisterToNetPort(VoidNetBuildCompBase comp)
        {
            if (voidNetBuildCompBases.Any((x) => { return x == comp; }))
            {
                zzLib.Log.Warning("存在重复注册的组件");
                return;
            }
            voidNetBuildCompBases.Add(comp);
        }
        /// <summary>
        /// 从节点中移除耗能组件
        /// </summary>
        /// <param name="comp"></param>
        public void RestoreFormNetPort(VoidNetBuildCompBase comp)
        {
            voidNetBuildCompBases.Remove(comp);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            VoidNet = parent.Map.GetComponent<MapVoidEnergyNet>();
            flickableComp = parent.GetComp<CompFlickable>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return;
            //这里包括了其他组件的基础待机耗能
            float energtCostPerTick = CompsEnergyCost;
            //cost
            if (InitCompleted)
            {
                if(energtCostPerTick > energyCache)
                { 
                    initCountdown = initTicks;
                    foreach (VoidNetBuildCompBase compBase in voidNetBuildCompBases)
                    {
                        compBase.NotifyPostOffline();
                    }
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
                    initCountdown--;
                }
            }
            //recharge
            energyCache += VoidNet.GetEnergy(Mathf.Min(energyRechargePerSec/60f,energyCacheMax - energyCache));
            if (DebugSettings.godMode)
                energyCache = energyCacheMax;
        }


        public override void CompTickRare()
        {
            base.CompTickRare();

            if (flickableComp != null && !flickableComp.SwitchIsOn)
                return;

            //rare tick =250tick
            float energtCostPerTick = CompsEnergyCost * 250f;
            //cost
            if (InitCompleted)
            {
                if (energtCostPerTick > energyCache)
                {
                    initCountdown = initTicks;
                    foreach (VoidNetBuildCompBase compBase in voidNetBuildCompBases)
                    {
                        compBase.NotifyPostOffline();
                    }
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
                    initCountdown-=250;
                }
            }
            //recharge
            energyCache += VoidNet.GetEnergy(Mathf.Min(energyRechargePerSec / 60f * 250f, energyCacheMax - energyCache));
            if (DebugSettings.godMode)
                energyCache = energyCacheMax;
        }




        public override string CompInspectStringExtra()
        {
            if ((ShowMode & ShowInfoMode.InspectString) != ShowInfoMode.InspectString) return "";
            StringBuilder str = new StringBuilder();
            if (!PowerOn)
                str.AppendLine("启动进度: " +((float)(initTicks-initCountdown)/(float)initTicks*100).ToString("f1")+"%");
            else
                str.AppendLine("幽能缓存: " + energyCache.ToString("f1") + "/" + energyCacheMax.ToString("f2"));
            str.Append("幽能需求: " + (CompsEnergyCost * 60f).ToString("f3") + "/秒");



            return str.ToString().Trim();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if ((ShowMode & ShowInfoMode.Gizmo) == ShowInfoMode.Gizmo) 
                yield return new Gizmo_VoidNetPortStatus
                {
                    voidNetPort = this,
                    voidNetBuildShield = parent.TryGetComp<VoidNetBuildShield>()
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
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Offline",
                    action = delegate ()
                    {
                        energyCache = 0;
                        initCountdown = initTicks;
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
            Scribe_Values.Look(ref initCountdown, "initCountdown", 60);
            Scribe_Values.Look(ref ShowMode, "ShowMode", ShowInfoMode.Both);
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
    [Flags]
    public enum ShowInfoMode
    {
        None,
        InspectString,
        Gizmo,
        Both,
    }
}
