#if FIXED_POINT_MATH
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
    public class BinaryEcsWriter : IEcsWriter
    {
        private readonly Stream m_Stream;
        private readonly BinaryWriter m_Writer;

        public BinaryEcsWriter(Stream stream)
        {
            m_Stream = stream;
            m_Writer = new(m_Stream);
        }

        public void Write(bool value) => m_Writer.Write(value);
        public void Write(string value) => m_Writer.Write(value);
        public void Write(int value) => m_Writer.Write(value);
        public void Write(byte value) => m_Writer.Write(value);
        public void Write(Single value)
        {
#if FIXED_POINT_MATH
            m_Writer.Write(value.RawValue);
#else
            m_Writer.Write(value);
#endif
        }
//        public void Write(float3 value)
//        {
//#if FIXED_POINT_MATH
//            m_Writer.Write(value.x.RawValue);
//            m_Writer.Write(value.y.RawValue);
//            m_Writer.Write(value.z.RawValue);
//#else
//            m_Writer.Write(value.x);
//            m_Writer.Write(value.y);
//            m_Writer.Write(value.z);
//#endif
//        }

        public void Write(ReadOnlySpan<byte> buffer) => m_Writer.Write(buffer);

        public void WriteUnmanaged<T>(ref T obj) where T : unmanaged => m_Writer.WriteUnmanaged(ref obj);
        public void WriteArrayUnmanaged<T>(ref T[] array, int length) where T : unmanaged => m_Writer.WriteArrayUnmanaged(ref array, length);

        public void Dispose()
        {
            m_Stream?.Dispose();
            m_Writer?.Dispose();
        }

        public void Reset()
        {
            m_Stream.Position = 0;
        }
    }
}
