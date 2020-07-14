using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    public abstract class VoidNetBuildCompBase:ThingComp
    {
        protected VoidNetPort netPort;
        private Prop.VoidNetBuildCompBaseProp prop;

        //是否注册为常驻耗能组件,只有注册为耗能组件的才会消耗能量
        protected bool registerToNetPort = true;
        //忽略最低储能限制
        protected bool ignoreSavingEnergy = false;


        //存档

        //待机时每秒耗能
        protected float energyCostPerSec = 0f;

        public virtual float EnergyCostPerTick
        {
            get
            {
                return energyCostPerSec / 60f;
            }
            set
            {
                energyCostPerSec = value * 60f;
            }
        }
        public virtual bool PowerOn
        {
            get
            {
                return netPort != null && netPort.PowerOn;
            }
        }

        public virtual float EnergyCostPerSec
        {
            get
            {
                return energyCostPerSec;
            }
            set
            {
                energyCostPerSec = value;
            }
        }


        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            prop = (Prop.VoidNetBuildCompBaseProp)props;

            registerToNetPort = prop.registerToNetPort;
            ignoreSavingEnergy = prop.ignoreSavingEnergy;
            energyCostPerSec = prop.energyCostPerSec;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            netPort = parent.TryGetComp<VoidNetPort>();
            //注册待机耗能
            if (netPort != null && registerToNetPort)
                netPort.RegisterToNetPort(this);


            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            netPort.RestoreFormNetPort(this);
            base.PostDeSpawn(map);
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            switch (signal)
            {
                case "ScheduledOn":
                    energyCostPerSec = prop.energyCostPerSec;
                    break;
                case "ScheduledOff":
                    energyCostPerSec = 0;
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 主动消耗能量(消耗值高于VoidNetPort组件的储能时只会返回false)
        /// </summary>
        /// <param name="count"></param>
        /// <returns>是否成功消耗</returns>
        public bool CostEnergy(float count)
        {
            if (netPort == null)
                return false;
            return netPort.CostEnergy(count);
        }
        /// <summary>
        /// 检测能否消耗能量值
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool CanCostEnergy(float count)
        {
            if (netPort == null)
                return false;
            return netPort.CanCostEnergy(count);
        }

        /// <summary>
        /// 能源不足离线时回调
        /// </summary>
        public virtual void NotifyPostOffline()
        {

        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref energyCostPerSec, "energyCostPerSec",0);
            base.PostExposeData();
        }


    }
}
