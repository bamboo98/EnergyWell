using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    /*
     * 你以为真的是用Tower传输的?
     * 1.地图组件根据tower世界升级的数量,将地图缓存提升到世界缓存
     * 2.地图组件根据tower,从well中提取能量缓存
     * 3.耗电组件从地图缓存中提取能量,地图缓存不够时尝试提取世界缓存
     * 
     */
    public class VoidNetTower:ThingComp
    {
        public MapVoidEnergyNet MapNetNode;
        public WorldVoidEnergyNet WorldNetNode;
        private Prop.VoidNetTowerProp prop;

        private float energyTransportPerSec = 1f;
        private bool transportToWorld = false;

        public float EnergyTransportPerSec
        {
            get
            {
                return energyTransportPerSec;
            }
        }
        public bool TurnOn
        {
            get;
        } = true;

        public bool TransportToWorld
        {
            get
            {
                return transportToWorld;
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.VoidNetTowerProp)props;

            energyTransportPerSec = prop.energyTransportPerSec;
            transportToWorld = prop.transportToWorld;

        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            parent.Map.GetComponent<MapVoidEnergyNet>()?.AddTower(this);


            base.PostSpawnSetup(respawningAfterLoad);
        }
        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<MapVoidEnergyNet>()?.RemoveTower(this);
            base.PostDeSpawn(map);
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
                    defaultLabel = "DEBUG: transportToWorld",
                    action = delegate ()
                    {
                        transportToWorld = !transportToWorld;
                    }
                };
            }
            yield break;
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder str = new StringBuilder();
            str.Append("幽能传输率: " + energyTransportPerSec.ToString("f1") + " 单位/秒");
                //str.AppendLine("\n地图缓存: " + MapNetNode.EnergyCache.ToString("f3") + " / " + MapNetNode.EnergyCacheMax.ToString("f3"));
                //str.AppendLine("世界缓存: " + WorldNetNode.EnergyCache.ToString("f3") + " / " + WorldNetNode.EnergyCacheMax.ToString("f3"));
                str.Append(MapNetNode.GetBurdenStr());
                if (TransportToWorld)
                {
                    str.Append(WorldNetNode.GetBurdenStr());
                }
            return str.ToString();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energyTransportPerSec, "energyTransportPerSec");
            Scribe_Values.Look(ref transportToWorld, "transportToWorld");
        }

    }
}
