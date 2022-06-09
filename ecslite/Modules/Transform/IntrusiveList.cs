#if ENABLE_IL2CPP
#define INLINE_METHODS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Saro.Entities.Collections
{
    /*
     * 功能等价于 链表，性能更好？
     */

    public struct IntrusiveListNode : IEcsComponent
    {
        public EcsPackedEntityWithWorld next;
        public EcsPackedEntityWithWorld prev;
        public EcsPackedEntityWithWorld data;
    }

    public interface IIntrusiveList
    {
        int Count { get; }

        void Add(in EcsPackedEntityWithWorld entity);
        void AddFirst(in EcsPackedEntityWithWorld entity);
        bool Remove(in EcsPackedEntityWithWorld entity);
        int RemoveAll(in EcsPackedEntityWithWorld entity);
        bool Replace(in EcsPackedEntityWithWorld entity, int index);
        bool Insert(in EcsPackedEntityWithWorld entity, int onIndex);
        void Clear(bool destroyData = false);
        bool RemoveAt(int index, bool destroyData = false);
        int RemoveRange(int from, int to, bool destroyData = false);
        EcsPackedEntityWithWorld GetValue(int index);
        bool Contains(in EcsPackedEntityWithWorld entity);
        EcsPackedEntityWithWorld GetFirst();
        EcsPackedEntityWithWorld GetLast();
        bool RemoveLast(bool destroyData = false);
        bool RemoveFirst(bool destroyData = false);

        IEnumerator<EcsPackedEntityWithWorld> GetRange(int from, int to);

        // BufferArray<EcsPackedEntityWithWorld> ToArray();
        IntrusiveList.Enumerator GetEnumerator();
    }

#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    [System.Serializable]
    public struct IntrusiveList : IIntrusiveList
    {
#if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif

#if DEBUG
        internal List<EcsPackedEntityWithWorld> DataArrayForDebug
        {
            get
            {
                var list = new List<EcsPackedEntityWithWorld>();

                foreach (var item in this)
                {
                    list.Add(item);
                }

                return list;
            }
        }
#endif

        public struct Enumerator : IEnumerator<EcsPackedEntityWithWorld>
        {
            private readonly EcsPackedEntityWithWorld m_Root;
            private EcsPackedEntityWithWorld m_Head;
            private int m_Id;

            EcsPackedEntityWithWorld IEnumerator<EcsPackedEntityWithWorld>.Current
                => m_Root.world.NodePool.Get(m_Id).data;

            public ref readonly EcsPackedEntityWithWorld Current
                => ref m_Root.world.NodePool.Get(m_Id).data;

#if INLINE_METHODS
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
            public Enumerator(IntrusiveList list)
            {
                m_Root = list.m_Root;
                m_Head = list.m_Root;
                m_Id = -1;
            }

#if INLINE_METHODS
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
            public bool MoveNext()
            {
                if (m_Head.IsAlive() == false) return false;

                m_Id = m_Head.id;

                m_Head = m_Head.world.NodePool.Get(m_Head.id).next;

                return true;
            }

#if INLINE_METHODS
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
            public void Reset()
            {
                m_Head = m_Root;
                m_Id = -1;
            }

            object IEnumerator.Current => throw new NotSupportedException("use Generic version");

            public void Dispose()
            {
            }
        }

        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private EcsPackedEntityWithWorld m_Root;

        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private EcsPackedEntityWithWorld m_Head;

        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private int m_Count;

        public int Count => m_Count;

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Put EcsPackedEntityWithWorld data into array.
        /// </summary>
        /// <returns>Buffer array from pool</returns>
        // #if INLINE_METHODS
        //         [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        // #endif
        //         public BufferArray<EcsPackedEntityWithWorld> ToArray()
        //         {
        //             var arr = PoolArray<EcsPackedEntityWithWorld>.Spawn(count);
        //             var i = 0;
        //             foreach (var EcsPackedEntityWithWorld in this)
        //             {
        //                 arr.arr[i++] = EcsPackedEntityWithWorld;
        //             }
        //
        //             return arr;
        //         }

        /// <summary>
        /// Find an element.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(in EcsPackedEntityWithWorld entity)
        {
            if (m_Count == 0) return false;

            var node = FindNode(in entity);
            if (node.IsAlive() == true)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear the list.
        /// </summary>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear(bool destroyData = false)
        {
            while (m_Root.IsAlive() == true)
            {
                var node = m_Root;
                m_Root = m_Root.world.NodePool.Get(m_Root.id).next;
                //root = root.Get<IntrusiveListNode>().next;
                if (destroyData == true) DestroyData(node);
                node.Destroy();
            }

            m_Root = EcsPackedEntityWithWorld.k_Null;
            m_Head = EcsPackedEntityWithWorld.k_Null;
            m_Count = 0;
        }

        /// <summary>
        /// Returns enumeration of nodes in range [from..to)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public IEnumerator<EcsPackedEntityWithWorld> GetRange(int from, int to)
        {
            while (from < to)
            {
                var node = FindNode(from);
                if (node.IsAlive() == true)
                {
                    yield return node.world.NodePool.Get(node.id).data;
                    //yield return node.Get<IntrusiveListNode>().data;
                }
                else
                {
                    ++from;
                }
            }
        }

        /// <summary>
        /// Remove node at index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="destroyData">Destroy also EcsPackedEntityWithWorld data</param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public bool RemoveAt(int index, bool destroyData = false)
        {
            if (m_Count == 0) return false;

            var node = FindNode(index);
            if (node.IsAlive() == true)
            {
                RemoveNode(in node, destroyData);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove nodes in range [from..to)
        /// </summary>
        /// <param name="from">Must be exists in list, could not be out of list range</param>
        /// <param name="to">May be out of list range, but greater than from</param>
        /// <param name="destroyData">Destroy also EcsPackedEntityWithWorld data</param>
        /// <returns>Returns count of removed elements</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public int RemoveRange(int from, int to, bool destroyData = false)
        {
            if (m_Count == 0) return 0;

            var count = 0;
            var node = FindNode(from);
            if (node.IsAlive() == true)
            {
                while (from < to)
                {
                    if (node.IsAlive() == true)
                    {
                        var next = node.world.NodePool.Get(node.id).next;
                        //var next = node.Get<IntrusiveListNode>().next;
                        RemoveNode(in node, destroyData);
                        node = next;
                        ++count;
                        ++from;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Get value by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>EcsPackedEntityWithWorld data. EcsPackedEntityWithWorld.k_Null if not found.</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public EcsPackedEntityWithWorld GetValue(int index)
        {
            if (m_Count == 0) return EcsPackedEntityWithWorld.k_Null;

            var node = FindNode(index);
            if (node.IsAlive() == true)
            {
                return node.world.NodePool.Get(node.id).data;
                //return node.Get<IntrusiveListNode>().data;
            }

            return EcsPackedEntityWithWorld.k_Null;
        }

        /// <summary>
        /// Insert data at index onIndex.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="index"></param>
        /// <returns>Returns TRUE if successful</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Insert(in EcsPackedEntityWithWorld entity, int index)
        {
            if (m_Count == 0)
            {
                Add(in entity);
                return true;
            }

            if (index > m_Count) return false;

            if (index == m_Count)
            {
                Add(in entity);
                return true;
            }

            var node = FindNode(index);
            if (node.IsAlive() == true)
            {
                var newNode = CreateNode(in entity);
                ref var newNodeLink = ref newNode.world.NodePool.Get(newNode.id);
                //ref var newNodeLink = ref newNode.Get<IntrusiveListNode>();

                var link = node.world.NodePool.Get(node.id);
                //var link = node.Get<IntrusiveListNode>();
                if (link.prev.IsAlive() == true)
                {
                    link.prev.world.NodePool.Get(link.prev.id).next = newNode;
                    //link.prev.Get<IntrusiveListNode>().next = newNode;
                    newNodeLink.prev = link.prev;
                }
                else
                {
                    m_Root = newNode;
                }

                newNodeLink.next = node;
                link.prev = newNode;

                ++m_Count;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Replace data by index.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="index"></param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Replace(in EcsPackedEntityWithWorld entity, int index)
        {
            if (m_Count == 0) return false;

            var node = FindNode(index);
            if (node.IsAlive() == true)
            {
                ref var link = ref node.world.NodePool.Get(node.id);
                //ref var link = ref node.Get<IntrusiveListNode>();
                link.data = entity;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove data from list.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Remove(in EcsPackedEntityWithWorld entity)
        {
            if (m_Count == 0) return false;

            var node = FindNode(entity);
            if (node.IsAlive())
            {
                RemoveNode(node, destroyData: false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove all nodes data from list.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Returns count of removed elements</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public int RemoveAll(in EcsPackedEntityWithWorld entity)
        {
            if (m_Count == 0) return 0;

            var root = m_Root;
            var count = 0;
            do
            {
                var nextLink = root.world.NodePool.Get(root.id);
                //var nextLink = root.Get<IntrusiveListNode>();
                if (nextLink.data == entity)
                {
                    RemoveNode(root, destroyData: false);
                    ++count;
                }

                root = nextLink.next;
            }
            while (root.IsAlive() == true);

            return count;
        }

        /// <summary>
        /// Add new data at the end of the list.
        /// </summary>
        /// <param name="entity"></param>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public void Add(in EcsPackedEntityWithWorld entity)
        {
            var node = IntrusiveList.CreateNode(in entity);
            if (m_Count == 0)
            {
                m_Root = node;
            }
            else
            {
                ref var nodeLink = ref node.world.NodePool.Get(node.id);
                //ref var nodeLink = ref node.Get<IntrusiveListNode>();
                ref var headLink = ref m_Head.world.NodePool.Get(m_Head.id);
                //ref var headLink = ref head.Get<IntrusiveListNode>();
                headLink.next = node;
                nodeLink.prev = m_Head;
            }

            m_Head = node;
            ++m_Count;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public void Add(in EcsPackedEntityWithWorld entity, out EcsPackedEntityWithWorld node)
        {
            node = IntrusiveList.CreateNode(entity);
            if (m_Count == 0)
            {
                m_Root = node;
            }
            else
            {
                ref var nodeLink = ref node.world.NodePool.Get(node.id);
                //ref var nodeLink = ref node.Get<IntrusiveListNode>();
                ref var headLink = ref m_Head.world.NodePool.Get(m_Head.id);
                //ref var headLink = ref head.Get<IntrusiveListNode>();
                headLink.next = node;
                nodeLink.prev = m_Head;
            }

            m_Head = node;
            ++m_Count;
        }

        /// <summary>
        /// Add new data at the end of the list.
        /// </summary>
        /// <param name="entity"></param>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public void AddFirst(in EcsPackedEntityWithWorld entity)
        {
            Insert(entity, 0);
        }

        /// <summary>
        /// Returns first element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public EcsPackedEntityWithWorld GetFirst()
        {
            if (m_Root.IsAlive() == false) return EcsPackedEntityWithWorld.k_Null;

            return m_Root.world.NodePool.Get(m_Root.id).data;
            //return root.Get<IntrusiveListNode>().data;
        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public EcsPackedEntityWithWorld GetLast()
        {
            if (m_Head.IsAlive() == false) return EcsPackedEntityWithWorld.k_Null;

            return m_Head.world.NodePool.Get(m_Head.id).data;
            //return head.Get<IntrusiveListNode>().data;
        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns TRUE on success</returns>
        public bool RemoveLast(bool destroyData = false)
        {
            if (m_Head.IsAlive() == false) return false;

            RemoveNode(in m_Head, destroyData);
            return true;
        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns TRUE on success</returns>
        public bool RemoveFirst(bool destroyData = false)
        {
            if (m_Head.IsAlive() == false) return false;

            RemoveNode(in m_Root, destroyData);
            return true;
        }

        #region Helpers

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private EcsPackedEntityWithWorld FindNode(in EcsPackedEntityWithWorld entity)
        {
            if (m_Count == 0) return EcsPackedEntityWithWorld.k_Null;

            var node = m_Root;
            do
            {
                var retNode = node;
                ref readonly var nodeLink = ref node.world.NodePool.Get(node.id);
                //ref readonly var nodeLink = ref node.Get<IntrusiveListNode>();
                var nodeData = nodeLink.data;
                node = nodeLink.next;
                if (nodeData == entity)
                {
                    return retNode;
                }
            }
            while (node.IsAlive() == true);


            return EcsPackedEntityWithWorld.k_Null;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private EcsPackedEntityWithWorld FindNode(int index)
        {
            var idx = 0;
            var node = m_Root;
            do
            {
                var retNode = node;
                ref readonly var nodeLink = ref node.world.NodePool.Get(node.id);
                //ref readonly var nodeLink = ref node.Get<IntrusiveListNode>();
                node = nodeLink.next;
                if (idx == index)
                {
                    return retNode;
                }

                ++idx;
                if (idx >= m_Count) break;
            } while (node.IsAlive() == true);

            return EcsPackedEntityWithWorld.k_Null;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private void RemoveNode(in EcsPackedEntityWithWorld node, bool destroyData)
        {
            var link = node.world.NodePool.Get(node.id);
            if (link.prev.IsAlive() == true)
            {
                link.prev.world.NodePool.Get(link.prev.id).next = link.next;
            }

            if (link.next.IsAlive() == true)
            {
                link.next.world.NodePool.Get(link.next.id).prev = link.prev;
            }

            if (node == m_Root) m_Root = link.next;
            if (node == m_Head) m_Head = link.prev;
            if (m_Head == m_Root && m_Root == node)
            {
                m_Root = EcsPackedEntityWithWorld.k_Null;
                m_Head = EcsPackedEntityWithWorld.k_Null;
            }

            if (destroyData)
                DestroyData(node);

            --m_Count;
            node.Destroy();
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private static EcsPackedEntityWithWorld CreateNode(in EcsPackedEntityWithWorld data)
        {
            var node = data.world.PackEntityWithWorld(data.world.NewEntity());
            node.world.NodePool.Add(node.id).data = data;
            return node;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private static void DestroyData(in EcsPackedEntityWithWorld node)
        {
            var ret = node.world.NodePool.Has(node.id);
            if (!ret) return;

            ref var data = ref node.world.NodePool.Get(node.id).data;
            if (data.IsAlive() == true) data.Destroy();
        }

        #endregion
    }
}