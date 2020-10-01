using Arcsecond;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ParsingExceptionTests
    {
        [Test]
        public void SimpleException()
        {
            var exception = new ParsingException("Illegal character 'a'", 15);

            Assert.That(exception.Message, Is.EqualTo("Illegal character 'a' at index 15"));
        }
    }
}
