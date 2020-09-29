using Arcsecond;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{
    [TestFixture]
    public class Episode7
    {
        [Test]
        public void UInt_Success()
        {
            var parser = Binary.U32();

            var input = new byte[] { 0x11, 0x22, 0x33, 0x44 };

            var state = parser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That((uint)state.Result, Is.EqualTo(287_454_020));
        }

        [Test]
        public void Int_Success()
        {
            var parser = Binary.I32();

            var input = new byte[] { 0xFF, 0xEE, 0xDD, 0xCC };

            var state = parser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That((int)state.Result, Is.EqualTo(-1_122_868));
        }

        [Test]
        public void String_Any()
        {
            var input = new byte[] {
                0x74, 0x68, 0x69, 0x73, 0x20, 0x69, 0x73, 0x20,
                0x61, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6e, 0x67
            };

            var parser = Binary.String(input.Length);

            var state = parser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("this is a string"));
        }

        [Test]
        public void String_Exact()
        {
            var input = new byte[] { 0x66, 0x6f, 0x6f };

            var parser = Binary.String("bar");

            var state = parser.Run(input);

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Expected 'bar', but got 'foo' at index 0"));
        }

        private class KeyValuePair
        {
            public readonly string Key;
            public readonly object Value;

            public KeyValuePair(string key, object value)
            {
                Key = key;
                Value = value;
            }
        }

        [Test]
        public void IpPacketHeader()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "packet.bin");

            var input = File.ReadAllBytes(testFile);

            KeyValuePair tag(string type, object value) => new KeyValuePair(type, value);

            var parser = Parser<byte[]>.SequenceOf(
                new Parser<byte[]>[] {
                    // 0 - 31
                    Binary.Bits(0xf0, increment: false).Map((result) => tag("Version", result)),
                    Binary.Bits(0x0f).Map((result) => tag("IHL", result)),

                    Binary.Bits(0xfc, increment: false).Map((result) => tag("DSCP", result)),
                    Binary.Bits(0x03).Map((result) => tag("ECN", result)),

                    Binary.U16().Map((result) => tag("Total Length", result)),
                    
                    // 32 - 63
                    Binary.U16().Map((result) => tag("ID", result)),
                    
                    Binary.Bits(0xe0, 2, false).Map((result) => tag("Flags", result)),
                    Binary.Bits(0x1f, 2).Map((result) => tag("Fragment Offset", result)),

                    // 64 - 95
                    Binary.U8.Map((result) => tag("TTL", result)),
                    Binary.U8.Map((result) => tag("Protocol", result)),
                    
                    Binary.U16().Map((result) => tag("Header Checksum", result)),
                    
                    // 96 - 127
                    Binary.U32().Map((result) => tag("Source Address", result)),
                    Binary.U32().Map((result) => tag("Destination Address", result))
                }).Map((result) =>
                {
                    var output = new Dictionary<string, object>();

                    foreach (object o in (IEnumerable<object>)result)
                    {
                        var kvp = (KeyValuePair)o;

                        output.Add(kvp.Key, kvp.Value);
                    }

                    return output;
                });

            // TODO: check IHL > 5 and parse options as remaining bytes
            // TODO: creat IPV4 header class to hold parsed output

            var state = parser.Run(input);

            Assert.That(state.IsError, Is.False);

            var r = (Dictionary<string, object>)state.Result;

            // 0 - 31
            Assert.That(r["Version"], Is.EqualTo(4));
            Assert.That(r["IHL"], Is.EqualTo(5));

            Assert.That(r["DSCP"], Is.Zero);
            Assert.That(r["ECN"], Is.Zero);

            Assert.That(r["Total Length"], Is.EqualTo(68));

            // 32 - 63
            Assert.That(r["ID"], Is.EqualTo(0xAD0B));
            Assert.That(r["Flags"], Is.Zero);
            Assert.That(r["Fragment Offset"], Is.Zero);

            // 64 - 95
            Assert.That(r["TTL"], Is.EqualTo(64));
            Assert.That(r["Protocol"], Is.EqualTo(0x11));
            Assert.That(r["Header Checksum"], Is.EqualTo(0x7272));

            // 96 - 127
            Assert.That(r["Source Address"], Is.EqualTo(0xac1402fd)); // 172.20.2.253
            Assert.That(r["Destination Address"], Is.EqualTo(0xac140006)); // 172.20.0.6
        }
    }
}
