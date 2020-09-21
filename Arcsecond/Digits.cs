using System.Text.RegularExpressions;

namespace Arcsecond
{
    public class Digits : Parser
    {
        private static readonly Regex regex = new Regex("^[0-9]+");

        public Digits()
        {
            Transform = delegate (ParserState state)
            {
                if (state.IsError)
                {
                    return state;
                }

                var slicedInput = state.Input.Slice(state.Index);

                if (slicedInput.Length == 0)
                {
                    return ParserState.SetError(state, $"Got unexpected end of input");
                }

                var match = regex.Match(slicedInput);

                if (match.Success)
                {
                    return ParserState.SetResult(state, match.Value, state.Index + match.Value.Length);
                }

                return ParserState.SetError(state, $"Could not match digits at index {state.Index}");
            };
        }
    }
}
