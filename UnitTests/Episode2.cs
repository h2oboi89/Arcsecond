using Arcsecond;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Episode2
    {
        [Test]
        public void StringParser_SuccessfulParse()
        {
            var parser = Strings.Parser("Hello, World!");

            var state = parser.Run("Hello, World!");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("Hello, World!"));
            Assert.That(state.Index, Is.EqualTo(13));
        }

        [Test]
        public void StringParser_UnsuccessfulParse()
        {
            var parser = Strings.Parser("Hello, World!");

            var state = parser.Run("Goodbye, World!");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got 'Goodbye, Worl'"));
            Assert.That(state.Index, Is.EqualTo(0));
        }

        [Test]
        public void StringParser_InsufficientInput()
        {
            var parser = Strings.Parser("Hello, World!");

            var state = parser.Run("");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got unexpected end of input"));
            Assert.That(state.Index, Is.EqualTo(0));
        }

        [Test]
        public void SequenceOf_SuccessfulParse()
        {
            var parser = Parser<string>.SequenceOf(
                new Parser<string>[] {
                    Strings.Parser("Hello, World!"),
                    Strings.Parser("Goodbye, World!")
                });

            var state = parser.Run("Hello, World!Goodbye, World!");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new string[] { "Hello, World!", "Goodbye, World!" }));
            Assert.That(state.Index, Is.EqualTo(28));
        }

        [Test]
        public void SequenceOf_NoSuccess()
        {
            var parser = Parser<string>.SequenceOf(
                new Parser<string>[] {
                    Strings.Parser("Hello, World!"),
                    Strings.Parser("Goodbye, World!")
                });

            var state = parser.Run("Goodbye, World!");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Hello, World!', but got 'Goodbye, Worl'"));
            Assert.That(state.Index, Is.EqualTo(0));
        }

        [Test]
        public void SequenceOf_PartialSuccess()
        {
            var parser = Parser<string>.SequenceOf(
                new Parser<string>[] {
                    Strings.Parser("Hello, World!"),
                    Strings.Parser("Goodbye, World!")
                });

            var state = parser.Run("Hello, World!");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Tried to match 'Goodbye, World!', but got unexpected end of input"));
            Assert.That(state.Index, Is.EqualTo(0));
        }
    }
}
