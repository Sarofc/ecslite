using System;
using System.Runtime.CompilerServices;
using Saro.Entities.Serialization;
using Saro.Utility;

namespace Saro.Entities
{
    // TODO 后面再考虑 反序列化时，EcsPool的扩容问题

    // TODO EcsFilter 也要处理。。。

    partial interface IEcsPool
    {
        void GetPoolState(ref BasePoolState outData);
        void SetPoolState(in BasePoolState inData);

        /// <summary>
        /// 重置所有的component数据
        /// </summary>
        void Reset();
    }

    [Serializable]
    public abstract class BasePoolState
    {
        public int m_DenseItemsCount;
        public int[] m_SparseItems;
        public int[] m_RecycledItems;
        public int m_RecycledItemsCount;
    }

    [Serializable]
    public class PoolStateManaged : BasePoolState
    {
        public object[] m_DenseItems;
    }

    [Serializable]
    public class PoolStateUnmanaged : BasePoolState
    {
        public byte[] m_DenseItems;
        public int sizeofType;
    }

    partial class EcsPoolManaged<T>
    {
        private bool m_HasCloneableInterface;

        private int m_SparseItemsCount => Math.Min(m_World.m_EntitiesCount, m_SparseItems.Length);

        void InitPoolState()
        {
            m_HasCloneableInterface = typeof(ICloneable).IsAssignableFrom(m_Type);
        }

        private void GetComponentArray(ref object[] componentData)
        {
            //m_DenseItems;
            //m_SparseItems;
        }

        private unsafe void EnsurePoolStateCapacity(ref PoolStateManaged state)
        {
            var sparseItemsCount = m_SparseItemsCount;
            if (state == null)
            {
                state = new PoolStateManaged
                {
                    m_SparseItems = new int[sparseItemsCount],
                    m_RecycledItems = new int[m_RecycledItemsCount],
                    m_DenseItems = new object[m_DenseItemsCount],
                };
            }
            else
            {
                if (state.m_SparseItems == null || state.m_SparseItems.Length < sparseItemsCount)
                    state.m_SparseItems = new int[sparseItemsCount];

                if (state.m_RecycledItems == null || state.m_RecycledItems.Length < m_RecycledItemsCount)
                    state.m_RecycledItems = new int[m_RecycledItemsCount];

                if (state.m_DenseItems == null || state.m_DenseItems.Length < m_DenseItemsCount)
                    state.m_DenseItems = new object[m_DenseItemsCount];
            }
        }

        void IEcsPool.SetPoolState(in BasePoolState inState)
        {
            if (inState is PoolStateManaged state)
            {
                if (m_HasCloneableInterface)
                {
                    m_DenseItemsCount = state.m_DenseItemsCount;
                    m_RecycledItemsCount = state.m_RecycledItemsCount;

                    ArrayUtility.CopyFast(state.m_SparseItems, 0, m_SparseItems, 0, state.m_SparseItems.Length);
                    ArrayUtility.CopyFast(state.m_RecycledItems, 0, m_RecycledItems, 0, m_RecycledItemsCount);

                    SetComponentDatas(state.m_DenseItems, m_DenseItems, m_DenseItemsCount);
                }
                else
                {
                    Log.ERROR($"component {m_Type.Name} is not impl {nameof(ICloneable)}, deserialize ignore");
                }
            }
        }

        void IEcsPool.GetPoolState(ref BasePoolState outState)
        {
            if (m_HasCloneableInterface)
            {
                var state = outState as PoolStateManaged;
                EnsurePoolStateCapacity(ref state);

                state.m_DenseItemsCount = m_DenseItemsCount;
                state.m_RecycledItemsCount = m_RecycledItemsCount;

                ArrayUtility.CopyFast(m_SparseItems, 0, state.m_SparseItems, 0, m_SparseItemsCount);
                ArrayUtility.CopyFast(m_RecycledItems, 0, state.m_RecycledItems, 0, m_RecycledItemsCount);

                GetComponentDatas(m_DenseItems, state.m_DenseItems, m_DenseItemsCount);

                outState = state;
            }
            else
            {
                Log.ERROR($"component {m_Type.Name} is not impl {nameof(ICloneable)}, serialize ignore");
            }
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

        private unsafe void EnsurePoolStateCapacity(ref PoolStateUnmanaged state)
        {
            var sparseItemsCount = m_SparseItemsCount;
            var denseItemsSize = sizeof(T) * m_DenseItemsCount;
            if (state == null)
            {
                state = new PoolStateUnmanaged
                {
                    m_SparseItems = new int[sparseItemsCount],
                    m_RecycledItems = new int[m_RecycledItemsCount],
                    m_DenseItems = new byte[denseItemsSize],
                };
            }
            else
            {
                if (state.m_SparseItems == null || state.m_SparseItems.Length < sparseItemsCount)
                    state.m_SparseItems = new int[sparseItemsCount];

                if (state.m_RecycledItems == null || state.m_RecycledItems.Length < m_RecycledItemsCount)
                    state.m_RecycledItems = new int[m_RecycledItemsCount];

                if (state.m_DenseItems == null || state.m_DenseItems.Length < denseItemsSize)
                    state.m_DenseItems = new byte[denseItemsSize];
            }
        }

        unsafe void IEcsPool.SetPoolState(in BasePoolState inState)
        {
            if (inState is PoolStateUnmanaged state)
            {
                m_DenseItemsCount = state.m_DenseItemsCount;
                m_RecycledItemsCount = state.m_RecycledItemsCount;

                if (m_DenseItemsCount > 0)
                {
                    var srcSize = sizeof(byte) * state.m_DenseItems.Length;
                    if (srcSize > 0)
                    {
                        var dstSize = sizeof(T) * m_DenseItemsCount;
                        if (srcSize != dstSize)
                            throw new ArgumentOutOfRangeException($"{nameof(IEcsPool.SetPoolState)} {nameof(m_DenseItems)} {srcSize} != {dstSize}");

                        fixed (byte* pSrc = &state.m_DenseItems[0])
                        fixed (T* pDst = &m_DenseItems[0])
                        {
                            Log.Assert(srcSize == dstSize, $"{nameof(m_DenseItems)} {srcSize} != {dstSize}");
                            Buffer.MemoryCopy(pSrc, pDst, srcSize, srcSize);
                        }
                    }
                }

                ArrayUtility.CopyFast(state.m_SparseItems, 0, m_SparseItems, 0, state.m_SparseItems.Length);
                ArrayUtility.CopyFast(state.m_RecycledItems, 0, m_RecycledItems, 0, m_RecycledItemsCount);
            }
        }

        unsafe void IEcsPool.GetPoolState(ref BasePoolState outState)
        {
            var state = outState as PoolStateUnmanaged;
            EnsurePoolStateCapacity(ref state);

            state.m_DenseItemsCount = m_DenseItemsCount;
            state.m_RecycledItemsCount = m_RecycledItemsCount;

            var srcSize = sizeof(T) * m_DenseItemsCount;
            if (srcSize > 0)
            {
                var dstSize = sizeof(byte) * state.m_DenseItems.Length;
                if (srcSize != dstSize)
                    throw new ArgumentOutOfRangeException($"{nameof(IEcsPool.GetPoolState)} {nameof(m_DenseItems)} {srcSize} != {dstSize}");

                fixed (byte* pDst = &state.m_DenseItems[0])
                fixed (T* pSrc = &m_DenseItems[0])
                {
                    Buffer.MemoryCopy(pSrc, pDst, srcSize, srcSize);
                }
            }

            ArrayUtility.CopyFast(m_SparseItems, 0, state.m_SparseItems, 0, m_SparseItemsCount);
            ArrayUtility.CopyFast(m_RecycledItems, 0, state.m_RecycledItems, 0, m_RecycledItemsCount);

            outState = state;
        }

        void IEcsPool.Reset()
        {
            ArrayUtility.ClearFast(m_SparseItems);
            ArrayUtility.ClearFast(m_RecycledItems);
            ArrayUtility.ClearFast(m_DenseItems);

            m_DenseItemsCount = 1;
            m_RecycledItemsCount = 0;
        }
    }
}
