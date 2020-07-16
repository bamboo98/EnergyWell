using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using zzLib.Comp;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    public class CompProp_UseVoidEnergyBeginning_Key : CompProperties_UseEffect
    {
        public CompProp_UseVoidEnergyBeginning_Key()
        {
            compClass = typeof(Comp_UseVoidEnergyBeginning_Key);
        }
        public List<ThingDef> byProduct;
        public float byProductChance;

        public ResearchProjectDef research;
        public bool allowRandomResearch;
        public float chance;

    }

    class Comp_UseVoidEnergyBeginning_Key: CompUseEffect
    {

        private CompProp_UseVoidEnergyBeginning_Key prop;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (CompProp_UseVoidEnergyBeginning_Key)props;
        }

        private CompProperties_UseEffect Props
        {
            get
            {
                return (CompProperties_UseEffect)this.props;
            }
        }
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);


            if (!Rand.Chance(prop.chance))
            {
                if (prop.byProduct != null && Rand.Chance(prop.byProductChance))
                {
                    Thing thing = ThingMaker.MakeThing(prop.byProduct.RandomElement());
                    GenPlace.TryPlaceThing(thing, usedBy.PositionHeld, usedBy.MapHeld, ThingPlaceMode.Direct, null, null, default(Rot4));
                    Messages.Message("GetByProduct".Translate(thing.Label), new LookTargets(thing), MessageTypeDefOf.PositiveEvent, true);
                    return;
                }

                Messages.Message("GetNothing".Translate(), MessageTypeDefOf.PositiveEvent, true);
                return;
            }

            if (!prop.research.IsFinished)
            {
                FinishInstantly(prop.research, usedBy);
                if (prop.byProduct != null)
                {
                    Thing thing = ThingMaker.MakeThing(prop.byProduct[0]);
                    GenPlace.TryPlaceThing(thing, usedBy.PositionHeld, usedBy.MapHeld, ThingPlaceMode.Direct, null, null, default(Rot4));
                    Messages.Message("GetByProduct".Translate(thing.Label), new LookTargets(thing), MessageTypeDefOf.PositiveEvent, true);
                }
                return;
            }
            if (!prop.allowRandomResearch)
            {
                Messages.Message("GetNothing".Translate(), MessageTypeDefOf.PositiveEvent, true);
                return;
            }
            ResearchProjectDef proj;
            if (TryRandomlyUnfinishedResearch(out proj))
            {
                int exp = Rand.Range(500, 3000);
                //不是这个函数
                //Find.ResearchManager.AddTechprints(proj, exp);
                Messages.Message("GetResearchTechExp".Translate("<还没做好>",exp), MessageTypeDefOf.PositiveEvent, true);

            }
        }


        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            failReason = null;
            return true;
        }

        private bool TryRandomlyUnfinishedResearch(out ResearchProjectDef researchProj)
        {
            return (from x in DefDatabase<ResearchProjectDef>.AllDefs
                    where !x.IsFinished
                    select x).TryRandomElement(out researchProj);
        }

        private void FinishInstantly(ResearchProjectDef proj, Pawn usedBy)
        {
            Find.ResearchManager.FinishProject(proj, false, usedBy);
            Messages.Message("MessageResearchProjectFinishedByItem".Translate(proj.label), MessageTypeDefOf.PositiveEvent, true);
        }


    }
}
