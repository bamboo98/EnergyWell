﻿
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace zhuzi.AdvancedEnergy.EnergyWell
{
    public class Manager : Mod
    {
        public ModContentPack Pack { get; }

        private static Setting setting = new Setting();

        public readonly Harmony harmony;
        
        public Manager(ModContentPack content) : base(content)
        {
            Pack = content;
            setting = GetSettings<Setting>();
            zzLib.Log.Message("VoidEnergy Init");
            harmony = new Harmony("com.zhuzi.VoidEnergy");
            harmony.PatchAll();
        }
        public override string SettingsCategory() => Pack.Name;
        public override void DoSettingsWindowContents(Rect inRect) => setting.DoWindowContents(inRect);

    }


    public class Setting : ModSettings
    {
        //存档field
        public static bool DEBUG_MODE = false;


        public override void ExposeData()
        {
            base.ExposeData();

        }


        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);



            list.End();
        }



    }
}
