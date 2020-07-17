using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    public class EnergyWell : ThingComp
    {

        private Prop.EnergyWellProp prop;
        private CompPowerTrader powerTrader;

        public MapVoidEnergyNet MapNetNode;
        public WorldVoidEnergyNet WorldNetNode;



        private float energyStorageMax = 500f;
        private float produceEnergyPerSec = 0.1f;
        private float heatAccumulationRate = 1f;
        private float activePower = 18000f;
        private float thermalConductivity = 1f;

        private float parentArea = 4f;

        public float ProduceEnergy
        {
            get
            {
                if (heatStorage <= 0)
                    return produceEnergyPerSec / 60f;
                if (heatStorage <= 50)
                    return produceEnergyPerSec / 60f * (Mathf.Pow(heatStorage / 40f, 3f) + 1f);
                if (heatStorage <= 100)
                    return produceEnergyPerSec / 60f * (Mathf.Pow((heatStorage - 50f) / 40f, 3f) + 2.953125f);
                if (heatStorage <= 150)
                    return produceEnergyPerSec / 60f * (Mathf.Pow((heatStorage - 100f) / 40f, 3f) + 4.90625f);
                if (heatStorage <= 200)
                    return produceEnergyPerSec / 60f * (Mathf.Pow((heatStorage - 150f) / 40f, 3f) + 6.859375f);
                if (heatStorage < 225)
                    return produceEnergyPerSec / 60f * (8.8125f - (heatStorage - 200f) * 0.3525f);
                return 0f;
            }
        }

        //save


        private float energyStorage = 0f;
        private float heatStorage = 0;
        private bool active = false;
        private bool outSide = false;

        private float remainderSec = 0;



        public float EnergyStorage
        {
            private set
            {
                energyStorage = value;
            }
            get
            {
                return energyStorage;
            }
        }

        public float EnergyStorageMax
        {
            get
            {
                return energyStorageMax;
            }
        }



        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.EnergyWellProp)props;

            energyStorageMax = prop.energyStorageMax;
            produceEnergyPerSec = prop.produceEnergyPerSec;
            heatAccumulationRate = prop.heatAccumulationRate;
            thermalConductivity = prop.thermalConductivity;
            parentArea = parent.def.size.Area;
        }


        //节省计算量,产能在MapComponentTick里计算,过热在RareTick里计算
        public bool TryGetEnergy(float want, ref float MapCache, ref float totalWant)
        {
            bool disallowProduct = false;
            bool disallowOutPut = false;
            if (outSide)
            {
                energyStorage *= 0.999f;//每tick流失0.1%储量
                disallowProduct = true;//禁用产能
                //return false;
            }
            if ((heatStorage > 400f || !powerTrader.PowerOn) && active)
            {
                disallowProduct = true;
                disallowOutPut = true;//禁用能量输出
                active = false;
                powerTrader.powerOutputInt = 0;
                powerTrader.PowerOn = false;
                energyStorage = 0;
                //return false;
            }

            //先产能
            if (!disallowProduct && active)
            {
                float targetStorage = energyStorage + ProduceEnergy;
                if (targetStorage > energyStorageMax)
                {
                    targetStorage = energyStorageMax;
                }
                energyStorage = targetStorage;
            }

            if (disallowOutPut)
                return false;
            if (want == 0) return true;
            //提取能量
            if (energyStorage >= want)
            {
                MapCache += want;
                totalWant -= want;
                energyStorage -= want;
                return true;
            }
            else
            {
                MapCache += energyStorage;
                totalWant -= energyStorage;
                energyStorage = 0;
                return false;
            }
        }


        private bool HasEnoughEnergy()
        {
            float? num;
            if (powerTrader == null)
            {
                num = null;
            }
            else
            {
                PowerNet powerNet = powerTrader.PowerNet;
                num = ((powerNet != null) ? new float?(powerNet.CurrentStoredEnergy()) : null);
            }
            float? num2 = num;
            float num3 = activePower;
            return num2.GetValueOrDefault() >= num3 & num2 != null;
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002480 File Offset: 0x00000680
        public bool UseResources()
        {
            if (!this.HasEnoughEnergy())
            {
                return false;
            }
            float val = activePower;
            for (int i = 0; i < powerTrader.PowerNet.batteryComps.Count; i++)
            {
                CompPowerBattery compPowerBattery = powerTrader.PowerNet.batteryComps[i];
                float num = Math.Min(val, compPowerBattery.StoredEnergy);
                val -= num;
                compPowerBattery.DrawPower(num);
            }
            return true;
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            parent.Map.GetComponent<MapVoidEnergyNet>()?.AddWell(this);

            powerTrader = parent.GetComp<CompPowerTrader>();

            powerTrader.powerOutputInt = 0;

            //outSide = parent.GetRoom().UsesOutdoorTemperature;
        }

        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<MapVoidEnergyNet>()?.RemoveWell(this);
            base.PostDeSpawn(map);
        }

        private bool disableExplosionMessage = false;
        private bool disableOverHeatMessage = false;

        private float HeatDissipationCoefficient = 0f;


        public override void CompTickRare()
        {
            base.CompTickRare();

            HeatDissipationCoefficient = parentArea * 0.1f;
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(parent))
            {
                float sum = 0;
                if (cell.GetCover(parent.Map) != null)
                {
                    sum = cell.GetCover(parent.Map).def.fillPercent;
                }
                HeatDissipationCoefficient += (1f - sum) * 0.2f;
            }
            //HeatDissipationCoefficient /= parent.def.size.x * parent.def.size.z;

            outSide = parent.GetRoom().UsesOutdoorTemperature;
            //if (outSide)
            //{
            //    heatStorage *= 0.9f;
            //}

            //if (energyStorage / energyStorageMax > 0.5)
            //{

            //精确一点,分多次计算
            float roomTemperature = parent.AmbientTemperature;
            for (remainderSec += 4.16666667f; remainderSec >= 1f; remainderSec--)
            {
                if (active)
                {
                    if (heatStorage > 200f)//0.15是之前产能公式在200~201的导数
                        heatStorage += (heatAccumulationRate * ((heatStorage - 200f) * 0.15f + 8.8125f) * produceEnergyPerSec) / parentArea;
                    else
                        heatStorage += (heatAccumulationRate * ProduceEnergy * 60f) / parentArea;
                }

                //}
                //else
                //{
                //    heatStorage = Mathf.Max(0, heatStorage - 1f);
                //}
                //热交换
                if (heatStorage > roomTemperature)
                {
                    //交换的热量=温差*导热系数*散热面积
                    float dltHeat = (heatStorage - roomTemperature) * HeatDissipationCoefficient * thermalConductivity;

                    //自身降低的温度=热量/面积
                    heatStorage -= dltHeat / parentArea;
                    //外部提高的温度(其实也是热量/面积),直接把热量丢到房间里去就行了
                    parent.GetRoomGroup()?.PushHeat(heatStorage);

                    //float dltHeat = GenTemperature.ControlTemperatureTempChange(parent.Position, parent.Map, (heatStorage-parent.AmbientTemperature)*0.5f, 1000f);
                    //RoomGroup rg = parent.GetRoomGroup();
                    //rg.Temperature += dltHeat;

                    //heatStorage -= dltHeat *0.5f *(float)rg.CellCount/9f;//Mathf.Max(0, (heatStorage - parent.AmbientTemperature) * 0.2f);
                    //不低于环境温度
                    heatStorage = Mathf.Max(roomTemperature, heatStorage, 0);
                    //zzLib.Log.Message("环境" + parent.AmbientTemperature + " 热量" + heatStorage + " 环境升温" + dltHeat);
                }

            }
            

            //过热
            if (heatStorage > 250f && Rand.Chance(0.1f))
            {
                GenExplosion.DoExplosion(parent.Position, parent.Map, 3.9f, DamageDefOf.Bomb, null, 25, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);

                if (!disableExplosionMessage)
                {
                    Messages.Message("OverHeatAndExplosion".Translate(), new LookTargets(parent), MessageTypeDefOf.ThreatSmall);
                    disableExplosionMessage = true;
                }
            }
            if (heatStorage > 225 && !disableOverHeatMessage)
            {
                Messages.Message("OverHeatAndStop".Translate(), new LookTargets(parent), MessageTypeDefOf.NegativeEvent);
                disableOverHeatMessage = true;
            }
            if (heatStorage < 150)
            {
                disableOverHeatMessage = false;
                disableExplosionMessage = false;
            }
        }




        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!active)
                yield return new Command_Action
                {
                    defaultLabel = "ActiveEnergyWell".Translate(),
                    defaultDesc = "GizMos_ActiveWellDesc".Translate(activePower),
                    action = delegate ()
                    {
                        if (UseResources())
                        {
                            active = true;
                            powerTrader.powerOutputInt = -1000;
                            powerTrader.PowerOn = true;
                            IntVec3 loc = IntVec3.FromVector3(new Vector3((float)this.parent.Position.x, (float)this.parent.Position.y, (float)(this.parent.Position.z - 2)));
                            GenSpawn.Spawn(Resources.ThingDefs.EnergyWellActiving, loc, this.parent.Map, WipeMode.Vanish);
                        }
                        else
                        {
                            Messages.Message("DontHaveEnoughPower".Translate(), MessageTypeDefOf.SilentInput);
                        }
                    }
                };
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
                        energyStorage = energyStorageMax;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Clear",
                    action = delegate ()
                    {
                        energyStorage = 0;
                        heatStorage = 0;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Heat+1",
                    action = delegate ()
                    {
                        heatStorage += 1;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Heat+10",
                    action = delegate ()
                    {
                        heatStorage += 10;
                    }
                };
            }
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder str = new StringBuilder();
            if (!active)
            {
                str.AppendLine("EnergyWellInactive".Translate());
            }
            else if (outSide)
            {
                str.AppendLine("EnergyWellNeedRoomWithRoof".Translate());
            }
            else if (heatStorage > 225f)
            {
                str.AppendLine("OverHeatWarning".Translate());
            }
            else
            {
                str.AppendLine("GizMos_ProduceEnergyPerSec".Translate((ProduceEnergy * 60f).ToString("f3")));
            }
            str.AppendLine("VoidEnergyCache".Translate(energyStorage.ToString("f1"), energyStorageMax.ToString("f2")));
            str.AppendLine("EnergyWellHeatLabel".Translate(heatStorage.ToString("f2")));
            str.Append("HeatDissipationCoefficient".Translate((HeatDissipationCoefficient * 100f).ToString("f0")));

            return str.ToString().Trim();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energyStorage, "energyStorage", 0f);
            Scribe_Values.Look(ref heatStorage, "heatStorage", 0);
            Scribe_Values.Look(ref remainderSec, "remainderSec", 0);
            Scribe_Values.Look(ref active, "active", false);
            Scribe_Values.Look(ref outSide, "outSide", false);

        }



    }
}
