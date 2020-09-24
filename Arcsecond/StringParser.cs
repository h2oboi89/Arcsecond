namespace Arcsecond
{
    public partial class Parser
    {
        public static Parser String(string target) => new Parser(delegate (ParserState state)
        {
            if (state.IsError)
            {
                return state;
            }

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
