#if ENABLE_IL2CPP
#define INLINE_METHODS
#endif

namespace Saro.Entities.Transforms
{
#if INLINE_METHODS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public static class EcsTransformUtility
    {
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void OnEntityDestroy(in EcsPackedEntityWithWorld toDestroy)
        {
            if (toDestroy.world.ParentPool.Has(toDestroy.id))
            {
                // 此entity这个调用完毕后，就要被销毁，可以不用处理坐标系问题
                SetParent_Internal(toDestroy, EcsPackedEntityWithWorld.k_Null);
            }

            if (toDestroy.world.ChildrenPool.Has(toDestroy.id))
            {
                // TODO: Possible stack overflow while using Clear(true) because of OnEntityDestroy call
                ref var nodes = ref toDestroy.world.ChildrenPool.Get(toDestroy.id);
                foreach (ref readonly var child in nodes.items)
                {
                    child.world.ParentPool.Del(child.id);
                }
                nodes.items.Clear(destroyData: true);
            }
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetParent(this int child, int root, EcsWorld world)
        {
            var _root = world.PackEntityWithWorld(root);
            var _child = world.PackEntityWithWorld(child);
            _child.SetParent(_root, worldPositionStays: true);
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetParent(this in EcsPackedEntityWithWorld child, in EcsPackedEntityWithWorld root)
        {
            child.SetParent(root, worldPositionStays: true);
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetParent(this in EcsPackedEntityWithWorld child, in EcsPackedEntityWithWorld root,
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

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static EcsPackedEntityWithWorld GetRoot(this in EcsPackedEntityWithWorld child)
        {
            EcsPackedEntityWithWorld root;
            var parent = child;
            do
            {
                root = parent;
                parent = parent.world.ParentPool.Add(parent.id).entity;
            }
            while (parent.IsAlive());

            return root;
        }

        [System.Obsolete("use HasParent and GetParent instead", true)]
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryGetParent(this int child, EcsWorld world, out EcsPackedEntityWithWorld parent)
        {
            var r = world.ParentPool.TryGet(child, out var c);
            parent = c.entity;
            return r;
        }

        [System.Obsolete("use HasParent and GetParent instead", true)]
#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryGetParent(this in EcsPackedEntityWithWorld entity, out EcsPackedEntityWithWorld parent)
        {
            return TryGetParent(entity.id, entity.world, out parent);
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasParent(this int entity, EcsWorld world)
        {
            return world.ParentPool.Has(entity);
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasParent(this in EcsPackedEntityWithWorld entity)
        {
            return HasParent(entity.id, entity.world);
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static EcsPackedEntityWithWorld GetParent(this int entity, EcsWorld world)
        {
            return world.ParentPool.Add(entity).entity;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static EcsPackedEntityWithWorld GetParent(this in EcsPackedEntityWithWorld entity)
        {
            return GetParent(entity.id, entity.world);
        }


#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static void SetParent_Internal(in EcsPackedEntityWithWorld entity, in EcsPackedEntityWithWorld root)
        {
            if (entity == root) return;

            if (root == EcsPackedEntityWithWorld.k_Null)
            {
                ref readonly var parent = ref entity.world.ParentPool.Add(entity.id).entity;
                if (!parent.IsAlive()) return;

                ref var nodes = ref parent.world.ChildrenPool.Add(parent.id);
                entity.world.ParentPool.Del(entity.id);
                nodes.items.Remove(entity);
                return;
            }

            {
                ref var parent = ref entity.world.ParentPool.Add(entity.id).entity;
                if (parent == root || !root.IsAlive())
                {
                    return;
                }

                if (FindInHierarchy(in entity, in root)) return;

                if (parent.IsAlive())
                {
                    entity.SetParent(EcsPackedEntityWithWorld.k_Null);
                }

                parent = root;
                ref var rootNodes = ref root.world.ChildrenPool.Add(root.id);
                rootNodes.items.Add(entity);
            }
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static bool FindInHierarchy(int child, EcsWorld world, in EcsPackedEntityWithWorld root)
        {
            // ref readonly var childNode = xxx 貌似会导致防御性拷贝？
            ref var childNodes = ref world.ChildrenPool.Add(child);
            if (childNodes.items.Contains(root))
            {
                return true;
            }

            foreach (ref readonly var cc in childNodes.items)
            {
                if (FindInHierarchy(cc.id, cc.world, root)) return true;
            }

            return false;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static bool FindInHierarchy(in EcsPackedEntityWithWorld entity, in EcsPackedEntityWithWorld root)
        {
            return FindInHierarchy(entity.id, entity.world, root);
        }

        // TODO new version?
        // #if INLINE_METHODS
        //         [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        // #endif
        //         public static void OnEntityVersionChanged(in EcsPackedEntityWithWorld entity)
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
        // #if INLINE_METHODS
        //         [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        // #endif
        //         public static uint GetVersionInHierarchy(this in EcsPackedEntityWithWorld entity)
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