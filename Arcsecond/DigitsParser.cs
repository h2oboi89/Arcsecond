using System.Text.RegularExpressions;

namespace Arcsecond
{
    public partial class Parser
    {
        private static readonly Regex digitsRegex = new Regex("^[0-9]+");

        public static readonly Parser Digits = new Parser(delegate (ParserState state)
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

            var match = digitsRegex.Match(slicedInput);

            if (match.Success)
            {
                return ParserState.SetResult(state, match.Value, state.Index + match.Value.Length);
            }

            return ParserState.SetError(state, $"Could not match digits at index {state.Index}");
        });
    }
}
