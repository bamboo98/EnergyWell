using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    public class VoidNetBuildShield:ThingComp
    {


        private VoidNetPort VoidNet;

        private float shieldMax = 200f;
        private float shieldRecharge = 5f;
        private float shieldConvertRate = 0.023f;
        private float shieldDamagedRate = 1f;
        private int shieldInitTick = 600;

        //save
        private float energyCur = 200f;
        private float shieldCur = 200;
        private int shieldInit = 600;
        //private int 


        public float ShieldMax
        {
            get
            {
                return shieldMax;
            }
        }
        public float ShieldCur
        {
            get
            {
                return shieldCur;
            }
        }
        public float ShieldInit
        {
            get
            {
                return shieldInit;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            //给护盾充能
            if (!VoidNet.IsSavingEnergy && shieldMax > 0 && shieldRecharge > 0)
            {
                if (shieldInit-- > 0)
                {
                    if (!VoidNet.CostEnergy(shieldRecharge / 60f * shieldConvertRate / 3f))
                    {
                        shieldInit = shieldInitTick;
                    }
                }
                else
                {
                    float need = Mathf.Min(shieldRecharge / 60f, shieldMax - shieldCur) * shieldConvertRate;
                    if (need > 0 && VoidNet.CostEnergy(need))
                    {
                        shieldCur += shieldRecharge / 60f;
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref energyCur, "energyCur", 0f);
            Scribe_Values.Look(ref shieldCur, "shieldCur", 0f);
            Scribe_Values.Look(ref shieldInit, "shieldInit", 600);
            Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
            base.PostExposeData();
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            VoidNet = parent.TryGetComp<VoidNetPort>();

            base.PostSpawnSetup(respawningAfterLoad);
        }

        private Vector3 impactAngleVect;
        private int lastKeepDisplayTick = -9999;
        private int lastAbsorbDamageTick = -9999;
        private int KeepDisplayingTicks = 1000;
        private bool ShouldDisplay
        {
            get
            {
                return (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks);
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (shieldMax > 0 && shieldInit <= 0 && ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, shieldCur / shieldMax);
                Vector3 vector = parent.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, Resources.Materials.BubbleMat, 0);
            }
        }
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (absorbed) return;

            float impact = dinfo.Amount * shieldDamagedRate;
            if (impact > shieldCur)
            {
                dinfo.SetAmount(-shieldCur);
                shieldCur = 0;
                shieldInit = shieldInitTick;
                //剩余护盾相对冲击量越高,抵挡最后一次伤害的概率就越高
                absorbed = Rand.Chance(shieldCur / impact);
            }
            else
            {
                shieldCur -= impact;
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(parent.Position, parent.Map, false));
                impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                Vector3 loc = parent.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
                float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
                MoteMaker.MakeStaticMote(loc, parent.Map, ThingDefOf.Mote_ExplosionFlash, num);
                int num2 = (int)num;
                for (int i = 0; i < num2; i++)
                {
                    MoteMaker.ThrowDustPuff(loc, parent.Map, Rand.Range(0.8f, 1.2f));
                }
                int gt = Find.TickManager.TicksGame;
                lastAbsorbDamageTick = gt;
                lastKeepDisplayTick = gt;
                absorbed = true;
            }
        }



    }
}
