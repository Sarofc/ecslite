#if ENABLE_IL2CPP
#define INLINE_METHODS
#endif

namespace Saro.Entities
{
    using Transforms;

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
        public static void OnEntityDestroy(in EcsPackedEntityWithWorld entity)
        {
            if (entity.Has<Parent>() == true)
            {
                entity.SetParent(in EcsPackedEntityWithWorld.k_Null);
            }

            if (entity.Has<Children>() == true)
            {
                // TODO: Possible stack overflow while using Clear(true) because of OnEntityDestroy call
                ref var nodes = ref entity.Add<Children>();
                foreach (var child in nodes.items)
                {
                    child.Del<Parent>();
                }
                nodes.items.Clear(destroyData: true);
            }
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

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetParent(this in EcsPackedEntityWithWorld child, in EcsPackedEntityWithWorld root)
        {
            child.SetParent(in root, worldPositionStays: true);
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetParent(this in EcsPackedEntityWithWorld child, in EcsPackedEntityWithWorld root,
            bool worldPositionStays)
        {
            if (worldPositionStays == true)
            {
                var pos = child.GetPosition();
                var rot = child.GetRotation();
                SetParent_INTERNAL(in child, in root);
                child.SetPosition(pos);
                child.SetRotation(rot);
            }
            else
            {
                SetParent_INTERNAL(in child, in root);
            }
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static void SetParent_INTERNAL(in EcsPackedEntityWithWorld child, in EcsPackedEntityWithWorld root)
        {
            if (child == root) return;

            if (root == EcsPackedEntityWithWorld.k_Null)
            {
                ref var parent = ref child.Add<Parent>();
                if (parent.entity.IsAlive() == false) return;

                ref var nodes = ref parent.entity.Add<Children>();
                child.Del<Parent>();
                nodes.items.Remove(child);
                return;
            }

            {
                ref var parent = ref child.Add<Parent>();
                if (parent.entity == root || root.IsAlive() == false)
                {
                    return;
                }

                if (FindInHierarchy(in child, in root) == true) return;

                if (parent.entity.IsAlive() == true)
                {
                    child.SetParent(EcsPackedEntityWithWorld.k_Null);
                }

                parent.entity = root;
                ref var rootNodes = ref root.Add<Children>();
                rootNodes.items.Add(child);
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
                parent = parent.Add<Parent>().entity;
            } while (parent.IsAlive() == true);

            return root;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static bool FindInHierarchy(in EcsPackedEntityWithWorld child, in EcsPackedEntityWithWorld root)
        {
            var childNodes = child.Add<Children>();
            if (childNodes.items.Contains(root) == true)
            {
                return true;
            }

            foreach (var cc in childNodes.items)
            {
                if (FindInHierarchy(in cc, in root) == true) return true;
            }

            return false;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryGetParent(this in EcsPackedEntityWithWorld child, out EcsPackedEntityWithWorld parent)
        {
            var r = child.TryGet<Parent>(out var c);
            parent = c.entity;
            return r;
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool HasParent(this in EcsPackedEntityWithWorld child)
        {
            return child.Has<Parent>();
        }

#if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static EcsPackedEntityWithWorld GetParent(this in EcsPackedEntityWithWorld child)
        {
            return child.Add<Parent>().entity;
        }
    }
}