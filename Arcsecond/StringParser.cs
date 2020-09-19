namespace Arcsecond
{
    public class StringParser : Parser
    {
        private readonly string _target;

        public StringParser(string target)
        {
            _target = target;
        }

        public override ParserState Parse(ParserState state)
        {
            if (state.IsError)
            {
                return state;
            }

            var slicedInput = state.Input.Slice(state.Index);

            if (slicedInput.Length == 0)
            {
                return ParserState.SetError(state, $"Tried to match '{_target}', but got unexpected end of input");
            }

            if (slicedInput.Contains(_target))
            {
                return ParserState.SetResult(state, _target, state.Index + _target.Length);
            }

            return ParserState.SetError(state, $"Tried to match '{_target}', but got '{state.Input.Slice(state.Index, _target.Length)}'");
        }
    }
}
