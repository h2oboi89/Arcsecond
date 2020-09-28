using System.Text.RegularExpressions;

namespace Arcsecond
{
    public static class Numbers
    {
        public enum Bases
        {
            Binary,
            Octal,
            Decimal,
            Hexadecimal
        }

        public const int UNLIMITED = 0;

        public static Parser<string> Digits(Bases @base = Bases.Decimal, int minimum = 1, uint maximum = UNLIMITED)
        {
            var count = minimum + ",";

            if (maximum != UNLIMITED)
            {
                count += maximum;
            }

            var digits = string.Empty;

            switch (@base)
            {
                case Bases.Binary: digits = "0-1"; break;
                case Bases.Octal: digits = "0-7"; break;
                case Bases.Decimal: digits = "0-9"; break;
                case Bases.Hexadecimal: digits = "0-9a-fA-F"; break;
            }

            var digitsRegex = new Regex($"^[{digits}]{{{minimum},{(maximum == UNLIMITED ? string.Empty : maximum.ToString())}}}");

            return new Parser<string>((ParserState<string> state) =>
            {
                if (state.IsError) return state;

                var slicedInput = state.Input.Slice(state.Index);

                if (slicedInput.Length == 0)
                {
                    return ParserState<string>.SetError(state, $"Got unexpected end of input");
                }

                var match = digitsRegex.Match(slicedInput);

                if (match.Success)
                {
                    return ParserState<string>.SetResult(state, match.Value, state.Index + match.Value.Length);
                }

                return ParserState<string>.SetError(state, $"Could not match digits at index {state.Index}");
            });
        }

        // Following will be SequenceOf(Prefix, Digits)
        // Binary
        // Octal
        // Hex
        // Decimal
    }
}
