using System;
using System.Buffers;
using Saro.Entities.Serialization;
using Saro.FSnapshot;
using Saro.Utility;

namespace Saro.Entities
{
    partial class EcsWorld
    {
        internal void Serialize(IBufferWriter<byte> bufferWriter = null)
        {
            bufferWriter ??= new ArrayBufferWriter<byte>(1024);

            var writer = new FSnapshotWriter(bufferWriter);
            try
            {
                Serialize(ref writer);
            }
            catch (Exception e)
            {
                Log.ERROR(e);
            }
            finally
            {
                writer.Dispose();
            }
        }

        internal void Deserialize(ReadOnlySpan<byte> buffer)
        {
            var reader = new FSnapshotReader(buffer);
            try
            {
                Deserialize(ref reader);
            }
            catch (Exception e)
            {
                Log.ERROR(e);
            }
            finally
            {
                reader.Dispose();
            }
        }

        private void Serialize(ref FSnapshotWriter writer)
        {
            writer.WriteUnmanagedSpan(m_Entities.AsSpan(0, m_EntitiesCount));
            writer.WriteUnmanagedSpan(m_RecycledEntities.AsSpan(0, m_RecycledEntitiesCount));

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

        private void Deserialize(ref FSnapshotReader reader)
        {
            Span<EntityData> m_EntitiesSpan = m_Entities;
            m_EntitiesCount = reader.ReadUnmanagedSpan(ref m_EntitiesSpan);
            Span<int> m_RecycledEntitiesSpan = m_RecycledEntities;
            m_RecycledEntitiesCount = reader.ReadUnmanagedSpan(ref m_RecycledEntitiesSpan);

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
    }

    partial class EcsFilter
    {
        private int m_SparseItemsCount => Math.Min(m_World.m_EntitiesCount, m_SparseItems.Length);

        internal void Deserialize(ref FSnapshotReader reader)
        {
            Span<int> m_DenseItemsSpan = m_DenseItems;
            m_DenseItemsCount = reader.ReadUnmanagedSpan(ref m_DenseItemsSpan);
            Span<DelayedOp> m_DelayedOpsSpan = m_DelayedOps;
            m_DelayedOpsCount = reader.ReadUnmanagedSpan(ref m_DelayedOpsSpan);
            Span<int> m_SparseItemsSpan = m_SparseItems;
            reader.ReadUnmanagedSpan(ref m_SparseItemsSpan);
        }

        internal void Serialize(ref FSnapshotWriter writer)
        {
            writer.WriteUnmanagedSpan(m_DenseItems.AsSpan(0, m_DenseItemsCount));
            writer.WriteUnmanagedSpan(m_DelayedOps.AsSpan(0, m_DelayedOpsCount));
            writer.WriteUnmanagedSpan(m_SparseItems.AsSpan(0, m_SparseItemsCount));
        }

        internal void Reset()
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
