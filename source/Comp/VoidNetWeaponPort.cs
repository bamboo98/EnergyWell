using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class VoidNetEquipmentPort:ThingComp
    {

        public Things.VoidNetTerminal voidNetTerminal;
        protected int slientTick = -100;
        private int checkNetTick = -100;



        protected VoidNetWeaponShootMode compShootMode;
        public VoidNetWeaponShootMode CompShootMode
        {
            get
            {
                if(compShootMode==null)
                    compShootMode = parent.TryGetComp<VoidNetWeaponShootMode>();
                return compShootMode;
            }
        }




        protected float EnergyEachShot
        {
            get
            {
                return CompShootMode.EnergyCostRate;
            }
        }





        //生成物品和丢到地上都会触发
        //public override void PostSpawnSetup(bool respawningAfterLoad)
        //{
        //    base.PostSpawnSetup(respawningAfterLoad);
        //}




        //装备时触发
        public override void Notify_Equipped(Pawn pawn)
        {
            if (pawn != null)
            {
                voidNetTerminal = Things.VoidNetTerminal.FindTerminal(pawn);
                if (voidNetTerminal != null)
                {
                    voidNetTerminal.Online(this);
                }
            }
            base.Notify_Equipped(pawn);
        }
        //丢弃时触发(patch实现,可能不全)
        public virtual void Notify_Dropped(Pawn pawn)
        {
            if (pawn != null)
            {
                if (voidNetTerminal != null)
                {
                    voidNetTerminal.Offline(this);
                }
            }
        }

        //public override void Notify_UsedWeapon(Pawn pawn)
        //{

        //    base.Notify_UsedWeapon(pawn);
        //}


            /// <summary>
            /// 尝试瞄准
            /// </summary>
            /// <param name="__instance">Verb实例</param>
            /// <param name="castTarg"></param>
            /// <param name="destTarg"></param>
            /// <param name="surpriseAttack"></param>
            /// <param name="canHitNonTargetPawns"></param>
            /// <returns>是否可以瞄准</returns>
        public virtual bool TryStartCastOn(Verb __instance,
            LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
        {
            int nt = Find.TickManager.TicksGame;
            Pawn owner = __instance.CasterPawn;
            if (nt > checkNetTick)
            {
                checkNetTick = nt + 30;
                Things.VoidNetTerminal net = Things.VoidNetTerminal.FindTerminal(owner);
                if (net != null)
                {
                    if (voidNetTerminal == null || (voidNetTerminal != null && voidNetTerminal != net))
                    {
                        net.Online(this);
                    }
                }
                else if (voidNetTerminal != null)
                {
                    voidNetTerminal.Offline(this);
                }
            }

            if (voidNetTerminal == null)
                return false;
            if (!voidNetTerminal.CanCostEnergy(EnergyEachShot))
            {
                if (nt > slientTick)
                {
                    MoteMaker.ThrowText(owner.Position.ToVector3(), owner.Map, "TerminalOutOfEnergy".Translate(), Color.yellow, 2f);
                    slientTick = nt + 1800;
                }
                return false;
            }
            if (CompShootMode != null)
            {
                CompShootMode.PostPreStartShoot();
            }

            return true;

        }

        /// <summary>
        /// 是否可以射出下一发子弹(包括第一发)
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="burstShotsLeft">剩下可以射出的子弹数量</param>
        /// <returns>返回true则正常射出,可以修改burstShotsLeft,返回false则无法射出子弹,回到Idle状态,修改burstShotsLeft无效</returns>
        public virtual bool TryCastNextBurstShot(Verb __instance,ref int burstShotsLeft)
        {
            if (voidNetTerminal == null)
                return false;
            if (voidNetTerminal.CostEnergy(EnergyEachShot))
            {

                if (CompShootMode != null && burstShotsLeft == 1)
                {
                    CompShootMode.PostPreLastShoot();
                }
                if(compShootMode!=null && burstShotsLeft > 1)
                {
                    CompShootMode.PostPreNonLastShoot();
                }
                return true;
            }
            return false;

        }


    }
}
