using Arcsecond;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class Episode4
    {
        [Test]
        public void BetweenSuccess()
        {
            var betweenParens = Parser.Between(Strings.Parser("("), Strings.Parser(")"));

            var parser = betweenParens(Strings.Letters);

            var state = parser.Run("(hello)");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo("hello"));
        }

        [Test]
        public void BetweenFailure()
        {
            var betweenParens = Parser.Between(Strings.Parser("("), Strings.Parser(")"));

            var parser = betweenParens(Strings.Letters);

            var state = parser.Run("(123)");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Result, Is.Null);
            Assert.That(state.Error, Is.EqualTo("Could not match letters at index 1"));
        }

        private enum ParsedTypes
        {
            String,
            Number,
            DiceRoll
        }

        private class StringType
        {
            public readonly ParsedTypes Type = ParsedTypes.String;
            public readonly string Value;

            public StringType(string value)
            {
                Value = value;
            }
        }

        private class NumberType
        {
            public readonly ParsedTypes Type = ParsedTypes.Number;
            public readonly int Value;

            public NumberType(int value)
            {
                Value = value;
            }
        }

        private class DiceRollType
        {
            public readonly ParsedTypes Type = ParsedTypes.DiceRoll;
            public readonly int[] Value;

            public DiceRollType(int number, int sides)
            {
                Value = new int[] { number, sides };
            }
        }

        [Test]
        public void ChainSuccess()
        {
            var stringParser = Strings.Letters.Map((result) => new StringType((string)result));
            var numberParser = Numbers.Digits().Map((result) => new NumberType(int.Parse((string)result)));
            var diceRollParser = Parser.SequenceOf(new Parser[] {
                Numbers.Digits(),
                Strings.Parser("d"),
                Numbers.Digits()
            }).Map((results) =>
            {
                var r = (List<object>)results;
                var number = int.Parse((string)r[0]);
                var sides = int.Parse((string)r[2]);

                return new DiceRollType(number, sides);
            });

            var parser = Parser.SequenceOf(new Parser[] { Strings.Letters, Strings.Parser(":") })
                .Map((results) => ((List<object>)results)[0])
                .Chain((type) =>
                {
                    var t = (string)type;

                    switch (t)
                    {
                        case "string":
                            return stringParser;
                        case "number":
                            return numberParser;
                        default: // "diceroll"
                            return diceRollParser;
                    }
                });

            var state = parser.Run("string:hello");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.InstanceOf<StringType>());
            Assert.That(((StringType)state.Result).Value, Is.EqualTo("hello"));

            state = parser.Run("number:123");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.InstanceOf<NumberType>());
            Assert.That(((NumberType)state.Result).Value, Is.EqualTo(123));

            state = parser.Run("diceroll:2d8");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.InstanceOf<DiceRollType>());
            Assert.That(((DiceRollType)state.Result).Value, Is.EqualTo(new int[] { 2, 8 }));
        }
    }
}
