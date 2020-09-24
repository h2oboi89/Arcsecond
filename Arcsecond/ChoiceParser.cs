using System.Collections.Generic;

namespace Arcsecond
{
    public partial class Parser
    {
        public static Parser Choice(IEnumerable<Parser> parsers) => new Parser(delegate (ParserState state)
        {
            if (state.IsError)
            {
                return state;
            }

            foreach (var parser in parsers)
            {
                var nextState = parser.Transform(state);

                if (!nextState.IsError)
                {
                    return nextState;
                }
            }

            return ParserState.SetError(state, $"Unable to match with any parser at index {state.Index}");
        });
    }
}
