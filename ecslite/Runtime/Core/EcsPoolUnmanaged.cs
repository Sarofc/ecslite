using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Saro.Entities
{
    // 意义就是直接确定内存连续，可以使用指针算法

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public sealed partial class EcsPoolUnmanaged<T> : IEcsPool where T : unmanaged, IEcsComponent
    {
        private readonly Type m_Type;
        private readonly EcsWorld m_World;
        private readonly int m_ID;

        // 1-based index.
        private T[] m_DenseItems;
        private int m_DenseItemsCount;
        private int[] m_SparseItems;
        private int[] m_RecycledItems;
        private int m_RecycledItemsCount;

        public bool IsSingleton { get; private set; }

        public override string ToString()
        {
            return $"type: {m_Type.Name} id: {m_ID} denseItem: {m_DenseItems.Length} recycledItems: {m_RecycledItems.Length} sparseItems: {m_SparseItems.Length}";
        }

        internal EcsPoolUnmanaged(EcsWorld world, int id, int denseCapacity, int sparseCapacity, int recycledCapacity)
        {
            m_Type = typeof(T);

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!typeof(IEcsComponent).IsAssignableFrom(m_Type))
                throw new EcsException($"{m_Type} is not impl {nameof(IEcsComponent)}");
#endif

            IsSingleton = typeof(IEcsComponentSingleton).IsAssignableFrom(m_Type);

            m_World = world;
            m_ID = id;
            m_DenseItems = new T[denseCapacity + 1];
            m_SparseItems = new int[sparseCapacity];
            m_DenseItemsCount = 1;
            m_RecycledItems = new int[recycledCapacity];
            m_RecycledItemsCount = 0;
        }

#if UNITY_2020_3_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        private void ReflectionSupportHack()
        {
            m_World.GetPoolUnmanaged<T>();
            m_World.Filter().IncUnmanaged<T>().ExcUnmanaged<T>().End();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld() => m_World;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetId() => m_ID;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetComponentType() => m_Type;

        void IEcsPool.Resize(int capacity) { if (!IsSingleton) Array.Resize(ref m_SparseItems, capacity); }

        object IEcsPool.GetRaw(int entity) => Get(entity);

        void IEcsPool.SetRaw(int entity, object dataRaw)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (dataRaw == null || dataRaw.GetType() != m_Type) { throw new EcsException("Invalid component data, valid \"{typeof (T).Name}\" instance required."); }
            if (m_SparseItems[entity] <= 0) { throw new EcsException($"Component \"{typeof(T).Name}\" not attached to entity."); }
#endif
            m_DenseItems[m_SparseItems[entity]] = (T)dataRaw;
        }

        void IEcsPool.AddRaw(int entity, object dataRaw)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (dataRaw == null || dataRaw.GetType() != m_Type) { throw new EcsException("Invalid component data, valid \"{typeof (T).Name}\" instance required."); }
#endif

            GetOrAdd(entity);
        }

        public T[] RawDenseItems => m_DenseItems;
        public int RawDenseItemsCount => m_DenseItemsCount;
        public int[] RawSparseItems => m_SparseItems;
        public int[] RawRecycledItems => m_RecycledItems;
        public int RawRecycledItemsCount => m_RecycledItemsCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetOrAdd(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            // 不检验0号entity，让dummy通过
            if (entity != 0 && !m_World.IsEntityAlive(entity)) { throw new EcsException($"{typeof(T).Name}::{nameof(GetOrAdd)}. Cant touch destroyed entity: {entity} world: {m_World.worldId} world: {m_World.worldId}"); }

            if (IsSingleton) throw new EcsException($"{m_Type.FullName} is SingletonComponent, use {nameof(EcsWorld.GetSingleton)} instead");
#endif

            return ref GetOrAddInternal(entity);
        }

        internal ref T GetOrAddInternal(int entity)
        {
            // API 调整
            // 已拥有，就直接返回组件
            if (m_SparseItems[entity] > 0)
            {
                return ref m_DenseItems[m_SparseItems[entity]];
            }

            int idx;
            if (m_RecycledItemsCount > 0)
            {
                idx = m_RecycledItems[--m_RecycledItemsCount];
            }
            else
            {
                idx = m_DenseItemsCount;
                if (m_DenseItemsCount == m_DenseItems.Length)
                {
                    Array.Resize(ref m_DenseItems, m_DenseItemsCount << 1);
                }
                m_DenseItemsCount++;
                //m_AutoReset?.Invoke(ref m_DenseItems[idx]);
            }
            m_SparseItems[entity] = idx;
            m_World.OnEntityChange_Add_Internal(entity, m_ID);
            m_World.m_Entities[entity].compsCount++;

#if DEBUG || LEOECSLITE_WORLD_EVENTS
            m_World.RaiseEntityChangeEvent(entity);
#endif
            return ref m_DenseItems[idx];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!m_World.IsEntityAlive(entity)) { throw new EcsException($"{typeof(T).Name}::{nameof(Get)}. Cant touch destroyed entity: {entity} world: {m_World.worldId}"); }

            if (m_SparseItems[entity] == 0) { throw new EcsException($"Cant get \"{typeof(T).Name}\" component - not attached. entity: {entity}"); }
#endif

            //return ref GetDenseItem(GetSparseIndex(entity));
            return ref m_DenseItems[m_SparseItems[entity]];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!m_World.IsEntityAlive(entity)) { throw new EcsException($"{typeof(T).Name}::{nameof(Has)}. Cant touch destroyed entity: {entity} world: {m_World.worldId}"); }
#endif

            if (m_SparseItems.Length <= entity) return false; // 兼容singleton改动，singleton组件只会分配1个

            return m_SparseItems[entity] > 0;
        }

        public void Del(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!m_World.IsEntityAlive(entity)) { throw new EcsException($"{typeof(T).Name}::{nameof(Del)}. Cant touch destroyed entity: {entity} world: {m_World.worldId}"); }
#endif
            ref var sparseData = ref m_SparseItems[entity];
            if (sparseData > 0)
            {
                m_World.OnEntityChange_Remove_Internal(entity, m_ID);
                if (m_RecycledItemsCount == m_RecycledItems.Length)
                {
                    Array.Resize(ref m_RecycledItems, m_RecycledItemsCount << 1);
                }
                m_RecycledItems[m_RecycledItemsCount++] = sparseData;
                //if (m_AutoReset != null)
                //{
                //    m_AutoReset.Invoke(ref m_DenseItems[sparseData]);
                //}
                //else
                {
                    //GetDenseItem(sparseData) = default;
                    m_DenseItems[sparseData] = default;
                }
                sparseData = 0;
                ref var entityData = ref m_World.m_Entities[entity];
                entityData.compsCount--;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                m_World.RaiseEntityChangeEvent(entity);
#endif
                if (entityData.compsCount == 0)
                {
                    // component 数量为 0 了，此entity没有任何意义了，直接删！
                    m_World.DelEntity_Internal(entity);
                }
            }
        }
    }

    partial class EcsPoolUnmanaged<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T GetUnsafe(int entity, T* denseItems, int* sparseItems) => ref denseItems[sparseItems[entity]];

        // TODO 可以跳过边界检查，但il2cpp貌似可以直接设置
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int GetSparseIndex(int entity) => ref Unsafe.Add(ref MemoryMarshal.GetReference<int>(m_SparseItems), entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T GetDenseItem(int index) => ref Unsafe.Add(ref MemoryMarshal.GetReference<T>(m_DenseItems), index);
    }
}
