using Arcsecond;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class Episode6
    {
        [Test]
        public void Bits_LowBits()
        {
            var bitsParser = Binary.Bits(0x0f);

            var input = new List<byte> { 0xab };

            var state = bitsParser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(0x0b));
            Assert.That(state.Index, Is.EqualTo(1));
        }

        [Test]
        public void Bits_HighBits()
        {
            var bitsParser = Binary.Bits(0xf0, false);

            var input = new List<byte> { 0xab };

            var state = bitsParser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(0x0a));
            Assert.That(state.Index, Is.Zero);
        }

        [Test]
        public void Bits_Increment()
        {
            var byteParser = Parser<List<byte>>.SequenceOf(new Parser<List<byte>>[] {
                Binary.Bits(0xf0, false),
                Binary.Bits(0x0f)
            });

            var input = new List<byte> { 0xab };

            var state = byteParser.Run(input);

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new byte[] { 0x0a, 0x0b }));
            Assert.That(state.Index, Is.EqualTo(1));
        }
    }
}
