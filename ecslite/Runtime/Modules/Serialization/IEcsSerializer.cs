#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;

namespace Saro.Entities.Serialization
{
    public interface IEcsSerializable
    {
        void Serialize(IEcsWriter writer);
        void Deserialize(IEcsReader reader);
    }

    public interface IEcsReader : IDisposable
    {
        bool ReadBoolean();
        string ReadString();
        int ReadInt32();
        byte ReadByte();
        Single ReadSingle();

        int Read(Span<byte> buffer);

        void ReadUnmanaged<T>(ref T obj) where T : unmanaged;
        int ReadArrayUnmanaged<T>(ref T[] array) where T : unmanaged;
        void ReadListUnmanaged<T>(ref List<T> list) where T : unmanaged;

        void ReadRef<T>(ref T @ref) where T : class;

        void Reset();
    }

    public interface IEcsWriter : IDisposable
    {
        void BeginWriteObject(string name, bool skipScope = false);
        void EndWriteObject();

        void Write(bool value);
        void Write(string value);
        void Write(int value);
        void Write(byte value);
        void Write(Single value);

        void Write(ReadOnlySpan<byte> buffer);

        void WriteUnmanaged<T>(ref T obj) where T : unmanaged;
        void WriteArrayUnmanaged<T>(ref T[] array, int length) where T : unmanaged;
        void WriteListUnmanaged<T>(ref List<T> list) where T : unmanaged;

        void WriteRef<T>(ref T @ref) where T : class;

        void Reset();
    }
}
