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
    public readonly struct EcsEntity : IEquatable<EcsEntity>
    {
        // TODO 问题比较严重，外部使用容易漏初始化，最好 null = default
        public static readonly EcsEntity k_Null = new(-1, 0, 0);

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
        public bool IsNull() => this == k_Null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive()
            => id != -1 && World != null && World.IsAlive() && World.IsEntityAlive(id) && World.GetEntityGen(id) == gen;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy() => World.DelEntity(id);

        public bool Equals(EcsEntity other)
            => id == other.id && gen == other.gen && world == other.world;

        public override bool Equals(object obj) => obj is EcsEntity other && Equals(other);

        public override int GetHashCode() => id; // 主要是减少碰撞，优化map的效率，同一个world的entity id肯定是唯一的

        public static bool operator !=(in EcsEntity x, in EcsEntity y) => !(x == y);

        public static bool operator ==(in EcsEntity x, in EcsEntity y) => x.Equals(y);

#if DEBUG // For using in IDE debugger.
        private object[] DebugComponentsViewForIDE
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

        private int DebugComponentsCountForIDE
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

        public override string ToString() => Name.GetEntityInfo(id, World);
#endif
    }
}