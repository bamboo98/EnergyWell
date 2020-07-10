using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Reflection;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class VoidNetWeaponShootMode:ThingComp
    {
        private VerbProperties verbProperties;


        private float base_WarmupTime
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

        public ShootMode WeaponShootMode = ShootMode.ThreeShoot;



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
                        return 1;
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
                        return 2.25f;
                    case ShootMode.ThreeShoot:
                        return 1f;
                    case ShootMode.FullAuto:
                        return 0.65f;
                    default:
                        return 1f;
                }
            }
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
                    Patch.Methons.NextWarmupTime = base_WarmupTime * 1.3f;
                    Patch.Methons.NextRangedWeapon_Cooldown = parent.GetStatValue(StatDefOf.RangedWeapon_Cooldown, true) * 1.3f;

                    break;
                case ShootMode.ThreeShoot:
                    break;
                case ShootMode.FullAuto:
                    Patch.Methons.NextWarmupTime = 0;
                    Patch.Methons.NextRangedWeapon_Cooldown = 0;
                    break;
                default:
                    break;
            }
            return true;
        }
        /// <summary>
        /// 在每发子弹射出之前触发
        /// </summary>
        /// <returns></returns>
        public virtual bool PostPreEachShoot(Verb verb)
        {
            //Patch.Methons.NextAdjustedAccuracyFactor
            switch (WeaponShootMode)
            {
                case ShootMode.OneShoot:
                    Patch.Methons.NextAdjustedAccuracyFactor = 0.35f;
                    break;
                case ShootMode.ThreeShoot:
                    break;
                case ShootMode.FullAuto:
                    Patch.Methons.NextAdjustedAccuracyFactor = -0.6f;
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
                    weaponDamageMultiplier.SetValue(projectile, (float)weaponDamageMultiplier.GetValue(projectile)*2f);
                    break;
                case ShootMode.ThreeShoot:
                    break;
                case ShootMode.FullAuto:
                    weaponDamageMultiplier.SetValue(projectile, (float)weaponDamageMultiplier.GetValue(projectile) * 0.5f);
                    break;
                default:
                    break;
            }
        }


        public override void PostExposeData()
        {
            Scribe_Values.Look(ref WeaponShootMode, "WeaponShootMode",ShootMode.ThreeShoot);
            base.PostExposeData();
        }

    }
}
