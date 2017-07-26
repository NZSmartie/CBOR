using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CBOR;

namespace CBOR.Tests
{
    [TestClass]
    public class CBORReaderTests
    {
        [TestMethod]
        public void TestPrimitives()
        {
            CBORReader reader;

            var source = new Dictionary<object, byte[]>
            {
                { false, new byte[] { 0xf4 } },
                { true, new byte[] { 0xf5 } },
                //{ null, new byte[] { 0xf6 } },
                //{ undefined, new byte[] { 0xf7 } },
                { 16ul, new byte[] { 0xf0 } },
                { 24ul, new byte[] { 0xf8, 0x18 } },
                { 255ul, new byte[] { 0xf8, 0xff } },
            };
            foreach (var pair in source)
            {
                reader = new CBORReader(new MemoryStream(pair.Value));
                reader.Read();
                
                Assert.AreEqual(pair.Key, reader.Value);
            }

            reader = new CBORReader(new MemoryStream(new byte[] { 0xf6 }));
            reader.Read();

            Assert.IsNull(reader.Value);
        }

        [TestMethod]
        public void TestIntegers()
        {
            var source = new Dictionary<object, byte[]>
            {
                { 0ul, new byte[] { 0x00 } },
                { 1ul, new byte[] { 0x01 } },
                { 10ul, new byte[] { 0x0a } },
                { 23ul, new byte[] { 0x17 } },
                { 24ul, new byte[] { 0x18, 0x18 } },
                { 25ul, new byte[] { 0x18, 0x19 } },
                { 100ul, new byte[] { 0x18, 0x64 } },
                { 1000ul, new byte[] { 0x19, 0x03, 0xe8 } },
                { 1000000ul, new byte[] { 0x1a, 0x00, 0x0f, 0x42, 0x40 } },
                { 1000000000000ul, new byte[] { 0x1b, 0x00, 0x00, 0x00, 0xe8, 0xd4, 0xa5, 0x10, 0x00 } },
                { 18446744073709551615ul, new byte[] { 0x1b, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff } },
                //{ 18446744073709551616, new byte[] { 0xc2, 0x49, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
                //{ -18446744073709551616, new byte[] { 0x3b, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff } },
                //{ -18446744073709551617, new byte[] { 0xc3, 0x49, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
                { -1l, new byte[] { 0x20 } },
                { -10l, new byte[] { 0x29 } },
                { -100l, new byte[] { 0x38, 0x63 } },
                { -1000l, new byte[] { 0x39, 0x03, 0xe7 } },
            };
            foreach (var pair in source) {
                CBORReader reader = new CBORReader(new MemoryStream(pair.Value));
                reader.Read();

                Assert.AreEqual(pair.Key, reader.Value);
            }
        }

        [TestMethod]
        public void TestHalfFloats()
        {
            var source = new List<Tuple<object, byte[]>>
            {
                new Tuple<object, byte[]>( 0.0, new byte[] { 0xf9, 0x00, 0x00 } ),
                new Tuple<object, byte[]>( -0.0, new byte[] { 0xf9, 0x80, 0x00 } ),
                new Tuple<object, byte[]>( 1.0, new byte[] { 0xf9, 0x3c, 0x00 } ),
                new Tuple<object, byte[]>( 1.5, new byte[] { 0xf9, 0x3e, 0x00 } ),
                new Tuple<object, byte[]>( 65504.0, new byte[] { 0xf9, 0x7b, 0xff } ),
                new Tuple<object, byte[]>( 5.960464477539063e-8, new byte[] { 0xf9, 0x00, 0x01 } ),
                new Tuple<object, byte[]>( 0.00006103515625, new byte[] { 0xf9, 0x04, 0x00 } ),
                new Tuple<object, byte[]>(  -4.0, new byte[] { 0xf9, 0xc4, 0x00 } ),
            };
            foreach (var pair in source)
            {
                CBORReader reader = new CBORReader(new MemoryStream(pair.Item2));
                reader.Read();

                Assert.AreEqual(pair.Item1, reader.Value);
            }
        }

        [TestMethod]
        public void TestFloats()
        {
            var source = new List<Tuple<object, byte[]>>
            {
                new Tuple<object, byte[]>( 1.1, new byte[] { 0xfb, 0x3f, 0xf1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9a } ),
                new Tuple<object, byte[]>( 100000.0f, new byte[] { 0xfa, 0x47, 0xc3, 0x50, 0x00 } ),
                new Tuple<object, byte[]>( 3.4028234663852886e+38f, new byte[] { 0xfa, 0x7f, 0x7f, 0xff, 0xff } ),
                new Tuple<object, byte[]>( 1.0e+300d, new byte[] { 0xfb, 0x7e, 0x37, 0xe4, 0x3c, 0x88, 0x00, 0x75, 0x9c } ),
                new Tuple<object, byte[]>( -4.1, new byte[] { 0xfb, 0xc0, 0x10, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66 } ),
                new Tuple<object, byte[]>( float.PositiveInfinity, new byte[] { 0xfa, 0x7f, 0x80, 0x00, 0x00 } ),
                new Tuple<object, byte[]>( float.NaN, new byte[] { 0xfa, 0x7f, 0xC0, 0x00, 0x00 } ),
                new Tuple<object, byte[]>( float.NegativeInfinity, new byte[] { 0xfa, 0xFf, 0x80, 0x00, 0x00 } ),
                new Tuple<object, byte[]>( double.PositiveInfinity, new byte[] { 0xfb, 0x7f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } ),
                new Tuple<object, byte[]>( double.NaN, new byte[] { 0xfb, 0x7f, 0xf8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } ),
                new Tuple<object, byte[]>( double.NegativeInfinity, new byte[] { 0xfb, 0xFf, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } ),
            };
            foreach (var pair in source)
            {
                CBORReader reader = new CBORReader(new MemoryStream(pair.Item2));
                reader.Read();

                Assert.AreEqual(pair.Item1, reader.Value);
            }
        }

        [TestMethod]
        public void TestStrings()
        {
            CBORReader reader;
            var byteStrings = new List<Tuple<byte[], byte[]>>
            {
                new Tuple<byte[], byte[]>( new byte[] { }, new byte[] { 0x40 } ),
                new Tuple<byte[], byte[]>( new byte [] {0x01, 0x02, 0x03, 0x04 } , new byte[] { 0x44, 0x01, 0x02, 0x03, 0x04 } ),
            };
            foreach (var pair in byteStrings)
            {
                reader = new CBORReader(new MemoryStream(pair.Item2));
                reader.Read();

                Assert.IsTrue(pair.Item1.SequenceEqual((byte[])reader.Value));
            }

            var textStrings = new List<Tuple<string, byte[]>>
            {
                new Tuple<string, byte[]>( "", new byte[] { 0x60 } ),
                new Tuple<string, byte[]>( "a", new byte[] { 0x61, 0x61 } ),
                new Tuple<string, byte[]>( "IETF", new byte[] { 0x64, 0x49, 0x45, 0x54, 0x46 } ),
                new Tuple<string, byte[]>( "\"\\", new byte[] { 0x62, 0x22, 0x5c } ),
                new Tuple<string, byte[]>( "\u00fc", new byte[] { 0x62, 0xc3, 0xbc } ),
                new Tuple<string, byte[]>( "\u6c34", new byte[] { 0x63, 0xe6, 0xb0, 0xb4 } ),
                new Tuple<string, byte[]>( "\ud800\udd51", new byte[] { 0x64, 0xf0, 0x90, 0x85, 0x91 } ),
            };
            foreach (var pair in textStrings)
            {
                reader = new CBORReader(new MemoryStream(pair.Item2));
                reader.Read();

                Assert.AreEqual(pair.Item1, reader.Value);
            }
        }

        [TestMethod]
        public void TestArray()
        {
            var expectedValues = new List<Tuple<ulong, CBORType>> {
                new Tuple<ulong, CBORType>(3, CBORType.ArrayBegin),
                new Tuple<ulong, CBORType>(1, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(2, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(3, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(0, CBORType.ArrayEnd)
            };
                
            var reader = new CBORReader(new MemoryStream(new byte[] { 0x83, 0x01, 0x02, 0x03 }));

            foreach (var expected in expectedValues)
            {
                reader.Read();

                Assert.AreEqual(expected.Item1, reader.Value);
                Assert.AreEqual(expected.Item2, reader.Type);
            }

            expectedValues = new List<Tuple<ulong, CBORType>> {
                new Tuple<ulong, CBORType>(25, CBORType.ArrayBegin),
                new Tuple<ulong, CBORType>(1,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(2,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(3,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(4,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(5,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(6,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(7,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(8,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(9,  CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(10, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(11, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(12, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(13, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(14, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(15, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(16, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(17, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(18, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(19, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(20, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(21, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(22, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(23, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(24, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(25, CBORType.PositiveInteger),
                new Tuple<ulong, CBORType>(0,  CBORType.ArrayEnd)
            };

            reader = new CBORReader(new MemoryStream(new byte[] {
                0x98, 0x19, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
                0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                0x17, 0x18, 0x18, 0x18, 0x19 }));

            foreach (var expected in expectedValues)
            {
                reader.Read();

                Assert.AreEqual(expected.Item1, reader.Value);
                Assert.AreEqual(expected.Item2, reader.Type);
            }
        }

    [TestMethod]
    public void TestNestedArray()
    {
        var expectedValues = new List<Tuple<ulong, CBORType>> {
            // [1, [2, 3], [4, 5]]
            new Tuple<ulong, CBORType>(3, CBORType.ArrayBegin),
            new Tuple<ulong, CBORType>(1, CBORType.PositiveInteger),
            new Tuple<ulong, CBORType>(2, CBORType.ArrayBegin),
            new Tuple<ulong, CBORType>(2, CBORType.PositiveInteger),
            new Tuple<ulong, CBORType>(3, CBORType.PositiveInteger),
            new Tuple<ulong, CBORType>(0, CBORType.ArrayEnd),
            new Tuple<ulong, CBORType>(2, CBORType.ArrayBegin),
            new Tuple<ulong, CBORType>(4, CBORType.PositiveInteger),
            new Tuple<ulong, CBORType>(5, CBORType.PositiveInteger),
            new Tuple<ulong, CBORType>(0, CBORType.ArrayEnd),
            new Tuple<ulong, CBORType>(0, CBORType.ArrayEnd),
        };

        var reader = new CBORReader(new MemoryStream(new byte[] { 0x83, 0x01, 0x82, 0x02, 0x03, 0x82, 0x04, 0x05 }));

        foreach (var expected in expectedValues)
        {
            reader.Read();

            Assert.AreEqual(expected.Item1, reader.Value);
            Assert.AreEqual(expected.Item2, reader.Type);
        }
        }
    }
}
