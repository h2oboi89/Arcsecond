using System;

namespace Arcsecond
{
    public partial class Parser
    {
        internal Func<ParserState, ParserState> Transform;

        internal Parser() { }

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

        public Parser Chain(Func<object, Parser> transformFunction)
        {
            return new Parser(delegate (ParserState state)
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var nextParser = transformFunction(nextState.Result);

                return nextParser.Transform(nextState);
            });
        }

        public Parser Map(Func<object, object> transformFunction)
        {
            return new Parser(delegate (ParserState state)
            {
                var nextState = Transform(state);

                if (nextState.IsError) return nextState;

                var transformedResult = transformFunction(nextState.Result);

                return ParserState.SetResult(nextState, transformedResult);
            });
        }

        public Parser ErrorMap(Func<object, int, object> transformFunction)
        {
            return new Parser(delegate (ParserState state)
            {
                var nextState = Transform(state);

                if (!nextState.IsError) return nextState;

                var transformedError = transformFunction(nextState.Error, nextState.Index);

                return ParserState.SetError(nextState, transformedError);
            });
        }
    }
}
