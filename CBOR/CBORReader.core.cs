using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CBOR.Extensions;

namespace CBOR
{
    internal class CBORReaderState
    {
        private CBORMajorType _majorType;
        internal CBORMajorType MajorType
        {
            get { return _majorType; }
            set
            {
                _majorType = value;
                IsCollection = CBORReader.Collectiontypes.Contains(value);
            }
        }

        internal int SimpleType;

        internal int Tag;

        internal bool IsCollection { get; private set; }

        internal bool IsIndefinite { get => Length < 0; set { if (value) Length = -1; } }

        internal int Length = 0;
    }

    public partial class CBORReader : IDisposable
    {
        internal static readonly CBORMajorType[] Collectiontypes = new CBORMajorType[] { CBORMajorType.ByteString, CBORMajorType.TextString, CBORMajorType.Array, CBORMajorType.Map };

#region Proxy memboers to the current State

        protected CBORMajorType MajorType { get => State.MajorType; set => State.MajorType = value; }

        protected int SimpleType { get => State.SimpleType; set => State.SimpleType = value; } 

        protected bool IsCollection => State.IsCollection;

        public int Tag { get => State.Tag; protected set => State.Tag = value; }

#endregion

        public CBORType Type { get; protected set; }

        public Object Value { get; protected set; }

        private readonly BinaryReader _reader;

        private readonly Stack<CBORReaderState> _state = new Stack<CBORReaderState>();

        private CBORReaderState ParentState => _state.Peek();

        private CBORReaderState State = new CBORReaderState();

        public CBORReader(Stream stream)
        {
            _reader = new BinaryReader(stream);

            // A null placeholder on the stack marks when the stack is empty. 
            _state.Push(null);
        }

        public void Read()
        {
            if (ParentState != null)
            {
                if (ParentState.Length > 0)
                    ParentState.Length--;
                else if (ParentState.Length == 0)
                {
                    Pop();
                    Value = 0ul;

                    if (State.MajorType == CBORMajorType.Array)
                        Type = CBORType.ArrayEnd;
                    else if (State.MajorType == CBORMajorType.Map)
                        Type = CBORType.MapEnd;
                    return;
                }
            }

            ReadInitialByte();

            if (ParentState != null 
                && (ParentState.MajorType == CBORMajorType.ByteString || ParentState.MajorType == CBORMajorType.TextString) 
                && ParentState.MajorType != MajorType && !(MajorType == CBORMajorType.Primitive && SimpleType == (int) CBORSimpleType.Break))
                throw new InvalidDataException("Indefinite byte/text string has incorrect nested major type");

            if (MajorType == CBORMajorType.Tagged)
            {
                State.Tag = (int)Value;
                // Todo: Support Optional Tags
                ReadInitialByte();
            }

            switch (MajorType)
            {
                case CBORMajorType.UnsignedInteger:
                    Type = CBORType.PositiveInteger;
                    // No need to do anyhting
                    break;
                case CBORMajorType.NegativeInteger:
                    Type = CBORType.NegativeInteger;
                    Value = -1 - Convert.ToInt64((ulong)Value);
                    break;
                case CBORMajorType.Primitive:
                    if (SimpleType == (int)CBORSimpleType.False)
                    {
                        Value = false;
                        Type = CBORType.Bool;
                    }
                    else if (SimpleType == (int)CBORSimpleType.True)
                    {
                        Value = true;
                        Type = CBORType.Bool;
                    }
                    else if (SimpleType == (int)CBORSimpleType.Null)
                    {
                        Value = null;
                        Type = CBORType.Null;
                    }
                    else if (SimpleType == (int)CBORSimpleType.Undefined)
                    {
                        Value = null;
                        Type = CBORType.Undefined;
                    }
                    else if (SimpleType == (int)CBORSimpleType.HalfFloat)
                    {
                        Type = CBORType.HalfFloat;
                        // Todo: Add support for IEEE 754 Half-Precision Floats
                        throw new NotImplementedException("IEEE 754 Half-Precision Floats is not supported.");
                    }
                    if (SimpleType == (int)CBORSimpleType.SingleFloat)
                    {
                        Value = BitConverter.ToSingle(BitConverter.GetBytes((ulong)Value), 0);
                        Type = CBORType.SingleFloat;
                    }
                    else if (SimpleType == (int)CBORSimpleType.DoubleFloat)
                    {
                        Value = BitConverter.ToDouble(BitConverter.GetBytes((ulong)Value), 0);
                        Type = CBORType.DoubleFloat;
                    }
                    else if (SimpleType == (int)CBORSimpleType.Break)
                    {
                        if (ParentState?.IsIndefinite ?? false)
                        {
                            Pop();
                            if(State.MajorType == CBORMajorType.Array)
                                Type = CBORType.ArrayEnd;
                            else if(State.MajorType == CBORMajorType.Map)
                                Type = CBORType.MapEnd;
                            else if (State.MajorType == CBORMajorType.ByteString)
                                Type = CBORType.BytesEnd;
                            else if (State.MajorType == CBORMajorType.TextString)
                                Type = CBORType.TextEnd;
                            Value = 0ul;
                        }
                        else
                            throw new InvalidDataException("Cannot break non indefinite collection");
                    }
                    //else
                        //throw new InvalidDataException();
                    break;
                case CBORMajorType.ByteString:
                    //Todo TextStrings and ByteStrings must have the same nested major type.
                    if (State.IsIndefinite)
                    {
                        if (ParentState?.MajorType == CBORMajorType.ByteString)
                            throw new InvalidDataException("Nested indefinite byte-string is not permitted");
                        Type = CBORType.BytesBegin;
                        Push();
                        break;
                    }
                    Type = CBORType.Bytes;
                    State.Length = Convert.ToInt32((ulong)Value);
                    Value = _reader.ReadBytes(State.Length);
                    break;
                case CBORMajorType.TextString:
                    if (State.IsIndefinite)
                    {
                        if (ParentState?.MajorType == CBORMajorType.TextString)
                            throw new InvalidDataException("Nested indefinite text-string is not permitted");
                        Type = CBORType.TextBegin;
                        Push();
                        break;
                    }
                    Type = CBORType.Text;
                    State.Length = Convert.ToInt32((ulong)Value);
                    Value = System.Text.Encoding.UTF8.GetString(_reader.ReadBytes(State.Length));
                    break;
                case CBORMajorType.Array:
                    Type = CBORType.ArrayBegin;
                    if(!State.IsIndefinite)
                        State.Length = Convert.ToInt32((ulong)Value);
                    Push();
                    break;
                case CBORMajorType.Map:
                    if (!State.IsIndefinite)
                        State.Length = Convert.ToInt32((ulong)Value)*2; // Maps always come in Key -> Value pairs. thus, items to read are doubled.
                    Type = CBORType.MapBegin;
                    Push();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void Push(CBORReaderState state = null)
        {
            _state.Push(State);
            State = state ?? new CBORReaderState();
        }

        private void Pop()
        {
            State = _state.Pop();
        }

        private void ReadInitialByte()
        {
            var ib = _reader.ReadByte();
            MajorType = (CBORMajorType)(ib >> 5);
            Value = (ulong)(SimpleType = ib & 0x1F);

            switch (SimpleType)
            {
                case 24: Value = (ulong)_reader.ReadByte(); break; // Simple value
                case 25: Value = (ulong)_reader.ReadUInt16BE(); break; // IEEE 754 Half-Precision Float
                case 26: Value = (ulong)_reader.ReadUInt32BE(); break; // IEEE 754 Single-Precision Float
                case 27: Value = (ulong)_reader.ReadUInt64BE(); break; // IEEE 754 Double-Precision Float
                case 28: case 29: case 30: throw new InvalidDataException(); // unassigned
                case 31: State.IsIndefinite = true; Value = -1l; break; // Indefinite array or map
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
