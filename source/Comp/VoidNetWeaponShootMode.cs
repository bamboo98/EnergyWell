using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Reflection;
using zhuzi.AdvancedEnergy.EnergyWell.Resources;
using UnityEngine;

namespace zhuzi.AdvancedEnergy.EnergyWell
{
    public enum ShootMode
    {
        /// <summary>
        /// 单发,瞄准时间+30%,伤害+100%,耗能+125%,后摇+30%,精度+35%
        /// </summary>
        OneShoot,
        /// <summary>
        /// 三连发,全部100%
        /// </summary>
        ThreeShoot,
        /// <summary>
        /// 全自动,瞄准时间-100%,伤害-50%,耗能-35%,后摇-100%,精度-60%
        /// </summary>
        FullAuto
    }
}
namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{

    class VoidNetWeaponShootMode:ThingComp
    {
        private VerbProperties verbProperties;

        private Prop.VoidNetWeaponShootModeProp prop;

        //save

        public ShootMode WeaponShootMode = ShootMode.ThreeShoot;
        private bool enableFullAuto = false;
        private int lastShootTick = -9999;
        private int continuous = 0;
        private float baseAmount = 10f;
        private float baseEnergyCost = 1f;
        private float baseWarmupTime = 1f;
        private float baseCooldown = 0;
        private int baseTicksBetweenBurstShots = 6;
        private float baseAccuracy = 1f;

        private float Base_WarmupTime
        {
            get
            {
                if (verbProperties == null)
                {
                    foreach (VerbProperties item in parent.def.Verbs)
                    {
                        if (item.verbClass == typeof(Verbs.VoidNetWeapon_Lanuch))
                            verbProperties = item;
                    }
                }
                return verbProperties.warmupTime;
            }
        }


        /// <summary>
        /// 获取单次射出的最大子弹数量
        /// </summary>
        public int ShotsPerBurst
        {
            get
            {
                switch (WeaponShootMode)
                {
                    case ShootMode.OneShoot:
                        return 1;
                    case ShootMode.ThreeShoot:
                        return 3;
                    case ShootMode.FullAuto:
                        return 5;
                    default:
                        return 1;
                }
            }
        }

        public float EnergyCostRate
        {
            get
            {
                switch (WeaponShootMode)
                {
                    case ShootMode.OneShoot:
                        return baseEnergyCost * 2.25f;
                    case ShootMode.ThreeShoot:
                        return baseEnergyCost * 1f;
                    case ShootMode.FullAuto:
                        return baseEnergyCost * 0.65f;
                    default:
                        return baseEnergyCost;
                }
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.VoidNetWeaponShootModeProp)props;

            enableFullAuto = prop.enableFullAuto;
            baseEnergyCost = prop.baseEnergyCost;
            baseAmount = prop.baseAmount;
            baseWarmupTime = prop.baseWarmupTime;
            baseCooldown = prop.baseCooldown;
            baseTicksBetweenBurstShots = prop.baseTicksBetweenBurstShots;
            baseAccuracy = prop.baseAccuracy;

        }


        /// <summary>
        /// 在开始瞄准时触发
        /// </summary>
        /// <returns></returns>
        public virtual bool PostPreStartShoot()
        {
            switch (WeaponShootMode)
            {
                case ShootMode.OneShoot:
                    Patch.Methons.NextWarmupTime = baseWarmupTime * 1.3f;

                    break;
                case ShootMode.ThreeShoot:
                    Patch.Methons.NextWarmupTime = baseWarmupTime;
                    break;
                case ShootMode.FullAuto:
                    Patch.Methons.NextWarmupTime = 0;
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// 在非最后一发子弹射出之前,修改下一发子弹的射出间隔
        /// </summary>
        public virtual void PostPreNonLastShoot()
        {


            switch (WeaponShootMode)
            {
                case ShootMode.OneShoot:
                    Patch.Methons.NextTicksBetweenBurstShots = baseTicksBetweenBurstShots;

                    break;
                case ShootMode.ThreeShoot:
                    Patch.Methons.NextTicksBetweenBurstShots = Mathf.RoundToInt((float)baseTicksBetweenBurstShots*1.5f);
                    break;
                case ShootMode.FullAuto:
                    Patch.Methons.NextTicksBetweenBurstShots = baseTicksBetweenBurstShots;
                    break;
                default:
                    break;
            }

        }


        /// <summary>
        /// 最后一发子弹射出之前,修改僵直时间
        /// </summary>
        public virtual void PostPreLastShoot()
        {

            switch (WeaponShootMode)
            {
                case ShootMode.OneShoot:
                    Patch.Methons.NextRangedWeapon_Cooldown = baseCooldown * 1.3f;

                    break;
                case ShootMode.ThreeShoot:
                    Patch.Methons.NextRangedWeapon_Cooldown = baseCooldown;
                    break;
                case ShootMode.FullAuto:
                    Patch.Methons.NextRangedWeapon_Cooldown = 0;
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 在每发子弹射出之前触发,修改精度加成
        /// </summary>
        /// <returns></returns>
        public virtual bool PostPreEachShoot(Verb verb)
        {
            //Patch.Methons.NextAdjustedAccuracyFactor
            switch (WeaponShootMode)
            {
                case ShootMode.OneShoot:
                    Patch.Methons.NextAdjustedAccuracyFactor = 1.35f * baseAccuracy - 1f;
                    break;
                case ShootMode.ThreeShoot:
                    Patch.Methons.NextAdjustedAccuracyFactor = 1f * baseAccuracy - 1f; ;
                    break;
                case ShootMode.FullAuto:
                    int gt = Find.TickManager.TicksGame;
                    if (gt - lastShootTick < baseTicksBetweenBurstShots * 3) 
                    {
                        Patch.Methons.NextAdjustedAccuracyFactor = 0.4f * baseAccuracy + (continuous++ * 0.01f) - 1f;
                    }
                    else
                    {
                        Patch.Methons.NextAdjustedAccuracyFactor = 0.4f * baseAccuracy - 1f;
                        continuous = 0;
                    }
                    lastShootTick = gt;
                    break;
                default:
                    break;
            }
            return true;
        }

        private static FieldInfo weaponDamageMultiplier;
        /// <summary>
        /// 在投射物发射之后触发,可以修改伤害(反射)
        /// </summary>
        /// <param name="projectile"></param>
        public virtual void PostPostProjectileLaunch(Projectile projectile)
        {
            if (weaponDamageMultiplier == null)
            {
                weaponDamageMultiplier = typeof(Projectile).GetField("weaponDamageMultiplier", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
            }
            if (weaponDamageMultiplier == null)
                return;
            switch (WeaponShootMode)
            {
                case ShootMode.OneShoot:
                    weaponDamageMultiplier.SetValue(projectile, (float)weaponDamageMultiplier.GetValue(projectile) * 2f * baseAmount);
                    break;
                case ShootMode.ThreeShoot:
                    weaponDamageMultiplier.SetValue(projectile, (float)weaponDamageMultiplier.GetValue(projectile) * 1f * baseAmount);
                    break;
                case ShootMode.FullAuto:
                    weaponDamageMultiplier.SetValue(projectile, (float)weaponDamageMultiplier.GetValue(projectile) * 0.5f * baseAmount);
                    break;
                default:
                    break;
            }
        }

        public new virtual IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            switch (WeaponShootMode)
            {
                case ShootMode.OneShoot:

                    yield return new Command_Action
                    {
                        defaultLabel = "OneShoot",
                        defaultDesc = "当前: 单发模式\n瞄准时间+30%,伤害+100%,耗能+125%,后摇+30%,精度+35%\n点击切换到三连发",
                        icon= Texture2Ds.Icon_OneShoot,
                        
                        action = delegate ()
                        {
                            WeaponShootMode = ShootMode.ThreeShoot;
                        }
                    };
                    break;
                case ShootMode.ThreeShoot:
                    if(enableFullAuto)
                        yield return new Command_Action
                        {
                            defaultLabel = "ThreeShoot",
                            defaultDesc = "当前: 三连发模式\n点击切换到全自动",
                            icon = Texture2Ds.Icon_ThreeShoot,
                            action = delegate ()
                            {
                                WeaponShootMode = ShootMode.FullAuto;
                            }
                        };
                    else
                        yield return new Command_Action
                        {
                            defaultLabel = "ThreeShoot",
                            defaultDesc = "当前: 三连发模式\n点击切换到单发",
                            icon = Texture2Ds.Icon_ThreeShoot,
                            action = delegate ()
                            {
                                WeaponShootMode = ShootMode.OneShoot;
                            }
                        };
                    break;
                case ShootMode.FullAuto:
                    yield return new Command_Action
                    {
                        defaultLabel = "FullAuto",
                        defaultDesc = "当前: 全自动模式\n瞄准时间-100%,伤害-50%,耗能-35%,后摇-100%,精度-60%\n点击切换到单发",
                        icon = Texture2Ds.Icon_FullAuto,
                        iconAngle = Find.TickManager.TicksGame % 360,
                        action = delegate ()
                        {
                            WeaponShootMode = ShootMode.OneShoot;
                        }
                    };
                    break;
                default:
                    break;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref WeaponShootMode, "WeaponShootMode", ShootMode.ThreeShoot);
            Scribe_Values.Look(ref lastShootTick, "lastShootTick", -9999);
            Scribe_Values.Look(ref continuous, "continuous", 0);
            Scribe_Values.Look(ref enableFullAuto, "enableFullAuto", false);
            base.PostExposeData();
        }

    }
}
