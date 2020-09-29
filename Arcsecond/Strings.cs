using System.Text.RegularExpressions;

namespace Arcsecond
{
    public static class Strings
    {
        private static readonly Regex lettersRegex = new Regex("^[A-Za-z]+");

        // TODO: model after Digits accept a range of characters to match?)
        public static readonly Parser<string> Letters = new Parser<string>((ParserState<string> state) =>
        {
            if (state.IsError) return state;

            var slicedInput = state.Input.Slice(state.Index);

            if (slicedInput.Length == 0)
            {
                return ParserState<string>.SetError(state, $"Got unexpected end of input");
            }

            var match = lettersRegex.Match(slicedInput);

            if (match.Success)
            {
                return ParserState<string>.SetResult(state, match.Value, state.Index + match.Value.Length);
            }

            return ParserState<string>.SetError(state, $"Could not match letters at index {state.Index}");
        });

        public static Parser<string> Parser(string target) => new Parser<string>((ParserState<string> state) =>
        {
            if (state.IsError) return state;

            var slicedInput = state.Input.Slice(state.Index);

            if (slicedInput.Length == 0)
            {
                return ParserState<string>.SetError(state, $"Tried to match '{target}', but got unexpected end of input");
            }

            if (slicedInput.StartsWith(target))
            {
                return ParserState<string>.SetResult(state, target, state.Index + target.Length);
            }

            return ParserState<string>.SetError(state, $"Tried to match '{target}', but got '{state.Input.Slice(state.Index, target.Length)}'");
        });
    }
}
