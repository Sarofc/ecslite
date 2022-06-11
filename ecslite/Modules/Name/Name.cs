using System;
using Saro.Entities.Authoring;

namespace Saro.Entities
{
    /// <summary>
    /// Name component for entity
    /// </summary>
    public struct Name : IEcsComponentAuthoring, IEquatable<Name>
    {
        public string name; // TODO FixedString?

        public Name(string name)
        {
            this.name = name;
        }

        public const string k_EntityNameFormat = "X8";

        public static string GetEntityName(int entity, EcsWorld world, string entityNameFormat = k_EntityNameFormat)
        {
            if (world.IsEntityAlive_Internal(entity))
            {
                var namePool = world.NamePool;
                if (namePool.Has(entity))
                {
                    return $"{namePool.Get(entity).name}";
                }
            }

            return entity.ToString(entityNameFormat);
        }

        public static string GetEntityInfo(int entity, EcsWorld world)
        {
            return $"{Name.GetEntityName(entity, world)}\t{world.worldID}:{entity}.{world.GetEntityGen(entity)}";
        }

        public bool Equals(Name other) => name == other.name;

        public override bool Equals(object obj) => obj is Name other && Equals(other);

        public override int GetHashCode() => (name != null ? name.GetHashCode() : 0);

        public static bool operator !=(in Name x, in Name y) => !(x == y);

        public static bool operator ==(in Name x, in Name y) => x.name == y.name;
    }
}