using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace zhuzi.AdvancedEnergy.EnergyWell.Resources
{
    [StaticConstructorOnStartup]
    internal class Texture2Ds
    {

        public static readonly Texture2D LimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Limited", true);
        public static readonly Texture2D UnlimitedTex = ContentFinder<Texture2D>.Get("UI/Icons/EntropyLimit/Unlimited", true);
        public static readonly Texture2D PsyfocusTargetTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));
        public static readonly Texture2D Icon_OneShoot = ContentFinder<Texture2D>.Get("Icon_OneShoot", true);
        public static readonly Texture2D Icon_ThreeShoot = ContentFinder<Texture2D>.Get("Icon_ThreeShoot", true);
        public static readonly Texture2D Icon_FullAuto = ContentFinder<Texture2D>.Get("Icon_FullAuto", true);
        public static readonly Texture2D FullVoidBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(70f / 255f, 0, 112f / 255f));
        public static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0f / 255f, 90f / 255f, 75f / 255f));

        public static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }

    [StaticConstructorOnStartup]
    internal class Materials
    {

        public static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);//, new Color(40f / 255f, 0, 115f / 255f));
        public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));
    }

    [DefOf]
    public static class ThingDefs
    {
        public static ThingDef VoidNetDeepDrill;
        public static ThingDef VoidNetTerminal_Scout;
        public static ThingDef EnergyWellActiving;
    }

    [DefOf]
    public static class JobDefs
    {
        public static JobDef OperateVoidNetDeepDrill;
    }

    [DefOf]
    public static class StatDefs
    {
        /// <summary>
        /// 幽能缓存上限
        /// </summary>
        public static StatDef VoidEnergyCacheMax;
        /// <summary>
        /// 幽能回充速率
        /// </summary>
        public static StatDef VoidEnergyRecharge;
        /// <summary>
        /// 护盾上限
        /// </summary>
        public static StatDef VoidEnergyShieldMax;
        /// <summary>
        /// 护盾回充速率
        /// </summary>
        public static StatDef VoidEnergyShieldRecharge;
        /// <summary>
        /// 护盾转化率
        /// </summary>
        public static StatDef VoidEnergyShieldConvertRate;
        /// <summary>
        /// 护盾韧性
        /// </summary>
        public static StatDef VoidEnergyShieldStrength;

        public static StatDef VoidEnergyCostPerSec;
    }

    [DefOf]
    public static class StatCategoryDefs
    {
        public static StatCategoryDef VoidEnergy;
    }

    [DefOf]
    public static class HediffDefs
    {
        public static HediffDef VoidEnergyHediffTerminalMoveSpeed;
    }

}
