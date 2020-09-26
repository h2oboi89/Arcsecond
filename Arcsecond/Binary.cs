using System.Collections.Generic;

namespace Arcsecond
{
    public static class Binary
    {
        // rewrite to be byte oriented and use map to extract bits?
        public static readonly Parser<List<byte>> Bit = new Parser<List<byte>>((ParserState<List<byte>> state) => 
        {
            if (state.IsError) return state;

            var byteOffset = state.Index / 8;

            if (byteOffset >= state.Input.Count)
            {
                return ParserState<List<byte>>.SetError(state, "Unexpected end of input");
            }

            var @byte = state.Input[0];
            var bitOffset = 7 - (state.Index % 8);

            var result = (@byte >> bitOffset) & 1;

            return ParserState<List<byte>>.SetResult(state, result, state.Index + 1);
        });

        public static readonly Parser<List<byte>> Zero = new Parser<List<byte>>((ParserState<List<byte>> state) =>
        {
            if (state.IsError) return state;

            var byteOffset = state.Index / 8;

            if (byteOffset >= state.Input.Count)
            {
                return ParserState<List<byte>>.SetError(state, "Unexpected end of input");
            }

            var @byte = state.Input[0];
            var bitOffset = 7 - (state.Index % 8);

            var result = (@byte >> bitOffset) & 1;

            if (result != 0)
            {
                return ParserState<List<byte>>.SetResult(state, $"Expected a 0, but got 1 at index {state.Index}");
            }

            return ParserState<List<byte>>.SetResult(state, result, state.Index + 1);
        });

        public static readonly Parser<List<byte>> One = new Parser<List<byte>>((ParserState<List<byte>> state) =>
        {
            if (state.IsError) return state;

            var byteOffset = state.Index / 8;

            if (byteOffset >= state.Input.Count)
            {
                return ParserState<List<byte>>.SetError(state, "Unexpected end of input");
            }

            var @byte = state.Input[0];
            var bitOffset = 7 - (state.Index % 8);

            var result = (@byte >> bitOffset) & 1;

            if (result != 1)
            {
                return ParserState<List<byte>>.SetResult(state, $"Expected a 1, but got 0 at index {state.Index}");
            }

            return ParserState<List<byte>>.SetResult(state, result, state.Index + 1);
        });

        // All Integer Types (u8, i8, u16, i16, ...)
        // 3 floating point types? (do in future?)
        // ASCII and Unicode strings (or is that just a map function?)

        // Handle BE and LE
    }
}
