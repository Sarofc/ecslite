﻿using System;

namespace Saro.Entities.Serialization
{
    public interface IEcsReader : IDisposable
    {
        int ReadInt32();
        int Read(Span<byte> buffer);


        void ReadUnmanaged<T>(ref T obj) where T : unmanaged;
        int ReadArrayUnmanaged<T>(ref T[] array) where T : unmanaged;
        void Reset();
    }

    public interface IEcsWriter : IDisposable
    {
        void Write(int value);
        void Write(ReadOnlySpan<byte> buffer);


        void WriteUnmanaged<T>(ref T obj) where T : unmanaged;
        void WriteArrayUnmanaged<T>(ref T[] array, int length) where T : unmanaged;

        void Reset(); // TODO 可能叫Seek更好？
    }
}
