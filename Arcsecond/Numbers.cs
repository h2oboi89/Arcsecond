using System.Text.RegularExpressions;

namespace Arcsecond
{
    /// <summary>
    /// String parsing utility parsers based around parsing numeric values.
    /// </summary>
    public static class Numbers
    {
        public enum Bases
        {
            Binary,
            Octal,
            Decimal,
            Hexadecimal
        }

        public static Parser<string> Digits(Bases @base = Bases.Decimal, int minimum = 1, int maximum = Strings.UNLIMITED)
        {
            // TODO: ensure max and minimum are sane

            var digits = string.Empty;

            switch (@base)
            {
                case Bases.Binary: digits = "0-1"; break;
                case Bases.Octal: digits = "0-7"; break;
                case Bases.Decimal: digits = "0-9"; break;
                case Bases.Hexadecimal: digits = "0-9a-fA-F"; break;
            }

            var digitsRegex = $"[{digits}]{{{minimum},{(maximum == Strings.UNLIMITED ? string.Empty : maximum.ToString())}}}";

            return Strings.Regex(digitsRegex, "digit(s)", 1);
        }

        public static Parser<string> Digit(Bases @base = Bases.Decimal) => Digits(@base, 1, 1);

        // Following will be SequenceOf(Prefix, Digits)
        // Binary
        // Octal
        // Hex
        // Decimal
    }
}
