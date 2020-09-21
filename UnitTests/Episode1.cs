using Arcsecond;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Episode1
    {
        [Test]
        public void StringParser_SuccessfulParse()
        {
            var parser = new StringParser("Hello, World!");

            var state = parser.Run("Hello, World!");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("Hello, World!"));
            Assert.That(state.Index, Is.EqualTo(13));
        }

        [Test]
        public void StringParser_UnsuccessfulParse()
        {
            var parser = new StringParser("Hello, World!");

            var state = parser.Run("Goodbye, World!");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got 'Goodbye, Worl'"));
            Assert.That(state.Index, Is.EqualTo(0));
        }

        [Test]
        public void StringParser_InsufficientInput()
        {
            var parser = new StringParser("Hello, World!");

            var state = parser.Run("");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got unexpected end of input"));
            Assert.That(state.Index, Is.EqualTo(0));
        }

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
            Assert.That(state.Index, Is.EqualTo(28));
        }

        [Test]
        public void SequenceOf_NoSuccess()
        {
            var parser = new SequenceOf(
                new Parser[] {
                    new StringParser("Hello, World!"),
                    new StringParser("Goodbye, World!")
                });

            var state = parser.Run("Goodbye, World!");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got 'Goodbye, Worl'"));
            Assert.That(state.Index, Is.EqualTo(0));
        }

        [Test]
        public void SequenceOf_PartialSuccess()
        {
            var parser = new SequenceOf(
                new Parser[] {
                    new StringParser("Hello, World!"),
                    new StringParser("Goodbye, World!")
                });

            var state = parser.Run("Hello, World!");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Goodbye, World!', but got unexpected end of input"));
            Assert.That(state.Index, Is.EqualTo(0));
        }
    }
}
