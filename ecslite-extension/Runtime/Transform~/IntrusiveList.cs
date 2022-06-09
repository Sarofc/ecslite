#if ENABLE_IL2CPP
#define INLINE_METHODS
#endif

namespace Saro.Entities.Collections
{
    using System;
    using System.Collections.Generic;

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

        void Add(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData);
        void AddFirst(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData);
        bool Remove(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData);
        int RemoveAll(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData);
        bool Replace(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData, int index);
        bool Insert(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData, int onIndex);
        void Clear(bool destroyData = false);
        bool RemoveAt(int index, bool destroyData = false);
        int RemoveRange(int from, int to, bool destroyData = false);
        EcsPackedEntityWithWorld GetValue(int index);
        bool Contains(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData);
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
        public struct Enumerator : System.Collections.Generic.IEnumerator<EcsPackedEntityWithWorld>
        {
            private readonly EcsPackedEntityWithWorld root;
            private EcsPackedEntityWithWorld head;
            private int id;

            // EcsPackedEntityWithWorld System.Collections.Generic.IEnumerator<EcsPackedEntityWithWorld>.Current => Worlds.currentWorld
            //     .GetData<IntrusiveListNode>(Worlds.currentWorld.GetEcsPackedEntityWithWorldById(this.id)).data;
            //
            // public ref readonly EcsPackedEntityWithWorld Current => ref Worlds.currentWorld
            //     .GetData<IntrusiveListNode>(Worlds.currentWorld.GetEcsPackedEntityWithWorldById(this.id)).data;

            // TODO
            EcsPackedEntityWithWorld System.Collections.Generic.IEnumerator<EcsPackedEntityWithWorld>.Current
                => id.Get<IntrusiveListNode>(root.world).data;

            public ref readonly EcsPackedEntityWithWorld Current
                => ref id.Get<IntrusiveListNode>(root.world).data;

#if INLINE_METHODS
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public Enumerator(IntrusiveList list)
            {
                this.root = list.root;
                this.head = list.root;
                this.id = -1;
            }

#if INLINE_METHODS
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public bool MoveNext()
            {
                if (this.head.IsAlive() == false) return false;

                this.id = this.head.id;

                this.head = this.head.Get<IntrusiveListNode>().next;
                return true;
            }

#if INLINE_METHODS
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public void Reset()
            {
                this.head = this.root;
                this.id = -1;
            }

            object System.Collections.IEnumerator.Current => throw new NullReferenceException();

            public void Dispose()
            {
            }
        }

        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private EcsPackedEntityWithWorld root;

        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private EcsPackedEntityWithWorld head;

        // [ME.ECS.Serializer.SerializeFieldAttribute]
        private int count;

        public int Count => this.count;

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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
        //         [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        // #endif
        //         public BufferArray<EcsPackedEntityWithWorld> ToArray()
        //         {
        //             var arr = PoolArray<EcsPackedEntityWithWorld>.Spawn(this.count);
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
        /// <param name="EcsPackedEntityWithWorldData"></param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool Contains(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData)
        {
            if (this.count == 0) return false;

            var node = this.FindNode(in EcsPackedEntityWithWorldData);
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
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void Clear(bool destroyData = false)
        {
            while (this.root.IsAlive() == true)
            {
                var node = this.root;
                this.root = this.root.Get<IntrusiveListNode>().next;
                if (destroyData == true) IntrusiveList.DestroyData(in node);
                node.Destroy();
            }

            this.root = EcsPackedEntityWithWorld.k_Null;
            this.head = EcsPackedEntityWithWorld.k_Null;
            this.count = 0;
        }

        /// <summary>
        /// Returns enumeration of nodes in range [from..to)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public IEnumerator<EcsPackedEntityWithWorld> GetRange(int from, int to)
        {
            while (from < to)
            {
                var node = this.FindNode(from);
                if (node.IsAlive() == true)
                {
                    yield return node.Get<IntrusiveListNode>().data;
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
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool RemoveAt(int index, bool destroyData = false)
        {
            if (this.count == 0) return false;

            var node = this.FindNode(index);
            if (node.IsAlive() == true)
            {
                this.RemoveNode(in node, destroyData);
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
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public int RemoveRange(int from, int to, bool destroyData = false)
        {
            if (this.count == 0) return 0;

            var count = 0;
            var node = this.FindNode(from);
            if (node.IsAlive() == true)
            {
                while (from < to)
                {
                    if (node.IsAlive() == true)
                    {
                        var next = node.Get<IntrusiveListNode>().next;
                        this.RemoveNode(in node, destroyData);
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
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public EcsPackedEntityWithWorld GetValue(int index)
        {
            if (this.count == 0) return EcsPackedEntityWithWorld.k_Null;

            var node = this.FindNode(index);
            if (node.IsAlive() == true)
            {
                return node.Get<IntrusiveListNode>().data;
            }

            return EcsPackedEntityWithWorld.k_Null;
        }

        /// <summary>
        /// Insert data at index onIndex.
        /// </summary>
        /// <param name="EcsPackedEntityWithWorldData"></param>
        /// <param name="onIndex"></param>
        /// <returns>Returns TRUE if successful</returns>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool Insert(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData, int onIndex)
        {
            if (this.count == 0)
            {
                this.Add(in EcsPackedEntityWithWorldData);
                return true;
            }

            if (onIndex > this.count) return false;

            if (onIndex == this.count)
            {
                this.Add(in EcsPackedEntityWithWorldData);
                return true;
            }

            var node = this.FindNode(onIndex);
            if (node.IsAlive() == true)
            {
                var newNode = IntrusiveList.CreateNode(in EcsPackedEntityWithWorldData);
                ref var newNodeLink = ref newNode.Get<IntrusiveListNode>();

                var link = node.Get<IntrusiveListNode>();
                if (link.prev.IsAlive() == true)
                {
                    link.prev.Get<IntrusiveListNode>().next = newNode;
                    newNodeLink.prev = link.prev;
                }
                else
                {
                    this.root = newNode;
                }

                newNodeLink.next = node;
                link.prev = newNode;

                ++this.count;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Replace data by index.
        /// </summary>
        /// <param name="EcsPackedEntityWithWorldData"></param>
        /// <param name="index"></param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool Replace(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData, int index)
        {
            if (this.count == 0) return false;

            var node = this.FindNode(index);
            if (node.IsAlive() == true)
            {
                ref var link = ref node.Get<IntrusiveListNode>();
                link.data = EcsPackedEntityWithWorldData;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove data from list.
        /// </summary>
        /// <param name="EcsPackedEntityWithWorldData"></param>
        /// <returns>Returns TRUE if data was found</returns>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool Remove(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData)
        {
            if (this.count == 0) return false;

            var node = this.FindNode(in EcsPackedEntityWithWorldData);
            if (node.IsAlive() == true)
            {
                this.RemoveNode(in node, destroyData: false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove all nodes data from list.
        /// </summary>
        /// <param name="EcsPackedEntityWithWorldData"></param>
        /// <returns>Returns count of removed elements</returns>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public int RemoveAll(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData)
        {
            if (this.count == 0) return 0;

            var root = this.root;
            var count = 0;
            do
            {
                var nextLink = root.Get<IntrusiveListNode>();
                if (nextLink.data == EcsPackedEntityWithWorldData)
                {
                    this.RemoveNode(root, destroyData: false);
                    ++count;
                }

                root = nextLink.next;
            } while (root.IsAlive() == true);

            return count;
        }

        /// <summary>
        /// Add new data at the end of the list.
        /// </summary>
        /// <param name="EcsPackedEntityWithWorldData"></param>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void Add(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData)
        {
            var node = IntrusiveList.CreateNode(in EcsPackedEntityWithWorldData);
            if (this.count == 0)
            {
                this.root = node;
            }
            else
            {
                ref var nodeLink = ref node.Get<IntrusiveListNode>();
                ref var headLink = ref this.head.Get<IntrusiveListNode>();
                headLink.next = node;
                nodeLink.prev = this.head;
            }

            this.head = node;
            ++this.count;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void Add(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData, out EcsPackedEntityWithWorld node)
        {
            node = IntrusiveList.CreateNode(in EcsPackedEntityWithWorldData);
            if (this.count == 0)
            {
                this.root = node;
            }
            else
            {
                ref var nodeLink = ref node.Get<IntrusiveListNode>();
                ref var headLink = ref this.head.Get<IntrusiveListNode>();
                headLink.next = node;
                nodeLink.prev = this.head;
            }

            this.head = node;
            ++this.count;
        }

        /// <summary>
        /// Add new data at the end of the list.
        /// </summary>
        /// <param name="EcsPackedEntityWithWorldData"></param>
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void AddFirst(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData)
        {
            this.Insert(in EcsPackedEntityWithWorldData, 0);
        }

        /// <summary>
        /// Returns first element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public EcsPackedEntityWithWorld GetFirst()
        {
            if (this.root.IsAlive() == false) return EcsPackedEntityWithWorld.k_Null;

            return this.root.Get<IntrusiveListNode>().data;
        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns instance, default if not found</returns>
        public EcsPackedEntityWithWorld GetLast()
        {
            if (this.head.IsAlive() == false) return EcsPackedEntityWithWorld.k_Null;

            return this.head.Get<IntrusiveListNode>().data;
        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns TRUE on success</returns>
        public bool RemoveLast(bool destroyData = false)
        {
            if (this.head.IsAlive() == false) return false;

            this.RemoveNode(in this.head, destroyData);
            return true;
        }

        /// <summary>
        /// Returns last element.
        /// </summary>
        /// <returns>Returns TRUE on success</returns>
        public bool RemoveFirst(bool destroyData = false)
        {
            if (this.head.IsAlive() == false) return false;

            this.RemoveNode(in this.root, destroyData);
            return true;
        }

        #region Helpers

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private EcsPackedEntityWithWorld FindNode(in EcsPackedEntityWithWorld EcsPackedEntityWithWorldData)
        {
            if (this.count == 0) return EcsPackedEntityWithWorld.k_Null;

            var node = this.root;
            do
            {
                var nodeEcsPackedEntityWithWorld = node;
                ref readonly var nodeLink = ref node.Get<IntrusiveListNode>();
                var nodeData = nodeLink.data;
                node = nodeLink.next;
                if (nodeData == EcsPackedEntityWithWorldData)
                {
                    return nodeEcsPackedEntityWithWorld;
                }
            } while (node.IsAlive() == true);


            return EcsPackedEntityWithWorld.k_Null;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private EcsPackedEntityWithWorld FindNode(int index)
        {
            var idx = 0;
            var node = this.root;
            do
            {
                var nodeEcsPackedEntityWithWorld = node;
                ref readonly var nodeLink = ref node.Get<IntrusiveListNode>();
                node = nodeLink.next;
                if (idx == index)
                {
                    return nodeEcsPackedEntityWithWorld;
                }

                ++idx;
                if (idx >= this.count) break;
            } while (node.IsAlive() == true);

            return EcsPackedEntityWithWorld.k_Null;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private void RemoveNode(in EcsPackedEntityWithWorld node, bool destroyData)
        {
            var link = node.Get<IntrusiveListNode>();
            if (link.prev.IsAlive() == true)
            {
                link.prev.Get<IntrusiveListNode>().next = link.next;
            }

            if (link.next.IsAlive() == true)
            {
                link.next.Get<IntrusiveListNode>().prev = link.prev;
            }

            if (node == this.root) this.root = link.next;
            if (node == this.head) this.head = link.prev;
            if (this.head == this.root && this.root == node)
            {
                this.root = EcsPackedEntityWithWorld.k_Null;
                this.head = EcsPackedEntityWithWorld.k_Null;
            }

            if (destroyData == true) IntrusiveList.DestroyData(in node);

            --this.count;
            node.Destroy();
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static EcsPackedEntityWithWorld CreateNode(in EcsPackedEntityWithWorld data)
        {
            var node = data.world.PackEntityWithWorld(data.world.NewEntity());
            node.Add<IntrusiveListNode>().data = data;
            return node;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static void DestroyData(in EcsPackedEntityWithWorld node)
        {
            var ret = node.TryGet<IntrusiveListNode>(out var listNode);
            if (!ret) return;
            ref var data = ref listNode.data;
            if (data.IsAlive() == true) data.Destroy();
        }

        #endregion
    }
}