#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;

namespace Saro.Entities.Serialization
{
    public interface IEcsReader : IDisposable
    {
        bool ReadBoolean();
        string ReadString();
        int ReadInt32();
        byte ReadByte();
        Single ReadSingle();
        //float3 ReadSingle3();

        int Read(Span<byte> buffer);

        void ReadUnmanaged<T>(ref T obj) where T : unmanaged;
        int ReadArrayUnmanaged<T>(ref T[] array) where T : unmanaged;

        void Reset();
    }

    public interface IEcsWriter : IDisposable
    {
        void Write(bool value);
        void Write(string value);
        void Write(int value);
        void Write(byte value);
        void Write(Single value);
        //void Write(float3 value);

        void Write(ReadOnlySpan<byte> buffer);

        void WriteUnmanaged<T>(ref T obj) where T : unmanaged;
        void WriteArrayUnmanaged<T>(ref T[] array, int length) where T : unmanaged;

        void Reset(); // TODO 可能叫Seek更好？
    }
}
