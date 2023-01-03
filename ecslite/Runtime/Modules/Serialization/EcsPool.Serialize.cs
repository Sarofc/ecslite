using System;
using System.Runtime.CompilerServices;
using CsvHelper;
using Saro.Entities.Serialization;
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
        //private bool m_HasCloneableInterface;

        private int m_SparseItemsCount => Math.Min(m_World.m_EntitiesCount, m_SparseItems.Length);

        void InitPoolState()
        {
            //m_HasCloneableInterface = typeof(ICloneable).IsAssignableFrom(m_Type);
        }

        private void GetComponentArray(ref object[] componentData)
        {
            //m_DenseItems;
            //m_SparseItems;
        }

        void IEcsPool.Deserialize(IEcsReader reader)
        {
            //m_DenseItemsCount = reader.ReadInt32();
            //m_RecycledItemsCount = reader.ReadInt32();

            reader.ReadArrayUnmanaged(ref m_SparseItems);
            reader.ReadArrayUnmanaged(ref m_RecycledItems);

            // TODO componetDatas
        }

        void IEcsPool.Serialize(IEcsWriter writer)
        {
            //writer.Write(m_DenseItemsCount);
            //writer.Write(m_RecycledItemsCount);

            writer.WriteArrayUnmanaged(ref m_SparseItems, m_SparseItemsCount);
            writer.WriteArrayUnmanaged(ref m_RecycledItems, m_RecycledItemsCount);

            // TODO componetDatas
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

        private void GetComponentDatas(T[] components, object[] componentDatas, int length)
        {
            int i = 1;
            for (; i < length; i++)
            {
                if (Has(i))
                {
                    var c = components[i] as ICloneable;
                    componentDatas[i] = c.Clone();
                }
            }
            for (; i < componentDatas.Length; i++)
            {
                componentDatas[i] = null;
            }
        }

        private void SetComponentDatas(object[] componentDatas, T[] components, int length)
        {
            // TODO 其他的array copy也优化，从1开始
            int i = 1; // entity有效值从1开始
            for (; i < length; i++)
            {
                var d = componentDatas[i];
                ref var c = ref components[i];

                // in case： 只要有autoreset，都要调用一下
                if (c != null && m_AutoReset != null)
                    m_AutoReset(ref c);

                if (Has(i))
                {
                    c = (T)d; // 只要has，就应该得有数据
                }
            }

            for (; i < components.Length; i++)
            {
                ref var c = ref components[i];

                // in case： 只要有autoreset，都要调用一下
                if (c != null && m_AutoReset != null)
                    m_AutoReset(ref c);
            }
        }
    }

    partial class EcsPoolUnmanaged<T>
    {
        private int m_SparseItemsCount => Math.Min(m_World.m_EntitiesCount, m_SparseItems.Length);

        unsafe void IEcsPool.Deserialize(IEcsReader reader)
        {
            //m_DenseItemsCount = reader.ReadInt32();
            //m_SparseItemsCount = reader.ReadInt32();
            //m_RecycledItemsCount = reader.ReadInt32();

            m_DenseItemsCount = reader.ReadArrayUnmanaged(ref m_DenseItems);
            m_RecycledItemsCount = reader.ReadArrayUnmanaged(ref m_RecycledItems);
            reader.ReadArrayUnmanaged(ref m_SparseItems);
        }

        unsafe void IEcsPool.Serialize(IEcsWriter writer)
        {
            //writer.Write(m_DenseItemsCount);
            //writer.Write(m_SparseItemsCount);
            //writer.Write(m_RecycledItemsCount);

            writer.WriteArrayUnmanaged(ref m_DenseItems, m_DenseItemsCount);
            writer.WriteArrayUnmanaged(ref m_RecycledItems, m_RecycledItemsCount);
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
