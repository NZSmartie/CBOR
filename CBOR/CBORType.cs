using System;

namespace CBOR
{
    public sealed class CBORType
    {
        public static readonly CBORType False = new CBORType(20);

        public static readonly CBORType True = new CBORType(21);

        public static readonly CBORType Null = new CBORType(22);

        public static readonly CBORType Undefined = new CBORType(23);

        public static readonly CBORType HalfFloat = new CBORType(25);

        public static readonly CBORType SingleFloat = new CBORType(26);

        public static readonly CBORType DoubleFloat = new CBORType(27);

        public static readonly CBORType Break = new CBORType(31);

        private readonly int _value;

        private CBORType(int value) {
            _value = value;
        }

        public static implicit operator int(CBORType type)
        {
            return type._value;
        }
    }
}
