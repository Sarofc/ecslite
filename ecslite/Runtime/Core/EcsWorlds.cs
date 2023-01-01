// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using Saro.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Saro.Entities
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public partial class EcsWorld
    {
        internal EntityData[] entities;
        private int m_EntitiesCount;
        private int[] m_RecycledEntities;
        private int m_RecycledEntitiesCount;
        private IEcsPool[] m_Pools;
        private int m_PoolsCount;
        private readonly int m_PoolDenseSize;
        private readonly int m_PoolRecycledSize;
        private readonly Dictionary<Type, IEcsPool> m_PoolHashes;
        private readonly Dictionary<int, EcsFilter> m_HashedFilters;
        private readonly List<EcsFilter> m_AllFilters;
        private List<EcsFilter>[] m_FiltersByIncludedComponents;
        private List<EcsFilter>[] m_FiltersByExcludedComponents;
        private Mask[] m_Masks;
        private int m_FreeMasksCount;
        private bool m_Destroyed;

        internal readonly List<EcsSystems> ecsSystemsList = new();

#if DEBUG || LEOECSLITE_WORLD_EVENTS
        private readonly List<IEcsWorldEventListener> m_EventListeners;

        public void AddEventListener(IEcsWorldEventListener listener)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (listener == null)
            {
                throw new EcsException("Listener is null.");
            }
#endif
            m_EventListeners.Add(listener);
        }

        public void RemoveEventListener(IEcsWorldEventListener listener)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (listener == null)
            {
                throw new EcsException("Listener is null.");
            }
#endif
            m_EventListeners.Remove(listener);
        }

        public void RaiseEntityChangeEvent(int entity)
        {
            for (int ii = 0, iMax = m_EventListeners.Count; ii < iMax; ii++)
            {
                m_EventListeners[ii].OnEntityChanged(entity);
            }
        }
#endif

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
        private readonly List<int> m_LeakedEntities = new(512);

        internal bool CheckForLeakedEntities()
        {
            if (m_LeakedEntities.Count > 0)
            {
                for (int i = 0, iMax = m_LeakedEntities.Count; i < iMax; i++)
                {
                    ref var entityData = ref entities[m_LeakedEntities[i]];
                    if (entityData.gen > 0 && entityData.compsCount == 0)
                    {
                        return true;
                    }
                }

                m_LeakedEntities.Clear();
            }

            return false;
        }
#endif

        internal readonly short worldId;
        internal readonly string worldName;
        private static EcsWorld[] s_Worlds = new EcsWorld[4];
        private readonly static IntDispenser k_WorldIdDispenser = new(-1);
        private readonly static object k_LockObject = new();

        public EcsWorld(string worldName, in Config cfg = default)
        {
            this.worldName = worldName;

            // entities.
            var capacity = cfg.entities > 0 ? cfg.entities : Config.k_EntitiesDefault;
            entities = new EntityData[capacity];
            capacity = cfg.recycledEntities > 0 ? cfg.recycledEntities : Config.k_RecycledEntitiesDefault;
            m_RecycledEntities = new int[capacity];
            m_EntitiesCount = 0;
            m_RecycledEntitiesCount = 0;
            // pools.
            capacity = cfg.pools > 0 ? cfg.pools : Config.k_PoolsDefault;
            m_Pools = new IEcsPool[capacity];
            m_PoolHashes = new Dictionary<Type, IEcsPool>(capacity);
            m_FiltersByIncludedComponents = new List<EcsFilter>[capacity];
            m_FiltersByExcludedComponents = new List<EcsFilter>[capacity];
            m_PoolDenseSize = cfg.poolDenseSize > 0 ? cfg.poolDenseSize : Config.k_PoolDenseSizeDefault;
            m_PoolRecycledSize = cfg.poolRecycledSize > 0 ? cfg.poolRecycledSize : Config.k_PoolRecycledSizeDefault;
            m_PoolsCount = 0;
            // filters.
            capacity = cfg.filters > 0 ? cfg.filters : Config.k_FiltersDefault;
            m_HashedFilters = new Dictionary<int, EcsFilter>(capacity);
            m_AllFilters = new List<EcsFilter>(capacity);
            // masks.
            m_Masks = new Mask[64];
            m_FreeMasksCount = 0;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            m_EventListeners = new List<IEcsWorldEventListener>(4);
#endif

            // rent worldID
            var newID = k_WorldIdDispenser.Rent();
            if (newID > short.MaxValue)
            {
                throw new EcsException($"only support {short.MaxValue} worlds");
            }

            worldId = (short)newID;

            lock (k_LockObject)
            {
                if (s_Worlds.Length <= worldId)
                {
                    var newLength = s_Worlds.Length << 1 > short.MaxValue ?
                        short.MaxValue :
                        s_Worlds.Length << 1;
                    Array.Resize(ref s_Worlds, newLength);
                }

                s_Worlds[worldId] = this;
            }

            InitDummyEntity();

            m_Destroyed = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsWorld GetWorld(int world)
        {
            if (world >= 0 && world < s_Worlds.Length)
                return s_Worlds[world];

            return null;
        }

        public void Destroy()
        {
            if (m_Destroyed)
            {
                return;
            }

            // 先销毁系统，再销毁entity
            foreach (var systems in ecsSystemsList)
            {
                systems.Destroy();
            }

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (CheckForLeakedEntities())
            {
                throw new EcsException($"Empty entity detected before EcsWorld.Destroy().");
            }
#endif
            for (var i = m_EntitiesCount - 1; i > 0; i--)
            {
                ref var entityData = ref entities[i];
                if (entityData.compsCount > 0)
                {
                    DelEntity_Internal(i); // world被销毁了，就不用管层级关系了，直接干掉
                }
            }

            m_Pools = Array.Empty<IEcsPool>();
            m_PoolHashes.Clear();
            m_HashedFilters.Clear();
            m_AllFilters.Clear();
            m_FiltersByIncludedComponents = Array.Empty<List<EcsFilter>>();
            m_FiltersByExcludedComponents = Array.Empty<List<EcsFilter>>();
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (var ii = m_EventListeners.Count - 1; ii >= 0; ii--)
            {
                m_EventListeners[ii].OnWorldDestroyed(this);
            }
#endif

            lock (k_LockObject)
            {
                s_Worlds[worldId] = null;
            }

            k_WorldIdDispenser.Return(worldId);

            m_Destroyed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive() => !m_Destroyed;

        public int NewEntity()
        {
            int entity;
            if (m_RecycledEntitiesCount > 0)
            {
                entity = m_RecycledEntities[--m_RecycledEntitiesCount];
                ref var entityData = ref entities[entity];
                entityData.gen = (short)-entityData.gen;
            }
            else
            {
                // new entity.
                if (m_EntitiesCount == entities.Length)
                {
                    // resize entities and component pools.
                    var newSize = m_EntitiesCount << 1;
                    Array.Resize(ref entities, newSize);
                    for (int i = 0, iMax = m_PoolsCount; i < iMax; i++)
                    {
                        m_Pools[i].Resize(newSize);
                    }

                    for (int i = 0, iMax = m_AllFilters.Count; i < iMax; i++)
                    {
                        m_AllFilters[i].ResizeSparseIndex(newSize);
                    }
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                    for (int ii = 0, iMax = m_EventListeners.Count; ii < iMax; ii++)
                    {
                        m_EventListeners[ii].OnWorldResized(newSize);
                    }
#endif
                }

                entity = m_EntitiesCount++;
                entities[entity].gen = 1;
            }
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            m_LeakedEntities.Add(entity);
#endif
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = m_EventListeners.Count; ii < iMax; ii++)
            {
                m_EventListeners[ii].OnEntityCreated(entity);
            }
#endif
            return entity;
        }

        /// <summary>
        /// 实际销毁entity，不会管层级关系，外部不要用
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="EcsException"></exception>
        internal void DelEntity_Internal(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity <= 0 || entity >= m_EntitiesCount)
            {
                throw new EcsException("Cant touch destroyed entity.");
            }
#endif
            ref var entityData = ref entities[entity];
            if (entityData.gen < 0)
            {
                return;
            }

            // kill components.
            if (entityData.compsCount > 0)
            {
                var idx = 0;
                while (entityData.compsCount > 0 && idx < m_PoolsCount)
                {
                    for (; idx < m_PoolsCount; idx++)
                    {
                        if (m_Pools[idx].Has(entity))
                        {
                            m_Pools[idx++].Del(entity);
                            break;
                        }
                    }
                }
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (entityData.compsCount != 0)
                {
                    throw new EcsException(
                        $"Invalid components count on entity {entity} => {entityData.compsCount}.");
                }
#endif
                return;
            }

            entityData.gen = (short)(entityData.gen == short.MaxValue ? -1 : -(entityData.gen + 1));
            if (m_RecycledEntitiesCount == m_RecycledEntities.Length)
            {
                Array.Resize(ref m_RecycledEntities, m_RecycledEntitiesCount << 1);
            }

            m_RecycledEntities[m_RecycledEntitiesCount++] = entity;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = m_EventListeners.Count; ii < iMax; ii++)
            {
                m_EventListeners[ii].OnEntityDestroyed(entity);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetComponentsCount(int entity) => entities[entity].compsCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetEntityGen(int entity) => entities[entity].gen;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAllocatedEntitiesCount() => m_EntitiesCount - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWorldSize() => entities.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPoolsCount() => m_PoolsCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntitiesCount() => m_EntitiesCount - m_RecycledEntitiesCount - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityData[] GetRawEntities() => entities;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFreeMaskCount() => m_FreeMasksCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsPool<T> GetPool<T>() where T : class, IEcsComponent, new() => GetPool<T>(m_PoolDenseSize, entities.Length, m_PoolRecycledSize);

        internal EcsPool<T> GetPool<T>(int denseCapacity, int sparseCapacity, int recycledCapacity) where T : class, IEcsComponent, new()
        {
            var poolType = typeof(T);
            if (m_PoolHashes.TryGetValue(poolType, out var rawPool))
            {
                return (EcsPool<T>)rawPool;
            }

            var pool = new EcsPool<T>(this, m_PoolsCount, denseCapacity, sparseCapacity, recycledCapacity);
            m_PoolHashes[poolType] = pool;
            if (m_PoolsCount == m_Pools.Length)
            {
                var newSize = m_PoolsCount << 1;
                Array.Resize(ref m_Pools, newSize);
                Array.Resize(ref m_FiltersByIncludedComponents, newSize);
                Array.Resize(ref m_FiltersByExcludedComponents, newSize);
            }
            m_Pools[m_PoolsCount++] = pool;
            return pool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EcsPoolUnmanaged<T> GetPoolUnmanaged<T>() where T : unmanaged, IEcsComponent => GetPoolUnmanaged<T>(m_PoolDenseSize, entities.Length, m_PoolRecycledSize);
        internal EcsPoolUnmanaged<T> GetPoolUnmanaged<T>(int denseCapacity, int sparseCapacity, int recycledCapacity) where T : unmanaged, IEcsComponent
        {
            var poolType = typeof(T);
            if (m_PoolHashes.TryGetValue(poolType, out var rawPool))
            {
                return (EcsPoolUnmanaged<T>)rawPool;
            }

            var pool = new EcsPoolUnmanaged<T>(this, m_PoolsCount, denseCapacity, sparseCapacity, recycledCapacity);
            m_PoolHashes[poolType] = pool;
            if (m_PoolsCount == m_Pools.Length)
            {
                var newSize = m_PoolsCount << 1;
                Array.Resize(ref m_Pools, newSize);
                Array.Resize(ref m_FiltersByIncludedComponents, newSize);
                Array.Resize(ref m_FiltersByExcludedComponents, newSize);
            }
            m_Pools[m_PoolsCount++] = pool;
            return pool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEcsPool GetPoolById(int id) => id >= 0 && id < m_PoolsCount ? m_Pools[id] : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEcsPool GetPoolByType(Type type) => m_PoolHashes.TryGetValue(type, out var pool) ? pool : null;

        public int GetAllEntities(ref int[] entities)
        {
            var count = GetEntitiesCount();
            if (entities == null || entities.Length < count)
                entities = new int[count];

            var id = 0;
            for (int i = 1, iMax = m_EntitiesCount; i < iMax; i++)
            {
                ref var entityData = ref this.entities[i];
                // should we skip empty entities here?
                if (entityData.gen > 0 && entityData.compsCount >= 0)
                    entities[id++] = i;
            }

            return count;
        }

        public int GetAllPools(ref IEcsPool[] pools)
        {
            var count = m_PoolsCount;
            if (pools == null || pools.Length < count)
                pools = new IEcsPool[count];

            Array.Copy(m_Pools, 0, pools, 0, m_PoolsCount);
            return m_PoolsCount;
        }

        public int GetAllFilters(ref EcsFilter[] filters)
        {
            var count = m_AllFilters.Count;

            if (filters == null || filters.Length < count)
                filters = new EcsFilter[count];

            m_AllFilters.CopyTo(filters);

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Mask Filter()
        {
            return m_FreeMasksCount > 0 ? m_Masks[--m_FreeMasksCount] : new Mask(this);
        }

        public int GetComponents(int entity, ref object[] array)
        {
            var itemsCount = entities[entity].compsCount;
            if (itemsCount == 0)
            {
                return 0;
            }

            if (array == null || array.Length < itemsCount)
            {
                array = new object[m_Pools.Length];
            }

            for (int i = 0, j = 0, iMax = m_PoolsCount; i < iMax; i++)
            {
                if (m_Pools[i].Has(entity))
                {
                    array[j++] = m_Pools[i].GetRaw(entity);
                }
            }

            return itemsCount;
        }

        public void GetComponents(int entity, ref List<object> list)
        {
            var itemsCount = entities[entity].compsCount;
            if (itemsCount == 0)
            {
                return;
            }

            if (list == null)
            {
                list = new List<object>(m_Pools.Length);
            }

            if (list.Count < itemsCount)
            {
                list.Capacity = itemsCount;
            }

            for (int i = 0, iMax = m_PoolsCount; i < iMax; i++)
            {
                if (m_Pools[i].Has(entity))
                {
                    list.Add(m_Pools[i].GetRaw(entity));
                }
            }
        }

        public int GetComponentTypes(int entity, ref Type[] array)
        {
            var itemsCount = entities[entity].compsCount;
            if (itemsCount == 0)
            {
                return 0;
            }

            if (array == null || array.Length < itemsCount)
            {
                array = new Type[m_Pools.Length];
            }

            for (int i = 0, j = 0, iMax = m_PoolsCount; i < iMax; i++)
            {
                var pool = m_Pools[i];

                if (pool.Has(entity))
                {
                    array[j++] = pool.GetComponentType();
                }
            }

            return itemsCount;
        }

        /// <summary>
        /// entity是否存活
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntityAlive(int entity)
        {
            return entity > 0 && entity < m_EntitiesCount && entities[entity].gen > 0;
        }

        public T GetSystem<T>() where T : class
        {
            for (int i = 0; i < ecsSystemsList.Count; i++)
            {
                var systems = ecsSystemsList[i];
                var ret = systems.GetSystem<T>();
                if (ret != null) return ret;
            }
            return null;
        }

        private (EcsFilter, bool) GetFilter_Internal(Mask mask, int capacity = 512)
        {
            var hash = mask.hash;
            var exists = m_HashedFilters.TryGetValue(hash, out var filter);
            if (exists)
            {
                return (filter, false);
            }

            // create new filter
            filter = new EcsFilter(this, mask, capacity, entities.Length);
            m_HashedFilters[hash] = filter;
            m_AllFilters.Add(filter);

            // add to component dictionaries for fast compatibility scan.
            for (int i = 0, iMax = mask.includeCount; i < iMax; i++)
            {
                var list = m_FiltersByIncludedComponents[mask.include[i]];
                if (list == null)
                {
                    list = new List<EcsFilter>(8);
                    m_FiltersByIncludedComponents[mask.include[i]] = list;
                }

                list.Add(filter);
            }

            for (int i = 0, iMax = mask.excludeCount; i < iMax; i++)
            {
                var list = m_FiltersByExcludedComponents[mask.exclude[i]];
                if (list == null)
                {
                    list = new List<EcsFilter>(8);
                    m_FiltersByExcludedComponents[mask.exclude[i]] = list;
                }

                list.Add(filter);
            }

            // scan exist entities for compatibility with new filter.
            for (int i = 1, iMax = m_EntitiesCount; i < iMax; i++)
            {
                ref var entityData = ref entities[i];
                if (entityData.compsCount > 0 && IsMaskCompatible(mask, i))
                {
                    filter.AddEntity(i);
                }
            }
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = m_EventListeners.Count; ii < iMax; ii++)
            {
                m_EventListeners[ii].OnFilterCreated(filter);
            }
#endif
            return (filter, true);
        }

        internal void OnEntityChange_Add_Internal(int entity, int componentType)
        {
            var includeList = m_FiltersByIncludedComponents[componentType];
            var excludeList = m_FiltersByExcludedComponents[componentType];

            // add component.
            if (includeList != null)
            {
                foreach (var filter in includeList)
                {
                    if (IsMaskCompatible(filter.GetMask(), entity))
                    {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                        if (filter.sparseEntities[entity] > 0)
                        {
                            throw new EcsException($"Entity already in filter. worldID: {worldId} hash: {filter.GetMask().hash} entity: {entity} componentType: {GetPoolById(componentType).GetComponentType().Name}");
                        }
#endif
                        filter.AddEntity(entity);
                    }
                }
            }

            if (excludeList != null)
            {
                foreach (var filter in excludeList)
                {
                    if (IsMaskCompatibleWithout(filter.GetMask(), entity, componentType))
                    {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                        if (filter.sparseEntities[entity] == 0)
                        {
                            throw new EcsException("Entity not in filter.");
                        }
#endif
                        filter.RemoveEntity(entity);
                    }
                }
            }
        }

        internal void OnEntityChange_Remove_Internal(int entity, int componentType)
        {
            var includeList = m_FiltersByIncludedComponents[componentType];
            var excludeList = m_FiltersByExcludedComponents[componentType];

            // remove component.
            if (includeList != null)
            {
                foreach (var filter in includeList)
                {
                    if (IsMaskCompatible(filter.GetMask(), entity))
                    {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                        if (filter.sparseEntities[entity] == 0)
                        {
                            throw new EcsException("Entity not in filter.");
                        }
#endif
                        filter.RemoveEntity(entity);
                    }
                }
            }

            if (excludeList != null)
            {
                foreach (var filter in excludeList)
                {
                    if (IsMaskCompatibleWithout(filter.GetMask(), entity, componentType))
                    {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                        if (filter.sparseEntities[entity] > 0)
                        {
                            throw new EcsException("Entity already in filter.");
                        }
#endif
                        filter.AddEntity(entity);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsMaskCompatible(Mask filterMask, int entity)
        {
            for (int i = 0, iMax = filterMask.includeCount; i < iMax; i++)
            {
                if (!m_Pools[filterMask.include[i]].Has(entity))
                {
                    return false;
                }
            }

            for (int i = 0, iMax = filterMask.excludeCount; i < iMax; i++)
            {
                if (m_Pools[filterMask.exclude[i]].Has(entity))
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsMaskCompatibleWithout(Mask filterMask, int entity, int componentId)
        {
            for (int i = 0, iMax = filterMask.includeCount; i < iMax; i++)
            {
                var typeId = filterMask.include[i];
                if (typeId == componentId || !m_Pools[typeId].Has(entity))
                {
                    return false;
                }
            }

            for (int i = 0, iMax = filterMask.excludeCount; i < iMax; i++)
            {
                var typeId = filterMask.exclude[i];
                if (typeId != componentId && m_Pools[typeId].Has(entity))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() => $"WorldID: {worldId}  WorldName: {worldName}";

        public struct Config
        {
            public int entities;
            public int recycledEntities;
            public int pools;
            public int filters;
            public int poolDenseSize;
            public int poolRecycledSize;

            internal const int k_EntitiesDefault = 512;
            internal const int k_RecycledEntitiesDefault = 512;
            internal const int k_PoolsDefault = 512;
            internal const int k_FiltersDefault = 512;
            internal const int k_PoolDenseSizeDefault = 512;
            internal const int k_PoolRecycledSizeDefault = 512;
        }

#if ENABLE_IL2CPP
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        public sealed partial class Mask
        {
            private readonly EcsWorld m_World;
            internal int[] include;
            internal int[] exclude;
            internal int includeCount;
            internal int excludeCount;
            internal int hash;

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            private bool m_Built;
#endif

            internal Mask(EcsWorld world)
            {
                m_World = world;
                include = new int[8];
                exclude = new int[2];
                Reset();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Reset()
            {
                includeCount = 0;
                excludeCount = 0;
                hash = 0;
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                m_Built = false;
#endif
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Inc<T>() where T : class, IEcsComponent, new()
            {
                var poolId = m_World.GetPool<T>().GetId();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (m_Built)
                {
                    throw new EcsException("Cant change built mask.");
                }

                if (Array.IndexOf(include, poolId, 0, includeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }

                if (Array.IndexOf(exclude, poolId, 0, excludeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }
#endif
                if (includeCount == include.Length)
                {
                    Array.Resize(ref include, includeCount << 1);
                }

                include[includeCount++] = poolId;
                return this;
            }

#if UNITY_2020_3_OR_NEWER
            [UnityEngine.Scripting.Preserve] // TODO 好像没啥用？
#endif
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Exc<T>() where T : class, IEcsComponent, new()
            {
                var poolId = m_World.GetPool<T>().GetId();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (m_Built)
                {
                    throw new EcsException("Cant change built mask.");
                }

                if (Array.IndexOf(include, poolId, 0, includeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }

                if (Array.IndexOf(exclude, poolId, 0, excludeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }
#endif
                if (excludeCount == exclude.Length)
                {
                    Array.Resize(ref exclude, excludeCount << 1);
                }

                exclude[excludeCount++] = poolId;
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Mask IncUnmanaged<T>() where T : unmanaged, IEcsComponent
            {
                var poolId = m_World.GetPoolUnmanaged<T>().GetId();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (m_Built)
                {
                    throw new EcsException("Cant change built mask.");
                }

                if (Array.IndexOf(include, poolId, 0, includeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }

                if (Array.IndexOf(exclude, poolId, 0, excludeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }
#endif
                if (includeCount == include.Length)
                {
                    Array.Resize(ref include, includeCount << 1);
                }

                include[includeCount++] = poolId;
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Mask ExcUnmanaged<T>() where T : unmanaged, IEcsComponent
            {
                var poolId = m_World.GetPoolUnmanaged<T>().GetId();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (m_Built)
                {
                    throw new EcsException("Cant change built mask.");
                }

                if (Array.IndexOf(include, poolId, 0, includeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }

                if (Array.IndexOf(exclude, poolId, 0, excludeCount) != -1)
                {
                    throw new EcsException($"{typeof(T).Name} already in constraints list.");
                }
#endif
                if (excludeCount == exclude.Length)
                {
                    Array.Resize(ref exclude, excludeCount << 1);
                }

                exclude[excludeCount++] = poolId;
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EcsFilter End(int capacity = 512)
            {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (m_Built)
                {
                    throw new EcsException("Cant change built mask.");
                }

                m_Built = true;
#endif
                // TODO hash 冲突怎么办？

                // sort include and exclude
                // mono的库，sort有gc，换成自己的
                ArrayUtility.Sort(include, 0, includeCount);
                ArrayUtility.Sort(exclude, 0, excludeCount);

                // calculate hash.
                hash = includeCount + excludeCount;
                for (int i = 0, iMax = includeCount; i < iMax; i++)
                {
                    hash = unchecked(hash * 314159 + include[i]);
                }

                for (int i = 0, iMax = excludeCount; i < iMax; i++)
                {
                    hash = unchecked(hash * 314159 - exclude[i]);
                }

                var (filter, isNew) = m_World.GetFilter_Internal(this, capacity);
                if (!isNew)
                {
                    Recycle();
                }

                return filter;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Recycle()
            {
                Reset();

                if (m_World.m_FreeMasksCount == m_World.m_Masks.Length)
                {
                    Array.Resize(ref m_World.m_Masks, m_World.m_FreeMasksCount << 1);
                }

                m_World.m_Masks[m_World.m_FreeMasksCount++] = this;
            }
        }

        public struct EntityData
        {
            public short gen;
            public short compsCount;
        }
    }

#if DEBUG || LEOECSLITE_WORLD_EVENTS
    public interface IEcsWorldEventListener
    {
        void OnEntityCreated(int entity);
        void OnEntityChanged(int entity);
        void OnEntityDestroyed(int entity);
        void OnFilterCreated(EcsFilter filter);
        void OnWorldResized(int newSize);
        void OnWorldDestroyed(EcsWorld world);
    }
#endif

    public static class EcsWorldExtensionUnmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsPoolUnmanaged<T> GetPool<T>(this EcsWorld world) where T : unmanaged, IEcsComponent => world.GetPoolUnmanaged<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsWorld.Mask Inc<T>(this EcsWorld.Mask mask) where T : unmanaged, IEcsComponent => mask.IncUnmanaged<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsWorld.Mask Exc<T>(this EcsWorld.Mask mask) where T : unmanaged, IEcsComponent => mask.ExcUnmanaged<T>();
    }
}
