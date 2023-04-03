// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

namespace Saro.Entities
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public sealed partial class EcsPoolManaged<T> : IEcsPool where T : class, IEcsComponent, new()
    {
        private Type m_Type;
        private EcsWorld m_World;
        private int m_ID;

        private EcsCleanupHandler<T> m_CleanupHandler;

        // 1-based index.
        private T[] m_DenseItems;
        private int m_DenseItemsCount;
        private int[] m_SparseItems;
        private int[] m_RecycledItems;
        private int m_RecycledItemsCount;

        public bool IsSingleton { get; private set; }

#if ENABLE_IL2CPP && !UNITY_EDITOR
        T m_CleanupFakeInstance = new T();
#endif

        public override string ToString()
        {
            return $"type: {m_Type.Name} id: {m_ID} denseItem: {m_DenseItems.Length} recycledItems: {m_RecycledItems.Length} sparseItems: {m_SparseItems.Length}";
        }

        internal EcsPoolManaged(EcsWorld world, int id, int denseCapacity, int sparseCapacity, int recycledCapacity)
        {
            ((IEcsPool)this).Init(world, id, denseCapacity, sparseCapacity, recycledCapacity);
        }

        void IEcsPool.Init(EcsWorld world, int id, int denseCapacity, int sparseCapacity, int recycledCapacity)
        {
            m_Type = typeof(T);

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!typeof(IEcsComponent).IsAssignableFrom(m_Type))
            {
                throw new EcsException($"{m_Type} is not impl {nameof(IEcsComponent)}");
            }
#endif

            IsSingleton = typeof(IEcsComponentSingleton).IsAssignableFrom(m_Type);

            m_World = world;
            m_ID = id;
            m_DenseItems = new T[denseCapacity + 1];
            m_SparseItems = new int[sparseCapacity];
            m_DenseItemsCount = 1;
            m_RecycledItems = new int[recycledCapacity];
            m_RecycledItemsCount = 0;

            SetupCleanup();

            InitPoolState();
        }

        private void SetupCleanup()
        {
            var hasCleanup = typeof(IEcsCleanup<T>).IsAssignableFrom(m_Type);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!hasCleanup)
            {
                throw new EcsException($"ManagedComponent<{m_Type.Name}> MUST have {nameof(IEcsCleanup<T>)} interface.");
            }
#endif
            if (hasCleanup)
            {
                var cleanupMethod = m_Type.GetMethod(nameof(IEcsCleanup<T>.Cleanup));
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (cleanupMethod == null)
                {
                    throw new EcsException($"{nameof(IEcsCleanup<T>)}<{m_Type.Name}> explicit implementation not supported, use implicit instead.");
                }
#endif
                m_CleanupHandler = (EcsCleanupHandler<T>)Delegate.CreateDelegate(
                    typeof(EcsCleanupHandler<T>),
#if ENABLE_IL2CPP && !UNITY_EDITOR
                    m_CleanupFakeInstance,
#else
                    null,
#endif
                    cleanupMethod);
            }
        }

#if UNITY_2020_3_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        private void ReflectionSupportHack()
        {
            m_World.GetOrAddPool<T>();
            m_World.Filter().Inc<T>().Exc<T>().End();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld() => m_World;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetComponentId() => m_ID;

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

        public T[] GetRawDenseItems() => m_DenseItems;

        public int GetRawDenseItemsCount() => m_DenseItemsCount;

        public int[] GetRawSparseItems() => m_SparseItems;

        public int[] GetRawRecycledItems() => m_RecycledItems;

        public int GetRawRecycledItemsCount() => m_RecycledItemsCount;

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
                m_DenseItems[idx] = CreateComponentInstance();
                m_CleanupHandler?.Invoke(ref m_DenseItems[idx]);
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
        private T CreateComponentInstance() => new T();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!m_World.IsEntityAlive(entity)) { throw new EcsException($"{typeof(T).Name}::{nameof(Get)}. Cant touch destroyed entity: {entity} world: {m_World.worldId}"); }

            if (m_SparseItems[entity] == 0) { throw new EcsException($"Cant get \"{typeof(T).Name}\" component - not attached. entity: {entity}"); }
#endif
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
                if (m_CleanupHandler != null)
                {
                    m_CleanupHandler.Invoke(ref m_DenseItems[sparseData]);
                }
                //else
                //{
                //    m_DenseItems[sparseData] = default;
                //}
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

    partial class EcsPoolManaged<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(EcsEntity entity) => Has(entity.id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(EcsEntity entity) => ref Get(entity.id);
    }
}