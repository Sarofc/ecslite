﻿using System;
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

    partial class EcsPoolManaged<T>
    {
        private bool m_HasEcsSerializeInterface;

        private int m_SparseItemsCount => IsSingleton ? m_SparseItems.Length : m_World.m_EntitiesCount;

        private void InitPoolState()
        {
            m_HasEcsSerializeInterface = typeof(IEcsSerializable).IsAssignableFrom(m_Type);
        }

        private int GetComponentIndex(int entity)
        {
            if (m_SparseItems.Length <= entity) return 0;
            return m_SparseItems[entity];
        }

        void IEcsPool.Deserialize(IEcsReader reader)
        {
            if (m_HasEcsSerializeInterface == false)
            {
                Log.WARN($"{m_Type.FullName} don't impl '{nameof(IEcsSerializable)}' interface. ");
                return;
            }

            var sparseItemsCount = reader.ReadArrayUnmanaged(ref m_SparseItems, 1) + 1;
            Log.Assert(sparseItemsCount == m_SparseItemsCount, $"sparseItemsCount not equal. {nameof(sparseItemsCount)} != {nameof(m_SparseItemsCount)} {sparseItemsCount} != {m_SparseItemsCount}");

            m_RecycledItemsCount = reader.ReadArrayUnmanaged(ref m_RecycledItems, 1) + 1;

            m_DenseItemsCount = reader.ReadInt32();
            using var _ = HashSetPool<int>.Rent(out var used);
            for (int e = 1; e < m_World.m_EntitiesCount; e++)
            {
                var index = GetComponentIndex(e);
                if (index > 0)
                {
                    ref var c = ref m_DenseItems[index];
                    if (c == null) c = CreateComponentInstance(); //  没有对象，就需要创建
                    ((IEcsSerializable)c).Deserialize(reader);

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
                Log.WARN($"{m_Type.FullName} don't impl '{nameof(IEcsSerializable)}' interface. ");
                return;
            }

#if DEBUG
            writer.BeginWriteObject($"Component: {typeof(T).Name}");
            {
#endif
                writer.WriteArrayUnmanaged(ref m_SparseItems, m_SparseItemsCount - 1, 1);
                writer.WriteArrayUnmanaged(ref m_RecycledItems, m_RecycledItemsCount - 1, 1);

#if DEBUG
                using var _ = HashSetPool<int>.Rent(out var used);
#endif
                writer.Write(m_DenseItemsCount);
                for (int e = 1; e < m_World.m_EntitiesCount; e++)
                {
                    var index = GetComponentIndex(e);
                    if (index > 0)
                    {
                        ref var c = ref m_DenseItems[index];
                        ((IEcsSerializable)c).Serialize(writer);
#if DEBUG
                        used.Add(index);
#endif
                    }
                }
                //Log.INFO($"component serialize count: {used.Count} [{string.Join(",", used)}]");
#if DEBUG
            }
            writer.EndWriteObject();
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

        void IEcsPool.Deserialize(IEcsReader reader)
        {
            m_RecycledItemsCount = reader.ReadArrayUnmanaged(ref m_RecycledItems, 1) + 1;
            var sparseItemsCount = reader.ReadArrayUnmanaged(ref m_SparseItems, 1) + 1;
            m_DenseItemsCount = reader.ReadArrayUnmanaged(ref m_DenseItems, 1) + 1;

            Log.Assert(sparseItemsCount == m_SparseItemsCount, $"sparseItemsCount not equal. {nameof(sparseItemsCount)} != {nameof(m_SparseItemsCount)} {sparseItemsCount} != {m_SparseItemsCount}");
        }

        void IEcsPool.Serialize(IEcsWriter writer)
        {
#if DEBUG
            writer.BeginWriteObject($"Component: {typeof(T).Name}");
            {
#endif
                writer.WriteArrayUnmanaged(ref m_RecycledItems, m_RecycledItemsCount - 1, 1);
                writer.WriteArrayUnmanaged(ref m_SparseItems, m_SparseItemsCount - 1, 1);

                writer.WriteArrayUnmanaged(ref m_DenseItems, m_DenseItemsCount - 1, 1);
#if DEBUG
            }
            writer.EndWriteObject();
#endif
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
