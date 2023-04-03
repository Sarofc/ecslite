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
        public static bool Has<T>(this int self, EcsWorld world) where T : unmanaged, IEcsComponent
        {
            var pool = world.GetOrAddPool(typeof(T));
            if (pool == null)
                return false;
            return pool.Has(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(in this EcsEntity self) where T : unmanaged, IEcsComponent
        {
            var pool = self.World.GetOrAddPool(typeof(T));
            if (pool == null)
                return false;
            return pool.Has(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(in this EcsEntity self) where T : unmanaged, IEcsComponent
        {
            var pool = self.World.GetOrAddPoolUnmanaged<T>();
            pool.Del(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(this int self, EcsWorld world) where T : unmanaged, IEcsComponent
        {
            var pool = world.GetOrAddPoolUnmanaged<T>();
            pool.Del(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAdd<T>(this EcsEntity self) where T : unmanaged, IEcsComponent
        {
            var pool = self.World.GetOrAddPoolUnmanaged<T>();
            return ref pool.GetOrAdd(self.id);
        }

        [System.Obsolete("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(this EcsEntity self) where T : unmanaged, IEcsComponent
        {
            var pool = self.World.GetOrAddPoolUnmanaged<T>();
            return ref pool.GetOrAdd(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAdd<T>(this int self, EcsWorld world) where T : unmanaged, IEcsComponent
        {
            var pool = world.GetOrAddPoolUnmanaged<T>();
            return ref pool.GetOrAdd(self);
        }

        [System.Obsolete("")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(this int self, EcsWorld world) where T : unmanaged, IEcsComponent
        {
            var pool = world.GetOrAddPoolUnmanaged<T>();
            return ref pool.GetOrAdd(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(in this EcsEntity self) where T : unmanaged, IEcsComponent
        {
            var pool = self.World.GetOrAddPoolUnmanaged<T>();
            return ref pool.Get(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this int self, EcsWorld world) where T : unmanaged, IEcsComponent
        {
            var pool = world.GetOrAddPoolUnmanaged<T>();
            return ref pool.Get(self);
        }
    }
}