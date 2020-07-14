using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace zhuzi.AdvancedEnergy.EnergyWell.Things
{
    class EnergyWellActiving:ThingWithComps
    {
        // Token: 0x06000007 RID: 7 RVA: 0x000021B8 File Offset: 0x000003B8
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.Angle = AngleRange.RandomInRange;
            this.StartTick = Find.TickManager.TicksGame;
            base.GetComp<CompAffectsSky>().StartFadeInHoldFadeOut(30, this.Duration - 30 - 15, 15, 1f);
            base.GetComp<CompOrbitalBeam>().StartAnimation(this.Duration, 10, this.Angle);
            MoteMaker.MakeBombardmentMote(base.Position, base.Map);
            MoteMaker.MakePowerBeamMote(base.Position, base.Map);
        }

        // Token: 0x06000008 RID: 8 RVA: 0x0000224B File Offset: 0x0000044B
        public override void Tick()
        {
            base.Tick();
            if (this.TicksPassed >= this.Duration)
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002288 File Offset: 0x00000488
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.Duration, "Duration", 0, false);
            Scribe_Values.Look<float>(ref this.Angle, "Angle", 0f, false);
            Scribe_Values.Look<int>(ref this.StartTick, "StartTick", 0, false);
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000022D5 File Offset: 0x000004D5
        public override void Draw()
        {
            base.Comps_PostDraw();
        }

        // Token: 0x0600000B RID: 11 RVA: 0x000022E0 File Offset: 0x000004E0
        //private void StartRandomFire()
        //{
        //    FireUtility.TryStartFireIn((from x in GenRadial.RadialCellsAround(base.Position, 25f, true)
        //                                where x.InBounds(base.Map)
        //                                select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(base.Position))), base.Map, Rand.Range(0.1f, 0.925f));
        //}

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x0600000C RID: 12 RVA: 0x0000233B File Offset: 0x0000053B
        protected int TicksLeft
        {
            get
            {
                return this.Duration - this.TicksPassed;
            }
        }

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x0600000D RID: 13 RVA: 0x0000234A File Offset: 0x0000054A
        protected int TicksPassed
        {
            get
            {
                return Find.TickManager.TicksGame - this.StartTick;
            }
        }

        // Token: 0x04000004 RID: 4
        private static readonly SimpleCurve DistanceChanceFactor = new SimpleCurve
        {
            {
                new CurvePoint(0f, 1f),
                true
            },
            {
                new CurvePoint(10f, 0f),
                true
            }
        };

        // Token: 0x04000005 RID: 5
        private static readonly FloatRange AngleRange = new FloatRange(-12f, 12f);

        // Token: 0x04000006 RID: 6
        private float Angle;

        // Token: 0x04000007 RID: 7
        public int Duration = 180;

        // Token: 0x04000008 RID: 8
        private int StartTick;
    
    }
}
