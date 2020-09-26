using Arcsecond;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class Episode6
    {
        [Test]
        public void Bit_ParseByte()
        {
            var bitParser = Binary.Bit;
            var byteParser = Parser<List<byte>>.SequenceOf(new Parser<List<byte>>[] {
                bitParser, bitParser, bitParser, bitParser,
                bitParser, bitParser, bitParser, bitParser
            });

            var input = new List<byte> { 0xaa };

            var state = byteParser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new byte[] { 1, 0, 1, 0, 1, 0, 1, 0 }));
        }

        [Test]
        public void OneAndZero_Success()
        {
            var byteParser = Parser<List<byte>>.SequenceOf(new Parser<List<byte>>[] {
                Binary.One, Binary.Zero, Binary.One, Binary.Zero,
                Binary.One, Binary.Zero, Binary.One, Binary.Zero
            });

            var input = new List<byte> { 0xaa };

            var state = byteParser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new byte[] { 1, 0, 1, 0, 1, 0, 1, 0 }));
        }
    }
}
