// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    /*
        大量数组时 性能太低
        var pool = world.GetPool<T>(); 大数组时，非常耗时
    */

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public static class EcsEntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity NewEcsEntity(this EcsWorld world)
        {
            var entity = world.NewEntity();
            return world.Pack(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity NewEcsEntity(this EcsWorld world, string name)
        {
            var entity = world.NewEntity(name);
            return world.Pack(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity Pack(this EcsWorld world, int entity)
        {
            return new(entity, world.GetEntityGen(entity), world.worldId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(this EcsEntity self) where T : struct, IEcsComponent
        {
            var pool = self.World.GetPool<T>();
            return ref pool.Add(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(this int self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            return ref pool.Add(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(this int self, T component, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            ref var cc = ref pool.Add(self);
            cc = component;
            return ref cc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this int self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            return pool.Has(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(in this EcsEntity ent) where T : struct, IEcsComponent
        {
            var pool = ent.World.GetPool<T>();
            return pool.Has(ent.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(in this EcsEntity self) where T : struct, IEcsComponent
        {
            var pool = self.World.GetPool<T>();
            return ref pool.Get(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this int self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            return ref pool.Get(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(in this EcsEntity self) where T : struct, IEcsComponent
        {
            var pool = self.World.GetPool<T>();
            pool.Del(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(this int self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            pool.Del(self);
        }
    }
}