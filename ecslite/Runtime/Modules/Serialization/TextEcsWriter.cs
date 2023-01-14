#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using Saro.Utility;

namespace Saro.Entities.Serialization
{
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

        public void BeginWriteObject(string name, bool skipScope = false)
        {
            m_SkipScope = skipScope;
            WriteIndent();
            m_Writer.Write(name);
            m_Writer.WriteLine();
            IncrementIndent();
        }

        public void EndWriteObject()
        {
            m_SkipScope = false;
            DecrementIndent();
        }

        public void Write(bool value) => WriteLineIndent(value.ToString());
        public void Write(string value) => WriteLineIndent(value);
        public void Write(int value) => WriteLineIndent(value.ToString());
        public void Write(byte value) => WriteLineIndent(value.ToString());
        public void Write(Single value) => WriteLineIndent(value.ToString());

        public void Write(ReadOnlySpan<byte> buffer)
        {
            if (m_SkipScope) return;

            WriteIndent();

            //m_Writer.Write(nameof(buffer));
            //m_Writer.Write(" : ");
            m_Writer.WriteLine(HashUtility.ToHexString(buffer));
        }

        public void WriteArrayUnmanaged<T>(ref T[] array, int length) where T : unmanaged
        {
            if (m_SkipScope) return;

            WriteIndent();

            //var typeName = array.GetType().Name;
            //m_Writer.Write(typeName);
            //m_Writer.Write(" : [");
            m_Writer.Write("[");
            for (int i = 0; i < length; i++)
            {
                m_Writer.Write(array[i].ToString());
                if (i < length - 1) m_Writer.Write(',');
            }
            m_Writer.Write(']');
            m_Writer.WriteLine();
        }
        public void WriteListUnmanaged<T>(ref List<T> list) where T : unmanaged
        {
            if (m_SkipScope) return;

            WriteIndent();

            //var typeName = list.GetType().Name;
            //m_Writer.Write(typeName);
            //m_Writer.Write(" : ");
            m_Writer.Write("[");
            for (int i = 0; i < list.Count; i++)
            {
                m_Writer.Write(list[i].ToString());
                if (i < list.Count - 1) m_Writer.Write(',');
            }
            m_Writer.Write(']');
            m_Writer.WriteLine();
        }

        private List<object> m_Refs = new();
        public void WriteRef<T>(ref T @ref) where T : class
        {
            if (m_SkipScope) return;

            WriteIndent();

            //m_Writer.Write(@ref.GetType().Name);

            if (@ref is IEcsSerializable serializable)
            {
                var index = m_Refs.IndexOf(@ref);
                var hasRef = index >= 0;
                if (hasRef)
                {
                    m_Writer.WriteLine($"$ref:{index}");
                }
                else
                {
                    m_Writer.WriteLine($"$ref:{m_Refs.Count}");
                    serializable.Serialize(this);

                    m_Refs.Add(@ref);
                }
            }
            else
            {
                Log.ERROR($"{@ref.GetType().Name} not impl {nameof(IEcsSerializable)}");
            }
        }

        public void WriteUnmanaged<T>(ref T obj) where T : unmanaged
        {
            if (m_SkipScope) return;

            WriteIndent();

            //m_Writer.Write(obj.GetType().Name);
            //m_Writer.Write(" : ");
            m_Writer.WriteLine(obj.ToString());
        }

        #region Scope management

        private bool m_SkipScope;

        public int CurrentIndent => m_CurrentIndent;

        void WriteIndent()
        {
            if (m_SkipScope) return;

            m_Writer.Write(m_IndentText);
        }

        void WriteLineIndent(string val)
        {
            if (m_SkipScope) return;

            m_Writer.Write(m_IndentText);
            m_Writer.WriteLine(val);
        }

        void IncrementIndent()
        {
            ++m_CurrentIndent;
            m_IndentText += "  ";
        }

        void DecrementIndent()
        {
            if (--m_CurrentIndent < 0)
            {
                throw new InvalidOperationException("Indent can't be less than 0");
            }

            m_IndentText = m_IndentText.Substring(0, m_CurrentIndent * 2);
        }

        int m_CurrentIndent;
        string m_IndentText;

        #endregion
    }
}
