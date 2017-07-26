using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CBOR;
using CBOR.Tests.Extensions;

namespace CBOR.Tests
{
    [TestClass]
    public class CBORReaderTests
    {
        [TestMethod]
        public void TestPrimitives()
        {
            var source = new List<Tuple<object, byte[]>>
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
                using (var reader = new CBORReader(new MemoryStream(pair.Item2)))
                {
                    reader.Read();

                    Assert.AreEqual(pair.Item1, reader.Value);
                }
            }

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0xf6 })))
            {
                reader.Read();

                Assert.IsNull(reader.Value);
            }
        }

        [TestMethod]
        public void TestIntegers()
        {
            var source = new List<Tuple<object, byte[]>>
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
                using (var reader = new CBORReader(new MemoryStream(pair.Item2)))
                {
                    reader.Read();

                    Assert.AreEqual(pair.Item1, reader.Value);
                }
            }
        }

        [TestMethod]
        public void TestHalfFloats()
        {
            var source = new List<Tuple<object, byte[]>>
            {
                { 0.0, new byte[] { 0xf9, 0x00, 0x00 } },
                { -0.0, new byte[] { 0xf9, 0x80, 0x00 } },
                { 1.0, new byte[] { 0xf9, 0x3c, 0x00 } },
                { 1.5, new byte[] { 0xf9, 0x3e, 0x00 } },
                { 65504.0, new byte[] { 0xf9, 0x7b, 0xff } },
                { 5.960464477539063e-8, new byte[] { 0xf9, 0x00, 0x01 } },
                { 0.00006103515625, new byte[] { 0xf9, 0x04, 0x00 } },
                { -4.0, new byte[] { 0xf9, 0xc4, 0x00 } },
            };
            foreach (var pair in source)
            {
                using (var reader = new CBORReader(new MemoryStream(pair.Item2)))
                {
                    reader.Read();

                    Assert.AreEqual(pair.Item1, reader.Value);
                }
            }
        }

        [TestMethod]
        public void TestFloats()
        {
            var source = new List<Tuple<object, byte[]>>
            {
                { 1.1, new byte[] { 0xfb, 0x3f, 0xf1, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9a } },
                { 100000.0f, new byte[] { 0xfa, 0x47, 0xc3, 0x50, 0x00 } },
                { 3.4028234663852886e+38f, new byte[] { 0xfa, 0x7f, 0x7f, 0xff, 0xff } },
                { 1.0e+300d, new byte[] { 0xfb, 0x7e, 0x37, 0xe4, 0x3c, 0x88, 0x00, 0x75, 0x9c } },
                { -4.1, new byte[] { 0xfb, 0xc0, 0x10, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66 } },
                { float.PositiveInfinity, new byte[] { 0xfa, 0x7f, 0x80, 0x00, 0x00 } },
                { float.NaN, new byte[] { 0xfa, 0x7f, 0xC0, 0x00, 0x00 } },
                { float.NegativeInfinity, new byte[] { 0xfa, 0xFf, 0x80, 0x00, 0x00 } },
                { double.PositiveInfinity, new byte[] { 0xfb, 0x7f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
                { double.NaN, new byte[] { 0xfb, 0x7f, 0xf8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
                { double.NegativeInfinity, new byte[] { 0xfb, 0xFf, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            };
            foreach (var pair in source)
            {
                using (var reader = new CBORReader(new MemoryStream(pair.Item2)))
                {
                    reader.Read();

                    Assert.AreEqual(pair.Item1, reader.Value);
                }
            }
        }

        [TestMethod]
        public void TestStrings()
        {
            var byteStrings = new List<Tuple<byte[], byte[]>>
            {
                { new byte[] { }, new byte[] { 0x40 } },
                { new byte[] { 0x01, 0x02, 0x03, 0x04 } , new byte[] { 0x44, 0x01, 0x02, 0x03, 0x04 } },
            };
            foreach (var pair in byteStrings)
            {
                using (var reader = new CBORReader(new MemoryStream(pair.Item2)))
                {
                    reader.Read();

                    Assert.IsTrue(pair.Item1.SequenceEqual((byte[])reader.Value));
                }
            }

            var textStrings = new List<Tuple<string, byte[]>>
            {
                { "", new byte[] { 0x60 } },
                { "a", new byte[] { 0x61, 0x61 } },
                { "IETF", new byte[] { 0x64, 0x49, 0x45, 0x54, 0x46 } },
                { "\"\\", new byte[] { 0x62, 0x22, 0x5c } },
                { "\u00fc", new byte[] { 0x62, 0xc3, 0xbc } },
                { "\u6c34", new byte[] { 0x63, 0xe6, 0xb0, 0xb4 } },
                { "\ud800\udd51", new byte[] { 0x64, 0xf0, 0x90, 0x85, 0x91 } },
            };
            foreach (var pair in textStrings)
            {
                using (var reader = new CBORReader(new MemoryStream(pair.Item2)))
                {
                    reader.Read();

                    Assert.AreEqual(pair.Item1, reader.Value);
                }
            }
        }

        [TestMethod]
        public void TestArray()
        {
            var expectedValues = new List<Tuple<ulong, CBORType>> {
                { 3ul, CBORType.ArrayBegin },
                { 1ul, CBORType.PositiveInteger },
                { 2ul, CBORType.PositiveInteger },
                { 3ul, CBORType.PositiveInteger },
                { 0ul, CBORType.ArrayEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0x83, 0x01, 0x02, 0x03 })))
            {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }

            expectedValues = new List<Tuple<ulong, CBORType>> {
                { 25ul, CBORType.ArrayBegin },
                { 1ul,  CBORType.PositiveInteger },
                { 2ul,  CBORType.PositiveInteger },
                { 3ul,  CBORType.PositiveInteger },
                { 4ul,  CBORType.PositiveInteger },
                { 5ul,  CBORType.PositiveInteger },
                { 6ul,  CBORType.PositiveInteger },
                { 7ul,  CBORType.PositiveInteger },
                { 8ul,  CBORType.PositiveInteger },
                { 9ul,  CBORType.PositiveInteger },
                { 10ul, CBORType.PositiveInteger },
                { 11ul, CBORType.PositiveInteger },
                { 12ul, CBORType.PositiveInteger },
                { 13ul, CBORType.PositiveInteger },
                { 14ul, CBORType.PositiveInteger },
                { 15ul, CBORType.PositiveInteger },
                { 16ul, CBORType.PositiveInteger },
                { 17ul, CBORType.PositiveInteger },
                { 18ul, CBORType.PositiveInteger },
                { 19ul, CBORType.PositiveInteger },
                { 20ul, CBORType.PositiveInteger },
                { 21ul, CBORType.PositiveInteger },
                { 22ul, CBORType.PositiveInteger },
                { 23ul, CBORType.PositiveInteger },
                { 24ul, CBORType.PositiveInteger },
                { 25ul, CBORType.PositiveInteger },
                { 0ul,  CBORType.ArrayEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] {
                0x98, 0x19, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
                0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                0x17, 0x18, 0x18, 0x18, 0x19 })))
            {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }
        }

        [TestMethod]
        public void TestNestedArray()
        {
            var expectedValues = new List<Tuple<ulong, CBORType>> {
                { 3ul, CBORType.ArrayBegin },
                { 1ul, CBORType.PositiveInteger },
                { 2ul, CBORType.ArrayBegin },
                { 2ul, CBORType.PositiveInteger },
                { 3ul, CBORType.PositiveInteger },
                { 0ul, CBORType.ArrayEnd },
                { 2ul, CBORType.ArrayBegin },
                { 4ul, CBORType.PositiveInteger },
                { 5ul, CBORType.PositiveInteger },
                { 0ul, CBORType.ArrayEnd },
                { 0ul, CBORType.ArrayEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0x83, 0x01, 0x82, 0x02, 0x03, 0x82, 0x04, 0x05 })))
            {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }
        }

        [TestMethod]
        public void TestMap()
        {
            var expectedValues = new List<Tuple<ulong, CBORType>> {
                { 0ul, CBORType.MapBegin },
                { 0ul, CBORType.MapEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0xa0 })))
            {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }

            //{ 1: 2, 3: 4}
            //0xa201020304

            expectedValues = new List<Tuple<ulong, CBORType>> {
                { 2ul, CBORType.MapBegin },
                { 1ul, CBORType.PositiveInteger },
                { 2ul, CBORType.PositiveInteger },
                { 3ul, CBORType.PositiveInteger },
                { 4ul, CBORType.PositiveInteger },
                { 0ul, CBORType.MapEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0xa2, 0x01, 0x02, 0x03, 0x04 })))
            {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }
        }

        [TestMethod]
        public void TestArrayMap()
        {
            //{"a": 1, "b": [2, 3]}        
            //0xa26161016162820203 

            var expectedValues = new List<Tuple<object, CBORType>> {
                { 2ul, CBORType.MapBegin },
                { "a", CBORType.Text },
                { 1ul, CBORType.PositiveInteger },
                { "b", CBORType.Text },
                { 2ul, CBORType.ArrayBegin },
                { 2ul, CBORType.PositiveInteger },
                { 3ul, CBORType.PositiveInteger },
                { 0ul, CBORType.ArrayEnd },
                { 0ul, CBORType.MapEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0xa2, 0x61, 0x61, 0x01, 0x61, 0x62, 0x82, 0x02, 0x03 })))
            {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }

            //["a", {"b": "c"}]        
            //0x826161a161626163

            expectedValues = new List<Tuple<object, CBORType>> {
                { 2ul, CBORType.ArrayBegin },
                { "a", CBORType.Text },
                { 1ul, CBORType.MapBegin },
                { "b", CBORType.Text },
                { "c", CBORType.Text },
                { 0ul, CBORType.MapEnd },
                { 0ul, CBORType.ArrayEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0x82, 0x61, 0x61, 0xa1, 0x61, 0x62, 0x61, 0x63 })))
            {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }
        }

        [TestMethod]
        public void TestIndefinites()
        {
            var expectedValues = new List<Tuple<object, CBORType>> {
                { -1l, CBORType.BytesBegin },
                { new byte[]{ 0x01, 0x02 }, CBORType.Bytes },
                { new byte[]{ 0x03, 0x04, 0x05 }, CBORType.Bytes },
                { 0ul, CBORType.BytesEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0x5f, 0x42, 0x01, 0x02, 0x43, 0x03, 0x04, 0x05, 0xff }))) {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    if (expected.Item1.GetType() == typeof(byte[]))
                    {
                        Assert.IsTrue(((byte[])expected.Item1).SequenceEqual((byte[])reader.Value));
                    }
                    else
                    {
                        Assert.AreEqual(expected.Item1, reader.Value);
                    }
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }

            expectedValues = new List<Tuple<object, CBORType>> {
                { -1l, CBORType.TextBegin },
                { "strea", CBORType.Text },
                { "ming", CBORType.Text},
                { 0ul, CBORType.TextEnd },
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0x7f, 0x65, 0x73, 0x74, 0x72, 0x65, 0x61, 0x64, 0x6d, 0x69, 0x6e, 0x67, 0xff }))) {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }

            expectedValues = new List<Tuple<object, CBORType>> {
                { -1l, CBORType.ArrayBegin },
                { 0ul, CBORType.ArrayEnd},
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0x9f, 0xff }))) {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }

            expectedValues = new List<Tuple<object, CBORType>> {
                { -1l, CBORType.ArrayBegin },
                { 1ul, CBORType.PositiveInteger },
                { 2ul, CBORType.ArrayBegin },
                { 2ul, CBORType.PositiveInteger },
                { 3ul, CBORType.PositiveInteger },
                { 0ul, CBORType.ArrayEnd},
                { -1l, CBORType.ArrayBegin },
                { 4ul, CBORType.PositiveInteger },
                { 5ul, CBORType.PositiveInteger },
                { 0ul, CBORType.ArrayEnd},
                { 0ul, CBORType.ArrayEnd},
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0x9f, 0x01, 0x82, 0x02, 0x03, 0x9f, 0x04, 0x05, 0xff, 0xff }))) {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }

            expectedValues = new List<Tuple<object, CBORType>> {
                { -1l, CBORType.MapBegin },
                { "a", CBORType.Text },
                { 1ul, CBORType.PositiveInteger },
                { "b", CBORType.Text },
                { -1l, CBORType.ArrayBegin },
                { 2ul, CBORType.PositiveInteger },
                { 3ul, CBORType.PositiveInteger },
                { 0ul, CBORType.ArrayEnd},
                { 0ul, CBORType.MapEnd},
            };

            using (var reader = new CBORReader(new MemoryStream(new byte[] { 0xbf, 0x61, 0x61, 0x01, 0x61, 0x62, 0x9f, 0x02, 0x03, 0xff, 0xff }))) {

                foreach (var expected in expectedValues)
                {
                    reader.Read();

                    Assert.AreEqual(expected.Item1, reader.Value);
                    Assert.AreEqual(expected.Item2, reader.Type);
                }
            }
        }
    }
}
