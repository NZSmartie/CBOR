using System;

namespace CBOR
{
    public enum CBORSimpleType
    {
        Unknown = 0,
        False = 20,
        True = 21,
        Null = 22,
        Undefined = 23,
        HalfFloat = 25,
        SingleFloat = 26,
        DoubleFloat = 27,
        Break = 31,
    }
}
