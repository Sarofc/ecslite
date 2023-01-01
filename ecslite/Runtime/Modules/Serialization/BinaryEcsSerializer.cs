using System;
using System.IO;
using Saro.Utility;

using Saro.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Saro.IO
{
    public static class BinaryReaderWriterExtension
    {
        [ThreadStatic]
        static byte[] s_buffer = new byte[1024];

        public unsafe static void WriteUnmanaged<T>(this BinaryWriter writer, in T obj) where T : unmanaged
        {
            var size = sizeof(T);
            fixed (T* ptr = &obj)
            {
                Span<byte> buffer = new((byte*)ptr, size);
                writer.Write(buffer);
            }
        }

        public unsafe static void ReadUnmanaged<T>(this BinaryReader reader, ref T obj) where T : unmanaged
        {
            var size = sizeof(T);
            //Span<byte> buffer = stackalloc byte[size]; // 大小限制
            Span<byte> buffer = new(s_buffer, 0, size);
            var count = reader.Read(buffer);
            Log.Assert(size == count, $"read bytes error: size != count: {size} != {count}");
            fixed (T* ptr = &obj)
            fixed (byte* pSrc = &s_buffer[0])
            {
                byte* pDst = (byte*)ptr;

                //UnsafeUtility.MemCpy(pSrc, pDst, count);
                Buffer.MemoryCopy(pSrc, pDst, size, size);
                //Unsafe.CopyBlock(, pDst, (uint)count);
            }
        }

        public static void WriteArrayUnmanaged<T>(this BinaryWriter writer, ref T[] array, int length) where T : unmanaged
        {
            writer.Write(length);
            if (length > 0)
            {
                unsafe
                {
                    var size = sizeof(T) * length;
                    fixed (T* ptr = &array[0])
                    {
                        Span<byte> buffer = new((byte*)ptr, size);
                        writer.Write(buffer);
                    }
                }
            }
        }

        public unsafe static int ReadArrayUnmanaged<T>(this BinaryReader reader, ref T[] array) where T : unmanaged
        {
            var arrayLength = reader.ReadInt32();
            if (array.Length < arrayLength)
                array = new T[arrayLength];
            var size = sizeof(T) * arrayLength;
            //Span<byte> buffer = stackalloc byte[size]; // TODO 大小限制
            var count = reader.Read(s_buffer, 0, size);
            Log.Assert(size == count, $"read bytes error: size != count: {size} != {count}");
            fixed (T* ptr = &array[0])
            fixed (byte* pSrc = &s_buffer[0])
            {
                byte* pDst = (byte*)ptr;

                //UnsafeUtility.MemCpy(pSrc, pDst, count);
                Buffer.MemoryCopy(pSrc, pDst, size, size);
                //Unsafe.CopyBlock(, pDst, (uint)count);
            }
            return arrayLength;
        }
    }
}

namespace Saro.Entities.Serialization
{
    public sealed class BinaryEcsSerializer : IEcsSerializer
    {
        public void Serialize(WorldData inData, IEcsWriter writer)
        {
            //var stream = new MemoryStream();
            //var writer = new BinaryWriter(stream);

            //data.entities
            //data.entitiesCount
            //data.recycledEntities
            //data.recycledEntitiesCount
            //data.pools =
            //data.poolsCount =

            writer.WriteArrayUnmanaged(ref inData.entities, inData.entitiesCount);
            //writer.Write(inData.entitiesCount);
            writer.WriteArrayUnmanaged(ref inData.recycledEntities, inData.recycledEntitiesCount);
            //writer.Write(inData.recycledEntitiesCount);
            //writer.WriteArrayUnmanaged(inData.pools);
            //writer.Write(inData.poolsCount);

            //var json = JsonHelper.ToJson(data);
            //writer.Write(json);
            //stream.Position = 0;
            //return stream;
        }

        public void Deserialize(IEcsReader reader, WorldData outData)
        {
            outData.entitiesCount = reader.ReadArrayUnmanaged(ref outData.entities);
            //outData.entitiesCount = reader.ReadInt32();
            outData.recycledEntitiesCount = reader.ReadArrayUnmanaged(ref outData.recycledEntities);
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
