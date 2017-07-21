using System;
using System.IO;

namespace CBOR.Extensions
{
    internal static class BinaryReaderExtension
    {
        internal static byte[] Reverse(this byte[] b)
        {
            Array.Reverse(b);
            return b;
        }

        internal static UInt16 ReadUInt16BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt16(reader.ReadBytesRequired(sizeof(UInt16)).Reverse(), 0);
            else
                return reader.ReadUInt16();
        }

        internal static Int16 ReadInt16BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt16(reader.ReadBytesRequired(sizeof(Int16)).Reverse(), 0);
            else
                return reader.ReadInt16();
        }

        internal static UInt32 ReadUInt32BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt32(reader.ReadBytesRequired(sizeof(UInt32)).Reverse(), 0);
            else
                return reader.ReadUInt32();
        }

        internal static Int32 ReadInt32BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt32(reader.ReadBytesRequired(sizeof(Int32)).Reverse(), 0);
            else
                return reader.ReadInt32();
        }

        internal static UInt64 ReadUInt64BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToUInt64(reader.ReadBytesRequired(sizeof(UInt64)).Reverse(), 0);
            else
                return reader.ReadUInt64();
        }

        internal static Int64 ReadInt64BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
                return BitConverter.ToInt64(reader.ReadBytesRequired(sizeof(UInt64)).Reverse(), 0);
            else
                return reader.ReadInt64();
        }

        internal static byte[] ReadBytesRequired(this BinaryReader reader, int length)
        {
            var result = reader.ReadBytes(length);

            if (result.Length != length)
                throw new EndOfStreamException(string.Format("{0} bytes required from stream, but only {1} returned.", length, result.Length));

            return result;
        }
    }
}
