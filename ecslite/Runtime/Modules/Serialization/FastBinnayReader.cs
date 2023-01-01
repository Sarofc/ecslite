namespace Saro.IO
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// TODO 先测试了性能再说
    /// 速度更快的二进制读写，并减少使用的byte空间，参考protobuf改来
    /// 参考文档 https://stackoverflow.com/questions/2036718/fastest-way-of-reading-and-writing-binary
    /// https://jacksondunstan.com/articles/3318
    /// </summary>
    public sealed class FastBinnayReader
    {
        readonly UTF8Encoding encoding = new UTF8Encoding();

        private Stream _source;
        private byte[] _ioBuffer;
        private int _ioIndex;
        private int _position;
        private int _available;

        public FastBinnayReader()
        {
            _ioBuffer = new byte[256];
        }

        public void Init(Stream s)
        {
            _source = s;

            _ioIndex = 0;
            _available = 0;
            _position = 0;
        }

        public uint ReadUInt32()
        {
            return ReadUInt32Variant(false);
        }
        private const int Int32Msb = ((int)1) << 31;
        private int Zag(uint ziggedValue)
        {
            int value = (int)ziggedValue;
            return (-(value & 0x01)) ^ ((value >> 1) & ~Int32Msb);
        }
        public int ReadInt32()
        {
            return Zag(ReadUInt32Variant(true));
        }

        public ulong ReadUInt64()
        {
            return ReadUInt64Variant();
        }

        private const long Int64Msb = ((long)1) << 63;
        private long Zag(ulong ziggedValue)
        {
            long value = (long)ziggedValue;
            return (-(value & 0x01L)) ^ ((value >> 1) & ~Int64Msb);
        }

        public long ReadInt64()
        {
            return Zag(ReadUInt64Variant());
        }

        public string ReadString()
        {
            int bytes = (int)ReadUInt32Variant(false);
            if (bytes == 0) return "";
            if (_available < bytes) Ensure(bytes, true);
            string s = encoding.GetString(_ioBuffer, _ioIndex, bytes);
            _available -= bytes;
            _position += bytes;
            _ioIndex += bytes;
            return s;
        }

        public bool ReadBoolean()
        {
            switch (ReadUInt32())
            {
                case 0: return false;
                case 1: return true;
                default: throw CreateException("Unexpected boolean value");
            }
        }

        internal int TryReadUInt32VariantWithoutMoving(bool trimNegative, out uint value)
        {
            if (_available < 10) Ensure(10, false);
            if (_available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = _ioIndex;
            value = _ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (_available == 1) throw EoF();

            uint chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (_available == 2) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (_available == 3) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (_available == 4) throw EoF();

            chunk = _ioBuffer[readPos];
            value |= chunk << 28; // can only use 4 bits from this chunk
            if ((chunk & 0xF0) == 0) return 5;

            if (trimNegative // allow for -ve values
                && (chunk & 0xF0) == 0xF0
                && _available >= 10
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0x01)
            {
                return 10;
            }
            throw CreateException("OverflowException");
        }

        private uint ReadUInt32Variant(bool trimNegative)
        {
            uint value;
            int read = TryReadUInt32VariantWithoutMoving(trimNegative, out value);
            if (read > 0)
            {
                _ioIndex += read;
                _available -= read;
                _position += read;
                return value;
            }
            throw EoF();
        }

        private int TryReadUInt64VariantWithoutMoving(out ulong value)
        {
            if (_available < 10) Ensure(10, false);
            if (_available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = _ioIndex;
            value = _ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (_available == 1) throw EoF();

            ulong chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (_available == 2) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (_available == 3) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (_available == 4) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) return 5;
            if (_available == 5) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) return 6;
            if (_available == 6) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) return 7;
            if (_available == 7) throw EoF();


            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) return 8;
            if (_available == 8) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) return 9;
            if (_available == 9) throw EoF();

            chunk = _ioBuffer[readPos];
            value |= chunk << 63; // can only use 1 bit from this chunk

            if ((chunk & ~(ulong)0x01) != 0) throw CreateException("OverflowException");
            return 10;
        }

        private ulong ReadUInt64Variant()
        {
            ulong value;
            int read = TryReadUInt64VariantWithoutMoving(out value);
            if (read > 0)
            {
                _ioIndex += read;
                _available -= read;
                _position += read;
                return value;
            }
            throw EoF();
        }

        internal void Ensure(int count, bool strict)
        {
            if (count > _ioBuffer.Length)
            {
                throw new Exception("too big byte block needs");
            }
            if (_ioIndex + count >= _ioBuffer.Length)
            {
                Buffer.BlockCopy(_ioBuffer, _ioIndex, _ioBuffer, 0, _available);
                _ioIndex = 0;
            }
            count -= _available;
            int writePos = _ioIndex + _available, bytesRead;
            int canRead = _ioBuffer.Length - writePos;

            while (count > 0 && canRead > 0 && (bytesRead = _source.Read(_ioBuffer, writePos, canRead)) > 0)
            {
                _available += bytesRead;
                count -= bytesRead;
                canRead -= bytesRead;
                writePos += bytesRead;
            }
            if (strict && count > 0)
            {
                throw EoF();
            }

        }

        private Exception CreateException(string message)
        {
            return new Exception(message);
        }

        private Exception EoF()
        {
            return new EndOfStreamException();
        }

        public void clear()
        {
            _source = null;
        }
    }
}