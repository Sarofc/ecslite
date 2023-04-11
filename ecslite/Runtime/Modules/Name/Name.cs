using System;
using System.Text;
using Saro.FSnapshot;

namespace Saro.Entities
{
    /// <summary>
    /// Name component for entity
    /// </summary>
    [FSnapshotable]
    public partial class EntityName : IEcsComponent, IEcsCleanup<EntityName>
    {
        [FSnapshot] public string name; // TODO FixedString 后，可以

        public EntityName() : this(null) { }

        public EntityName(string name)
        {
            this.name = name;
        }

        public void Cleanup(ref EntityName c)
        {
            c.name = null;
        }
    }

    partial class EntityName : IEquatable<EntityName>
    {
        public bool Equals(EntityName other)
        {
            if (other is null) return false;
            return name == other.name;
        }

        public override bool Equals(object obj) => obj is EntityName other && Equals(other);

        public override int GetHashCode() => (name != null ? name.GetHashCode() : 0);

        public static bool operator !=(in EntityName x, in EntityName y) => !(x == y);

        public static bool operator ==(in EntityName x, in EntityName y)
        {
            if (x is null && y is null) return true;
            if (x is null) return false;
            return x.Equals(y);
        }
    }

    public static class EntityNameUtility
    {
        public const string k_EntityNameFormat = "X8";

        public static void SetEntityName(this EcsEntity entity, string name)
        {
            SetEntityName(entity.id, entity.World, name);
        }

        public static void SetEntityName(this int entity, EcsWorld world, string name)
        {
            ref var cName = ref world.EntityNamePool.GetOrAdd(entity);
            cName.name = name;
        }

        public static string GetEntityName(this EcsEntity entity)
        {
            return GetEntityName(entity.id, entity.World);
        }

        public static string GetEntityName(this int entity, EcsWorld world)
        {
            if (entity <= EcsEntity.Null.id)
            {
                return "Entity-Null";
            }

            if (!world.IsAlive() || !world.IsEntityAlive(entity))
            {
                return "Entity-NonAlive";
            }

            var namePool = world.EntityNamePool;
            if (namePool.Has(entity))
            {
                return $"{namePool.Get(entity).name}";
            }

            return string.Empty; // 更安全
        }


        public static string GetEntityInfo(this EcsEntity entity)
        {
            return GetEntityInfo(entity.id, entity.World);
        }

        public static string GetEntityInfo(this int entity, EcsWorld world)
        {
            return $"{world.worldId}:{entity}.{world.GetEntityGen(entity)}({EntityNameUtility.GetEntityName(entity, world)})";
        }

        public static string GetEntityDetial(this int entity, EcsWorld world)
        {
            StringBuilder sb = null;
            if (world.IsAlive() && world.IsEntityAlive(entity))
            {
                Type[] types = null;
                var count = world.GetComponentTypes(entity, ref types);
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
            }

            return $"{EntityNameUtility.GetEntityInfo(entity, world)} [{sb}]";
        }
    }
}