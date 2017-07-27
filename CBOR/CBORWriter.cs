using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CBOR
{
    public class CBORWriter : IDisposable
    {
        private readonly BinaryWriter _writer;

        private Stack<CBORState> _state = new Stack<CBORState>();

        private CBORState State = new CBORState();

        private CBORState ParentState => _state.Peek();

        public CBORWriter(Stream output)
        {
            _writer = new BinaryWriter(output);
            _state.Push(null);
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Write(object value)
        {
            if (value is null)
                WriteNull();
            else if (value is bool)
                Write((bool)value);
            else if (value is byte)
                Write((byte)value);
            else if (value is short)
                Write((short)value);
            else if (value is ushort)
                Write((ushort)value);
            else if (value is int)
                Write((int)value);
            else if (value is uint)
                Write((uint)value);
            else if (value is long)
                Write((long)value);
            else if (value is ulong)
                Write((ulong)value);
            else if (value is float)
                Write((float)value);
            else if (value is double)
                Write((double)value);
            else if (value is byte[])
                Write((byte[])value);
            else if (value is string)
                Write((string)value);
        }

        public void WriteNull()
        {
            EncodeSimpleType(CBORMajorType.Primitive, CBORSimpleType.Null);
        }

        public void WriteUndefined()
        {
            EncodeSimpleType(CBORMajorType.Primitive, CBORSimpleType.Undefined);
        }

        public void WriteSimple(byte value)
        {
            Encode(CBORMajorType.Primitive, value);
        }

        public void Write(bool value)
        {
            EncodeSimpleType(CBORMajorType.Primitive, value ? CBORSimpleType.True : CBORSimpleType.False);
        }

        public void Write(int value)
        {
            EncodeValue(value);
        }

        public void Write(uint value)
        {
            Encode(CBORMajorType.UnsignedInteger, value);
        }

        public void Write(long value)
        {
            EncodeValue(value);
        }

        public void Write(ulong value)
        {
            Encode(CBORMajorType.UnsignedInteger, value);
        }

        public void Write(float value)
        {
            Encode(CBORMajorType.Primitive, BitConverter.GetBytes(value));
        }

        public void Write(double value)
        {
            Encode(CBORMajorType.Primitive, BitConverter.GetBytes(value));
        }

        public void Write(byte[] value)
        {
            Encode(CBORMajorType.ByteString, (ulong)value.Length);
            _writer.Write(value);
        }

        public void Write(string value)
        {
            Encode(CBORMajorType.TextString, (ulong)System.Text.Encoding.UTF8.GetByteCount(value));
            _writer.Write(System.Text.Encoding.UTF8.GetBytes(value));
        }

        public void BeginCollection(CBORMajorType collection, int length)
        {
            switch (collection)
            {
                case CBORMajorType.Array:
                case CBORMajorType.Map:
                    break;
                case CBORMajorType.TextString:
                case CBORMajorType.ByteString:
                    if (length >= 0)
                        throw new ArgumentException("Can not begin byte/text collection that is not indefinite");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (length < 0)
                EncodeSimpleType(collection, CBORSimpleType.Break);
            else
                Encode(collection, (ulong)length);

            State.Length = collection == CBORMajorType.Map ? length * 2 : length;
            State.MajorType = collection;
            Push();
        }

        public void EndCollection()
        {
            if (ParentState == null)
                throw new InvalidOperationException("Can not end non-existant collection");
            if(ParentState.Length>0)
                throw new InvalidOperationException("Collection can not be closed with insufficient items");

            if (ParentState.Length == 0)
            {
                Pop();
                return;
            }

            Pop();
            EncodeSimpleType(CBORMajorType.Primitive, CBORSimpleType.Break);

            // Need to account for Break not being treated as a new item in the collection
            if ((ParentState?.Length ?? -1) >= 0)
                ParentState.Length++;
        }

        private void Push(CBORState state = null)
        {
            _state.Push(State);
            State = state ?? new CBORState();
        }

        private void Pop()
        {
            State = _state.Pop();
        }

        private void EncodeValue(long value)
        {
            if (value < 0)
                Encode(CBORMajorType.NegativeInteger, (ulong)(-1 - value));
            else
                Encode(CBORMajorType.UnsignedInteger, (ulong)value);
        }

        private void EncodeSimpleType(CBORMajorType majorType, CBORSimpleType simpleType)
        {
            EncodeSimpleType(majorType, (byte)simpleType);
        }

        private void EncodeSimpleType(CBORMajorType majorType, byte value)
        {
            if (ParentState != null)
            {
                if (ParentState.Length > 0)
                    ParentState.Length--;
                else if (ParentState.Length == 0)
                    throw new IndexOutOfRangeException("Can not write more items to the collection");
            }
            _writer.Write((byte)(((int)majorType << 5) + (value & 0x1F)));
        }

        private void Encode(CBORMajorType majorType, UInt64 value)
        {
            if (value < 24)
                EncodeSimpleType(majorType, (byte)value);
            else if (value <= byte.MaxValue)
                Encode(majorType, new byte[] { (byte)value });
            else if (value <= ushort.MaxValue)
                Encode(majorType, BitConverter.GetBytes((ushort)value));
            else if (value <= uint.MaxValue)
                Encode(majorType, BitConverter.GetBytes((uint)value));
            else
                Encode(majorType, BitConverter.GetBytes(value));
        }

        private void Encode(CBORMajorType majorType, byte[] bytes)
        { 
            byte ib = (byte)((int)majorType << 5);

            if (bytes.Length == 1)
                _writer.Write((byte)(ib + 24));
            else if (bytes.Length == 2)
                _writer.Write((byte)(ib + 25));
            else if (bytes.Length == 4)
                _writer.Write((byte)(ib + 26));
            else if (bytes.Length == 8)
                _writer.Write((byte)(ib + 27));
            else
                throw new InvalidOperationException("Can not encode non primiative bytes");

            if (ParentState != null) {
                if (ParentState.Length > 0)
                    ParentState.Length--;
                else if(ParentState.Length == 0)
                    throw new IndexOutOfRangeException("Can not write more items to the collection");
            }

            if (BitConverter.IsLittleEndian)
                _writer.Write(bytes.Reverse().ToArray());
            else
                _writer.Write(bytes);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
