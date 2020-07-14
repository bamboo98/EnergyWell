﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class VoidNetDeepDrill:VoidNetBuildCompBase
    {

        private Prop.VoidNetDeepDrillProp prop;

        // Token: 0x04002CE7 RID: 11495
        private float portionProgress;

        // Token: 0x04002CE8 RID: 11496
        private float portionYieldPct;

        // Token: 0x04002CE9 RID: 11497
        private int lastUsedTick = -99999;

        // Token: 0x04002CEA RID: 11498
        private float WorkPerPortionBase = 10000f;

        public float ProgressToNextPortionPercent
        {
            get
            {
                return this.portionProgress / WorkPerPortionBase;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.VoidNetDeepDrillProp)props;

            WorkPerPortionBase = prop.WorkPerPortionBase;
        }


        // Token: 0x060050E2 RID: 20706 RVA: 0x001B2486 File Offset: 0x001B0686
        public override void PostExposeData()
        {
            Scribe_Values.Look<float>(ref this.portionProgress, "portionProgress", 0f, false);
            Scribe_Values.Look<float>(ref this.portionYieldPct, "portionYieldPct", 0f, false);
            Scribe_Values.Look<int>(ref this.lastUsedTick, "lastUsedTick", 0, false);
        }

        // Token: 0x060050E3 RID: 20707 RVA: 0x001B24C8 File Offset: 0x001B06C8
        public void DrillWorkDone(Pawn driller)
        {
            float statValue = driller.GetStatValue(StatDefOf.DeepDrillingSpeed, true);
            this.portionProgress += statValue;
            this.portionYieldPct += statValue * driller.GetStatValue(StatDefOf.MiningYield, true) / WorkPerPortionBase;
            this.lastUsedTick = Find.TickManager.TicksGame;
            if (this.portionProgress > WorkPerPortionBase)
            {
                this.TryProducePortion(this.portionYieldPct);
                this.portionProgress = 0f;
                this.portionYieldPct = 0f;
            }
        }

        // Token: 0x060050E4 RID: 20708 RVA: 0x001B2550 File Offset: 0x001B0750
        public override void PostDeSpawn(Map map)
        {
            this.portionProgress = 0f;
            this.portionYieldPct = 0f;
            this.lastUsedTick = -99999;
        }

        // Token: 0x060050E5 RID: 20709 RVA: 0x001B2574 File Offset: 0x001B0774
        private void TryProducePortion(float yieldPct)
        {
            ThingDef thingDef;
            int num;
            IntVec3 intVec;
            bool nextResource = this.GetNextResource(out thingDef, out num, out intVec);
            if (thingDef == null)
            {
                return;
            }
            int num2 = Mathf.Min(num, thingDef.deepCountPerPortion);
            if (nextResource)
            {
                this.parent.Map.deepResourceGrid.SetAt(intVec, thingDef, num - num2);
            }
            int stackCount = Mathf.Max(1, GenMath.RoundRandom((float)num2 * yieldPct));
            Thing thing = ThingMaker.MakeThing(thingDef, null);
            thing.stackCount = stackCount;
            GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, null, default(Rot4));
            if (nextResource && !this.ValuableResourcesPresent())
            {
                if (DeepDrillUtility.GetBaseResource(this.parent.Map, this.parent.Position) == null)
                {
                    Messages.Message("DeepDrillExhaustedNoFallback".Translate(), this.parent, MessageTypeDefOf.TaskCompletion, true);
                    return;
                }
                Messages.Message("DeepDrillExhausted".Translate(Find.ActiveLanguageWorker.Pluralize(DeepDrillUtility.GetBaseResource(this.parent.Map, this.parent.Position).label, -1)), this.parent, MessageTypeDefOf.TaskCompletion, true);
                for (int i = 0; i < 21; i++)
                {
                    IntVec3 c = intVec + GenRadial.RadialPattern[i];
                    if (c.InBounds(this.parent.Map))
                    {
                        ThingWithComps firstThingWithComp = c.GetFirstThingWithComp<VoidNetDeepDrill>(this.parent.Map);
                        if (firstThingWithComp != null && !firstThingWithComp.GetComp<VoidNetDeepDrill>().ValuableResourcesPresent())
                        {
                            firstThingWithComp.SetForbidden(true, true);
                        }
                    }
                }
            }
        }

        // Token: 0x060050E6 RID: 20710 RVA: 0x001B2719 File Offset: 0x001B0919
        private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
        {
            return DeepDrillUtility.GetNextResource(this.parent.Position, this.parent.Map, out resDef, out countPresent, out cell);
        }

        // Token: 0x060050E7 RID: 20711 RVA: 0x001B2739 File Offset: 0x001B0939
        public bool CanDrillNow()
        {
            return (this.netPort == null || this.netPort.PowerOn) && (DeepDrillUtility.GetBaseResource(this.parent.Map, this.parent.Position) != null || this.ValuableResourcesPresent());
        }

        // Token: 0x060050E8 RID: 20712 RVA: 0x001B2778 File Offset: 0x001B0978
        public bool ValuableResourcesPresent()
        {
            ThingDef thingDef;
            int num;
            IntVec3 intVec;
            return this.GetNextResource(out thingDef, out num, out intVec);
        }

        // Token: 0x060050E9 RID: 20713 RVA: 0x001B2791 File Offset: 0x001B0991
        public bool UsedLastTick()
        {
            return this.lastUsedTick >= Find.TickManager.TicksGame - 1;
        }

        // Token: 0x060050EA RID: 20714 RVA: 0x001B27AC File Offset: 0x001B09AC
        public override string CompInspectStringExtra()
        {
            if (!this.parent.Spawned)
            {
                return null;
            }
            ThingDef thingDef;
            int num;
            IntVec3 intVec;
            this.GetNextResource(out thingDef, out num, out intVec);
            if (thingDef == null)
            {
                return "DeepDrillNoResources".Translate();
            }
            return "ResourceBelow".Translate() + ": " + thingDef.LabelCap + "\n" + "ProgressToNextPortion".Translate() + ": " + this.ProgressToNextPortionPercent.ToStringPercent("F0");
        }








    }
}
