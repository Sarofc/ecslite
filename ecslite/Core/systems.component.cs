﻿// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    /*
        大量数组时 性能太低

        var pool = ent.World.GetPool<T>(); 大数组时，非常耗时

        改为 ComponentPool.TPool.直接引用 EcsPool
    */

    public static partial class EcsEntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(in this EcsEntity self) where T : struct, IEcsComponent
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

        [System.Obsolete("use Has and Get instead", true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(this int self, EcsWorld world, out T component) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();

            return pool.TryGet(self, out component);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this int self, EcsWorld world) => world.IsEntityAlive_Internal(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(in this EcsEntity self) => self.World.DelEntity(self.id);
    }
}