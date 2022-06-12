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
     * TODO Ǳ��Σ��
     * 
     * ���ܴ����ⲿϵͳ(ecs֮��) ����entity, world������, ��new��һ���µ�ͬid��world, ��ȡentity���ݻᱨ��
     * 
     */
    public readonly struct EcsEntity : IEquatable<EcsEntity>
    {
        public static readonly EcsEntity k_Null = new(-1, 0, 0);

        public readonly int id;
        internal readonly short gen;
        internal readonly short world;

        public EcsWorld World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => EcsWorld.s_Worlds[world];
        }

        // �ⲿ��Ҫ���ã�ʹ�� world.Pack
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

        public override int GetHashCode() => id; // TODO ����ô������, ԭ����, ͬһ��world��entity,���ܷŵ�һ��map��

        public static bool operator !=(in EcsEntity x, in EcsEntity y) => !(x == y);

        public static bool operator ==(in EcsEntity x, in EcsEntity y) => x.Equals(y);

#if DEBUG
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

        // For using in IDE debugger.
        public override string ToString() => Name.GetEntityDetial(id, World);
#endif
    }
}