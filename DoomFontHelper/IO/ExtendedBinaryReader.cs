using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DoomFontHelper.Helpers.IO
{
    public class ExtendedBinaryReader : BinaryReader
    {
        public ExtendedBinaryReader(Stream input) : base(input)
        {

        }

        public string ReadStringToNull()
        {
            List<byte> bytes = new List<byte>();
            byte b;
            while ((b = ReadByte()) != 0)
            {
                bytes.Add(b);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public void CheckSignature(string sig)
        {
            var sigBytes = Encoding.UTF8.GetBytes(sig);

            var data = ReadBytes(sigBytes.Length);

            if (!data.SequenceEqual(sigBytes))
            {
                var enc = Encoding.GetEncoding(Encoding.UTF8.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                throw new IOException($"Invalid signature. Expected {sig}, got {enc.GetString(data)}");
            }
        }

        public T ReadStruct<T>() where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            Span<byte> arr = stackalloc byte[size];
            BaseStream.Read(arr);
            return MemoryMarshal.Cast<byte, T>(arr)[0];
        }

        public T[] ReadStructArray<T>(int totalSize) where T : struct
        {
            T[] arr = new T[totalSize / Marshal.SizeOf(typeof(T))];
            Read(MemoryMarshal.Cast<T, byte>(arr));
            return arr;
        }

        public string ReadStringAtPosition(int position)
        {
            long prevPos = BaseStream.Position;

            BaseStream.Position = position;
            string result = ReadStringToNull();

            BaseStream.Position = prevPos;
            return result;
        }

        public string ReadFixedLengthAsciiString(int length)
        {
            byte[] bytes = ReadBytes(length);
            int zeroIndex = Array.IndexOf<byte>(bytes, 0);
            return Encoding.ASCII.GetString(bytes, 0, zeroIndex);
        }

        public T[] ReadObjectArray<T>(int count) where T : IBinarySerializable, new()
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                T obj = new T();
                obj.Load(this);
                result[i] = obj;
            }
            return result;
        }

        public void AlignStream(int alignBase = 16)
        {
            long pos = BaseStream.Position;
            if (pos % alignBase != 0)
            {
                pos = pos - pos % alignBase + alignBase;
                BaseStream.Position = pos;
            }
        }

        public void CheckedAlignStream(int alignBase)
        {
            long pos = BaseStream.Position;
            if (pos % alignBase != 0)
            {
                int count = (int)(alignBase - pos % alignBase);
                for (int i = 0; i < count; i++)
                {
                    if (ReadByte() != 0)
                    {
                        throw new IOException($"CheckedAlign has read a non-null byte at {BaseStream.Position - 1}");
                    }
                }
            }
        }

        /// <summary>
        /// Reads a signed short value from the stream.
        /// </summary>
        public short ReadInt16Reverse()
        {
            return ReadReverse<short>(BitConverter.GetBytes(ReadInt16()));
        }

        /// <summary>
        /// Reads an unsigned short value from the stream.
        /// </summary>
        public ushort ReadUInt16Reverse()
        {
            return ReadReverse<ushort>(BitConverter.GetBytes(ReadUInt16()));
        }

        /// <summary>
        /// Reads a signed integer value from the stream.
        /// </summary>
        public int ReadInt32Reverse()
        {
            return ReadReverse<int>(BitConverter.GetBytes(ReadInt32()));
        }

        /// <summary>
        /// Reads an unsigned integer value from the stream.
        /// </summary>
        public uint ReadUInt32Reverse()
        {
            return ReadReverse<uint>(BitConverter.GetBytes(ReadUInt32()));
        }

        /// <summary>
        /// Reads a signed long value from the stream.
        /// </summary>
        public long ReadInt64Reverse()
        {
            return ReadReverse<long>(BitConverter.GetBytes(ReadInt64()));
        }

        /// <summary>
        /// Reads an unsigned long value from the stream.
        /// </summary>
        public ulong ReadUInt64Reverse()
        {
            return ReadReverse<ulong>(BitConverter.GetBytes(ReadUInt64()));
        }

        /// <summary>
        /// Reads a single value value from the stream.
        /// </summary>
        public float ReadFloat32Reverse()
        {
            return ReadReverse<float>(BitConverter.GetBytes(ReadSingle()));
        }

        /// <summary>
        /// Reads a double value value from the stream.
        /// </summary>
        public double ReadFloat64Reverse()
        {
            return ReadReverse<double>(BitConverter.GetBytes(ReadDouble()));
        }

        private T ReadReverse<T>(byte[] data) where T : struct
        {
            data.AsSpan().Reverse();

            return MemoryMarshal.Cast<byte, T>(data)[0];
        }

        public byte[] ReaBytesReverse(int count)
        {
            return [.. ReadBytes(count).Reverse()];
        }
    }
}
