using System;

namespace Arcsecond
{
    public class Parser
    {
        public ParserState Run(string input)
        {
            var state = ParserState.Initialize(input);

            var result = Parse(state);

            if (result.IsError)
            {
                return ParserState.SetError(state, result.Error);
            }

            return ParserState.SetResult(state, result.Result, result.Index);
        }

        public virtual ParserState Parse(ParserState state) => state;

        public ParserState Fork(string target, Func<object, ParserState, ParserState> errorFunction, Func<object, ParserState, ParserState> successFunction)
        {
            var state = ParserState.Initialize(target);

            var newState = Parse(state);

            if (newState.IsError)
            {
                return errorFunction(newState.Error, newState);
            }

            return successFunction(newState.Result, newState);
        }
    }
}
