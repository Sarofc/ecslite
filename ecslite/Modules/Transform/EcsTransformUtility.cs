using System.Runtime.CompilerServices;
using Saro.Entities.Collections;
using Saro.Pool;
using static PlasticGui.LaunchDiffParameters;

namespace Saro.Entities.Transforms
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public static class EcsTransformUtility
    {
        internal static void OnEntityDestroy(EcsEntity toDestroy)
        {
            if (toDestroy.World.ParentPool.Has(toDestroy.id))
            {
                SetParent_Internal(toDestroy, EcsEntity.k_Null);
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

        static void __OnEntityDestroy(EcsEntity toDestroy)
        {
            if (toDestroy.World.ParentPool.Has(toDestroy.id))
            {
                SetParent_Internal(toDestroy, EcsEntity.k_Null);
            }

            if (toDestroy.World.ChildrenPool.Has(toDestroy.id))
            {
                // TODO: Possible stack overflow while using Clear(true) because of OnEntityDestroy call
                ref var nodes = ref toDestroy.World.ChildrenPool.Get(toDestroy.id);
                foreach (ref readonly var child in nodes.items)
                {
                    child.World.ParentPool.Del(child.id);
                }
                nodes.items.Clear(destroyData: true);
            }
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetParent(this int child, int root, EcsWorld world)
        {
            var _root = world.Pack(root);
            var _child = world.Pack(child);
            _child.SetParent(_root, worldPositionStays: true);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetParent(this EcsEntity child, EcsEntity root)
        {
            child.SetParent(root, worldPositionStays: true);
        }

        public static void SetParent(this EcsEntity child, EcsEntity root, bool worldPositionStays)
        {
            if (worldPositionStays)
            {
                var pos = child.GetPosition();
                var rot = child.GetRotation();
                SetParent_Internal(child, root);
                child.SetPosition(pos);
                child.SetRotation(rot);
            }
            else
            {
                SetParent_Internal(child, root);
            }
        }

        public static EcsEntity GetRoot(this EcsEntity child)
        {
            EcsEntity root;
            var parent = child;
            do
            {
                root = parent;
                parent = parent.World.ParentPool.Add(parent.id).entity;
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
            return world.ParentPool.Add(entity).entity;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity GetParent(this EcsEntity entity)
        {
            return GetParent(entity.id, entity.World);
        }

        private static void SetParent_Internal(EcsEntity entity, EcsEntity root)
        {
            if (entity == root) return;

            if (root == EcsEntity.k_Null)
            {
                ref readonly var parent = ref entity.World.ParentPool.Add(entity.id).entity;
                if (!parent.IsAlive()) return;

                ref var nodes = ref parent.World.ChildrenPool.Add(parent.id);
                entity.World.ParentPool.Del(entity.id);
                nodes.items.Remove(entity);
                return;
            }

            {
                ref var parent = ref entity.World.ParentPool.Add(entity.id).entity;
                if (parent == root || !root.IsAlive())
                {
                    return;
                }

                if (FindInHierarchy(entity, root)) return;

                if (parent.IsAlive())
                {
                    entity.SetParent(EcsEntity.k_Null);
                }

                parent = root;
                ref var rootNodes = ref root.World.ChildrenPool.Add(root.id);
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
            ref var childNodes = ref world.ChildrenPool.Add(child);
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

        // TODO new version?
        //         public static void OnEntityVersionChanged(EcsEntityWithWorld entity)
        //         {
        //             if (entity.TryGet<Nodes>(out var nodes) == true)
        //             {
        //                 var world = entity.world;
        //                 foreach (var item in nodes.items)
        //                 {
        //                     world.IncrementEntityVersion(in item);
        //                     // TODO: Possible stack overflow while using OnEntityVersionChanged call
        //                     world.OnEntityVersionChanged(in item);
        //                 }
        //             }
        //         }
        //
        //         public static uint GetVersionInHierarchy(this EcsEntityWithWorld entity)
        //         {
        //             var v = entity.GetVersion();
        //             var ent = entity;
        //             while (ent.TryGet<Container>(out var container) == true)
        //             {
        //                 ent = container.entity;
        //                 v += ent.GetVersion();
        //             }
        //             return v;
        //         }

    }
}