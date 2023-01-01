using System;
using System.Collections.Generic;
using System.IO;
using Saro.Entities.Serialization;

namespace Saro.Entities
{
    // TODO 改成包含 MGF ISerializationWriter/ISerializationReader 的方式，在world里直接处理了，不搞那么多类了，这样网络也能使用

    [Serializable]
    public class WorldData
    {
        public EcsWorld.EntityData[] entities;
        public int entitiesCount;
        public int[] recycledEntities;
        public int recycledEntitiesCount;
        public BasePoolData[] pools;
        public int poolsCount;
    }

    partial class EcsWorld
    {
        public void SetWorldData(WorldData data)
        {
            entities = data.entities;
            m_EntitiesCount = data.entitiesCount;
            m_RecycledEntities = data.recycledEntities;
            m_RecycledEntitiesCount = data.recycledEntitiesCount;
            DeserializePools(data.pools, m_Pools);
            //m_Pools = data.pools;
            m_PoolsCount = data.poolsCount;
        }

        public void GetWorldData(ref WorldData data)
        {
            data.entities = entities;
            data.entitiesCount = m_EntitiesCount;
            data.recycledEntities = m_RecycledEntities;
            data.recycledEntitiesCount = m_RecycledEntitiesCount;
            SerializePools(m_Pools, data.pools);
            //data.pools = m_Pools;
            data.poolsCount = m_PoolsCount;
        }

        private static void DeserializePools(BasePoolData[] datas, IEcsPool[] pools)
        {
            //if(pools.Length < poolDatas.Length) 
            //    pools = new IEcsPool[]

            for (int i = 0; i < datas.Length; i++)
            {
                var pool = pools[i];
                if (pool == null)
                    pool.SetPoolData(datas[i]);
            }
        }

        private static void SerializePools(IEcsPool[] pools, BasePoolData[] datas)
        {
            for (int i = 0; i < pools.Length; i++)
                pools[i].GetPoolData(ref datas[i]);
        }

        //public WorldData GetWorldData()
        //{
        //    var data = new WorldData();
        //    GetWorldData(ref data);
        //    return data;
        //}
    }
}
