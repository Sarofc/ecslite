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
    public static class EcsEntityExtensionsUnmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAddUnmanaged<T>(this EcsEntity self) where T : unmanaged, IEcsComponent
        {
            var pool = self.World.GetPoolUnmanaged<T>();
            return ref pool.GetOrAdd(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAddUnmanaged<T>(this int self, EcsWorld world) where T : unmanaged, IEcsComponent
        {
            var pool = world.GetPoolUnmanaged<T>();
            return ref pool.GetOrAdd(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetUnmanaged<T>(in this EcsEntity self) where T : unmanaged, IEcsComponent
        {
            var pool = self.World.GetPoolUnmanaged<T>();
            return ref pool.Get(self.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetUnmanaged<T>(this int self, EcsWorld world) where T : unmanaged, IEcsComponent
        {
            var pool = world.GetPoolUnmanaged<T>();
            return ref pool.Get(self);
        }
    }
}