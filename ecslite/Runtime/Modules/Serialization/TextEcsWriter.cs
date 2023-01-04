﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Saro.Entities.Serialization
{
    // TODO 改成yaml ？保证可读性

    public class TextEcsWriter : IEcsWriter
    {
        private Stream m_Stream;
        private StreamWriter m_Writer;

        public TextEcsWriter(Stream stream)
        {
            m_Stream = stream;
            m_Writer = new StreamWriter(m_Stream);
        }

        public void Dispose()
        {
            m_Writer?.Dispose();
            m_Stream?.Dispose();
        }

        public void Reset()
        {
            m_Stream.Position = 0;
        }

        public void Write(int value)
        {
            m_Writer.WriteLine(value);
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            m_Writer.Write(nameof(buffer));
            m_Writer.Write(" : ");
            m_Writer.WriteLine(buffer.ToString());
        }

        public void WriteArrayUnmanaged<T>(ref T[] array, int length) where T : unmanaged
        {
            if (length > 0)
            {
                m_Writer.Write(length);

                m_Writer.Write(" : [");
                for (int i = 0; i < length; i++)
                {
                    m_Writer.Write(array[i].ToString());
                    if (i < length - 1) m_Writer.Write(',');
                }
                m_Writer.Write(']');
            }
            m_Writer.WriteLine();
        }

        public void WriteUnmanaged<T>(ref T obj) where T : unmanaged
        {
            m_Writer.WriteLine(obj.ToString());
        }
    }
}