// ----------------------------------------------------------------------------
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
        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(in this EcsPackedEntity self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            return ref pool.Add(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(in this EcsPackedEntityWithWorld self) where T : struct, IEcsComponent
        {
            var pool = self.world.GetPool<T>();
            return ref pool.Add(self.id);
        }

        // TODO 没有检查 ent 是否存活
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

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this int self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            return pool.Has(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(in this EcsPackedEntity ent, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            return pool.Has(ent.id);
        }

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(in this EcsPackedEntityWithWorld ent) where T : struct, IEcsComponent
        {
            var pool = ent.world.GetPool<T>();
            return pool.Has(ent.id);
        }

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(in this EcsPackedEntityWithWorld self) where T : struct, IEcsComponent
        {
            var pool = self.world.GetPool<T>();
            return ref pool.Get(self.id);
        }

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(in this EcsPackedEntity self, EcsWorld world) where T : struct, IEcsComponent
        {
            //if(self.Unpack(world, out _))
            {
                var pool = world.GetPool<T>();
                return ref pool.Get(self.id);
            }
        }

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this int self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            return ref pool.Get(self);
        }

        // TODO 没有检查 ent 是否存活
        [System.Obsolete("use Has and Get instead", true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(this int self, EcsWorld world, out T component) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();

            return pool.TryGet(self, out component);
        }

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(in this EcsPackedEntityWithWorld self) where T : struct, IEcsComponent
        {
            var pool = self.world.GetPool<T>();
            pool.Del(self.id);
        }

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(in this EcsPackedEntity self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            pool.Del(self.id);
        }

        // TODO 没有检查 ent 是否存活
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(this int self, EcsWorld world) where T : struct, IEcsComponent
        {
            var pool = world.GetPool<T>();
            pool.Del(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(in this EcsPackedEntity self) => self.id == 0 && self.gen == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(in this EcsPackedEntity self, EcsWorld world) => self.Unpack(world, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this int self, EcsWorld world) => world.IsEntityAlive_Internal(self);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(in this EcsPackedEntity self, EcsWorld world) => world.DelEntity(self.id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(in this EcsPackedEntityWithWorld self) => self.world.DelEntity(self.id);
    }
}