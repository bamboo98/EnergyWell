using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zzLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections;

namespace zhuzi.AdvancedEnergy.EnergyWell.Comp
{
    public class WorldVoidEnergyNet:GameComponent
    {
        private readonly Game game;
        public readonly List<EnergyWell> Wells = new List<EnergyWell>();
        public readonly List<VoidNetTower> Towers = new List<VoidNetTower>();
        public WorldVoidEnergyNet(Game g)
        {
            game = g;
        }

        public float EnergyCache = 0;
        public float EnergyCacheMax = 0;

        private readonly Queue<balanceInfo> balanceInfos = new Queue<balanceInfo>();
        //private float energyNeedTotal = 0;
        //private float usingNet = 0;

        private float energyNeedTotal
        {
            get
            {
                float sum = 0;
                foreach (balanceInfo item in balanceInfos)
                {
                    sum += item.value;
                }
                return sum;
            }
        }

        public override void GameComponentTick()
        {
            //energyNeedTotal = usingNet;
            //usingNet = 0;
            base.GameComponentTick();
            //统计下世界网络能缓存的最大能量
            EnergyCacheMax = 0;
            foreach (VoidNetTower comp in Towers)
            {
                if (!comp.TurnOn) continue;
                if (comp.TransportToWorld)
                {
                    EnergyCacheMax += comp.EnergyTransportPerSec;
                }
            }
            //缓存250ticks的能量,防止rareTick的comp不够用
            EnergyCacheMax *= 4.17f;

            int gt = Find.TickManager.TicksGame;
            while (balanceInfos.Count > 0)
            {
                if (gt - balanceInfos.Peek().tick > 249)
                {
                    balanceInfos.Dequeue();
                }
                else
                    break;
            }
        }
        public void AddWell(EnergyWell comp)
        {
            if (Wells.Any((c) => c == comp))
            {
                //zzLib.Log.Warning("World重复注册 at" + comp.parent.Position.ToString());
                return;
            }
            Wells.Add(comp);
            comp.WorldNetNode = this;
        }
        public void RemoveWell(EnergyWell comp)
        {
            Wells.Remove(comp);
            comp.WorldNetNode = null;
        }

        public void AddTower(VoidNetTower comp)
        {
            if (Towers.Any((c) => c == comp))
            {
                //zzLib.Log.Warning("World重复注册 at" + comp.parent.Position.ToString());
                return;
            }
            Towers.Add(comp);
            comp.WorldNetNode = this;
        }

        public void RemoveTower(VoidNetTower comp)
        {
            Towers.Remove(comp);
            comp.WorldNetNode = null;
        }

        public void AddEnergy(ref float count,float maxTransport)
        {
            if (EnergyCache >= EnergyCacheMax) return;
            float needEnergy = Mathf.Min(count, maxTransport, EnergyCacheMax - EnergyCache);
            count -= needEnergy;
            EnergyCache += needEnergy;
        }
        /// <summary>
        /// 从世界组件获取能量,不足时不获取,返回false
        /// </summary>
        /// <param name="count">获取的能量数量</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetEnergy(float count)
        {
            //usingNet += count;
            addBalanceInfo(count);
            if (EnergyCache<count)
                return false;
            EnergyCache -= count;
            return true;
        }
        /// <summary>
        /// 从世界组件获取能量,有多少就获取多少,不足时返回false
        /// </summary>
        /// <param name="count">想要多少能量</param>
        /// <returns>实际给了多少能量</returns>
        public float GetEnergy(float count)
        {
            //usingNet += count;
            addBalanceInfo(count);
            if (EnergyCache < count)
            {
                float real = EnergyCache;
                EnergyCache = 0;
                return real;
            }
            EnergyCache -= count;
            return count;
        }
        private void addBalanceInfo(float count)
        {
            if(count>0)
                balanceInfos.Enqueue(new balanceInfo
                {
                    tick = Find.TickManager.TicksGame,
                    value = count

                });
        }
        private float produceEnergyPerSec
        {
            get
            {
                float sum = 0;

                foreach (EnergyWell item in Wells)
                {
                    sum += item.ProduceEnergy * 60f;
                }

                return sum;
            }
        }
        public string GetBurdenStr()
        {
            if (EnergyCacheMax == 0)
                return "\n" + "NoVoidEnergyEnter".Translate();
            float storage = 0;
            float storageMax = 0;
            foreach (EnergyWell item in Wells)
            {
                storage += item.EnergyStorage;
                storageMax += item.EnergyStorageMax;
            }
            return "\n" + "VoidNetWorld".Translate() + "VoidNetLabel1".Translate(produceEnergyPerSec.ToString("f3")) +
                "\n" + "VoidNetLabel2".Translate((energyNeedTotal / EnergyCacheMax * 100f).ToString("f0")) +
                "\n" + "VoidNetLabel3".Translate(storage.ToString("f2"), storageMax.ToString("f2"));
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref EnergyCache, "EnergyCache", 0);

            base.ExposeData();
        }

    }
    struct balanceInfo
    {
        public int tick;
        public float value;
    }
    public class MapVoidEnergyNet : MapComponent
    {

        public readonly List<EnergyWell> Wells = new List<EnergyWell>();
        public readonly List<VoidNetTower> Towers = new List<VoidNetTower>();
        private readonly WorldVoidEnergyNet WorldNet;

        private Queue<balanceInfo> balanceInfos = new Queue<balanceInfo>();


        private bool hasNetNode = false;
        //save

        public float EnergyCache = 0;
        public float EnergyCacheMax = 0;

        //private float energyNeedTotal = 0;
        //private float usingNet = 0;

        private float energyNeedTotal
        {
            get
            {
                float sum = 0;
                foreach (balanceInfo item in balanceInfos)
                {
                    sum += item.value;
                }
                return sum;
            }
        }

        private float produceEnergyPerSec
        {
            get
            {
                float sum = 0;

                foreach (EnergyWell item in Wells)
                {
                    sum += item.ProduceEnergy * 60f;
                }

                return sum;
            }
        }

        public MapVoidEnergyNet(Map map) : base(map)
        {
            WorldNet = Current.Game.GetComponent<WorldVoidEnergyNet>();
            if (WorldNet == null)
            {
                zzLib.Log.Error("Cannot Find WorldVoidEnergyNet");
            }
            zzLib.Log.Message("VoidNet MapComp Init");

        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }




        public bool TryGetEnergy(float count)
        {
            //节点都没建立你玩个鸡掰
            if (!hasNetNode || Towers.Count == 0) return false;
            if (count <= 0) return true;
            //usingNet += count;
            addBalanceInfo(count);
            if (EnergyCache < count)
            {
                float part = count - EnergyCache;
                if (WorldNet.TryGetEnergy(part))
                {
                    EnergyCache = 0;
                    return true;
                }
                return false;
            }
            EnergyCache -= count;
            return true;
        }

        public float GetEnergy(float count)
        {
            //节点都没建立你玩个鸡掰
            if (!hasNetNode || Towers.Count == 0) return 0;
            if (count <= 0) return count;
            //usingNet += count;
            addBalanceInfo(count);
            if (EnergyCache < count)
            {
                float real = EnergyCache;
                EnergyCache = 0;
                return real+WorldNet.GetEnergy(count-real);
            }
            EnergyCache -= count;
            return count;
        }


        public override void MapComponentTick()
        {
            float wantMapCache = 0;
            float wantWorldCache = 0;
            //energyNeedTotal = usingNet;
            //usingNet = 0;

            //统计下要提取的能量和当前地图网络能缓存的最大能量
            foreach (VoidNetTower comp in Towers)
            {
                if (!comp.TurnOn) continue;
                wantMapCache += comp.EnergyTransportPerSec;
                if (comp.TransportToWorld)
                {
                    wantWorldCache += comp.EnergyTransportPerSec;
                }
            }

            //缓存250ticks的能量,防止rareTick的comp不够用
            EnergyCacheMax = wantMapCache*4.17f;

            //秒换成tick
            wantMapCache /= 60;
            wantWorldCache /= 60;

            //把冗余能量输送到世界节点
            if (EnergyCache>0)
                WorldNet.AddEnergy(ref EnergyCache,wantWorldCache);


            //从能量井里提取
            wantMapCache = Mathf.Min(wantMapCache, EnergyCacheMax - EnergyCache);
            List<EnergyWell> fullWells = Wells.ListFullCopy();
            List<int> waitToRemove = new List<int>();
            //要计算产能,先走一遍流程再说
            do
            {
                waitToRemove.Clear();
                float perWell = wantMapCache / (float)fullWells.Count;
                for (int i = 0; i < fullWells.Count; i++)
                {
                    if(!fullWells[i].TryGetEnergy(perWell, ref EnergyCache,ref wantMapCache))
                    {
                        waitToRemove.Insert(0,i);
                    }
                }
                foreach (int index in waitToRemove)
                {
                    fullWells.RemoveAt(index);
                }
            } while (wantMapCache > 0.0001 && fullWells.Count != 0) ;


            int gt = Find.TickManager.TicksGame;
            while (balanceInfos.Count > 0)
            {
                if (gt - balanceInfos.Peek().tick > 249)
                {
                    balanceInfos.Dequeue();
                }
                else
                    break;
            }
            base.MapComponentTick();
        }


        



        public void AddWell(EnergyWell comp)
        {
            if (Wells.Any((c) => c == comp))
            {
                //zzLib.Log.Warning("Map重复注册 at" + comp.parent.Position.ToString());
                return;
            }
            Wells.Add(comp);
            comp.MapNetNode = this;
            WorldNet.AddWell(comp);
        }
        public void RemoveWell(EnergyWell comp)
        {
            Wells.Remove(comp);
            comp.MapNetNode = null;
            WorldNet.RemoveWell(comp);
        }

        public void AddTower(VoidNetTower comp)
        {
            if (Towers.Any((c) => c == comp))
            {
                //zzLib.Log.Warning("World重复注册 at" + comp.parent.Position.ToString());
                return;
            }
            Towers.Add(comp);
            comp.MapNetNode = this;
            hasNetNode = true;
            WorldNet.AddTower(comp);
        }

        public void RemoveTower(VoidNetTower comp)
        {
            Towers.Remove(comp);
            comp.MapNetNode = null;
            hasNetNode = !Towers.NullOrEmpty();
            WorldNet.RemoveTower(comp);
        }



        private void addBalanceInfo(float count)
        {
            if (count > 0)
                balanceInfos.Enqueue(new balanceInfo
                {
                    tick = Find.TickManager.TicksGame,
                    value = count

                });
        }


        public string GetBurdenStr()
        {
            if (EnergyCacheMax == 0)
                return "\n" + "NoVoidEnergyEnter".Translate();
            float storage = 0;
            float storageMax = 0;
            foreach (EnergyWell item in Wells)
            {
                storage += item.EnergyStorage;
                storageMax += item.EnergyStorageMax;
            }
            return "\n" + "VoidNetMap".Translate() + "VoidNetLabel1".Translate(produceEnergyPerSec.ToString("f3")) +
                "\n" + "VoidNetLabel2".Translate((energyNeedTotal / EnergyCacheMax * 100f).ToString("f0")) +
                "\n" + "VoidNetLabel3".Translate(storage.ToString("f2"), storageMax.ToString("f2"));
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnergyCache, "EnergyCache", 0);


        }



    }


}
