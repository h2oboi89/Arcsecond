using System.Collections.Generic;

namespace Arcsecond
{
    public class ManyAtLeast : Parser
    {
        public ManyAtLeast(int minimum, Parser parser)
        {
            Transform = delegate (ParserState state)
            {
                if (state.IsError)
                {
                    return state;
                }

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
                    return ParserState.SetError(nextState, $"Unable to match any input using parser at index {nextState.Index}");
                }

                return ParserState.SetResult(nextState, results);
            };
        }
    }
}
