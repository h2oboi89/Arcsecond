using System.Collections.Generic;

namespace Arcsecond
{
    public static class Binary
    {
        public static Parser<List<byte>> Bits(byte mask, bool increment = true) => new Parser<List<byte>>((ParserState<List<byte>> state) =>
        {
            if (state.IsError) return state;

            if (state.Index > state.Input.Count)
            {
                return ParserState<List<byte>>.SetError(state, "Unexpected end of input");
            }

            var bits = (byte)(state.Input[state.Index] & mask);

            byte temp = mask;

            while((temp & 0x01) == 0)
            {
                bits >>= 1;
                temp >>= 1;
            }

            return ParserState<List<byte>>.SetResult(state, bits, increment ? state.Index + 1 : state.Index);
        });

        // All Integer Types (u8, i8, u16, i16, ...)
        // 3 floating point types? (do in future?)
        // ASCII and Unicode strings (or is that just a map function?)

        // Handle BE and LE
    }
}
