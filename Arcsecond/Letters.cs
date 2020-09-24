using System.Text.RegularExpressions;

namespace Arcsecond
{
    public sealed class Letters : Parser
    {
        private static readonly Regex regex = new Regex("^[A-Za-z]+");

        static Letters() { }

        private Letters()
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

                return ParserState.SetError(state, $"Could not match letters at index {state.Index}");
            };
        }

        public static Letters Instance { get; } = new Letters();
    }
}
