using Arcsecond;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class Episode5
    {
        [Test]
        public void SeparatedBy_Success()
        {
            var betweenSquareBrackets = Parser.Between(Parser.String("["), Parser.String("]"));
            var commaSeparated = Parser.SeparatedBy(Parser.String(","));

            var parser = betweenSquareBrackets(commaSeparated(Parser.Digits.Map((result) => int.Parse((string)result))));

            var state = parser.Run("[1,2,3,4,5]");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new int[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void SeparatedByAtLeast_Success()
        {
            var betweenSquareBrackets = Parser.Between(Parser.String("["), Parser.String("]"));
            var commaSeparated = Parser.SeparatedByAtLeast(1, Parser.String(","));

            var parser = betweenSquareBrackets(commaSeparated(Parser.Digits.Map((result) => int.Parse((string)result))));

            var state = parser.Run("[1]");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new int[] { 1 }));
        }

        [Test]
        public void SeparatedByAtLeast_Failure()
        {
            var betweenSquareBrackets = Parser.Between(Parser.String("["), Parser.String("]"));
            var commaSeparated = Parser.SeparatedByAtLeast(1, Parser.String(","));

            var parser = betweenSquareBrackets(commaSeparated(Parser.Digits.Map((result) => int.Parse((string)result))));

            var state = parser.Run("[]");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Unable to match any input using parser at index 1"));
        }

        //[Test]
        //public void SeparatedBy_SuccessRecursive()
        //{
        //    var betweenSquareBrackets = Parser.Between(Parser.String("["), Parser.String("]"));
        //    var commaSeparated = Parser.SeparatedBy(Parser.String(","));

        //    var parser = betweenSquareBrackets(commaSeparated(Parser.Digits.Map((result) => int.Parse((string)result))));

        //    var state = parser.Run("[1,[[2],3,4],5]");

        //    Assert.That(state.IsError, Is.False);
        //    Assert.That(state.Result, Is.EqualTo(new object[] { 1, new object[] { new object[] { 2 }, 3, 4 }, 5 }));
        //}
    }
}
