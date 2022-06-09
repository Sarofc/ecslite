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
    public readonly struct EcsPackedEntity : IEquatable<EcsPackedEntity>
    {
        public static readonly EcsPackedEntity k_Null = default;
        public readonly int id;
        internal readonly int gen;

        public EcsPackedEntity(int id, int gen)
        {
            this.gen = gen;
            this.id = id;
        }

        public bool Equals(EcsPackedEntity other)
        {
            return id == other.id && gen == other.gen;
        }

        public override bool Equals(object obj)
        {
            return obj is EcsPackedEntity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, gen);
        }

        public static bool operator !=(in EcsPackedEntity x, in EcsPackedEntity y)
        {
            return !(x == y);
        }

        public static bool operator ==(in EcsPackedEntity x, in EcsPackedEntity y)
        {
            if (x.id != y.id) return false;
            if (x.gen != y.gen) return false;

            return true;
        }
    }

    public readonly struct EcsPackedEntityWithWorld : IEquatable<EcsPackedEntityWithWorld>
    {
        public static readonly EcsPackedEntityWithWorld k_Null = default;
        public readonly int id;
        internal readonly int gen;
        internal readonly EcsWorld world;

        public EcsPackedEntityWithWorld(int id, int gen = 0, EcsWorld world = null)
        {
            this.id = id;
            this.gen = gen;
            this.world = world;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNull() => id == 0 && gen == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAlive()
        {
            if (world == null || !world.IsAlive() ||
                !world.IsEntityAlive_Internal(id) ||
                world.GetEntityGen(id) != gen)
            {
                return false;
            }
            return true;
        }


#if DEBUG
        // For using in IDE debugger.
        internal object[] DebugComponentsView
        {
            get
            {
                if (world != null && world.IsAlive() && world.IsEntityAlive_Internal(id) &&
                    world.GetEntityGen(id) == gen)
                {
                    object[] array = new object[DebugComponentsCount];
                    var count = world.GetComponents(id, ref array);
                    return array;
                }
                return null;
            }
        }

        // For using in IDE debugger.
        internal int DebugComponentsCount
        {
            get
            {
                if (world != null && world.IsAlive() && world.IsEntityAlive_Internal(id) &&
                    world.GetEntityGen(id) == gen)
                {
                    return world.GetComponentsCount(id);
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
            if (world == null || !world.IsAlive() || !world.IsEntityAlive_Internal(id) || world.GetEntityGen(id) != gen)
            {
                return "Entity-NonAlive";
            }
            System.Type[] types = null;
            var count = world.GetComponentTypes(id, ref types);
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

            return $"\'{Name.GetEntityName(id, world)}\' {id}:{gen} [{sb}]";
        }
#endif
        public bool Equals(EcsPackedEntityWithWorld other)
        {
            return id == other.id && gen == other.gen && Equals(world, other.world);
        }

        public override bool Equals(object obj)
        {
            return obj is EcsPackedEntityWithWorld other && Equals(other);
        }

        public override int GetHashCode()
        {
            // TODO calc hash
            return HashCode.Combine(id, gen, world);
        }

        public static bool operator !=(in EcsPackedEntityWithWorld x, in EcsPackedEntityWithWorld y)
        {
            return !(x == y);
        }

        public static bool operator ==(in EcsPackedEntityWithWorld x, in EcsPackedEntityWithWorld y)
        {
            if (x.id != y.id) return false;
            if (x.gen != y.gen) return false;
            if (x.world != y.world) return false;

            return true;
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static partial class EcsEntityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsPackedEntity PackEntity(this EcsWorld world, int entity)
        {
            return new(entity, world.GetEntityGen(entity));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unpack(this in EcsPackedEntity packed, EcsWorld world, out int entity)
        {
            if (!world.IsAlive() || !world.IsEntityAlive_Internal(packed.id) ||
                world.GetEntityGen(packed.id) != packed.gen)
            {
                entity = -1;
                return false;
            }
            entity = packed.id;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsTo(this in EcsPackedEntity a, in EcsPackedEntity b)
        {
            return a.id == b.id && a.gen == b.gen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsPackedEntityWithWorld PackEntityWithWorld(this EcsWorld world, int entity)
        {
            return new(entity, world.GetEntityGen(entity), world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unpack(this in EcsPackedEntityWithWorld packedEntity, out EcsWorld world, out int entity)
        {
            if (packedEntity.world == null || !packedEntity.world.IsAlive() ||
                !packedEntity.world.IsEntityAlive_Internal(packedEntity.id) ||
                packedEntity.world.GetEntityGen(packedEntity.id) != packedEntity.gen)
            {
                world = null;
                entity = -1;
                return false;
            }
            world = packedEntity.world;
            entity = packedEntity.id;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsTo(this in EcsPackedEntityWithWorld a, in EcsPackedEntityWithWorld b)
        {
            return a.id == b.id && a.gen == b.gen && a.world == b.world;
        }
    }
}