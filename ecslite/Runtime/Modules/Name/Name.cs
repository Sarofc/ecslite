using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Saro.Entities
{
    /// <summary>
    /// Name component for entity
    /// </summary>
    public partial class Name : IEcsManagedComponent<Name>
    {
        public string name; // TODO FixedString 后，可以

        public Name() : this(null) { }

        public Name(string name)
        {
            this.name = name;
        }

        public void AutoReset(ref Name c)
        {
            c.name = null;
        }
    }

    partial class Name : IEquatable<Name>
    {
        public bool Equals(Name other) => name == other.name;

        public override bool Equals(object obj) => obj is Name other && Equals(other);

        public override int GetHashCode() => (name != null ? name.GetHashCode() : 0);

        public static bool operator !=(in Name x, in Name y) => !(x == y);

        public static bool operator ==(in Name x, in Name y) => x.name == y.name;
    }

    partial class Name // utility
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

            var namePool = world.NamePool;
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
            return $"{world.worldId}:{entity}.{world.GetEntityGen(entity)}({Name.GetEntityName(entity, world, entityNameFormat)})";
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

            return $"{Name.GetEntityInfo(entity, world, entityNameFormat)} [{sb}]";
        }
    }
}