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

        public int ReadInt32() => m_Reader.ReadInt32();
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
