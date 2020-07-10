using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    class EnergyWell :ThingComp
    {




        private CompFlickable CompFlickable;

        public MapVoidEnergyNet MapNetNode;
        public WorldVoidEnergyNet WorldNetNode;

        //save
        private bool connectToWorld = false;

        private float energyStorage = 0f;

        private float energyStorageMax = 500f;

        private float produceEnergyPerSec = 0.1f;

        private float overStorageCount = 0;



        public float EnergyStorage
        {
            private set
            {
                energyStorage = value;
            }
            get
            {
                return energyStorage;
            }
        }

        public float EnergyStorageMax
        {
            get
            {
                return energyStorageMax;
            }
        }




        public bool ConnectToWorld {
            get
            {
                return connectToWorld;
            }
        }


        //节省计算量,产能在MapComponentTick里计算,过热在RareTick里计算
        public bool TryGetEnergy(float want,ref float MapCache,ref float totalWant)
        {
            //先产能
            if (CompFlickable.SwitchIsOn) { 
                float targetStorage = energyStorage + produceEnergyPerSec / 60;
                if (targetStorage > energyStorageMax)
                {
                    targetStorage = energyStorageMax;
                }
                energyStorage = targetStorage;
            }
            if (want == 0) return true;
            //提取能量
            if (energyStorage >= want)
            {
                MapCache += want;
                totalWant -= want;
                energyStorage -= want;
                return true;
            }
            else
            {
                MapCache += energyStorage;
                totalWant -= energyStorage;
                energyStorage = 0;
                return false;
            }
        }





        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            parent.Map.GetComponent<MapVoidEnergyNet>()?.AddWell(this);
            CompFlickable = parent.TryGetComp<CompFlickable>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<MapVoidEnergyNet>()?.RemoveWell(this);
            base.PostDeSpawn(map);
        }

        public override void CompTick()
        {
            base.CompTick();
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate ()
                    {
                        energyStorage = energyStorageMax;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Clear",
                    action = delegate ()
                    {
                        energyStorage = 0;
                        overStorageCount = 0;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Heat+100",
                    action = delegate ()
                    {
                        overStorageCount += 100;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Heat+1000",
                    action = delegate ()
                    {
                        overStorageCount += 1000;
                    }
                };
            }
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("幽能储量: " + energyStorage.ToString("f1") + " / " + energyStorageMax.ToString("f2"));
            str.Append("热能: " + overStorageCount);

            return str.ToString().Trim();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref connectToWorld, "connectToWorld", false);
            Scribe_Values.Look(ref energyStorage, "energyStorage", 0f);
            Scribe_Values.Look(ref overStorageCount, "overStorageCount", 0);

        }



    }
}
