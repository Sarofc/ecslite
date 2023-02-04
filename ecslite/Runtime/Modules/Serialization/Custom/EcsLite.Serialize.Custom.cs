using System;
using System.Buffers;
using Saro.FSnapshot;
using Saro.Pool;
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

    partial interface IEcsPool
    {
        void Serialize(ref FSnapshotWriter writer);
        void Deserialize(ref FSnapshotReader reader);

        /// <summary>
        /// 重置所有的component数据
        /// </summary>
        void Reset();
    }

    partial class EcsPoolManaged<T>
    {
        private bool m_HasEcsSerializeInterface;

        private int m_SparseItemsCount => IsSingleton ? m_SparseItems.Length : m_World.m_EntitiesCount;

        private void InitPoolState()
        {
            m_HasEcsSerializeInterface = typeof(IFSnapshotable).IsAssignableFrom(m_Type);

            if (m_HasEcsSerializeInterface == false)
            {
                Log.WARN($"{m_Type.FullName} don't impl '{nameof(IFSnapshotable)}' interface. ");
            }
        }

        private int GetComponentIndex(int entity)
        {
            if (m_SparseItems.Length <= entity) return 0;
            return m_SparseItems[entity];
        }

        void IEcsPool.Deserialize(ref FSnapshotReader reader)
        {
            if (m_HasEcsSerializeInterface == false)
                return;

            var m_SparseItemsSpan = new Span<int>(m_SparseItems);
            var sparseItemsCount = reader.ReadUnmanagedSpan(ref m_SparseItemsSpan);
            Log.Assert(sparseItemsCount == m_SparseItemsCount, $"sparseItemsCount not equal. {nameof(sparseItemsCount)} != {nameof(m_SparseItemsCount)} {sparseItemsCount} != {m_SparseItemsCount}");

            var m_RecycledItemsSpan = new Span<int>(m_RecycledItems);
            m_RecycledItemsCount = reader.ReadUnmanagedSpan(ref m_RecycledItemsSpan);

            reader.ReadUnmanaged(ref m_DenseItemsCount);
            using var _ = HashSetPool<int>.Rent(out var used);
            for (int e = 1; e < m_World.m_EntitiesCount; e++)
            {
                var index = GetComponentIndex(e);
                if (index > 0)
                {
                    ref var c = ref m_DenseItems[index];
                    if (c == null) c = CreateComponentInstance(); //  没有对象，就需要创建
                    ((IFSnapshotable)c).RestoreSnapshot(ref reader);

                    used.Add(index);
                }
            }

#if DEBUG
            //Log.INFO($"component deserialize count: {used.Count} [{string.Join(",", used)}]");
#endif

            int componentIndex = 0;
            for (; componentIndex < m_DenseItemsCount; componentIndex++)
            {
                if (used.Contains(componentIndex)) continue;
                var c = m_DenseItems[componentIndex];
                if (c == null) continue;
                m_AutoReset?.Invoke(ref c);
            }
            for (; componentIndex < m_DenseItems.Length; componentIndex++)
            {
                var c = m_DenseItems[componentIndex];
                if (c == null) continue;
                m_AutoReset?.Invoke(ref c);
            }
        }

        void IEcsPool.Serialize(ref FSnapshotWriter writer)
        {
            if (m_HasEcsSerializeInterface == false)
                return;

            writer.WriteUnmanagedSpan(m_SparseItems.AsSpan(0, m_SparseItemsCount));
            writer.WriteUnmanagedSpan(m_RecycledItems.AsSpan(0, m_RecycledItemsCount));

#if DEBUG
            using var _ = HashSetPool<int>.Rent(out var used);
#endif
            writer.WriteUnmanaged(m_DenseItemsCount);
            for (int e = 1; e < m_World.m_EntitiesCount; e++)
            {
                var index = GetComponentIndex(e);
                if (index > 0)
                {
                    ref var c = ref m_DenseItems[index];
                    ((IFSnapshotable)c).TakeSnapshot(ref writer);
#if DEBUG
                    used.Add(index);
#endif
                }
            }
            //Log.INFO($"component serialize count: {used.Count} [{string.Join(",", used)}]");
        }

        void IEcsPool.Reset()
        {
            ArrayUtility.ClearFast(m_SparseItems);
            ArrayUtility.ClearFast(m_RecycledItems);

            for (int i = 0; i < m_DenseItems.Length; i++)
            {
                var c = m_DenseItems[i];

                if (m_AutoReset != null)
                    m_AutoReset(ref c);
            }

            m_DenseItemsCount = 1;
            m_RecycledItemsCount = 0;
        }
    }

    partial class EcsPoolUnmanaged<T>
    {
        private int m_SparseItemsCount => IsSingleton ? m_SparseItems.Length : m_World.m_EntitiesCount;

        void IEcsPool.Deserialize(ref FSnapshotReader reader)
        {
            Span<int> m_RecycledItemsSpan = m_RecycledItems;
            m_RecycledItemsCount = reader.ReadUnmanagedSpan(ref m_RecycledItemsSpan);
            Span<int> m_SparseItemsSpan = m_SparseItems;
            var sparseItemsCount = reader.ReadUnmanagedSpan(ref m_SparseItemsSpan);
            Span<T> m_DenseItemsSpan = m_DenseItems;
            m_DenseItemsCount = reader.ReadUnmanagedSpan(ref m_DenseItemsSpan);

            Log.Assert(sparseItemsCount == m_SparseItemsCount, $"sparseItemsCount not equal. {nameof(sparseItemsCount)} != {nameof(m_SparseItemsCount)} {sparseItemsCount} != {m_SparseItemsCount}");
        }

        void IEcsPool.Serialize(ref FSnapshotWriter writer)
        {
            writer.WriteUnmanagedSpan(m_RecycledItems.AsSpan(0, m_RecycledItemsCount));
            writer.WriteUnmanagedSpan(m_SparseItems.AsSpan(0, m_SparseItemsCount));
            writer.WriteUnmanagedSpan(m_DenseItems.AsSpan(0, m_DenseItemsCount));
        }

        void IEcsPool.Reset()
        {
            ArrayUtility.ClearFast(m_DenseItems, 0, m_DenseItemsCount);
            ArrayUtility.ClearFast(m_SparseItems, 0, m_SparseItemsCount);
            ArrayUtility.ClearFast(m_RecycledItems, 0, m_RecycledItemsCount);

            m_DenseItemsCount = 1;
            m_RecycledItemsCount = 0;
        }
    }
}
