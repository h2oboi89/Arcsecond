using Arcsecond;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class Episode5
    {
        [Test]
        public void SeparatedBy_Success()
        {
            var betweenSquareBrackets = Parser<string>.Between(Strings.Parser("["), Strings.Parser("]"));
            var commaSeparated = Parser<string>.SeparatedBy(Strings.Parser(","));

            var parser = betweenSquareBrackets(commaSeparated(Numbers.Digits().Map((result) => int.Parse((string)result))));

            var state = parser.Run("[1,2,3,4,5]");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new int[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void SeparatedByAtLeast_Success()
        {
            var betweenSquareBrackets = Parser<string>.Between(Strings.Parser("["), Strings.Parser("]"));
            var commaSeparated = Parser<string>.SeparatedByAtLeast(1, Strings.Parser(","));

            var parser = betweenSquareBrackets(commaSeparated(Numbers.Digits().Map((result) => int.Parse((string)result))));

            var state = parser.Run("[1]");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new int[] { 1 }));
        }

        [Test]
        public void SeparatedByAtLeast_Failure()
        {
            var betweenSquareBrackets = Parser<string>.Between(Strings.Parser("["), Strings.Parser("]"));
            var commaSeparated = Parser<string>.SeparatedByAtLeast(1, Strings.Parser(","));

            var parser = betweenSquareBrackets(commaSeparated(Numbers.Digits().Map((result) => int.Parse((string)result))));

            var state = parser.Run("[]");

            Assert.That(state.IsError, Is.True);
            Assert.That(state.Error, Is.EqualTo("Unable to match any input using parser at index 1"));
        }

        [Test]
        public void SeparatedBy_SuccessRecursive()
        {
            var betweenSquareBrackets = Parser<string>.Between(Strings.Parser("["), Strings.Parser("]"));
            var commaSeparated = Parser<string>.SeparatedBy(Strings.Parser(","));

            var recursiveparser = Parser<string>.Lazy;
            
            var valueParser = Parser<string>.Choice(new Parser<string>[] {
                Numbers.Digits().Map((result) => int.Parse((string)result)),
                recursiveparser
            });
            
            var arrayParser = betweenSquareBrackets(commaSeparated(valueParser));

            recursiveparser.InitializeLazy(arrayParser);

            var state = arrayParser.Run("[1,[[2],3,4],5]");

            Assert.That(state.IsError, Is.False);
            Assert.That(state.Result, Is.EqualTo(new object[] { 1, new object[] { new object[] { 2 }, 3, 4 }, 5 }));
        }

        private enum Types
        {
            Number,
            Operation
        };

        private class ParsedType
        {
            public readonly Types Type;

            public ParsedType(Types type)
            {
                Type = type;
            }
        }

        private class NumberType : ParsedType
        {
            public readonly float Value;

            public NumberType(object value) : base(Types.Number)
            {
                Value = float.Parse((string)value);
            }

            public override string ToString()
            {
                return $"{{ type: {Type}, value: {Value} }}";
            }
        }

        private class OperationType : ParsedType
        {
            public readonly string Operator;
            public readonly object Left;
            public readonly object Right;

            public OperationType(object results) : base(Types.Operation)
            {
                var r = (List<object>)results;

                Operator = (string)r[0];
                Left = r[2];
                Right = r[4];
            }

            public override string ToString()
            {
                return $"{{ type: {Type}, operator: {Operator}, Left: {Left}, Right: {Right} }}";
            }
        }

        [Test]
        public void MathDemo()
        {
            var betweenParens = Parser<string>.Between(Strings.Parser("("), Strings.Parser(")"));
            var space = Strings.Parser(" ");

            var number = Numbers.Digits().Map(x => new NumberType(x));

            var operatorParser = Parser<string>.Choice(new Parser<string>[]{
                Strings.Parser("+"),
                Strings.Parser("-"),
                Strings.Parser("*"),
                Strings.Parser("/"),
            });

            var expression = Parser<string>.Lazy;

            var operationParser = betweenParens(Parser<string>.SequenceOf(new Parser<string>[]
            {
                operatorParser,
                space,
                expression,
                space,
                expression
            })).Map(results => new OperationType(results));

            expression.InitializeLazy(Parser<string>.Choice(new Parser<string>[]
            {
                number,
                operationParser
            }));

            var input = "(+ (* 10 2) (- (/ 50 2) 3))";

            var state = expression.Run(input);

            float evaluate(object ast)
            {
                var node = ast as ParsedType;

                if (node.Type == Types.Number)
                {
                    return ((NumberType)node).Value;
                }
                else
                {
                    var operation = node as OperationType;

                    var op = operation.Operator;

                    var left = evaluate(operation.Left);
                    var right = evaluate(operation.Right);

                    switch (op)
                    {
                        case "+":
                            return left + right;
                        case "-":
                            return left - right;
                        case "*":
                            return left * right;
                        case "/":
                            return left / right;
                        default:
                            throw new NotImplementedException("Unknown Comamnd");
                    }
                }
            }

            Assert.That(state.IsError, Is.False);
            Assert.That(evaluate(state.Result), Is.EqualTo(42));
        }
    }
}
