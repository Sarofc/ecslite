using System;
using CsvHelper;
using Saro.Entities.Serialization;
using Saro.Utility;
using UnityEngine;

namespace Saro.Entities
{
    partial class EcsWorld
    {
        public void Deserialize(IEcsReader reader)
        {
            //m_EntitiesCount = reader.ReadInt32();
            //m_RecycledEntitiesCount = reader.ReadInt32();

            m_EntitiesCount = reader.ReadArrayUnmanaged(ref m_Entities);
            m_RecycledEntitiesCount = reader.ReadArrayUnmanaged(ref m_RecycledEntities);

            m_PoolsCount = reader.ReadInt32();
            int poolIndex = 0;
            for (; poolIndex < m_PoolsCount; poolIndex++)
            {
                var pool = m_Pools[poolIndex];
                if (pool == null) continue;
                pool.Deserialize(reader);
            }
            for (; poolIndex < m_Pools.Length; poolIndex++)
            {
                var pool = m_Pools[poolIndex];
                if (pool == null) continue;
                pool.Reset();
            }

            int filterIndex = 0;
            var filterCount = reader.ReadInt32();
            for (; filterIndex < filterCount; filterIndex++)
            {
                var filter = m_AllFilters[filterIndex];
                filter.Deserialize(reader);
            }
            for (; filterIndex < m_AllFilters.Count; filterIndex++)
            {
                var filter = m_AllFilters[filterIndex];
                if (filter == null) continue;
                filter.Reset();
            }
        }

        public void Serialize(IEcsWriter writer)
        {
            //writer.Write(m_EntitiesCount);
            //writer.Write(m_RecycledEntitiesCount);

            writer.WriteArrayUnmanaged(ref m_Entities, m_EntitiesCount);
            writer.WriteArrayUnmanaged(ref m_RecycledEntities, m_RecycledEntitiesCount);

            writer.Write(m_PoolsCount);
            for (int i = 0; i < m_PoolsCount; i++)
            {
                var pool = m_Pools[i];
                if (pool == null) continue;
                pool.Serialize(writer);
            }

            writer.Write(m_AllFilters.Count);
            for (int i = 0; i < m_AllFilters.Count; i++)
            {
                var filter = m_AllFilters[i];
                filter.Serialize(writer);
            }
        }
    }

    partial class EcsFilter
    {
        private int m_SparseItemsCount => Math.Min(m_World.m_EntitiesCount, m_SparseItems.Length);

        public void Deserialize(IEcsReader reader)
        {
            m_DenseItemsCount = reader.ReadArrayUnmanaged(ref m_DenseItems);
            m_DelayedOpsCount = reader.ReadArrayUnmanaged(ref m_DelayedOps);
            reader.ReadArrayUnmanaged(ref m_SparseItems);
        }

        public void Serialize(IEcsWriter writer)
        {
            writer.WriteArrayUnmanaged(ref m_DenseItems, m_DenseItemsCount);
            writer.WriteArrayUnmanaged(ref m_DelayedOps, m_DelayedOpsCount);
            writer.WriteArrayUnmanaged(ref m_SparseItems, m_SparseItemsCount);
        }

        public void Reset()
        {
            ArrayUtility.ClearFast(m_DenseItems, 0, m_DenseItemsCount);
            ArrayUtility.ClearFast(m_DelayedOps, 0, m_DelayedOpsCount);
            ArrayUtility.ClearFast(m_SparseItems, 0, m_SparseItemsCount);

            m_DenseItemsCount = 0;
            m_LockCount = 0;
            m_DelayedOpsCount = 0;
        }
    }
}
