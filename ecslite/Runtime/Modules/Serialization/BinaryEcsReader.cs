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
using System.Collections.Generic;

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

        public int Read(Span<byte> buffer) => m_Reader.Read(buffer);

        public void ReadUnmanaged<T>(ref T obj) where T : unmanaged => m_Reader.ReadUnmanaged(ref obj);
        public int ReadArrayUnmanaged<T>(ref T[] array, int index = 0) where T : unmanaged => m_Reader.ReadArrayUnmanaged(ref array, index);
        public void ReadListUnmanaged<T>(ref List<T> list) where T : unmanaged => m_Reader.ReadListUnmanaged(ref list);

        private List<object> m_Refs = new();
        public void ReadRef<T>(ref T @ref) where T : class
        {
            if (@ref is IEcsSerializable serializable)
            {
                var isRef = m_Reader.ReadBoolean();
                if (isRef)
                {
                    var index = m_Reader.ReadInt32();
                    @ref = (T)m_Refs[index];
                }
                else
                {
                    serializable.Deserialize(this);
                    m_Refs.Add(@ref);
                }
            }
            else
            {
                Log.ERROR($"{@ref.GetType().Name} not impl {nameof(IEcsSerializable)}");
            }
        }

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
