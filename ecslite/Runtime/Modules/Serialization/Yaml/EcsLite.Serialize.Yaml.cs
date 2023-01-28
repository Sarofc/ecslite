using System;
using System.Collections.Generic;
using Saro.Entities.Serialization;

namespace Saro.Entities
{
    partial interface IEcsPool
    {
        internal void SerializeIntoYAML(YamlWriter writer);
    }

    partial class EcsWorld
    {
        internal void SerializeIntoYAML(YamlWriter writer)
        {
            writer.WriteInlineSequence(nameof(m_Entities), new Span<EntityData>(m_Entities, 1, m_EntitiesCount - 1));

            //yaml.WriteInlineSequence(nameof(m_RecycledEntities), new Span<int>(m_RecycledEntities, 0, m_RecycledEntitiesCount));

            using (writer.WriteCollection(nameof(m_Pools)))
            {
                for (int poolId = 1; poolId < m_PoolsCount; poolId++)
                {
                    var pool = m_Pools[poolId];
                    pool.SerializeIntoYAML(writer);
                }
            }
        }
    }

    partial class EcsPoolManaged<T>
    {
        void IEcsPool.SerializeIntoYAML(YamlWriter yaml)
        {
            //yaml.WriteInlineSequence(typeof(T).Name, new Span<T>(m_DenseItems, 1, m_DenseItemsCount - 1));

            using (yaml.WriteCollection(typeof(T).Name))
            {
                for (int i = 1; i < m_DenseItemsCount - 1; i++)
                {
                    var item = m_DenseItems[i];

                    //yaml.WriteKeyValue("- ", item);

                    if (item == null) yaml.WriteLine("null");
                    else yaml.WriteLine($"- {item.ToString()}");
                }
            }
        }
    }

    partial class EcsPoolUnmanaged<T>
    {
        void IEcsPool.SerializeIntoYAML(YamlWriter yaml)
        {
            yaml.WriteInlineSequence(typeof(T).Name, new Span<T>(m_DenseItems, 1, m_DenseItemsCount - 1));
        }
    }
}
