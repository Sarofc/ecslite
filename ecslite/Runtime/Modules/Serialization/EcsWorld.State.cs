using System;
using Saro.Entities.Serialization;
using Saro.Utility;
using UnityEngine;

namespace Saro.Entities
{
    [Serializable]
    public class WorldState
    {
        public EcsWorld.EntityData[] entities;
        public int m_EntitiesCount;
        public int[] m_RecycledEntities;
        public int m_RecycledEntitiesCount;
        [SerializeReference]
        public BasePoolState[] m_Pools;
        public int m_PoolsCount;
    }

    partial class EcsWorld
    {
        public void SetWorldState(in WorldState state)
        {
            m_EntitiesCount = state.m_EntitiesCount;
            m_RecycledEntitiesCount = state.m_RecycledEntitiesCount;
            m_PoolsCount = state.m_PoolsCount;

            ArrayUtility.CopyFast(state.entities, 0, m_Entities, 0, m_EntitiesCount);
            ArrayUtility.CopyFast(state.m_RecycledEntities, 0, m_RecycledEntities, 0, m_RecycledEntitiesCount);

            SetPoolStates(state.m_Pools, m_Pools, m_PoolsCount);
        }

        public void GetWorldState(ref WorldState data)
        {
            EnsureWorldStateCapacity(ref data);

            data.m_EntitiesCount = m_EntitiesCount;
            data.m_RecycledEntitiesCount = m_RecycledEntitiesCount;
            data.m_PoolsCount = m_PoolsCount;

            ArrayUtility.CopyFast(m_Entities, 0, data.entities, 0, m_EntitiesCount);
            ArrayUtility.CopyFast(m_RecycledEntities, 0, data.m_RecycledEntities, 0, m_RecycledEntitiesCount);

            GetPoolStates(m_Pools, data.m_Pools, m_PoolsCount);
        }

        static Predicate<EntityData> s_Predicate = (EntityData d) => d.gen <= 0;

        private void EnsureWorldStateCapacity(ref WorldState state)
        {
            if (state == null)
            {
                state = new WorldState
                {
                    entities = new EntityData[m_EntitiesCount],
                    m_RecycledEntities = new int[m_RecycledEntitiesCount],
                    m_Pools = new BasePoolState[m_PoolsCount],
                };
            }
            else
            {
                if (state.entities == null || state.entities.Length < m_EntitiesCount)
                    state.entities = new EntityData[m_EntitiesCount];

                if (state.m_RecycledEntities == null || state.m_RecycledEntities.Length < m_RecycledEntitiesCount)
                    state.m_RecycledEntities = new int[m_RecycledEntitiesCount];

                if (state.m_Pools == null || state.m_Pools.Length < m_PoolsCount)
                    state.m_Pools = new BasePoolState[m_PoolsCount];
            }
        }

        private static void SetPoolStates(BasePoolState[] datas, IEcsPool[] pools, int datasCount)
        {
            //if(pools.Length < poolDatas.Length) 
            //    pools = new IEcsPool[]

            // TODO 反序列化时，可能存在datas有数据，pools没对象的问题
            // 这种情况下，需要根据data的type，创建pool实例
            // 如果是同一个world实例，那应该就没这个问问题

            int i = 0;
            for (; i < datasCount; i++)
            {
                var data = datas[i];
                var pool = pools[i];

                if (data == null)
                {
                    Log.ERROR($"data is null. id : {i}");
                    continue;
                }
                if (pool == null)
                {
                    Log.ERROR($"pool is null. poolId : {i}");
                    continue;
                }
                pool.SetPoolState(data);
            }

            for (; i < pools.Length; i++)
            {
                var pool = pools[i];
                if (pool != null)
                {
                    pool.Reset(); // 反序列化回来，多余的pool数据应该要全部Clear掉
                }
            }
        }

        private static void GetPoolStates(IEcsPool[] pools, BasePoolState[] datas, int poolsCount)
        {
            int i = 0;
            for (; i < poolsCount; i++)
            {
                var pool = pools[i];
                if (pool != null)
                {
                    pool.GetPoolState(ref datas[i]);
                }
                else
                {
                    Log.ERROR($"pool is null. poolId : {i}");
                }
            }
            //for (; i < datas.Length; i++)
            //{
            //    datas[i] = null;
            //}
        }
    }
}
