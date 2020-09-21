using System.Collections.Generic;

namespace Arcsecond
{
    public class SequenceOf : Parser
    {
        public SequenceOf(IEnumerable<Parser> parsers)
        {
            Transform = delegate (ParserState state)
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
            };
        }
    }
}
