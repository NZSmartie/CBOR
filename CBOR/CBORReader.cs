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

    public class CBORReader : IDisposable
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
                        // Todo: figure out how to represent "undefined" in C# 
                        Type = CBORType.Undefined;
                        //throw new NotImplementedException("Undefined simple type is not supported.");
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
                        }
                        else
                            throw new InvalidDataException("Cannot break non indefinite collection");
                    }
                    //else
                        //throw new InvalidDataException();
                    break; 
                case CBORMajorType.ByteString:
                    Type = CBORType.Bytes;
                    PrepareCollection();
                    if (MajorType != CBORMajorType.ByteString)
                        throw new InvalidDataException("Indefinite byte-string has incorrect nested major type");
                    if(State.IsIndefinite)
                        throw new InvalidDataException("Nested indefinite byte-string is not permitted");
                    Value = _reader.ReadBytes(State.Length);
                    break;
                case CBORMajorType.TextString:
                    Type = CBORType.Text;
                    PrepareCollection();
                    if (MajorType != CBORMajorType.TextString)
                        throw new InvalidDataException("Indefinite text-string has incorrect nested major type");
                    if (State.IsIndefinite)
                        throw new InvalidDataException("Nested indefinite text-string is not permitted");
                    Value = System.Text.Encoding.UTF8.GetString(_reader.ReadBytes(State.Length));
                    break;
                case CBORMajorType.Array:
                    PrepareCollection();
                    Type = CBORType.ArrayBegin;
                    Push();
                    break;
                case CBORMajorType.Map:
                    PrepareCollection();
                    State.Length *= 2;
                    Type = CBORType.MapBegin;
                    Push();
                    break;
                default:
                    throw new InvalidOperationException();
            }

            System.Diagnostics.Debug.WriteLine($"Read ended with a type of {Value?.GetType()?.Name ?? "null"}");
        }

        private void PrepareCollection()
        {
            if (State.IsIndefinite)
            {
                //Todo TextStrings and ByteStrings must have the same nested major type.
                Push();
                ReadInitialByte();
            }
            else
            {
                State.Length = Convert.ToInt32((ulong)Value);
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
                case 31: State.IsIndefinite = true; break; // Indefinite array or map
            }
        }

        /// <summary>
        /// Skips the children of the current type 
        /// </summary>
        public void Skip()
        {
            
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
