using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class VoidNetAutoRepair:ThingComp
    {

        public float RepairRatePerSec = 0.01f;// *血量上限/转换率=消耗幽能
        public float VoidEnergyConvertRate = 500f;


        private MapVoidEnergyNet VoidNet;
        private int MaxHitPoints;
        private bool TwiceCheck;


        //存档field
        private bool repairing = false;
        private int lastRepairTick = -100;
        private float remainder = 0f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            VoidNet = parent.Map.GetComponent<MapVoidEnergyNet>();
            if (!parent.def.useHitPoints)
                MaxHitPoints = 0;
            else
                MaxHitPoints = parent.MaxHitPoints;
            TwiceCheck = parent.def.tickerType == TickerType.Rare;
            base.PostSpawnSetup(respawningAfterLoad);
        }


        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            if (repairing || MaxHitPoints == 0) return;
            repairing = true;
            lastRepairTick = Find.TickManager.TicksGame;
        }

        private void TryRepair()
        {
            try
            {
                if (!repairing || VoidNet==null) return;
                int cur = parent.HitPoints;
                if (MaxHitPoints <= cur)
                {
                    repairing = false;
                    remainder = 0;
                    return;
                }

                int gt = Find.TickManager.TicksGame;

                float needHeal = Mathf.Min((float)MaxHitPoints * RepairRatePerSec * (gt - lastRepairTick).TicksToSeconds(), (float)(MaxHitPoints - cur) - remainder);

                float heal = VoidEnergyConvertRate *
                    VoidNet.GetEnergy(needHeal / VoidEnergyConvertRate) + remainder;//别忘了加上零头

                remainder = heal % 1;
                if (heal >= 1)
                {
                    parent.HitPoints = Mathf.Min(MaxHitPoints, cur + Mathf.FloorToInt(heal));
                }
                lastRepairTick = gt;
                if (TwiceCheck && MaxHitPoints <= parent.HitPoints)
                {
                    repairing = false;
                    remainder = 0;
                }
            }
            catch (System.Exception e)
            {
                if(Prefs.DevMode)
                    Log.Warning(e.Message);
                throw;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            TryRepair();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            TryRepair();
        }



        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref repairing, "repairing", false);
            Scribe_Values.Look(ref lastRepairTick, "lastRepairTick", -9999);
            Scribe_Values.Look(ref remainder, "remainder", 0);
        }

    }
}
