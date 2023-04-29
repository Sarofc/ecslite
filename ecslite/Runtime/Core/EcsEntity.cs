// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Saro.Entities
{
    
    public readonly partial struct EcsEntity : IEquatable<EcsEntity>
    {
        public static readonly EcsEntity Null = default;

        public readonly int id;
        internal readonly short gen;
        internal readonly short world;

        public EcsWorld World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => EcsWorld.GetWorld(world);
        }

        // 外部使用 world.Pack
        internal EcsEntity(int id, short gen = 0, short worldID = 0)
        {
            this.id = id;
            this.gen = gen;
            this.world = worldID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNull() => this == Null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsAlive()
            => World != null && World.IsAlive() && World.IsEntityAlive(id) && World.GetEntityGen(id) == gen;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Destroy() => World.DelEntity(id);

        public readonly int GetComponents(ref object[] array)
        {
            if (IsAlive())
            {
                return World.GetComponents(id, ref array);
            }
            return 0;
        }

        public bool Equals(EcsEntity other)
            => id == other.id && gen == other.gen && world == other.world;

        public override bool Equals(object obj) => obj is EcsEntity other && Equals(other);

        public override int GetHashCode() => id; // 主要是减少碰撞，优化map的效率，同一个world的entity id肯定是唯一的

        public static bool operator !=(EcsEntity x, EcsEntity y) => !(x == y);

        public static bool operator ==(EcsEntity x, EcsEntity y) => x.Equals(y);

        public override string ToString() => this.GetEntityInfo();

#if DEBUG // For using in IDE debugger.

        private readonly object[] DebugComponentsViewForIDE
        {
            get
            {
                if (World != null && World.IsAlive() && World.IsEntityAlive(id) &&
                    World.GetEntityGen(id) == gen)
                {
                    object[] array = new object[DebugComponentsCountForIDE];
                    var count = World.GetComponents(id, ref array);
                    return array;
                }
                return null;
            }
        }

        private readonly int DebugComponentsCountForIDE
        {
            get
            {
                if (World != null && World.IsAlive() && World.IsEntityAlive(id) &&
                    World.GetEntityGen(id) == gen)
                {
                    return World.GetComponentsCount(id);
                }
                return 0;
            }
        }
#endif
    }
}