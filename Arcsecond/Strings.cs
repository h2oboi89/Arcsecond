using System.Text.RegularExpressions;

namespace Arcsecond
{
    public static class Strings
    {
        public const int UNLIMITED = 0;

        internal static Parser<string> Regex(string regularExpression, string expected, int expectedLength)
        {
            var regex = new Regex("^" + regularExpression);

            return new Parser<string>((ParserState<string> state) =>
            {
                if (state.IsError) return state;

                var slicedInput = state.Input.Slice(state.Index);

                if (slicedInput.Length == 0)
                {
                    return ParserState<string>.SetError(state, new ParsingException($"Got unexpected end of input", state.Index));
                }

                var match = regex.Match(slicedInput);

                if (match.Success)
                {
                    return ParserState<string>.SetResult(state, match.Value, state.Index + match.Value.Length);
                }

                return ParserState<string>.SetError(state, new ParsingException($"Tried to match {expected}, but got '{slicedInput.Slice(0, expectedLength)}'", state.Index));
            });
        }

        public static Parser<string> RegularExpression(string regularExpression) => Regex(regularExpression, "using '^" + regularExpression + "'", -1);


        // TODO: ensure max and minimum are sane
        public static Parser<string> Letters(int minimum = 1, int maximum = UNLIMITED) => Regex($"[A-Za-z]{{{minimum},{(maximum == UNLIMITED ? string.Empty : maximum.ToString())}}}", "letter(s)", 1);

        public static Parser<string> Letter() => Letters(1, 1);

        public static Parser<string> Whitespace() => Regex("\\s+", "whitespace", 1);

        public static Parser<string> OptionalWhitespace() => Parser<string>.Possibly(Whitespace());

        public static Parser<string> Parser(string target) => new Parser<string>((ParserState<string> state) =>
        {
            if (state.IsError) return state;

            var slicedInput = state.Input.Slice(state.Index, target.Length);

            if (slicedInput.Length == 0)
            {
                return ParserState<string>.SetError(state, new ParsingException($"Tried to match '{target}', but got unexpected end of input", state.Index));
            }

            if (slicedInput.StartsWith(target))
            {
                return ParserState<string>.SetResult(state, target, state.Index + target.Length);
            }

            return ParserState<string>.SetError(state, new ParsingException($"Tried to match '{target}', but got '{slicedInput}'", state.Index));
        });

        public static Parser<string> Character(char c) => Parser(c.ToString());

        public static Parser<string> AnyCharacter() => new Parser<string>((ParserState<string> state) =>
        {
            if (state.IsError) return state;

            var slicedInput = state.Input.Slice(state.Index, 1);

            if (slicedInput.Length == 0)
            {
                return ParserState<string>.SetError(state, new ParsingException($"Tried to match any character, but got unexpected end of input", state.Index));
            }

            return ParserState<string>.SetResult(state, slicedInput, state.Index + 1);
        });

        // TODO: Peek(n)
        public static Parser<string> Peek() => new Parser<string>((ParserState<string> state) =>
        {
            if (state.IsError) return state;

            if (state.Index < state.Input.Length)
            {
                return ParserState<string>.SetResult(state, state.Input[state.Index]);
            }

            return ParserState<string>.SetError(state, new ParsingException("Unexpected end of input", state.Index));
        });

        // TODO: LookAhead

        // TODO: everyCharUntil

        // TODO: anyCharExcept
    }
}
