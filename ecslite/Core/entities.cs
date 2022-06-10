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
    /*
     * TODO Ǳ��Σ��
     * 
     * ���ܴ����ⲿϵͳ(ecs֮��) ����entity, world������, ��new��һ���µ�ͬid��world, ��ȡentity���ݻᱨ��
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
        {
            if (World == null || !World.IsAlive() ||
                !World.IsEntityAlive_Internal(id) ||
                World.GetEntityGen(id) != gen)
            {
                return false;
            }
            return true;
        }


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
            System.Type[] types = null;
            var count = World.GetComponentTypes(id, ref types);
            System.Text.StringBuilder sb = null;
            if (count > 0)
            {
                sb = new System.Text.StringBuilder(512);
                for (var i = 0; i < count; i++)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(types[i].Name);
                }
            }

            return $"\'{Name.GetEntityName(id, World)}\' {world}:{id}.{gen} [{sb}]";
        }
#endif
        public bool Equals(EcsEntity other)
            => id == other.id && gen == other.gen && Equals(World, other.World);

        public override bool Equals(object obj)
            => obj is EcsEntity other && Equals(other);

        public override int GetHashCode() => id; // TODO ����ô�����, ԭ����, ͬһ��world��entity,���ܷŵ�һ��map��

        public static bool operator !=(in EcsEntity x, in EcsEntity y) => !(x == y);

        public static bool operator ==(in EcsEntity x, in EcsEntity y) => x.id == y.id && x.gen == y.gen && x.World == y.World;
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