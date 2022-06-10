// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Saro.Entities
{
#if LEOECSLITE_FILTER_EVENTS
    public interface IEcsFilterEventListener
    {
        void OnEntityAdded(int entity);
        void OnEntityRemoved(int entity);
    }
#endif

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed partial class EcsFilter
    {
        private readonly EcsWorld m_World;
        private readonly EcsWorld.Mask m_Mask;
        private int[] m_DenseEntities;
        private int m_EntitiesCount;
        internal int[] sparseEntities;
        private int m_LockCount;
        private DelayedOp[] m_DelayedOps;
        private int m_DelayedOpsCount;
#if LEOECSLITE_FILTER_EVENTS
        IEcsFilterEventListener[] m_EventListeners = new IEcsFilterEventListener[4];
        int m_EventListenersCount;
#endif

        internal EcsFilter(EcsWorld world, EcsWorld.Mask mask, int denseCapacity, int sparseCapacity)
        {
            m_World = world;
            m_Mask = mask;
            m_DenseEntities = new int[denseCapacity];
            sparseEntities = new int[sparseCapacity];
            m_EntitiesCount = 0;
            m_DelayedOps = new DelayedOp[512];
            m_DelayedOpsCount = 0;
            m_LockCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld()
        {
            return m_World;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntitiesCount()
        {
            return m_EntitiesCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] GetRawEntities()
        {
            return m_DenseEntities;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] GetSparseIndex()
        {
            return sparseEntities;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            m_LockCount++;
            return new Enumerator(this);
        }

#if LEOECSLITE_FILTER_EVENTS
        public void AddEventListener(IEcsFilterEventListener eventListener)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (eventListener == null) { throw new EcsException("Listener is null."); }
#endif
            if (m_EventListeners.Length == m_EventListenersCount)
            {
                Array.Resize(ref m_EventListeners, m_EventListenersCount << 1);
            }
            m_EventListeners[m_EventListenersCount++] = eventListener;
        }

        public void RemoveEventListener(IEcsFilterEventListener eventListener)
        {
            for (var i = 0; i < m_EventListenersCount; i++)
            {
                if (m_EventListeners[i] == eventListener)
                {
                    m_EventListenersCount--;
                    // cant fill gap with last element due listeners order is important.
                    Array.Copy(m_EventListeners, i + 1, m_EventListeners, i, m_EventListenersCount - i);
                    break;
                }
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResizeSparseIndex(int capacity)
        {
            Array.Resize(ref sparseEntities, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EcsWorld.Mask GetMask()
        {
            return m_Mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddEntity(int entity)
        {
            if (AddDelayedOp(true, entity)) { return; }
            if (m_EntitiesCount == m_DenseEntities.Length)
            {
                Array.Resize(ref m_DenseEntities, m_EntitiesCount << 1);
            }
            m_DenseEntities[m_EntitiesCount++] = entity;
            sparseEntities[entity] = m_EntitiesCount;
#if LEOECSLITE_FILTER_EVENTS
            ProcessEventListeners_Add(entity);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveEntity(int entity)
        {
            if (AddDelayedOp(false, entity)) { return; }
            var idx = sparseEntities[entity] - 1;
            sparseEntities[entity] = 0;
            m_EntitiesCount--;
            if (idx < m_EntitiesCount)
            {
                m_DenseEntities[idx] = m_DenseEntities[m_EntitiesCount];
                sparseEntities[m_DenseEntities[idx]] = idx + 1;
            }
#if LEOECSLITE_FILTER_EVENTS
            ProcessEventListeners_Removed(entity);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddDelayedOp(bool added, int entity)
        {
            if (m_LockCount <= 0) { return false; }
            if (m_DelayedOpsCount == m_DelayedOps.Length)
            {
                Array.Resize(ref m_DelayedOps, m_DelayedOpsCount << 1);
            }
            ref var op = ref m_DelayedOps[m_DelayedOpsCount++];
            op.added = added;
            op.entity = entity;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unlock()
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (m_LockCount <= 0) { throw new EcsException($"Invalid lock-unlock balance for \"{GetType().Name}\"."); }
#endif
            m_LockCount--;
            if (m_LockCount == 0 && m_DelayedOpsCount > 0)
            {
                for (int i = 0, iMax = m_DelayedOpsCount; i < iMax; i++)
                {
                    ref var op = ref m_DelayedOps[i];
                    if (op.added)
                    {
                        AddEntity(op.entity);
                    }
                    else
                    {
                        RemoveEntity(op.entity);
                    }
                }
                m_DelayedOpsCount = 0;
            }
        }

#if LEOECSLITE_FILTER_EVENTS
        void ProcessEventListeners_Add(int entity)
        {
            for (var i = 0; i < m_EventListenersCount; i++)
            {
                m_EventListeners[i].OnEntityAdded(entity);
            }
        }

        void ProcessEventListeners_Removed(int entity)
        {
            for (var i = 0; i < m_EventListenersCount; i++)
            {
                m_EventListeners[i].OnEntityRemoved(entity);
            }
        }
#endif

        public struct Enumerator : IDisposable
        {
            private readonly EcsFilter m_Filter;
            private readonly int[] m_Entities;
            private readonly int m_Count;
            private int m_Idx;

            public Enumerator(EcsFilter filter)
            {
                m_Filter = filter;
                m_Entities = filter.m_DenseEntities;
                m_Count = filter.m_EntitiesCount;
                m_Idx = -1;
            }

            public int Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Entities[m_Idx];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++m_Idx < m_Count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                m_Filter.Unlock();
            }
        }

        private struct DelayedOp
        {
            public bool added;
            public int entity;
        }
    }
}