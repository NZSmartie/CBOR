using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CBOR
{
    public partial class CBORReader
    {
        public CultureInfo Culture = CultureInfo.InvariantCulture;

        public bool ReadBool()
        {
            Read();
            switch (Type)
            {
                case CBORType.Bool:
                    return (bool)Value;
                case CBORType.PositiveInteger:
                    return Convert.ToBoolean((ulong)Value);
                case CBORType.NegativeInteger:
                    return Convert.ToBoolean((long)Value);
                case CBORType.HalfFloat:
                    throw new NotImplementedException("IEEE 754 Half-Precision Floats is not supported.");
                case CBORType.SingleFloat:
                    return Convert.ToBoolean((float)Value);
                case CBORType.DoubleFloat:
                    return Convert.ToBoolean((double)Value);
                case CBORType.Text:
                    return Convert.ToBoolean((string)Value, Culture);
                case CBORType.TextBegin:
                    Read();
                    return Convert.ToBoolean((string)Value, Culture);
            }

            throw new InvalidOperationException();
        }

        public Int16 ReadInt16()
        {
            Read();
            switch (Type)
            {
                case CBORType.PositiveInteger:
                    return Convert.ToInt16((ulong)Value);
                case CBORType.NegativeInteger:
                    return Convert.ToInt16((long)Value);
                case CBORType.HalfFloat:
                    throw new NotImplementedException("IEEE 754 Half-Precision Floats is not supported.");
                case CBORType.SingleFloat:
                    return Convert.ToInt16((float)Value);
                case CBORType.DoubleFloat:
                    return Convert.ToInt16((double)Value);
            }

            throw new InvalidOperationException();
        }

        public Int32 ReadInt32()
        {
            Read();
            switch (Type)
            {
                case CBORType.PositiveInteger:
                    return Convert.ToInt32((ulong)Value);
                case CBORType.NegativeInteger:
                    return Convert.ToInt32((long)Value);
                case CBORType.HalfFloat:
                    throw new NotImplementedException("IEEE 754 Half-Precision Floats is not supported.");
                case CBORType.SingleFloat:
                    return Convert.ToInt32((float)Value);
                case CBORType.DoubleFloat:
                    return Convert.ToInt32((double)Value);
            }

            throw new InvalidOperationException();
        }

        public Int64 ReadInt64()
        {
            Read();
            switch (Type)
            {
                case CBORType.PositiveInteger:
                    return Convert.ToInt64((ulong)Value);
                case CBORType.NegativeInteger:
                    return Convert.ToInt64((long)Value);
                case CBORType.HalfFloat:
                    throw new NotImplementedException("IEEE 754 Half-Precision Floats is not supported.");
                case CBORType.SingleFloat:
                    return Convert.ToInt64((float)Value);
                case CBORType.DoubleFloat:
                    return Convert.ToInt64((double)Value);
            }

            throw new InvalidOperationException();
        }

        public string ReadString()
        {
            Read();
            switch (Type)
            {
                case CBORType.Bool:
                    return Convert.ToString((bool)Value, Culture);
                case CBORType.PositiveInteger:
                    return Convert.ToString((ulong)Value, Culture);
                case CBORType.NegativeInteger:
                    return Convert.ToString((long)Value, Culture);
                case CBORType.HalfFloat:
                    throw new NotImplementedException("IEEE 754 Half-Precision Floats is not supported.");
                case CBORType.SingleFloat:
                    return Convert.ToString((float)Value, Culture);
                case CBORType.DoubleFloat:
                    return Convert.ToString((double)Value, Culture);
                case CBORType.Text:
                    return (string)Value;
                case CBORType.TextBegin:
                    Read();
                    return (string)Value;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Skips the children of the current type 
        /// </summary>
        public void Skip()
        {
            throw new NotImplementedException();
        }
    }
}
