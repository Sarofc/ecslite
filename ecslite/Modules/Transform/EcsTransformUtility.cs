using System.Runtime.CompilerServices;

namespace Saro.Entities.Transforms
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public static class EcsTransformUtility
    {
        public static void OnEntityDestroy(in EcsEntity toDestroy)
        {
            if (toDestroy.World.ParentPool.Has(toDestroy.id))
            {
                // 此entity这个调用完毕后，就要被销毁，可以不用处理坐标系问题
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
        public static void SetParent(this in EcsEntity child, in EcsEntity root)
        {
            child.SetParent(root, worldPositionStays: true);
        }

        public static void SetParent(this in EcsEntity child, in EcsEntity root,
            bool worldPositionStays)
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

        public static EcsEntity GetRoot(this in EcsEntity child)
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
        public static bool HasParent(this in EcsEntity entity)
        {
            return HasParent(entity.id, entity.World);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity GetParent(this int entity, EcsWorld world)
        {
            return world.ParentPool.Add(entity).entity;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity GetParent(this in EcsEntity entity)
        {
            return GetParent(entity.id, entity.World);
        }

        private static void SetParent_Internal(in EcsEntity entity, in EcsEntity root)
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

                if (FindInHierarchy(in entity, in root)) return;

                if (parent.IsAlive())
                {
                    entity.SetParent(EcsEntity.k_Null);
                }

                parent = root;
                ref var rootNodes = ref root.World.ChildrenPool.Add(root.id);
                rootNodes.items.Add(entity);
            }
        }

        private static bool FindInHierarchy(int child, EcsWorld world, in EcsEntity root)
        {
            /*
             ref readonly var childNodes = ref world.ChildrenPool.Add(child);

             反编译后，多一个 var items = childNodes.items; 操作
             
             childNodes.items 出现防御性拷贝? 貌似不太对

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
        private static bool FindInHierarchy(in EcsEntity entity, in EcsEntity root)
        {
            return FindInHierarchy(entity.id, entity.World, root);
        }

        // TODO new version?
        //         public static void OnEntityVersionChanged(in EcsEntityWithWorld entity)
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
        //         public static uint GetVersionInHierarchy(this in EcsEntityWithWorld entity)
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