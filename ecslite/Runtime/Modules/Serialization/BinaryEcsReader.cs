﻿#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;
using System.IO;
using Saro.IO;

namespace Saro.Entities.Serialization
{
    public class BinaryEcsReader : IEcsReader
    {
        private readonly Stream m_Stream;
        private readonly BinaryReader m_Reader;

        public BinaryEcsReader(Stream stream)
        {
            m_Stream = stream;
            m_Reader = new(m_Stream);
        }

        public bool ReadBoolean() => m_Reader.ReadBoolean();
        public string ReadString() => m_Reader.ReadString();
        public int ReadInt32() => m_Reader.ReadInt32();
        public byte ReadByte() => m_Reader.ReadByte();
        public Single ReadSingle()
        {
#if FIXED_POINT_MATH
            return sfloat.FromRaw(m_Reader.ReadUInt32());
#else
            return m_Reader.ReadSingle();
#endif
        }
//        public float3 ReadSingle3()
//        {
//#if FIXED_POINT_MATH
//            var x = sfloat.FromRaw(m_Reader.ReadUInt32());
//            var y = sfloat.FromRaw(m_Reader.ReadUInt32());
//            var z = sfloat.FromRaw(m_Reader.ReadUInt32());
//#else
//            var x = m_Reader.ReadSingle();
//            var y = m_Reader.ReadSingle();
//            var z = m_Reader.ReadSingle();
//#endif
//            return new float3(x, y, z);
//        }

        public int Read(Span<byte> buffer) => m_Reader.Read(buffer);

        public void ReadUnmanaged<T>(ref T obj) where T : unmanaged => m_Reader.ReadUnmanaged(ref obj);
        public int ReadArrayUnmanaged<T>(ref T[] array) where T : unmanaged => m_Reader.ReadArrayUnmanaged(ref array);

        public void Dispose()
        {
            m_Stream?.Dispose();
            m_Reader?.Dispose();
        }

        public void Reset()
        {
            m_Stream.Position = 0;
        }
    }
}
