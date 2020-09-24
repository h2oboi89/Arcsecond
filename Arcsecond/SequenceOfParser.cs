using System.Collections.Generic;

namespace Arcsecond
{
    public partial class Parser
    {
        public static Parser SequenceOf(IEnumerable<Parser> parsers) => new Parser(delegate (ParserState state)
        {
            if (state.IsError)
            {
                return state;
            }

            var results = new List<object>();
            var nextState = state;

            foreach (var parser in parsers)
            {
                nextState = parser.Transform(nextState);

                results.Add(nextState.Result);
            }

            return ParserState.SetResult(nextState, results);
        });
    }
}
