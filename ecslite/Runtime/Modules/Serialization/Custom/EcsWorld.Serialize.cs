using System;
using Saro.Entities.Serialization;
using Saro.FSnapshot;
using Saro.Utility;

namespace Saro.Entities
{
    partial class EcsWorld
    {
        public void Deserialize(ref FSnapshotReader reader)
        {
            m_EntitiesCount = reader.ReadUnmanagedArray(ref m_Entities);
            m_RecycledEntitiesCount = reader.ReadUnmanagedArray(ref m_RecycledEntities);

            {
                reader.ReadUnmanaged(ref m_PoolsCount);
                int poolId = 1; // 0号index，肯定是 Dummy，可以忽略掉
                for (; poolId < m_PoolsCount; poolId++)
                {
                    var pool = m_Pools[poolId];
                    //if (pool == null) continue; // TODO 没有对象，就需要创建
                    pool.Deserialize(ref reader);
                }
                for (; poolId < m_Pools.Length; poolId++)
                {
                    var pool = m_Pools[poolId];
                    if (pool == null) continue;
                    pool.Reset();
                }
            }

            {
                var filterCount = 0;
                reader.ReadUnmanaged(ref filterCount);
                int filterIndex = 0;
                for (; filterIndex < filterCount; filterIndex++)
                {
                    var filter = m_AllFilters[filterIndex];
                    filter.Deserialize(ref reader);
                }
                for (; filterIndex < m_AllFilters.Count; filterIndex++)
                {
                    var filter = m_AllFilters[filterIndex];
                    if (filter == null) continue;
                    filter.Reset();
                }
            }
        }

        public void Serialize(ref FSnapshotWriter writer)
        {
            writer.WriteUnmanagedArray(m_Entities, m_EntitiesCount);
            writer.WriteUnmanagedArray(m_RecycledEntities, m_RecycledEntitiesCount);

            writer.WriteUnmanaged(m_PoolsCount);
            for (int poolId = 1; poolId < m_PoolsCount; poolId++) // 0号pool是Dummy，忽略掉
            {
                var pool = m_Pools[poolId];
                pool.Serialize(ref writer);
            }
            writer.WriteUnmanaged(m_AllFilters.Count);
            for (int i = 0; i < m_AllFilters.Count; i++)
            {
                var filter = m_AllFilters[i];
                filter.Serialize(ref writer);
            }
        }
    }

    partial class EcsFilter
    {
        private int m_SparseItemsCount => Math.Min(m_World.m_EntitiesCount, m_SparseItems.Length);

        public void Deserialize(ref FSnapshotReader reader)
        {
            m_DenseItemsCount = reader.ReadUnmanagedArray(ref m_DenseItems);
            m_DelayedOpsCount = reader.ReadUnmanagedArray(ref m_DelayedOps);
            reader.ReadUnmanagedArray(ref m_SparseItems);
        }

        public void Serialize(ref FSnapshotWriter writer)
        {
            writer.WriteUnmanagedArray(m_DenseItems, m_DenseItemsCount);
            writer.WriteUnmanagedArray(m_DelayedOps, m_DelayedOpsCount);
            writer.WriteUnmanagedArray(m_SparseItems, m_SparseItemsCount);
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
