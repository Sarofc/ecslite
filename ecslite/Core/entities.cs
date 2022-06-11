// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Text;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Saro.Entities
{
    /*
     * TODO 潜在危险
     * 
     * 可能存在外部系统(ecs之外) 引用entity, world被销毁, 又new了一个新的同id的world, 获取entity数据会报错
     * 
     */
    public readonly struct EcsEntity : IEquatable<EcsEntity>
    {
        public static readonly EcsEntity k_Null = default;

        public readonly int id;
        internal readonly short gen;
        internal readonly short world;

        public EcsWorld World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => EcsWorld.s_Worlds[world];
        }

        public EcsEntity(int id, short gen = 0, short worldID = 0)
        {
            this.id = id;
            this.gen = gen;
            this.world = worldID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNull() => id == 0 && gen == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive()
            => World != null && World.IsAlive() && World.IsEntityAlive_Internal(id) && World.GetEntityGen(id) == gen;

        public bool Equals(EcsEntity other)
            => id == other.id && gen == other.gen && Equals(World, other.World);

        public override bool Equals(object obj) => obj is EcsEntity other && Equals(other);

        public override int GetHashCode() => id; // TODO 先这么处理吧, 原则上, 同一个world的entity,才能放到一个map里

        public static bool operator !=(in EcsEntity x, in EcsEntity y) => !(x == y);

        public static bool operator ==(in EcsEntity x, in EcsEntity y)
            => x.id == y.id && x.gen == y.gen && x.World == y.World;


#if DEBUG
        private object[] DebugComponentsViewForIDE
        {
            get
            {
                if (World != null && World.IsAlive() && World.IsEntityAlive_Internal(id) &&
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
                if (World != null && World.IsAlive() && World.IsEntityAlive_Internal(id) &&
                    World.GetEntityGen(id) == gen)
                {
                    return World.GetComponentsCount(id);
                }
                return 0;
            }
        }

        // For using in IDE debugger.
        public override string ToString()
        {
            if (id == 0 && gen == 0)
            {
                return "Entity-Null";
            }
            if (World == null || !World.IsAlive() || !World.IsEntityAlive_Internal(id) || World.GetEntityGen(id) != gen)
            {
                return "Entity-NonAlive";
            }
            Type[] types = null;
            var count = World.GetComponentTypes(id, ref types);
            StringBuilder sb = null;
            if (count > 0)
            {
                sb = new StringBuilder(512);
                for (var i = 0; i < count; i++)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(types[i].Name);
                }
            }

            return $"{Name.GetEntityInfo(id, World)} [{sb}]";
        }
#endif
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static partial class EcsEntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsEntity Pack(this EcsWorld world, int entity)
        {
            return new(entity, world.GetEntityGen(entity), world.worldID);
        }
    }
}