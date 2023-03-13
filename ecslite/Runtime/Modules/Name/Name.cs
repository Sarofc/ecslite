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

    partial class EntityName // utility
    {
        public const string k_EntityNameFormat = "X8";

        public static string GetEntityName(EcsEntity entity, string entityNameFormat = k_EntityNameFormat)
        {
            return GetEntityName(entity.id, entity.World, entityNameFormat);
        }

        public static string GetEntityName(int entity, EcsWorld world, string entityNameFormat = k_EntityNameFormat)
        {
            if (entity <= EcsEntity.k_Null.id)
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

            return entity.ToString(entityNameFormat);
        }


        public static string GetEntityInfo(EcsEntity entity, string entityNameFormat = k_EntityNameFormat)
        {
            return GetEntityInfo(entity.id, entity.World, entityNameFormat);
        }

        public static string GetEntityInfo(int entity, EcsWorld world, string entityNameFormat = k_EntityNameFormat)
        {
            return $"{world.worldId}:{entity}.{world.GetEntityGen(entity)}({EntityName.GetEntityName(entity, world, entityNameFormat)})";
        }

        public static string GetEntityDetial(int entity, EcsWorld world, string entityNameFormat = k_EntityNameFormat)
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

            return $"{EntityName.GetEntityInfo(entity, world, entityNameFormat)} [{sb}]";
        }
    }
}