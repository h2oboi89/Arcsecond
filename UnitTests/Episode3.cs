using Arcsecond;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Episode3
    {
        private readonly Letters Letters = Letters.Instance;
        private readonly Digits Digits = Digits.Instance;

        [Test]
        public void Map_TransformsResult()
        {
            var parser = new StringParser("hello").Map((result) => ((string)result).ToUpper());

            var state = parser.Run("hello");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("HELLO"));
        }

        [Test]
        public void ErrorMap_TransformsError()
        {
            var parser = new StringParser("hello").ErrorMap((error, index) => $"Expected a greeting @ {index}");

            var state = parser.Run("goodbye");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Expected a greeting @ 0"));
        }

        [Test]
        public void Letters_Success()
        {
            var parser = Letters;

            var state = parser.Run("abcdefg");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("abcdefg"));
        }

        [Test]
        public void Letters_Failure()
        {
            var parser = Letters;

            var state = parser.Run("123456");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Could not match letters at index 0"));
        }

        [Test]
        public void Digits_Success()
        {
            var parser = Digits;

            var state = parser.Run("123456");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("123456"));
        }

        [Test]
        public void Digits_Failure()
        {
            var parser = Digits;

            var state = parser.Run("abcdefg");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Could not match digits at index 0"));
        }

        [Test]
        public void Choice_Success()
        {
            var parser = new Choice(new Parser[] { Digits, Letters });

            var state = parser.Run("abc123");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("abc"));

            state = parser.Run("123abc");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("123"));
        }

        [Test]
        public void Choice_Failure()
        {
            var parser = new Choice(new Parser[] { Digits, Letters });

            var state = parser.Run("#$%");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Unable to match with any parser at index 0"));
        }

        [Test]
        public void Many_Success()
        {
            var parser = new Many(new Choice(new Parser[] { Digits, Letters }));

            var state = parser.Run("123abc456");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new string[] { "123", "abc", "456" }));
        }

        [Test]
        public void Many_EmptyResult()
        {
            var parser = new Many(new Choice(new Parser[] { Digits, Letters }));

            var state = parser.Run("#$%");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.Empty);
        }

        [Test]
        public void Many_One_Success()
        {
            var parser = new ManyAtLeast(1, new Choice(new Parser[] { Digits, Letters }));

            var state = parser.Run("123abc456");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new string[] { "123", "abc", "456" }));
        }

        [Test]
        public void Many_One_Failure()
        {
            var parser = new ManyAtLeast(1, new Choice(new Parser[] { Digits, Letters }));

            var state = parser.Run("#$%");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Unable to match any input using parser at index 0"));
        }
    }
}
