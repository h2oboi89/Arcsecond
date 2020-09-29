namespace Arcsecond
{
    public struct ParserState<T>
    {
        public readonly T Input;
        public readonly int Index;
        public readonly object Result;
        public readonly object Error;

        public ParserState(T input, int index, object result, object error)
        {
            Input = input;
            Index = index;
            Result = result;
            Error = error;
        }

        public static ParserState<T> Initialize(T input)
        {
            return new ParserState<T>(input, 0, null, null);
        }

        public static ParserState<T> SetError(ParserState<T> state, object error)
        {
            return new ParserState<T>(state.Input, state.Index, state.Result, error);
        }

        public static ParserState<T> SetResult(ParserState<T> state, object result)
        {
            return new ParserState<T>(state.Input, state.Index, result, state.Error);
        }

        public static ParserState<T> SetResult(ParserState<T> state, object result, int index)
        {
            return new ParserState<T>(state.Input, index, result, state.Error);
        }

        public bool IsError => Error != null;
    }
}
