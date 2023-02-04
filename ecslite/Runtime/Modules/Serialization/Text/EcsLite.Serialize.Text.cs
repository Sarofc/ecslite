using System;
using System.Collections.Generic;
using Saro.Entities.Serialization;
using Saro.FSnapshot;

namespace Saro.Entities
{
    partial interface IEcsPool
    {
        internal void Dump(ref FTextWriter writer);
    }

    partial class EcsWorld
    {
        internal void Dump(ref FTextWriter writer)
        {
            writer.BeginWriteObject(nameof(EcsWorld));
            writer.WriteUnmanagedSpan(new Span<EntityData>(m_Entities, 1, m_EntitiesCount - 1));

            for (int poolId = 1; poolId < m_PoolsCount; poolId++)
            {
                var pool = m_Pools[poolId];
                pool.Dump(ref writer);
            }
        }
    }

    partial class EcsPoolManaged<T>
    {
        void IEcsPool.Dump(ref FTextWriter writer)
        {
            for (int i = 1; i < m_DenseItemsCount - 1; i++)
            {
                var item = m_DenseItems[i];

                if (item == null) writer.WriteString("null");
                else writer.WriteObject(item);
            }
        }
    }

    partial class EcsPoolUnmanaged<T>
    {
        void IEcsPool.Dump(ref FTextWriter writer)
        {
            writer.WriteUnmanagedSpan(new Span<T>(m_DenseItems, 1, m_DenseItemsCount - 1));
        }
    }
}
