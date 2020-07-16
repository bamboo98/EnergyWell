using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;
using zhuzi.AdvancedEnergy.EnergyWell.Resources;

namespace zhuzi.AdvancedEnergy.EnergyWell.Things
{
    public class VoidNetTerminal: Apparel
    {
        

        private List<ThingComp> onlineComp = new List<ThingComp>();
        private Comp.WorldVoidEnergyNet worldNet;



        public bool IsSavingEnergy
        {
            get
            {
                return energyCur / energyCacheMax <= savingRate;
            }
        }
        public float EnergyCur
        {
            get
            {
                return energyCur;
            }
        }
        public float EnergyCacheMax
        {
            get
            {
                return energyCacheMax;
            }
        }
        public float ShieldMax
        {
            get
            {
                return shieldMax;
            }
        }
        public float ShieldCur
        {
            get
            {
                return shieldCur;
            }
        }
        public float ShieldInit
        {
            get
            {
                return shieldInit;
            }
        }

        public float EnergyPencert
        {
            get
            {
                return energyCur / energyCacheMax;
            }
        }


        private Comp.MapVoidEnergyNet MapNet
        {
            get
            {
                return (Wearer!=null?Wearer.Map:Map)?.GetComponent<Comp.MapVoidEnergyNet>();
            }
        }

        private Comp.WorldVoidEnergyNet WorldNet
        {
            get
            {
                if (worldNet == null)
                {
                    worldNet = Current.Game.GetComponent<Comp.WorldVoidEnergyNet>();
                }
                return worldNet;
            }
        }

        private float shieldMax = 200f;
        private float shieldRecharge = 5f;
        private float shieldConvertRate = 43f;
        private float shieldDamagedRate = 1f;
        private float shieldInitSec = 10f;




        private float energyCacheMax = 4f;
        private float energyRecharge = 0.1f;
        private float energyCostPerSec = 0f;

        private bool worldConnect = false;
        private float terminalInitSec = 2f;


        //save
        private float energyCur = 0f;
        private float shieldCur = 0;
        private int shieldInit = 600;
        public float savingRate = 0.25f;
        private int terminalInit = 120;


        public override void PostMake()
        {
            base.PostMake();
            energyCacheMax = this.GetStatValue(StatDefs.VoidEnergyCacheMax);
            energyRecharge = this.GetStatValue(StatDefs.VoidEnergyRecharge);
            shieldMax = this.GetStatValue(StatDefs.VoidEnergyShieldMax);
            shieldRecharge = this.GetStatValue(StatDefs.VoidEnergyShieldRecharge);
            shieldConvertRate = this.GetStatValue(StatDefs.VoidEnergyShieldConvertRate);
            shieldDamagedRate = this.GetStatValue(StatDefs.VoidEnergyShieldStrength);
            energyCostPerSec = this.GetStatValue(StatDefs.VoidEnergyCostPerSec);
        }




        public virtual bool CanCostEnergy(float count)
        {
            return energyCur >= count;
        }
        public virtual bool CostEnergy(float count)
        {
            if (count <= 0)
                return true;
            if (energyCur < count)
                return false;
            energyCur -= count;
            return true;
        }

        public void Online(ThingComp comp)
        {
            if (!onlineComp.Any((x) => { return x == comp; }))
            {
                onlineComp.Add(comp);
                if (comp is Comp.VoidNetEquipmentPort)
                    (comp as Comp.VoidNetEquipmentPort).voidNetTerminal = this;
            }
        }
        public void Offline(ThingComp comp)
        {
            onlineComp.Remove(comp);
            if(comp is Comp.VoidNetEquipmentPort)
                (comp as Comp.VoidNetEquipmentPort).voidNetTerminal = null;
        }

        private void OfflineAll()
        {
            foreach (ThingComp comp in onlineComp)
            {
                if (comp is Comp.VoidNetEquipmentPort)
                    (comp as Comp.VoidNetEquipmentPort).voidNetTerminal = null;
            }
            onlineComp.Clear();
        }


        public static VoidNetTerminal FindTerminal(Pawn pawn)
        {
            if (pawn == null)
                return null;
            
            foreach (ThingWithComps thingWithComps in pawn.apparel.WornApparel)
            {
                if (thingWithComps is VoidNetTerminal)
                    return thingWithComps as VoidNetTerminal;
            }
            return null;
        }

        private Vector3 impactAngleVect;
        private int lastKeepDisplayTick = -9999;
        private int lastAbsorbDamageTick = -9999;
        private int KeepDisplayingTicks = 1000;

        private bool ShouldDisplay
        {
            get
            {
                Pawn wearer = Wearer;
                return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) || Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks);
            }
        }
        
        private static readonly MaterialPropertyBlock PropBlock = new MaterialPropertyBlock();

        float oriH = Rand.Value;
        float oriS = Rand.Range(0.4f,1f);
        float oriV = 1f;// Rand.Value;
        float tarH = Rand.Value;
        float tarS = Rand.Range(0.4f, 1f);
        float tarV = 1f;// Rand.Value;
        float changeProcess = 0f;

        public override void DrawWornExtras()
        {
            if (ShieldMax>0 && shieldInit<=0 && ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, shieldCur/shieldMax);
                Vector3 vector = Wearer.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                PropBlock.SetColor(ShaderPropertyIDs.Color, 
                    Color.HSVToRGB(
                        oriH + (tarH - oriH) * changeProcess, 
                        oriS + (tarS - oriS) * changeProcess, 
                        oriV + (tarV - oriV) * changeProcess
                        ));
                changeProcess += 0.016f;
                if (changeProcess >= 1f)
                {
                    oriH = tarH;
                    oriS = tarS;
                    oriV = tarV;
                    tarH = Rand.Value;
                    tarS = Rand.Range(0.4f, 1f);
                    tarV = 1f;// Rand.Value;
                    changeProcess -= 1f;
                }

                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default;
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.Materials.BubbleMat, 0,null,0, PropBlock);
            }
        }
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (ShieldMax <= 0 || shieldInit > 0) 
            {
                //受到伤害时重置启动倒计时
                shieldInit = shieldInitSec.SecondsToTicks();
                return false;
            }
            dinfo.SetAmount(dinfo.Amount / shieldDamagedRate);
            float impact = dinfo.Amount;
            if (impact> shieldCur)
            {
                dinfo.SetAmount(-shieldCur);
                shieldCur = 0;
                shieldInit = shieldInitSec.SecondsToTicks();
                //剩余护盾相对冲击量越高,抵挡最后一次伤害的概率就越高
                return Rand.Chance(shieldCur/impact);
            }
            else
            {
                shieldCur -= impact;
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
                impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                Vector3 loc = Wearer.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
                float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
                MoteMaker.MakeStaticMote(loc, Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
                int num2 = (int)num;
                for (int i = 0; i < num2; i++)
                {
                    MoteMaker.ThrowDustPuff(loc, Wearer.Map, Rand.Range(0.8f, 1.2f));
                }
                int gt = Find.TickManager.TicksGame;
                lastAbsorbDamageTick = gt;
                lastKeepDisplayTick = gt;
            }
            return true;
        }

        private Pawn oldWearer;

        private Hediff VoidEnergyHediffTerminalMoveSpeed;
        public virtual void PostPostTerminalOff()
        {
            if (def == ThingDefs.VoidNetTerminal_Scout)
            {
                if (VoidEnergyHediffTerminalMoveSpeed != null && oldWearer!=null && !oldWearer.Destroyed)
                    oldWearer.health.RemoveHediff(VoidEnergyHediffTerminalMoveSpeed);
                VoidEnergyHediffTerminalMoveSpeed = null;
            }
            oldWearer = null;
        }
        public virtual void PostPostTerminalOn()
        {
            if (def == ThingDefs.VoidNetTerminal_Scout)
            {
                if (VoidEnergyHediffTerminalMoveSpeed != null && oldWearer!= null && !oldWearer.Destroyed)
                    oldWearer.health.RemoveHediff(VoidEnergyHediffTerminalMoveSpeed);
                VoidEnergyHediffTerminalMoveSpeed = HediffMaker.MakeHediff(HediffDefs.VoidEnergyHediffTerminalMoveSpeed, Wearer, null);
                Wearer.health.AddHediff(VoidEnergyHediffTerminalMoveSpeed, null, null, null);
            }
            oldWearer = Wearer;
        }


        public override void Tick()
        {
            base.Tick();
            if (Wearer == null)
            {
                if (onlineComp.Count > 0)
                {
                    OfflineAll();
                }
                shieldCur = 0;
                energyCur = 0;
                shieldInit = shieldInitSec.SecondsToTicks();
                terminalInit = terminalInitSec.SecondsToTicks();
                PostPostTerminalOff();
                return;
            }
            oldWearer = Wearer;
            bool rechargeShield = true;

            if (energyCostPerSec > 0 && !CostEnergy(energyCostPerSec / 60f))
            {//报告离线
                if (terminalInit <= 0)
                {
                    PostPostTerminalOff();
                }
                //离线
                terminalInit = terminalInitSec.SecondsToTicks();
            }
            else if (terminalInit-- == 0)
            {//报告上线
                PostPostTerminalOn();
            }
            else if (terminalInit > 0)
                //正在启动,不进行护盾充能
                rechargeShield = false;


            float need;

            if (rechargeShield)
            {
                //给护盾充能
                if (shieldMax>0 && shieldRecharge > 0 && savingRate < energyCur / energyCacheMax)
                {
                    if (shieldInit-- > 0)
                    {
                        if (!CostEnergy(shieldRecharge / 60f / shieldConvertRate / 3f))
                        {
                            shieldInit = shieldInitSec.SecondsToTicks();
                        }
                    }
                    else
                    {
                        need = Mathf.Min(shieldRecharge/60f,shieldMax - shieldCur)/shieldConvertRate;
                        if (need > 0 && CostEnergy(need))
                        {
                            shieldCur += shieldRecharge / 60f;
                        }
                    }
                }
            }


            //给自己充能
            Comp.MapVoidEnergyNet mapNet = MapNet;
            if (mapNet == null)
            {
                return;
            }
            need = Mathf.Min(energyCacheMax - energyCur, energyRecharge/60f);
            if (need == 0)
                return;
            float geted= mapNet.GetEnergy(need);
            energyCur += geted;
            need -= geted;
            if (need <= 0 || (!worldConnect && !DebugSettings.godMode))
                return;
            energyCur += WorldNet.GetEnergy(need);
        }
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == Wearer)
            {
                yield return new Gizmo_VoidNetTerminalStatus
                {
                    terminal = this
                };
            }
            if (Prefs.DevMode)
                yield return new Command_Action
                {
                    defaultLabel = "Full",
                    defaultDesc = "立刻满充能",
                    action = delegate ()
                    {
                        energyCur = energyCacheMax;
                    }
                };
            if (Wearer.equipment.Primary!=null)
            {
                Comp.VoidNetWeaponShootMode comp=Wearer.equipment.Primary.TryGetComp<Comp.VoidNetWeaponShootMode>();
                if (comp != null)
                {
                    foreach (Gizmo item in comp.CompGetGizmosExtra())
                    {
                        yield return item;
                    }
                }
            }

            yield break;
        }
        
        public override void Notify_Equipped(Pawn pawn)
        {

            shieldCur = 0;
            energyCur = 0;
            shieldInit = shieldInitSec.SecondsToTicks();
            terminalInit = terminalInitSec.SecondsToTicks();
            base.Notify_Equipped(pawn);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (onlineComp.Count > 0)
            {
                OfflineAll();
            }
            PostPostTerminalOff();
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (onlineComp.Count > 0)
            {
                OfflineAll();
            }
            PostPostTerminalOff();
            base.Destroy(mode);
        }


        public override void ExposeData()
        {
            Scribe_Values.Look(ref energyCacheMax, "energyCacheMax", 4f);
            Scribe_Values.Look(ref energyRecharge, "energyRecharge", 0.1f);
            Scribe_Values.Look(ref shieldMax, "shieldMax", 200f);
            Scribe_Values.Look(ref shieldRecharge, "shieldRecharge", 5f);
            Scribe_Values.Look(ref shieldConvertRate, "shieldConvertRate", 43f);
            Scribe_Values.Look(ref shieldDamagedRate, "shieldDamagedRate", 1f);
            Scribe_Values.Look(ref shieldInitSec, "shieldInitSec", 10f);
            Scribe_Values.Look(ref terminalInit, "terminalInit");

            Scribe_Values.Look(ref energyCur, "energyCur", 0f);
            Scribe_Values.Look(ref shieldCur, "shieldCur", 0f);
            Scribe_Values.Look(ref shieldInit, "shieldInit", 600);
            Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
            Scribe_Values.Look(ref savingRate, "savingRate", 0.25f);

            Scribe_References.Look(ref VoidEnergyHediffTerminalMoveSpeed, "VoidEnergyHediffTerminalMoveSpeed", false);
            Scribe_References.Look(ref oldWearer, "oldWearer", false);

            //zzLib.Log.Message(Scribe.mode.ToString());
            //zzLib.Log.Message("hediff: " + VoidEnergyHediffTerminalMoveSpeed==null?"null":VoidEnergyHediffTerminalMoveSpeed.ToStringSafe());
            //zzLib.Log.Message("oldWearer: " + oldWearer==null?"null":oldWearer.ToStringSafe());

            base.ExposeData();
        }
    }

    [StaticConstructorOnStartup]
    public class Gizmo_VoidNetTerminalStatus : Gizmo
    {
        public Gizmo_VoidNetTerminalStatus()
        {
            order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        private void DrowLimitLine(Rect rect,float percent)
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
            Rect rect = new Rect(topLeft.x, topLeft.y-40, GetWidth(maxWidth), 75f+40);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 3f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, terminal.LabelCap);

            if (terminal.ShieldMax <= 0)
            {

                Rect rect4 = rect2;
                rect4.yMin = rect2.y + rect2.height / 3f;
                float fillPercent = terminal.EnergyCur / terminal.EnergyCacheMax;
                Widgets.FillableBar(rect4, fillPercent, Texture2Ds.FullVoidBarTex, Texture2Ds.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, (terminal.EnergyCur * 100f).ToString("F0") + " / " + (terminal.EnergyCacheMax * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;

            }
            else
            {

                Rect rect4 = rect2;
                rect4.yMin = rect2.y + rect2.height / 3f;
                rect4.height = rect4.height / 2f - 2f;
                float fillPercent = terminal.ShieldCur / terminal.ShieldMax;
                Widgets.FillableBar(rect4, fillPercent, Texture2Ds.FullShieldBarTex, Texture2Ds.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                if (terminal.ShieldInit > 0)
                    Widgets.Label(rect4, "启动中:"+(terminal.ShieldInit/60f).ToString("f1")+"秒");
                else
                    Widgets.Label(rect4,(terminal.ShieldCur).ToString("F0") + " / " + (terminal.ShieldMax).ToString("F0"));

                Text.Anchor = TextAnchor.UpperLeft;




                Rect rect5 = rect4;
                rect5.yMin = rect4.yMax + 4f;
                rect5.height = rect4.height;
                float fillPercent2 = terminal.EnergyCur / terminal.EnergyCacheMax;
                Widgets.FillableBar(rect5, fillPercent2, Texture2Ds.FullVoidBarTex, Texture2Ds.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect5,(terminal.EnergyCur * 100f).ToString("F0") + " / " + (terminal.EnergyCacheMax * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;



                Pawn pawn = terminal.Wearer;

                if (pawn.IsColonistPlayerControlled)
                {
                    DrowLimitLine(rect5, terminal.savingRate);
                    bool flag = Mouse.IsOver(rect5);
                    if (flag && Input.GetMouseButton(0))
                    {
                        Vector2 mousePosition = Event.current.mousePosition;
                        terminal.savingRate = (mousePosition.x - (rect5.x + 3f)) / (rect5.width - 8f);
                        SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
                    }
                    TooltipHandler.TipRegion(rect5, "ToolTip_SetPersionalTerminalSavingRate".Translate((terminal.savingRate*100f).ToString("f0")));
                }


            }





            return new GizmoResult(GizmoState.Clear);
        }

        public VoidNetTerminal terminal;

    }
}
