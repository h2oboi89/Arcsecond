// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using System.Collections;
using System.Collections.Generic;
using Arcsecond;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Episode1
    {
        [Test]
        public void SequenceOf_SuccessfulParse()
        {
            var parser = new SequenceOf(
                new Parser[] {
                    new StringParser("Hello, World!"),
                    new StringParser("Goodbye, World!")
                });

            var state = parser.Run("Hello, World!Goodbye, World!");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new string[] { "Hello, World!", "Goodbye, World!" }));
        }

        [Test]
        public void StringParser_SuccessfulParse()
        {
            var parser = new StringParser("Hello, World!");

            var state = parser.Run("Hello, World!");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("Hello, World!"));
        }

        [Test]
        public void StringParser_UnsuccessfulParse()
        {
            var parser = new StringParser("Hello, World!");

            var state = parser.Run("Goodbye, World!");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got 'Goodbye, Worl'"));
        }

        [Test]
        public void StringParser_InsufficientInput()
        {
            var parser = new StringParser("Hello, World!");

            var state = parser.Run("");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got unexpected end of input"));
        }
    }
}
