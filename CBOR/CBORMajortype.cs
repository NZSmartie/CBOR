using System;

namespace CBOR
{
    public enum CBORMajorType : int
    {
        UnsignedInteger = 0,
        NegativeInteger = 1,
        ByteString = 2,
        TextString = 3,
        Array = 4,
        Map = 5,
        Tagged = 6,
        Primitive = 7,
    }
}
