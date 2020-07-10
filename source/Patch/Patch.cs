using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace zhuzi.AdvancedEnergy.EnergyWell.Patch
{

    public class Methons
    {


        public static float? warmupTime_Before;
        public static float? warmupTime_Target;
        public static int? ticksBetweenBurstShots_Before;
        public static int? ticksBetweenBurstShots_Target;

        public static float? adjustedAccuracyFactor;
        public static float NextAdjustedAccuracyFactor
        {
            set
            {
                adjustedAccuracyFactor = value;
            }
        }


        public static float? rangedWeapon_Cooldown;
        /// <summary>
        /// 修改武器完成射击后的僵直秒数
        /// </summary>
        public static float NextRangedWeapon_Cooldown
        {
            set
            {
                rangedWeapon_Cooldown = value;
            }
        }

        public static float? rangedWeapon_Cooldown_Prop;
        public static float NextRangedWeapon_Cooldown_Prop
        {
            set
            {
                rangedWeapon_Cooldown_Prop = value;
            }
        }
        /// <summary>
        /// 修改武器瞄准的基础时间
        /// </summary>
        public static float NextWarmupTime
        {
            set
            {
                warmupTime_Target = value;
            }
        }
        /// <summary>
        /// 没实装,因为只能修改下一发子弹的
        /// </summary>
        public static int NextTicksBetweenBurstShots
        {
            set
            {
                ticksBetweenBurstShots_Target = value;
            }
        }
    }




    [HarmonyPatch(typeof(Pawn_EquipmentTracker))]
    internal class Pawn_EquipmentTracker_TryDropEquipment_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("TryDropEquipment")]
        public static void TryDropEquipment_Postfix(Pawn_EquipmentTracker __instance, ref bool __result, 
            ThingWithComps eq,ref ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
        {
            if (!__result)
            {
                return;
            }
            Comp.VoidNetEquipmentPort cp1 = eq.TryGetComp<Comp.VoidNetEquipmentPort>();
            Comp.VoidNetEquipmentPort cp2 = resultingEq.TryGetComp<Comp.VoidNetEquipmentPort>();
            
            if(cp1!=null && cp2 != null)
            {
                cp1.Notify_Dropped(__instance.pawn);

            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("Notify_EquipmentRemoved")]
        public static void Notify_EquipmentRemoved_Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            Comp.VoidNetEquipmentPort cp1 = eq.TryGetComp<Comp.VoidNetEquipmentPort>();
            if (cp1 != null)
            {
                cp1.Notify_Dropped(__instance.pawn);
            }
        }

    }

    //[HarmonyPatch(typeof(Building_TurretGun))]
    //internal class Building_TurretGun_Patch
    //{
    //    [HarmonyPostfix]
    //    [HarmonyPatch("BurstCooldownTime")]
    //    public static void BurstCooldownTime_Postfix(ref float __result)
    //    {
    //        if (Methons.rangedWeapon_Cooldown_Prop != null)
    //        {
    //            __result = ((float)Methons.rangedWeapon_Cooldown_Prop);
    //            Methons.rangedWeapon_Cooldown_Prop = null;
    //        }
    //    }

    //}

    [HarmonyPatch(typeof(VerbProperties))]
    internal class VerbProperties_Patch
    {

        private enum RangeCategory : byte
        {
            // Token: 0x040048E7 RID: 18663
            Touch,
            // Token: 0x040048E8 RID: 18664
            Short,
            // Token: 0x040048E9 RID: 18665
            Medium,
            // Token: 0x040048EA RID: 18666
            Long
        }

        /// <summary>
        /// 用于修改射击后的僵直时间
        /// </summary>
        /// <param name="__result"></param>
        [HarmonyPostfix]
        [HarmonyPatch("AdjustedCooldownTicks")]
        public static void AdjustedCooldownTicks_Postfix(ref int __result)
        {
            if (Methons.rangedWeapon_Cooldown_Prop != null)
            {
                __result = ((float)Methons.rangedWeapon_Cooldown_Prop).SecondsToTicks();
                Methons.rangedWeapon_Cooldown_Prop = null;
            }
        }

        /// <summary>
        /// 用于修改每发子弹的射击精度
        /// </summary>
        /// <param name="__result"></param>
        [HarmonyPostfix]
        [HarmonyPatch("AdjustedAccuracy")]
        public static void AdjustedAccuracy_Postfix(ref float __result)
        {
            if (Methons.adjustedAccuracyFactor != null)
            {
                __result *= (float)(1f + Methons.adjustedAccuracyFactor);
            }
        }

    }
    [HarmonyPatch(typeof(PlaceWorker_ShowTurretRadius))]
    internal class PlaceWorker_ShowTurretRadius_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AllowsPlacing")]
        
        public static bool PlaceWorker_ShowTurretRadius_Prefix(ref AcceptanceReport __result ,BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            __result = true;


            VerbProperties verbProperties = ((ThingDef)checkingDef).building.turretGunDef.Verbs.Find((VerbProperties v) => v.verbClass == typeof(Verbs.VoidNetWeapon_Lanuch));
            if (verbProperties == null) return true;
            if (verbProperties.range > 0f)
            {
                GenDraw.DrawRadiusRing(loc, verbProperties.range);
            }
            if (verbProperties.minRange > 0f)
            {
                GenDraw.DrawRadiusRing(loc, verbProperties.minRange);
            }
            return false;
        }
    }



    [HarmonyPatch(typeof(Verb))]
    internal class Verb_Patch
    {

        private static bool disableNext = false;


        [HarmonyPrefix]
        [HarmonyPatch("TryStartCastOn")]
        [HarmonyPatch(new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool) })]

        //瞄准时间改这个__instance.verbProps.warmupTime,改完要在postfix里改回去
        public static bool TryStartCastOn_Prefix(Verb __instance, ref bool __result,
            LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
        {
            if (__instance.EquipmentSource == null)
                return true;
            Comp.VoidNetEquipmentPort cp = __instance.EquipmentSource.TryGetComp<Comp.VoidNetEquipmentPort>();
            if (cp == null)
            {
                return true;
            }
            bool flag=true;
            if (!cp.TryStartCastOn(__instance, castTarg, destTarg, surpriseAttack, canHitNonTargetPawns))
            {
                flag = false;
                __result = false;
                //只有返回true的时候设置这个才有效,因为false根本不开火
                Methons.warmupTime_Target = null;
            }
            else if (Methons.warmupTime_Target != null)
            {
                Methons.warmupTime_Before = __instance.verbProps.warmupTime;
                __instance.verbProps.warmupTime = (float)Methons.warmupTime_Target;
                Methons.warmupTime_Target = null;
            }
            return flag;
        }

        [HarmonyPostfix]
        [HarmonyPatch("TryStartCastOn")]
        [HarmonyPatch(new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool) })]
        public static void TryStartCastOn_Postfix(Verb __instance, ref bool __result,
            LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
        {
            if (Methons.warmupTime_Before != null)
            {
                __instance.verbProps.warmupTime = (float)Methons.warmupTime_Before;
                Methons.warmupTime_Before = null;
            }
        }




        [HarmonyPostfix]
        [HarmonyPatch("Available")]

        public static void Available_Postfix(ref bool __result)
        {
            if (disableNext)
            {
                disableNext = false;
                __result = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("TryCastNextBurstShot")]
        //子弹射出间隔时间改这个__instance.verbProps.ticksBetweenBurstShots,改完要在postfix里改回去
        //射击后僵直改__instance.EquipmentSource的StatDefOf.RangedWeapon_Cooldown,改完要在postfix里改回去
        public static bool TryCastNextBurstShot_Prefix(Verb __instance,ref int ___burstShotsLeft)
        {
            if (__instance.EquipmentSource == null)
                return true;
            Comp.VoidNetEquipmentPort cp = __instance.EquipmentSource.TryGetComp<Comp.VoidNetEquipmentPort>();
            if (cp == null) return true;
            if (!cp.TryCastNextBurstShot(__instance, ref ___burstShotsLeft))
            {
                //仍然正常进函数,但是下一次Available会返回false,让他射不出去
                ___burstShotsLeft = 1;
                disableNext = true;
            }
            //剩余射击数为0的时候看看要不要修改rangedWeapon_Cooldown
            if (___burstShotsLeft == 1 && Methons.rangedWeapon_Cooldown != null)
            {
                Methons.NextRangedWeapon_Cooldown_Prop = Math.Max((float)Methons.rangedWeapon_Cooldown, __instance.verbProps.ticksBetweenBurstShots.TicksToSeconds());
                Methons.rangedWeapon_Cooldown = null;
            }
            return true;
        }
        //[HarmonyPostfix]
        //[HarmonyPatch("TryCastNextBurstShot")]
        //public static void TryCastNextBurstShot_Postfix(Verb __instance, ref int ___burstShotsLeft)
        //{

        //    if (ticksBetweenBurstShots_Before != null)
        //    {
        //        __instance.verbProps.ticksBetweenBurstShots = (int)ticksBetweenBurstShots_Before;
        //        ticksBetweenBurstShots_Before = null;
        //    }
        //}


    }



}
