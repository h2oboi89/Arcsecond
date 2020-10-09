using System;
using System.Collections.Generic;

namespace Arcsecond
{
    /// <summary>
    /// Base class for all parsing.
    /// </summary>
    /// <typeparam name="T">Type of input that will be parsed.</typeparam>
    public class Parser<T>
    {
        private Func<ParserState<T>, ParserState<T>> Transform;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="transform">Method that parses a section of the input.</param>
        public Parser(Func<ParserState<T>, ParserState<T>> transform)
        {
            Transform = transform;
        }

        /// <summary>
        /// Creates a parser with a placeholder transform function.
        /// Used to create <see cref="Parser{T}"/>s that have circular references to each other.
        /// </summary>
        public static LazyParser<T> Lazy => new LazyParser<T>();

        /// <summary>
        /// Parser extension class for <see cref="Lazy"/>.
        /// </summary>
        /// <typeparam name="T">Type of input that will be parserd.</typeparam>
        public class LazyParser<T> : Parser<T>
        {
            internal LazyParser() : base(s => s) { }

            /// <summary>
            /// Replaces placeholder transform function with actual transform function.
            /// </summary>
            /// <param name="parser">Parser whose transform function this parser will use.</param>
            public void Initialize(Parser<T> parser)
            {
                Transform = parser.Transform;
            }
        }

        /// <summary>
        /// Utility method used by another <see cref="Parser{T}"/> to apply this <see cref="Parser{T}"/>s transform to a state.
        /// Useful when another <see cref="Parser{T}"/> is identical to current <see cref="Parser{T}"/> with some extra logic.
        /// </summary>
        /// <param name="state">Current <see cref="ParserState{T}"/></param>
        /// <returns>Updated <see cref="ParserState{T}"/> resulting from calling the transform function.</returns>
        public ParserState<T> Apply(ParserState<T> state) => Transform(state);

        /// <summary>
        /// Parses the <paramref name="input"/> using this <see cref="Parser{T}"/>s transform function.
        /// </summary>
        /// <param name="input">Input tot parse.</param>
        /// <returns><see cref="ParserState{T}"/> that is the result of calling the transform function.</returns>
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

        // TODO: Fork

        /// <summary>
        /// Utility method that applies <paramref name="transform"/> to the <see cref="ParserState{T}"/> that is the result of calling the transform function.
        /// Useful for modifying the <see cref="ParserState{T}.Result"/> (ie: converting raw object(s) into another type with more structure.
        /// Counterpart to <see cref="ErrorMap(Func{ParsingException, int, ParsingException})"/>.
        /// </summary>
        /// <param name="transform">Function to modify the <see cref="ParserState{T}.Result"/></param>
        /// <returns><see cref="Parser{T}"/> that will run current <see cref="Parser{T}"/>s transform and then the provided <paramref name="transform"/>.</returns>
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

        // TODO: mapTo

        /// <summary>
        /// Utility method for chaining parsers together sequentially based on previous <see cref="ParserState{T}.Result"/>.
        /// Useful for creating contextual parsers. 
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
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

        // TODO: MapFromData

        // TODO: ChainFromData

        /// <summary>
        /// Utility method that applies <paramref name="transform"/> to the <see cref="ParserState{T}"/> that is the result of calling the transform function.
        /// Useful for modifying the <see cref="ParserState{T}.Error"/>.
        /// Counterpart to <see cref="Map(Func{object, object})"/>.
        /// </summary>
        /// <param name="transform">Function to modify the <see cref="ParserState{T}.Error"/></param>
        /// <returns><see cref="Parser{T}"/> that will run current <see cref="Parser{T}"/>s transform and then the provided <paramref name="transform"/>.</returns>
        public Parser<T> ErrorMap(Func<ParsingException, int, ParsingException> transform)
        {
            return new Parser<T>((ParserState<T> state) =>
            {
                var nextState = Transform(state);

                if (!nextState.IsError) return nextState;

                var transformedError = transform(nextState.Error, nextState.Index);

                return ParserState<T>.SetError(nextState, transformedError);
            });
        }

        // TODO: errorMapTo

        // TODO: ErrorChain

        // TODO: Data functions (setData, withData, mapData, getData

        // TODO: coroutine

        public static Parser<T> Possibly(Parser<T> parser) => new Parser<T>((ParserState<T> state) =>
        {
            if (state.IsError) return state;

            var newState = parser.Transform(state);

            return newState.IsError ? state : newState;
        });

        public static Func<Parser<T>, Parser<T>> Between(Parser<T> left, Parser<T> right) =>
            (Parser<T> content) =>
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

            return ParserState<T>.SetError(state, new ParsingException("Unable to match with any parser", state.Index));
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
                return ParserState<T>.SetError(nextState, new ParsingException("Unable to match any input using parser", state.Index));
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
                    return ParserState<T>.SetError(state, new ParsingException("Unable to match any input using parser", state.Index));
                }

                return ParserState<T>.SetResult(nextState, results);
            });
        };

        public static Func<Parser<T>, Parser<T>> SeparatedBy(Parser<T> separatorParser) => SeparatedByAtLeast(0, separatorParser);

        // TODO: NamedSequenceof (SequenceOf but takes { Key, Parser> and returns { Key, Result } pairs)

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

        public static Parser<T> Fail(ParsingException error) => 
            new Parser<T>((ParserState<T> state) => ParserState<T>.SetError(state, error));

        public static Parser<T> Succeed(object result) => 
            new Parser<T>((ParserState<T> state) => ParserState<T>.SetResult(state, result));

        // TODO: succeedWith

        // TODO: exactly

        // TODO: everythingUntil

        // TODO: anythingExcept

        // TODO: endOfInput

        // TODO: skip

        // TODO: pipe / compose

        // TODO: takeRight, takeLeft

        // TODO: tap

        // TODO: decide

        // TODO: either

        // TODO: toValue
    }
}
