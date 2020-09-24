using System.Text.RegularExpressions;

namespace Arcsecond
{
    public partial class Parser
    {
        private static readonly Regex lettersRegex = new Regex("^[A-Za-z]+");

        public static readonly Parser Letters = new Parser(delegate (ParserState state)
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

            var match = lettersRegex.Match(slicedInput);

            if (match.Success)
            {
                return ParserState.SetResult(state, match.Value, state.Index + match.Value.Length);
            }

            return ParserState.SetError(state, $"Could not match letters at index {state.Index}");
        });
    }
}
