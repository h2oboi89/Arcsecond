using System;
using System.Collections.Generic;

namespace Arcsecond
{

    // TODO: make generic
    public sealed class Parser<T>
    {
        private Func<ParserState<T>, ParserState<T>> Transform;

        public Parser(Func<ParserState<T>, ParserState<T>> transform)
        {
            Transform = transform;
        }

        public static Parser<T> Lazy => new Parser<T>(s => s);

        public void InitializeLazy(Parser<T> parser)
        {
            Transform = parser.Transform;
        }

        public ParserState<T> Run(T input)
        {
            var state = ParserState<T>.Initialize(input);

            var result = Transform(state);

            if (result.IsError)
            {
                return ParserState<T>.SetError(state, result.Error);
            }
            else
            {
                return ParserState<T>.SetResult(state, result.Result, result.Index);
            }
        }

        public Parser<T> Chain(Func<object, Parser<T>> transform)
        {
            return new Parser<T>((ParserState<T> state) =>
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var nextParser = transform(nextState.Result);

                return nextParser.Transform(nextState);
            });
        }

        public Parser<T> Map(Func<object, object> transform)
        {
            return new Parser<T>((ParserState<T> state) =>
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var transformedResult = transform(nextState.Result);

                return ParserState<T>.SetResult(nextState, transformedResult);
            });
        }

        public Parser<T> ErrorMap(Func<object, int, object> transform)
        {
            return new Parser<T>((ParserState<T> state) =>
            {
                var nextState = Transform(state);

                if (!nextState.IsError) return nextState;

                var transformedError = transform(nextState.Error, nextState.Index);

                return ParserState<T>.SetError(nextState, transformedError);
            });
        }

        public static Func<Parser<T>, Parser<T>> Between(Parser<T> left, Parser<T> right) => (Parser<T> content) =>
            SequenceOf(new Parser<T>[] { left, content, right })
            .Map((results) => ((List<object>)results)[1]);

        public static Parser<T> Choice(IEnumerable<Parser<T>> parsers) => new Parser<T>((ParserState<T> state) =>
        {
            if (state.IsError) return state;

            foreach (var parser in parsers)
            {
                var nextState = parser.Transform(state);

                if (!nextState.IsError) return nextState;
            }

            return ParserState<T>.SetError(state, $"Unable to match with any parser at index {state.Index}");
        });

        public static Parser<T> ManyAtLeast(int minimum, Parser<T> parser) => new Parser<T>((ParserState<T> state) =>
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
                return ParserState<T>.SetError(nextState, $"Unable to match any input using parser at index {nextState.Index}");
            }

            return ParserState<T>.SetResult(nextState, results);
        });

        public static Parser<T> Many(Parser<T> parser) => ManyAtLeast(0, parser);

        public static Func<Parser<T>, Parser<T>> SeparatedByAtLeast(int minimum, Parser<T> separatorParser) => (Parser<T> valueParser) =>
        {
            return new Parser<T>((ParserState<T> state) =>
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
                    return ParserState<T>.SetError(state, $"Unable to match any input using parser at index {state.Index}");
                }

                return ParserState<T>.SetResult(nextState, results);
            });
        };

        public static Func<Parser<T>, Parser<T>> SeparatedBy(Parser<T> separatorParser) => SeparatedByAtLeast(0, separatorParser);

        public static Parser<T> SequenceOf(IEnumerable<Parser<T>> parsers) => new Parser<T>((ParserState<T> state) =>
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

            return ParserState<T>.SetResult(nextState, results);
        });
    }
}
