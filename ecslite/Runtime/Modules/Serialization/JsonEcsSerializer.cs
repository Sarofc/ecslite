//using System;
//using System.IO;
//using Saro.Utility;

//namespace Saro.Entities.Serialization
//{
//    [Obsolete]
//    public sealed class JsonEcsSerializer : IEcsSerializer
//    {
//        public Stream Serialize(WorldData world)
//        {
//            var stream = new MemoryStream();
//            var writer = new StreamWriter(stream);
//            var json = JsonHelper.ToJson(world);
//            writer.Write(json);
//            stream.Position = 0;
//            return stream;
//        }

//        public WorldData Deserialize(Stream stream)
//        {
//            var reader = new StreamReader(stream);
//            var json = reader.ReadToEnd();
//            return JsonHelper.FromJson<WorldData>(json);
//        }
//    }
//}
