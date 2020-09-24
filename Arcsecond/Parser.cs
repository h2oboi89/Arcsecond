using System;

namespace Arcsecond
{
    public partial class Parser
    {
        public readonly Func<ParserState, ParserState> Transform;

        public Parser(Func<ParserState, ParserState> transform)
        {
            Transform = transform;
        }

        public ParserState Run(string input)
        {
            var state = ParserState.Initialize(input);

            var result = Transform(state);

            if (result.IsError)
            {
                return ParserState.SetError(state, result.Error);
            }

            return ParserState.SetResult(state, result.Result, result.Index);
        }

        public Parser Chain(Func<object, Parser> transform)
        {
            return new Parser(delegate (ParserState state)
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var nextParser = transform(nextState.Result);

                return nextParser.Transform(nextState);
            });
        }

        public Parser Map(Func<object, object> transform)
        {
            return new Parser(delegate (ParserState state)
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var transformedResult = transform(nextState.Result);

                return ParserState.SetResult(nextState, transformedResult);
            });
        }

        public Parser ErrorMap(Func<object, int, object> transform)
        {
            return new Parser(delegate (ParserState state)
            {
                var nextState = Transform(state);

                if (!nextState.IsError) return nextState;

                var transformedError = transform(nextState.Error, nextState.Index);

                return ParserState.SetError(nextState, transformedError);
            });
        }
    }
}
