using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;

namespace zhuzi.AdvancedEnergy.EnergyWell.AI
{
    class JobDriver_OperateVoidNetDeepDrill : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x06002CDA RID: 11482 RVA: 0x000FE951 File Offset: 0x000FCB51
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Uninstall);
            this.FailOn(() => !this.job.targetA.Thing.TryGetComp<Comp.VoidNetDeepDrill>().CanDrillNow());
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil work = new Toil();
            work.tickAction = delegate ()
            {
                Pawn actor = work.actor;
                ((Building)actor.CurJob.targetA.Thing).GetComp<Comp.VoidNetDeepDrill>().DrillWorkDone(actor);
                actor.skills.Learn(SkillDefOf.Mining, 0.065f, false);
            };
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            work.activeSkill = (() => SkillDefOf.Mining);
            yield return work;
            yield break;
        }
    }
}
