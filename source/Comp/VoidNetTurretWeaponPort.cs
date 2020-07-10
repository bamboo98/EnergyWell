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
    /// <summary>
    /// 炮塔上武器专用接口
    /// </summary>
    class VoidNetTurretEquipmentPort: VoidNetEquipmentPort
    {

        private VoidNetPort compVoidNetPort;

        public Builds.VoidNetTurret turret;
        private VoidNetPort CompVoidNetPort
        {
            get
            {
                if (compVoidNetPort == null)
                {
                    compVoidNetPort = turret?.TryGetComp<VoidNetPort>();
                }
                return compVoidNetPort;
            }
        }

        //private float energyEachShot = 0.2f;
        //private int slientTick = -100;



        //private VoidNetWeaponShootMode compShootMode;
        //private VoidNetWeaponShootMode CompShootMode
        //{
        //    get
        //    {
        //        if(compShootMode==null)
        //            compShootMode = parent.TryGetComp<VoidNetWeaponShootMode>();
        //        return compShootMode;
        //    }
        //}


        //private float EnergyEachShot
        //{
        //    get
        //    {
        //        return energyEachShot * CompShootMode.EnergyCostRate;
        //    }
        //}





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
        public override bool TryStartCastOn(Verb __instance,
            LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
        {
            int nt = Find.TickManager.TicksGame;



            if (CompVoidNetPort == null)
            {
                return false;
            }
            if (!CompVoidNetPort.CanCostEnergy(EnergyEachShot))
            {
                if (nt > slientTick)
                {
                    MoteMaker.ThrowText(turret.Position.ToVector3(), turret.Map, "炮塔能量不足", Color.yellow, 2f);
                    slientTick = nt + 1800;
                }
                return false;
            }
            //炮塔的瞄准不在这里
            //if (CompShootMode != null)
            //{
            //    //zzLib.Log.
            //    CompShootMode.PostPreStartShoot();
            //}

            return true;

        }

        /// <summary>
        /// 是否可以射出下一发子弹(包括第一发)
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="burstShotsLeft">剩下可以射出的子弹数量</param>
        /// <returns>返回true则正常射出,可以修改burstShotsLeft,返回false则无法射出子弹,回到Idle状态,修改burstShotsLeft无效</returns>
        public override bool TryCastNextBurstShot(Verb __instance,ref int burstShotsLeft)
        {
            if (CompVoidNetPort == null)
                return false;
            if (CompVoidNetPort.CostEnergy(EnergyEachShot))
            {

                if (CompShootMode != null && burstShotsLeft == 1)
                {
                    CompShootMode.PostPreLastShoot();
                }
                return true;
            }
            return false;

        }


    }
}
