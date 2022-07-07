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
        public EcsEntity next;
        public EcsEntity prev;
        public EcsEntity data;
    }

    public interface IIntrusiveList
    {
        int Count { get; }

        void Add(in EcsEntity entity);
        void AddFirst(in EcsEntity entity);
        bool Remove(in EcsEntity entity);
        int RemoveAll(in EcsEntity entity);
        bool Replace(in EcsEntity entity, int index);
        bool Insert(in EcsEntity entity, int onIndex);
        void Clear(bool destroyData = false);
        bool RemoveAt(int index, bool destroyData = false);
        int RemoveRange(int from, int to, bool destroyData = false);
        EcsEntity GetValue(int index);
        bool Contains(in EcsEntity entity);
        EcsEntity GetFirst();
        EcsEntity GetLast();
        bool RemoveLast(bool destroyData = false);
        bool RemoveFirst(bool destroyData = false);

        IEnumerator<EcsEntity> GetRange(int from, int to);

        EcsEntity[] ToArray();
    }

#if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    [System.Serializable]
    public struct IntrusiveList : IIntrusiveList
    {
        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private EcsEntity m_Root;

        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private EcsEntity m_Head;

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
        /// Put EcsEntity data into array.
        /// </summary>
        /// <returns>Buffer array from pool</returns>
        public EcsEntity[] ToArray()
        {
            var arr = new EcsEntity[m_Count];
            var i = 0;
            foreach (ref readonly var EcsEntity in this)
            {
                arr[i++] = EcsEntity;
            }

            return arr;
        }

        /// <summary>
        /// Find an element.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(in EcsEntity entity)
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
                m_Root = m_Root.World.NodePool.Get(m_Root.id).next;
                //root = root.Get<IntrusiveListNode>().next;
                if (destroyData == true) DestroyData(node);
                node.Destroy();
            }

            m_Root = EcsEntity.k_Null;
            m_Head = EcsEntity.k_Null;
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
        public IEnumerator<EcsEntity> GetRange(int from, int to)
        {
            while (from < to)
            {
                var node = FindNode(from);
                if (node.IsAlive() == true)
                {
                    yield return node.World.NodePool.Get(node.id).data;
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
        /// <param name="destroyData">Destroy also EcsEntity data</param>
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
        /// <param name="destroyData">Destroy also EcsEntity data</param>
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
                        var next = node.World.NodePool.Get(node.id).next;
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
        /// <returns>EcsEntity data. EcsEntity.k_Null if not found.</returns>
#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public EcsEntity GetValue(int index)
        {
            if (m_Count == 0) return EcsEntity.k_Null;

            var node = FindNode(index);
            if (node.IsAlive() == true)
            {
                return node.World.NodePool.Get(node.id).data;
                //return node.Get<IntrusiveListNode>().data;
            }

            return EcsEntity.k_Null;
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
        public bool Insert(in EcsEntity entity, int index)
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
                ref var newNodeLink = ref newNode.World.NodePool.Get(newNode.id);
                //ref var newNodeLink = ref newNode.Get<IntrusiveListNode>();

                var link = node.World.NodePool.Get(node.id);
                //var link = node.Get<IntrusiveListNode>();
                if (link.prev.IsAlive() == true)
                {
                    link.prev.World.NodePool.Get(link.prev.id).next = newNode;
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
        public bool Replace(in EcsEntity entity, int index)
        {
            if (m_Count == 0) return false;

            var node = FindNode(index);
            if (node.IsAlive() == true)
            {
                ref var link = ref node.World.NodePool.Get(node.id);
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
        public bool Remove(in EcsEntity entity)
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
        public int RemoveAll(in EcsEntity entity)
        {
            if (m_Count == 0) return 0;

            var root = m_Root;
            var count = 0;
            do
            {
                var nextLink = root.World.NodePool.Get(root.id);
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
        public void Add(in EcsEntity entity)
        {
            var node = IntrusiveList.CreateNode(in entity);
            if (m_Count == 0)
            {
                m_Root = node;
            }
            else
            {
                ref var nodeLink = ref node.World.NodePool.Get(node.id);
                //ref var nodeLink = ref node.Get<IntrusiveListNode>();
                ref var headLink = ref m_Head.World.NodePool.Get(m_Head.id);
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
        public void Add(in EcsEntity entity, out EcsEntity node)
        {
            node = IntrusiveList.CreateNode(entity);
            if (m_Count == 0)
            {
                m_Root = node;
            }
            else
            {
                ref var nodeLink = ref node.World.NodePool.Get(node.id);
                //ref var nodeLink = ref node.Get<IntrusiveListNode>();
                ref var headLink = ref m_Head.World.NodePool.Get(m_Head.id);
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
        public void AddFirst(in EcsEntity entity)
        {
            Insert(entity, 0);
        }

        /// <summary>
        /// Returns first element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public EcsEntity GetFirst()
        {
            if (m_Root.IsAlive() == false) return EcsEntity.k_Null;

            return m_Root.World.NodePool.Get(m_Root.id).data;
            //return root.Get<IntrusiveListNode>().data;
        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public EcsEntity GetLast()
        {
            if (m_Head.IsAlive() == false) return EcsEntity.k_Null;

            return m_Head.World.NodePool.Get(m_Head.id).data;
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
        private EcsEntity FindNode(in EcsEntity entity)
        {
            if (m_Count == 0) return EcsEntity.k_Null;

            var node = m_Root;
            do
            {
                var retNode = node;
                ref readonly var nodeLink = ref node.World.NodePool.Get(node.id);
                //ref readonly var nodeLink = ref node.Get<IntrusiveListNode>();
                var nodeData = nodeLink.data;
                node = nodeLink.next;
                if (nodeData == entity)
                {
                    return retNode;
                }
            }
            while (node.IsAlive() == true);


            return EcsEntity.k_Null;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private EcsEntity FindNode(int index)
        {
            var idx = 0;
            var node = m_Root;
            do
            {
                node = node.World.NodePool.Get(node.id).next;
                if (idx == index)
                {
                    return node;
                }

                ++idx;
                if (idx >= m_Count) break;
            } while (node.IsAlive() == true);

            return EcsEntity.k_Null;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private void RemoveNode(in EcsEntity node, bool destroyData)
        {
            var link = node.World.NodePool.Get(node.id);
            if (link.prev.IsAlive() == true)
            {
                link.prev.World.NodePool.Get(link.prev.id).next = link.next;
            }

            if (link.next.IsAlive() == true)
            {
                link.next.World.NodePool.Get(link.next.id).prev = link.prev;
            }

            if (node == m_Root) m_Root = link.next;
            if (node == m_Head) m_Head = link.prev;
            if (m_Head == m_Root && m_Root == node)
            {
                m_Root = EcsEntity.k_Null;
                m_Head = EcsEntity.k_Null;
            }

            if (destroyData)
                DestroyData(node);

            --m_Count;
            node.Destroy();
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private static EcsEntity CreateNode(in EcsEntity data)
        {
            var node = data.World.Pack(data.World.NewEntity());
            node.World.NodePool.Add(node.id).data = data;
            return node;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        private static void DestroyData(in EcsEntity node)
        {
            var ret = node.World.NodePool.Has(node.id);
            if (!ret) return;

            ref var data = ref node.World.NodePool.Get(node.id).data;
            if (data.IsAlive() == true) data.Destroy();
        }

        #endregion

#if DEBUG
        private List<EcsEntity> DebugListForIDE
        {
            get
            {
                var list = new List<EcsEntity>();

                foreach (var item in this)
                {
                    list.Add(item);
                }

                return list;
            }
        }
#endif


#if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
        public struct Enumerator
        {
            private readonly EcsEntity m_Root;
            private EcsEntity m_Head;
            private int m_Id;

            public ref readonly EcsEntity Current
                => ref m_Root.World.NodePool.Get(m_Id).data;

            public Enumerator(IntrusiveList list)
            {
                m_Root = list.m_Root;
                m_Head = list.m_Root;
                m_Id = -1;
            }

            public bool MoveNext()
            {
                if (m_Head.IsAlive() == false) return false;

                m_Id = m_Head.id;

                m_Head = m_Head.World.NodePool.Get(m_Head.id).next;

                return true;
            }
        }
    }
}