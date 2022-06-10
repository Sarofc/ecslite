using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Saro.Pool;

namespace Saro.Entities.Serialization
{
    public sealed class EcsJsonSerializer : IEcsSerializer<string>
    {
        JsonSerializerSettings m_JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        public string Serialize(int entity, EcsWorld world)
        {
            using (ListPool<object>.Rent(out var components))
            {
                world.GetComponents(entity, ref components);

                return JsonConvert.SerializeObject(components, m_JsonSerializerSettings);
            }
        }

        public int Deserialize(string json, EcsWorld world)
        {
            var ent = world.NewEntity();

            var components = JsonConvert.DeserializeObject<object[]>(json, m_JsonSerializerSettings);

            EcsSerializer.Initialize(ent, components, world, true);

            return ent;
        }
    }
}
