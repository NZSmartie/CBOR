using System;
using System.Collections.Generic;
using System.Text;

namespace CBOR
{
    public enum CBORType
    {
        Unknown,
        PositiveInteger,
        NegativeInteger,
        Bool,
        Null,
        Undefined,
        HalfFloat,
        SingleFloat,
        DoubleFloat,
        Bytes,
        BytesBegin,
        BytesEnd,
        Text,
        TextBegin,
        TextEnd,
        ArrayBegin,
        ArrayEnd,
        MapBegin,
        MapEnd,
    }
}
