using System.Text.RegularExpressions;

namespace Arcsecond
{
    public static class Strings
    {
        private static readonly Regex lettersRegex = new Regex("^[A-Za-z]+");

        // TODO: model after Digits
        public static readonly Parser Letters = new Parser((ParserState state) =>
        {
            if (state.IsError) return state;

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

        public static Parser Parser(string target) => new Parser((ParserState state) =>
        {
            if (state.IsError) return state;

            var slicedInput = state.Input.Slice(state.Index);

            if (slicedInput.Length == 0)
            {
                return ParserState.SetError(state, $"Tried to match '{target}', but got unexpected end of input");
            }

            if (slicedInput.StartsWith(target))
            {
                return ParserState.SetResult(state, target, state.Index + target.Length);
            }

            return ParserState.SetError(state, $"Tried to match '{target}', but got '{state.Input.Slice(state.Index, target.Length)}'");
        });
    }
}
