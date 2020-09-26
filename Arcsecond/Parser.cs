using System;
using System.Collections.Generic;

namespace Arcsecond
{

    // TODO: make generic
    public sealed class Parser
    {
        private Func<ParserState, ParserState> Transform;

        public Parser(Func<ParserState, ParserState> transform)
        {
            Transform = transform;
        }

        public static Parser Lazy => new Parser(s => s);

        public void InitializeLazy(Parser parser)
        {
            this.Transform = parser.Transform;
        }

        public ParserState Run(string input)
        {
            var state = ParserState.Initialize(input);

            var result = Transform(state);

            if (result.IsError)
            {
                return ParserState.SetError(state, result.Error);
            }
            else
            {
                return ParserState.SetResult(state, result.Result, result.Index);
            }
        }

        public Parser Chain(Func<object, Parser> transform)
        {
            return new Parser((ParserState state) =>
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var nextParser = transform(nextState.Result);

                return nextParser.Transform(nextState);
            });
        }

        public Parser Map(Func<object, object> transform)
        {
            return new Parser((ParserState state) =>
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var transformedResult = transform(nextState.Result);

                return ParserState.SetResult(nextState, transformedResult);
            });
        }

        public Parser ErrorMap(Func<object, int, object> transform)
        {
            return new Parser((ParserState state) =>
            {
                var nextState = Transform(state);

                if (!nextState.IsError) return nextState;

                var transformedError = transform(nextState.Error, nextState.Index);

                return ParserState.SetError(nextState, transformedError);
            });
        }

        public static Func<Parser, Parser> Between(Parser left, Parser right) => (Parser content) =>
            SequenceOf(new Parser[] { left, content, right })
            .Map((results) => ((List<object>)results)[1]);

        public static Parser Choice(IEnumerable<Parser> parsers) => new Parser((ParserState state) =>
        {
            if (state.IsError) return state;

            foreach (var parser in parsers)
            {
                var nextState = parser.Transform(state);

                if (!nextState.IsError) return nextState;
            }

            return ParserState.SetError(state, $"Unable to match with any parser at index {state.Index}");
        });

        public static Parser ManyAtLeast(int minimum, Parser parser) => new Parser((ParserState state) =>
        {
            if (state.IsError) return state;

            var results = new List<object>();
            var nextState = state;

            while (true)
            {
                var testState = parser.Transform(nextState);

                if (!testState.IsError)
                {
                    results.Add(testState.Result);
                    nextState = testState;
                }
                else
                {
                    break;
                }
            }

            if (results.Count < minimum)
            {
                return ParserState.SetError(nextState, $"Unable to match any input using parser at index {nextState.Index}");
            }

            return ParserState.SetResult(nextState, results);
        });

        public static Parser Many(Parser parser) => ManyAtLeast(0, parser);

        public static Func<Parser, Parser> SeparatedByAtLeast(int minimum, Parser separatorParser) => (Parser valueParser) =>
        {
            return new Parser((ParserState state) =>
            {
                if (state.IsError) return state;

                var results = new List<object>();
                var nextState = state;

                while (true)
                {
                    var testState = valueParser.Transform(nextState);

                    if (testState.IsError) break;

                    results.Add(testState.Result);
                    nextState = testState;

                    testState = separatorParser.Transform(nextState);

                    if (testState.IsError) break;

                    nextState = testState;
                }

                if (results.Count < minimum)
                {
                    return ParserState.SetError(state, $"Unable to match any input using parser at index {state.Index}");
                }

                return ParserState.SetResult(nextState, results);
            });
        };

        public static Func<Parser, Parser> SeparatedBy(Parser separatorParser) => SeparatedByAtLeast(0, separatorParser);

        public static Parser SequenceOf(IEnumerable<Parser> parsers) => new Parser((ParserState state) =>
        {
            if (state.IsError) return state;

            var results = new List<object>();
            var nextState = state;

            foreach (var parser in parsers)
            {
                nextState = parser.Transform(nextState);

                results.Add(nextState.Result);
            }

            if (nextState.IsError) return nextState;

            return ParserState.SetResult(nextState, results);
        });
    }
}
