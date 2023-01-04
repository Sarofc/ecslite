using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CsvHelper;
using Saro.Entities.Serialization;
using Saro.Pool;
using Saro.Utility;

namespace Saro.Entities
{
    partial interface IEcsPool
    {
        void Serialize(IEcsWriter writer);
        void Deserialize(IEcsReader reader);

        /// <summary>
        /// 重置所有的component数据
        /// </summary>
        void Reset();
    }

    public interface IEcsSerialize
    {
        void Serialize(IEcsWriter writer);
        void Deserialize(IEcsReader reader);
    }

    partial class EcsPoolManaged<T>
    {
        private bool m_HasEcsSerializeInterface;

        private int m_SparseItemsCount => IsSingleton ? m_SparseItems.Length : m_World.m_EntitiesCount;

        private void InitPoolState()
        {
            m_HasEcsSerializeInterface = typeof(IEcsSerialize).IsAssignableFrom(m_Type);
        }

        private int GetComponentIndex(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!m_World.IsEntityAlive(entity)) { throw new EcsException($"{typeof(T).Name}::{nameof(Has)}. Cant touch destroyed entity: {entity} world: {m_World.worldId}"); }
#endif

            if (m_SparseItems.Length <= entity) return 0; // 兼容singleton改动，singleton组件只会分配1个

            return m_SparseItems[entity];
        }

        void IEcsPool.Deserialize(IEcsReader reader)
        {
            if (m_HasEcsSerializeInterface == false)
            {
                Log.WARN($"{m_Type.FullName} don't impl '{nameof(IEcsSerialize)}' interface. ");
                return;
            }

            var sparseItemsCount = reader.ReadArrayUnmanaged(ref m_SparseItems);
            Log.Assert(sparseItemsCount == m_SparseItemsCount, $"sparseItemsCount not equal. {nameof(sparseItemsCount)} != {nameof(m_SparseItemsCount)} {sparseItemsCount} != {m_SparseItemsCount}");

            m_RecycledItemsCount = reader.ReadArrayUnmanaged(ref m_RecycledItems);

            m_DenseItemsCount = reader.ReadInt32();
            using var _ = HashSetPool<int>.Rent(out var used);
            for (int e = 1; e < m_World.m_EntitiesCount; e++)
            {
                var index = GetComponentIndex(e);
                if (index > 0)
                {
                    //ref var c = ref Get(e);
                    ref var c = ref m_DenseItems[index];
                    if (c == null) c = CreateComponentInstance(); //  没有对象，就需要创建
                    ((IEcsSerialize)c).Deserialize(reader);

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

        void IEcsPool.Serialize(IEcsWriter writer)
        {
            if (m_HasEcsSerializeInterface == false)
            {
                Log.WARN($"{m_Type.FullName} don't impl '{nameof(IEcsSerialize)}' interface. ");
                return;
            }

            //writer.WriteEntryName(typeof(T).FullName, true);

            //writer.WriteEntryName(nameof(m_SparseItems));
            writer.WriteArrayUnmanaged(ref m_SparseItems, m_SparseItemsCount);
            //writer.WriteEntryName(nameof(m_RecycledItems));
            writer.WriteArrayUnmanaged(ref m_RecycledItems, m_RecycledItemsCount);

#if DEBUG
            using var _ = HashSetPool<int>.Rent(out var used);
#endif
            //writer.WriteEntryName(nameof(m_DenseItems));
            writer.Write(m_DenseItemsCount);
            for (int e = 1; e < m_World.m_EntitiesCount; e++)
            {
                var index = GetComponentIndex(e);
                if (index > 0)
                {
                    ref var c = ref m_DenseItems[index];
                    ((IEcsSerialize)c).Serialize(writer);
#if DEBUG
                    used.Add(index);
#endif
                }
            }

#if DEBUG
            //Log.INFO($"component serialize count: {used.Count} [{string.Join(",", used)}]");
#endif
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

        unsafe void IEcsPool.Deserialize(IEcsReader reader)
        {
            m_DenseItemsCount = reader.ReadArrayUnmanaged(ref m_DenseItems);
            m_RecycledItemsCount = reader.ReadArrayUnmanaged(ref m_RecycledItems);
            var sparseItemsCount = reader.ReadArrayUnmanaged(ref m_SparseItems);
            Log.Assert(sparseItemsCount == m_SparseItemsCount, $"sparseItemsCount not equal. {nameof(sparseItemsCount)} != {nameof(m_SparseItemsCount)} {sparseItemsCount} != {m_SparseItemsCount}");
        }

        unsafe void IEcsPool.Serialize(IEcsWriter writer)
        {
            //writer.WriteEntryName(typeof(T).FullName, true);

            //writer.WriteEntryName(nameof(m_DenseItems));
            writer.WriteArrayUnmanaged(ref m_DenseItems, m_DenseItemsCount);
            //writer.WriteEntryName(nameof(m_RecycledItems));
            writer.WriteArrayUnmanaged(ref m_RecycledItems, m_RecycledItemsCount);
            //writer.WriteEntryName(nameof(m_SparseItems));
            writer.WriteArrayUnmanaged(ref m_SparseItems, m_SparseItemsCount);
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
