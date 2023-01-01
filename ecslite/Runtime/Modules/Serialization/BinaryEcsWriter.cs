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

        public void Write(int value) => m_Writer.Write(value);
        public void Write(ReadOnlySpan<byte> buffer) => m_Writer.Write(buffer);

        public void WriteUnmanaged<T>(in T obj) where T : unmanaged => m_Writer.WriteUnmanaged(in obj);
        public void WriteArrayUnmanaged<T>(ref T[] array, int length) where T : unmanaged => m_Writer.WriteArrayUnmanaged(ref array, length);

        public void Dispose()
        {
            m_Stream?.Dispose();
            m_Writer?.Dispose();
        }
    }
}
