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
    }
}
