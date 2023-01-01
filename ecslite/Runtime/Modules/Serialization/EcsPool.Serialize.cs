using System;
using System.Collections.Generic;
using System.IO;
using Saro.Utility;

namespace Saro.Entities
{
    partial interface IEcsPool
    {
        void SetPoolData(BasePoolData data);
        void GetPoolData(ref BasePoolData data);
    }

    [Serializable]
    public class BasePoolData
    {
        public int m_DenseItemsCount;

        public int[] m_SparseItems;

        public int[] m_RecycledItems;
        public int m_RecycledItemsCount;
    }

    [Serializable]
    public class PoolData<T> : BasePoolData
    {
        public T[] m_DenseItems;
    }

    [Serializable]
    public class PoolDataUnmanaged : BasePoolData
    {
        public byte[] m_DenseItems;
    }

    public interface ISerializeComponent<T> where T : IEcsComponent
    {
        object Serialize(in T c);
        T Deserialize(object d);
    }

    partial class EcsPoolManaged<T>
    {
        public delegate T SerializeHandler(in T component);
        public delegate void DeserializeHandler(T d, ref T c);

        private SerializeHandler m_SerializeHandler;
        private DeserializeHandler m_DeserializeHandler;

        void InitSerialize()
        {
            var hasSerializeInterface = typeof(ISerializeComponent<T>).IsAssignableFrom(m_Type);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!hasSerializeInterface && m_Type.GetInterface("ISerializeComponent`1") != null)
            {
                throw new EcsException($"ISerializeComponent should have <{m_Type.Name}> constraint for component \"{m_Type.Name}\".");
            }
#endif
            if (hasSerializeInterface)
            {
                {
                    var serializeMethod = m_Type.GetMethod(nameof(ISerializeComponent<T>.Serialize));
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    if (serializeMethod == null)
                    {
                        throw new EcsException(
                            $"ISerializeComponent<{m_Type.Name}> explicit implementation not supported, use implicit instead.");
                    }
#endif
                    m_SerializeHandler = (SerializeHandler)Delegate.CreateDelegate(typeof(SerializeHandler),
#if ENABLE_IL2CPP && !UNITY_EDITOR
                        m_AutoresetFakeInstance,
#else
                        null,
#endif
                        serializeMethod);
                }
                {
                    var deserializeMethod = m_Type.GetMethod(nameof(ISerializeComponent<T>.Deserialize));
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    if (deserializeMethod == null)
                    {
                        throw new EcsException(
                            $"ISerializeComponent<{m_Type.Name}> explicit implementation not supported, use implicit instead.");
                    }
#endif
                    m_DeserializeHandler = (DeserializeHandler)Delegate.CreateDelegate(typeof(DeserializeHandler),
#if ENABLE_IL2CPP && !UNITY_EDITOR
                        m_AutoresetFakeInstance,
#else
                        null,
#endif
                        deserializeMethod);
                }
            }
        }

        void IEcsPool.SetPoolData(BasePoolData data)
        {
            if (data is PoolData<T> _data)
            {
                if (m_DeserializeHandler != null)
                {
                    m_SparseItems = _data.m_SparseItems;
                    m_DenseItemsCount = _data.m_DenseItemsCount;
                    m_RecycledItems = _data.m_RecycledItems;
                    m_RecycledItemsCount = _data.m_RecycledItemsCount;
                    DeserializeComponents(_data.m_DenseItems, m_DenseItems);
                }
            }
        }

        void IEcsPool.GetPoolData(ref BasePoolData data)
        {
            if (data is PoolData<T> _data)
            {
                if (m_SerializeHandler != null)
                {
                    _data.m_SparseItems = m_SparseItems;
                    _data.m_DenseItemsCount = m_DenseItemsCount;
                    _data.m_RecycledItems = m_RecycledItems;
                    _data.m_RecycledItemsCount = m_RecycledItemsCount;
                    SerializeComponents(m_DenseItems, _data.m_DenseItems);
                }
            }
        }

        private void SerializeComponents(T[] components, T[] componentDatas)
        {
            if (componentDatas == null)
                componentDatas = new T[components.Length];
            else if (componentDatas.Length < components.Length)
                Array.Resize(ref componentDatas, components.Length);

            for (int i = 0; i < components.Length; i++)
            {
                // TODO 只序列化 正在使用的 component
                if (Has(i))
                {
                    componentDatas[i] = m_SerializeHandler(components[i]);
                }
            }
        }

        private void DeserializeComponents(T[] componentDatas, T[] components)
        {
            if (components == null)
                components = new T[componentDatas.Length];
            else if (components.Length < componentDatas.Length)
                Array.Resize(ref components, componentDatas.Length * 2);

            for (int i = 0; i < componentDatas.Length; i++)
            {
                var componentData = componentDatas[i];
                ref var c = ref components[i];

                // in case： 只要有autoreset，都要调用一下
                if (m_AutoReset != null)
                    m_AutoReset(ref c);

                if (Has(i))
                {
                    //if (componentData != null) // 只要has，就应该得有数据
                    {
                        m_DeserializeHandler(componentData, ref c);
                    }
                }
            }
        }
    }

    partial class EcsPoolUnmanaged<T>
    {
        void IEcsPool.SetPoolData(BasePoolData data)
        {
            m_SparseItems = data.m_SparseItems;
            m_DenseItemsCount = data.m_DenseItemsCount;
            m_RecycledItems = data.m_RecycledItems;
            m_RecycledItemsCount = data.m_RecycledItemsCount;
            //DeserializeComponents(data.m_DenseItems, m_DenseItems);
        }

        void IEcsPool.GetPoolData(ref BasePoolData data)
        {
            data.m_SparseItems = m_SparseItems;
            data.m_DenseItemsCount = m_DenseItemsCount;
            data.m_RecycledItems = m_RecycledItems;
            data.m_RecycledItemsCount = m_RecycledItemsCount;
            //SerializeComponents(m_DenseItems, data.m_DenseItems);
        }

        private void SerializeComponents(T[] components, T[] componentDatas)
        {
            //if (componentDatas == null)
            //    componentDatas = new T[components.Length];
            //else if (componentDatas.Length < components.Length)
            //    Array.Resize(ref componentDatas, components.Length);

            //unsafe
            //{
            //    fixed (T* pComponent = &components[0])
            //    fixed ()
            //    {
            //        MemoryUtility.MemCpy<T>(pComponent, 0, )
            //    }
            //}
        }

        private void DeserializeComponents(T[] componentDatas, T[] components)
        {
            //if (components == null)
            //    components = new T[componentDatas.Length];
            //else if (components.Length < componentDatas.Length)
            //    Array.Resize(ref components, componentDatas.Length * 2);

            // TODO
        }
    }
}
