using Arcsecond;
using NUnit.Framework;
using System;
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
            Assert.That(state.Error.Message, Is.EqualTo("Expected 'bar', but got 'foo' at index 0"));
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

        private class IpPacketV4Header
        {
            public byte Version;
            public byte HeaderLength;
            public byte DSCP;
            public byte ECN;
            public ushort PacketLength;
            public ushort ID;
            public byte Flags;
            public ushort FragmentOffset;
            public byte TTL;
            public byte Protocol;
            public ushort HeaderChecksum;
            public uint SourceAddress;
            public uint DestinationAddress;
            public byte[] Options;

            public const byte MinimumHeaderSize = 5;
        }

        private Parser<byte[]> CreateParser()
        {
            var byteParser = Binary.U8();
            var ushortParser = Binary.U16();
            var uintParser = Binary.U32();

            return Parser<byte[]>.SequenceOf(new Parser<byte[]>[] {
                    // 0 - 31
                    Binary.Bits(0xf0, increment: false), Binary.Bits(0x0f), // version & IHL
                    Binary.Bits(0xfc, increment: false), Binary.Bits(0x03), // DSCP & ECN
                    ushortParser, // Total Length
                    
                    // 32 - 63
                    ushortParser, // ID
                    Binary.Bits(0xe0, 2, false), Binary.Bits(0x1f, 2), // Flags & Fragment Offset

                    // 64 - 95
                    byteParser, // TTL
                    byteParser, // Protocol
                    ushortParser, // Header Checksum
                    
                    // 96 - 127
                    uintParser, // Source IP
                    uintParser // Destination IP
                })
                .Map((result) =>
                {
                    var parsedParts = (List<object>)result;

                    var header = new IpPacketV4Header
                    {
                        Version = Convert.ToByte(parsedParts[0]),
                        HeaderLength = Convert.ToByte(parsedParts[1]),
                        DSCP = Convert.ToByte(parsedParts[2]),
                        ECN = Convert.ToByte(parsedParts[3]),
                        PacketLength = Convert.ToUInt16(parsedParts[4]),

                        ID = Convert.ToUInt16(parsedParts[5]),
                        Flags = Convert.ToByte(parsedParts[6]),
                        FragmentOffset = Convert.ToUInt16(parsedParts[7]),

                        TTL = Convert.ToByte(parsedParts[8]),
                        Protocol = Convert.ToByte(parsedParts[9]),
                        HeaderChecksum = Convert.ToUInt16(parsedParts[10]),

                        SourceAddress = Convert.ToUInt32(parsedParts[11]),
                        DestinationAddress = Convert.ToUInt32(parsedParts[12])
                    };

                    return header;
                })
                .Chain((result) =>
                {
                    var header = (IpPacketV4Header)result;

                    var bytesRemaining = (header.HeaderLength - IpPacketV4Header.MinimumHeaderSize) * 4;

                    if (bytesRemaining > 0)
                    {
                        return Binary.Bytes(bytesRemaining).Map((remaining) =>
                        {
                            // 128+
                            header.Options = (byte[])remaining;

                            return header;
                        });
                    }
                    else
                    {
                        header.Options = new byte[0];

                        return Parser<byte[]>.Succeed(header);
                    }
                });
        }

        [Test]
        public void IpPacketHeaderWithOptions()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "packetWithOptions.bin");

            var input = File.ReadAllBytes(testFile);

            var parser = CreateParser();

            var state = parser.Run(input);

            Assert.That(state.IsError, Is.False);

            var packetHeader = (IpPacketV4Header)state.Result;

            // 0 - 31
            Assert.That(packetHeader.Version, Is.EqualTo(4));
            Assert.That(packetHeader.HeaderLength, Is.EqualTo(8));

            Assert.That(packetHeader.DSCP, Is.Zero);
            Assert.That(packetHeader.ECN, Is.Zero);

            Assert.That(packetHeader.PacketLength, Is.EqualTo(68));

            // 32 - 63
            Assert.That(packetHeader.ID, Is.EqualTo(0xAD0B));
            Assert.That(packetHeader.Flags, Is.Zero);
            Assert.That(packetHeader.FragmentOffset, Is.Zero);

            // 64 - 95
            Assert.That(packetHeader.TTL, Is.EqualTo(64));
            Assert.That(packetHeader.Protocol, Is.EqualTo(0x11));
            Assert.That(packetHeader.HeaderChecksum, Is.EqualTo(0x7272));

            // 96 - 127
            Assert.That(packetHeader.SourceAddress, Is.EqualTo(0xac1402fd)); // 172.20.2.253
            Assert.That(packetHeader.DestinationAddress, Is.EqualTo(0xac140006)); // 172.20.0.6

            // 128 - 225
            Assert.That(packetHeader.Options, Is.EqualTo(new byte[]{
                0x00, 0x01, 0x02, 0x03,
                0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0a, 0x0b}));
        }

        [Test]
        public void IpPacketHeaderWithOutOptions()
        {
            var testFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "packet.bin");

            var input = File.ReadAllBytes(testFile);

            var parser = CreateParser();

            var state = parser.Run(input);

            Assert.That(state.IsError, Is.False);

            var packetHeader = (IpPacketV4Header)state.Result;

            // 0 - 31
            Assert.That(packetHeader.Version, Is.EqualTo(4));
            Assert.That(packetHeader.HeaderLength, Is.EqualTo(5));

            Assert.That(packetHeader.DSCP, Is.Zero);
            Assert.That(packetHeader.ECN, Is.Zero);

            Assert.That(packetHeader.PacketLength, Is.EqualTo(68));

            // 32 - 63
            Assert.That(packetHeader.ID, Is.EqualTo(0xAD0B));
            Assert.That(packetHeader.Flags, Is.Zero);
            Assert.That(packetHeader.FragmentOffset, Is.Zero);

            // 64 - 95
            Assert.That(packetHeader.TTL, Is.EqualTo(64));
            Assert.That(packetHeader.Protocol, Is.EqualTo(0x11));
            Assert.That(packetHeader.HeaderChecksum, Is.EqualTo(0x7272));

            // 96 - 127
            Assert.That(packetHeader.SourceAddress, Is.EqualTo(0xac1402fd)); // 172.20.2.253
            Assert.That(packetHeader.DestinationAddress, Is.EqualTo(0xac140006)); // 172.20.0.6

            // 128 - 225
            Assert.That(packetHeader.Options, Is.Empty);
        }
    }
}
