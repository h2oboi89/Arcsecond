using System.Collections.Generic;

namespace Arcsecond
{
    public class SequenceOf : Parser
    {
        private readonly IEnumerable<Parser> _parsers;

        public SequenceOf(IEnumerable<Parser> parsers)
        {
            _parsers = parsers;
        }

        public override ParserState Parse(ParserState state)
        {
            if (state.IsError)
            {
                return state;
            }

            var results = new List<object>();
            var nextState = state;

            foreach(var parser in _parsers)
            {
                nextState = parser.Parse(nextState);

                results.Add(nextState.Result);
            }

            return ParserState.SetResult(nextState, results);
        }
    }
}
