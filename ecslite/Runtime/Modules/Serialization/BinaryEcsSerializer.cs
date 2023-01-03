using System;
using System.IO;
using Saro.Utility;

using Saro.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Saro.Entities.Serialization
{
    public sealed class BinaryEcsSerializer : IEcsSerializer
    {
        readonly IEcsWriter writer;
        readonly IEcsReader reader;
        readonly MemoryStream m_WriterStream;
        readonly MemoryStream m_ReaderStream;

        public BinaryEcsSerializer()
        {

        }

        public void Serialize(WorldState inData)
        {
            //var stream = new MemoryStream();
            //var writer = new BinaryWriter(stream);

            //data.entities
            //data.entitiesCount
            //data.recycledEntities
            //data.recycledEntitiesCount
            //data.pools =
            //data.poolsCount =

            writer.WriteArrayUnmanaged(ref inData.entities, inData.m_EntitiesCount);
            //writer.Write(inData.entitiesCount);
            writer.WriteArrayUnmanaged(ref inData.m_RecycledEntities, inData.m_RecycledEntitiesCount);
            //writer.Write(inData.recycledEntitiesCount);
            //writer.WriteArrayUnmanaged(inData.pools);
            //writer.Write(inData.poolsCount);

            //var json = JsonHelper.ToJson(data);
            //writer.Write(json);
            //stream.Position = 0;
            //return stream;
        }

        public void Deserialize(WorldState outData)
        {
            outData.m_EntitiesCount = reader.ReadArrayUnmanaged(ref outData.entities);
            //outData.entitiesCount = reader.ReadInt32();
            outData.m_RecycledEntitiesCount = reader.ReadArrayUnmanaged(ref outData.m_RecycledEntities);
            //outData.recycledEntitiesCount = reader.ReadInt32();
            //reader.ReadArrayUnmanaged(ref data.pools);
            //outData.poolsCount = reader.ReadInt32();
        }


        //public void Deserialize(Stream stream, ref WorldData data)
        //{
        //    using var reader = new BinaryReader(stream);

        //    reader.ReadArrayUnmanaged(ref data.entities);
        //    data.entitiesCount = reader.ReadInt32();
        //    reader.ReadArrayUnmanaged(ref data.recycledEntities);
        //    data.recycledEntitiesCount = reader.ReadInt32();
        //    //reader.ReadArrayUnmanaged(ref data.pools);
        //    data.poolsCount = reader.ReadInt32();
        //}

    }
}
