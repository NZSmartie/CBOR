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
    public class CBORWriterTests
    {
        [TestMethod]
        public void TestPrimitives()
        {
            var source = new List<Tuple<byte, byte[]>> {
                { (byte)16, new byte[] { 0xf0 } },
                { (byte)24, new byte[] { 0xf8, 0x18 } },
                { (byte)255, new byte[] { 0xf8, 0xff } },
            };

            foreach (var pair in source)
            {
                var output = new MemoryStream();
                using (var writer = new CBORWriter(output))
                {
                    writer.WriteSimple(pair.Item1);
                }

                var actual = output.ToArray();
                Assert.IsTrue(pair.Item2.SequenceEqual(actual));
            }
        }

        public void TestPrimitiveBool()
        {
            var expected = new byte[] { 0xf4 };

            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.Write(false);

            }

            Assert.IsTrue(expected.SequenceEqual(output.ToArray()));

            expected = new byte[] { 0xf5 };

            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.Write(true);

            }

            Assert.IsTrue(expected.SequenceEqual(output.ToArray()));
        }

        [TestMethod]
        public void TestNull()
        {
            var expected = new byte[] { 0xf6 };

            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.WriteNull();
            }

            Assert.IsTrue(expected.SequenceEqual(output.ToArray()));
        }

        [TestMethod]
        public void TestUndefined()
        {
            var expected = new byte[] { 0xf7 };

            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.WriteUndefined();
            }

            Assert.IsTrue(expected.SequenceEqual(output.ToArray()));
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
                var output = new MemoryStream();
                using (var writer = new CBORWriter(output))
                {
                    if(pair.Item1 is ulong)
                        writer.Write((ulong)pair.Item1);
                    else 
                        writer.Write((long)pair.Item1);
                }

                var actual = output.ToArray();
                Assert.IsTrue(pair.Item2.SequenceEqual(actual));
            }
        }

        //[TestMethod]
        //public void TestHalfFloats()
        //{
        //    var source = new List<Tuple<object, byte[]>>
        //    {
        //        { 0.0, new byte[] { 0xf9, 0x00, 0x00 } },
        //        { -0.0, new byte[] { 0xf9, 0x80, 0x00 } },
        //        { 1.0, new byte[] { 0xf9, 0x3c, 0x00 } },
        //        { 1.5, new byte[] { 0xf9, 0x3e, 0x00 } },
        //        { 65504.0, new byte[] { 0xf9, 0x7b, 0xff } },
        //        { 5.960464477539063e-8, new byte[] { 0xf9, 0x00, 0x01 } },
        //        { 0.00006103515625, new byte[] { 0xf9, 0x04, 0x00 } },
        //        { -4.0, new byte[] { 0xf9, 0xc4, 0x00 } },
        //    };
        //    foreach (var pair in source)
        //    {
        //        using (var reader = new CBORReader(new MemoryStream(pair.Item2)))
        //        {
        //            reader.Read();

        //            Assert.AreEqual(pair.Item1, reader.Value);
        //        }
        //    }
        //}

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
                { float.NaN, new byte[] { 0xfa, 0xff, 0xC0, 0x00, 0x00 } },
                { float.NegativeInfinity, new byte[] { 0xfa, 0xFf, 0x80, 0x00, 0x00 } },
                { double.PositiveInfinity, new byte[] { 0xfb, 0x7f, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
                { double.NaN, new byte[] { 0xfb, 0xff, 0xf8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
                { double.NegativeInfinity, new byte[] { 0xfb, 0xFf, 0xf0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            };

            foreach (var pair in source)
            {
                var output = new MemoryStream();
                using (var writer = new CBORWriter(output))
                {
                    writer.Write(pair.Item1);
                }

                var actual = output.ToArray();
                Assert.IsTrue(pair.Item2.SequenceEqual(actual));
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
                var output = new MemoryStream();
                using (var writer = new CBORWriter(output))
                {
                    writer.Write(pair.Item1);
                }

                var actual = output.ToArray();
                Assert.IsTrue(pair.Item2.SequenceEqual(actual));
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
                var output = new MemoryStream();
                using (var writer = new CBORWriter(output))
                {
                    writer.Write(pair.Item1);
                }

                var actual = output.ToArray();
                Assert.IsTrue(pair.Item2.SequenceEqual(actual));
            }
        }

        [TestMethod]
        public void TestArray()
        {
            var expected = new byte[] { 0x83, 0x01, 0x02, 0x03 };
            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Array, 3);
                writer.Write(1);
                writer.Write(2);
                writer.Write(3);
                writer.EndCollection();
            }

            var actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] {
                0x98, 0x19, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
                0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
                0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
                0x17, 0x18, 0x18, 0x18, 0x19 };
            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Array, 25);
                writer.Write(1);
                writer.Write(2);
                writer.Write(3);
                writer.Write(4);
                writer.Write(5);
                writer.Write(6);
                writer.Write(7);
                writer.Write(8);
                writer.Write(9);
                writer.Write(10);
                writer.Write(11);
                writer.Write(12);
                writer.Write(13);
                writer.Write(14);
                writer.Write(15);
                writer.Write(16);
                writer.Write(17);
                writer.Write(18);
                writer.Write(19);
                writer.Write(20);
                writer.Write(21);
                writer.Write(22);
                writer.Write(23);
                writer.Write(24);
                writer.Write(25);
                writer.EndCollection();
            }

            actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TestNestedArray()
        {
            var expected = new byte[] { 0x83, 0x01, 0x82, 0x02, 0x03, 0x82, 0x04, 0x05 };
            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Array, 3);
                writer.Write(1);
                writer.BeginCollection(CBORMajorType.Array, 2);
                writer.Write(2);
                writer.Write(3);
                writer.EndCollection();
                writer.BeginCollection(CBORMajorType.Array, 2);
                writer.Write(4);
                writer.Write(5);
                writer.EndCollection();
                writer.EndCollection();
            }

            var actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TestMap()
        {
            var expected = new byte[] { 0xa0 };
            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Map, 0);
                writer.EndCollection();
            }

            var actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] { 0xa2, 0x01, 0x02, 0x03, 0x04 };
            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Map, 2);
                writer.Write(1);
                writer.Write(2);
                writer.Write(3);
                writer.Write(4);
                writer.EndCollection();
            }

            actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TestArrayMap()
        {
            var expected = new byte[] { 0xa2, 0x61, 0x61, 0x01, 0x61, 0x62, 0x82, 0x02, 0x03 };
            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Map, 2);
                writer.Write("a");
                writer.Write(1);
                writer.Write("b");
                writer.BeginCollection(CBORMajorType.Array, 2);
                writer.Write(2);
                writer.Write(3);
                writer.EndCollection();
                writer.EndCollection();
            }

            var actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] { 0x82, 0x61, 0x61, 0xa1, 0x61, 0x62, 0x61, 0x63 };
            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Array, 2);
                writer.Write("a");
                writer.BeginCollection(CBORMajorType.Map, 1);
                writer.Write("b");
                writer.Write("c");
                writer.EndCollection();
                writer.EndCollection();
            }

            actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void TestIndefinites()
        {
            var expected = new byte[] { 0x5f, 0x42, 0x01, 0x02, 0x43, 0x03, 0x04, 0x05, 0xff };
            var output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.ByteString, -1);
                writer.Write(new byte[] { 0x01, 0x02 });
                writer.Write(new byte[] { 0x03, 0x04, 0x05 });
                writer.EndCollection();
            }

            var actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] { 0x7f, 0x65, 0x73, 0x74, 0x72, 0x65, 0x61, 0x64, 0x6d, 0x69, 0x6e, 0x67, 0xff };
            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.TextString, -1);
                writer.Write("strea");
                writer.Write("ming");
                writer.EndCollection();
            }

            actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] { 0x9f, 0xff };
            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Array, -1);
                writer.EndCollection();
            }

            actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] { 0x9f, 0x01, 0x82, 0x02, 0x03, 0x9f, 0x04, 0x05, 0xff, 0xff };
            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Array, -1);
                writer.Write(1);
                writer.BeginCollection(CBORMajorType.Array, 2);
                writer.Write(2);
                writer.Write(3);
                writer.EndCollection();
                writer.BeginCollection(CBORMajorType.Array, -1);
                writer.Write(4);
                writer.Write(5);
                writer.EndCollection();
                writer.EndCollection();
            }

            actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));

            expected = new byte[] { 0xbf, 0x61, 0x61, 0x01, 0x61, 0x62, 0x9f, 0x02, 0x03, 0xff, 0xff };
            output = new MemoryStream();
            using (var writer = new CBORWriter(output))
            {
                writer.BeginCollection(CBORMajorType.Map, -1);
                writer.Write("a");
                writer.Write(1);
                writer.Write("b");
                writer.BeginCollection(CBORMajorType.Array, -1);
                writer.Write(2);
                writer.Write(3);
                writer.EndCollection();
                writer.EndCollection();
            }

            actual = output.ToArray();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }
    }
}
