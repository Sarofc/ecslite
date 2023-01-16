#if UNITY_EDITOR

using System;
using MemoryPack;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saro.Entities.Serialization.Tests
{
    [MemoryPackable]
    [MemoryPackUnion(0, typeof(IPoolUnmanaged<int>))]
    [MemoryPackUnion(1, typeof(IPoolManaged<Foo>))]
    public partial interface IPool { }

    [MemoryPackable]
    [Serializable]
    public partial class IPoolUnmanaged<T> : IPool
        where T : unmanaged
    {
        public T[] array;
    }

    [MemoryPackable]
    [Serializable]
    public partial class IPoolManaged<T> : IPool
        where T : class
    {
        public T[] array;
    }

    [MemoryPackable]
    [Serializable]
    public partial class Foo
    {
        public int a;
    }

    [Serializable]
    [MemoryPackable]
    public partial struct WorldSerializeData
    {
        [ShowInInspector]
        public IPool[] pool;
    }

    public class MemoryPackTest : MonoBehaviour
    {
        public WorldSerializeData data;

        public byte[] bytes;

        [Button()]
        void Init()
        {
            data = new WorldSerializeData
            {
                pool = new IPool[]
                {
                    new IPoolManaged<Foo>()
                    {

                    },
                    new IPoolUnmanaged<int>()
                    {

                    },
                },
            };
        }

        [Button()]
        void Serialize()
        {
            bytes = MemoryPackSerializer.Serialize(data);
        }

        [Button()]
        void Deserialize()
        {
            MemoryPackSerializer.Deserialize(bytes, ref data);
        }
    }
}

#endif