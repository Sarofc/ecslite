using System.Runtime.CompilerServices;
using Saro.Entities.Collections;
using Saro.Pool;

namespace Saro.Entities.Transforms
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    
    public static partial class EcsTransformUtility
    {
        internal static void OnEntityDestroy(EcsEntity toDestroy)
        {
            if (toDestroy.World.ParentPool.Has(toDestroy.id))
            {
                SetParent_Internal(toDestroy, EcsEntity.Null);
            }

            using (StackPool<EcsEntity>.Rent(out var stack))
            {
                using (HashSetPool<EcsEntity>.Rent(out var visited))
                {
                    stack.Push(toDestroy);
                    while (stack.Count != 0)
                    {
                        var current = stack.Peek();
                        {
                            while (!visited.Contains(current) && current.World.ChildrenPool.Has(current.id))
                            {
                                ref var nodes = ref current.World.ChildrenPool.Get(current.id);
                                if (nodes.items.Count == 0) break;
                                foreach (ref readonly var child in nodes.items)
                                {
                                    stack.Push(child);
                                }
                                visited.Add(current);
                                current = stack.Peek();
                            }
                        }

                        {
                            var top = stack.Pop();
                            Log.Assert(top.IsAlive() == true, "Assert Failed. entity MUST be alive");
                            ref var nodes = ref top.World.ChildrenPool.Get(top.id);
                            nodes.items.Clear(false); // 只要删除节点自己就行了
                            top.World.DelEntity_Internal(top.id); // 节点的数据这里删
                        }
                    }
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetParent(this int child, int parent, EcsWorld world)
        {
            var _parent = world.Pack(parent);
            var _child = world.Pack(child);
            _child.SetParent(_parent, worldPositionStays: true);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetParent(this EcsEntity child, EcsEntity parent)
        {
            child.SetParent(parent, worldPositionStays: true);
        }

        public static void SetParent(this EcsEntity child, EcsEntity parent, bool worldPositionStays)
        {
            if (worldPositionStays)
            {
                var pos = child.GetPosition();
                var rot = child.GetRotation();
                SetParent_Internal(child, parent);
                child.SetPosition(pos);
                child.SetRotation(rot);
            }
            else
            {
                SetParent_Internal(child, parent);
            }
        }

        public static EcsEntity GetRoot(this EcsEntity child)
        {
            EcsEntity root;
            var parent = child;
            do
            {
                root = parent;
                parent = parent.World.ParentPool.GetOrAdd(parent.id).entity;
            }
            while (parent.IsAlive());

            return root;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static bool HasParent(this int entity, EcsWorld world)
        {
            return world.ParentPool.Has(entity);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static bool HasParent(this EcsEntity entity)
        {
            return HasParent(entity.id, entity.World);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity GetParent(this int entity, EcsWorld world)
        {
            return world.ParentPool.GetOrAdd(entity).entity;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity GetParent(this EcsEntity entity)
        {
            return GetParent(entity.id, entity.World);
        }

        private static void SetParent_Internal(EcsEntity entity, EcsEntity root)
        {
            if (entity == root) return;

            if (root == EcsEntity.Null)
            {
                ref readonly var parent = ref entity.World.ParentPool.GetOrAdd(entity.id).entity;
                if (!parent.IsAlive()) return;

                ref var nodes = ref parent.World.ChildrenPool.GetOrAdd(parent.id);
                entity.World.ParentPool.Del(entity.id);
                nodes.items.Remove(entity);
                return;
            }

            {
                ref var parent = ref entity.World.ParentPool.GetOrAdd(entity.id).entity;
                if (parent == root || !root.IsAlive())
                {
                    return;
                }

                if (FindInHierarchy(entity, root)) return;

                if (parent.IsAlive())
                {
                    entity.SetParent(EcsEntity.Null);
                }

                parent = root;
                ref var rootNodes = ref root.World.ChildrenPool.GetOrAdd(root.id);
                rootNodes.items.Add(entity);
            }
        }

        private static bool FindInHierarchy(int child, EcsWorld world, EcsEntity root)
        {
            /*
             ref readonly var childNodes = ref world.ChildrenPool.Add(child);
             var items = childNodes.items;
             childNodes.items 防御性拷贝
            */
            ref var childNodes = ref world.ChildrenPool.GetOrAdd(child);
            if (childNodes.items.Contains(root))
            {
                return true;
            }

            foreach (ref readonly var cc in childNodes.items)
            {
                if (FindInHierarchy(cc.id, cc.World, root)) return true;
            }

            return false;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static bool FindInHierarchy(EcsEntity entity, EcsEntity root)
        {
            return FindInHierarchy(entity.id, entity.World, root);
        }
    }
}