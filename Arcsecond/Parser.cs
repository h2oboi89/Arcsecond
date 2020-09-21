using System;

namespace Arcsecond
{
    public class Parser
    {
        internal Func<ParserState, ParserState> Transform;

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

        public Parser Map(Func<object, object> transformFunction)
        {
            var parser = new Parser
            {
                Transform = delegate (ParserState state)
                {
                    var nextState = Transform(state);

                    if (nextState.IsError) return nextState;

                    var transformedResult = transformFunction(nextState.Result);

                    return ParserState.SetResult(nextState, transformedResult);
                }
            };

            return parser;
        }

        public Parser ErrorMap(Func<object, int, object> transformFunction)
        {
            var parser = new Parser
            {
                Transform = delegate (ParserState state)
                {
                    var nextState = Transform(state);

                    if (!nextState.IsError) return nextState;

                    var transformedError = transformFunction(nextState.Error, nextState.Index);

                    return ParserState.SetError(nextState, transformedError);
                }
            };

            return parser;
        }
    }
}
