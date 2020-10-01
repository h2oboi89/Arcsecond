using System;
using System.Text;

namespace Arcsecond
{
    public static class Binary
    {
        public static Parser<byte[]> Bits(uint mask, int size = sizeof(byte), bool increment = true)
        {
            var minSize = sizeof(byte);
            var maxSize = sizeof(uint);

            if (size < minSize || size > maxSize)
            {
                throw new ArgumentException(nameof(size), $"size must be in range[{minSize},{maxSize}]");
            }

            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                if (state.Index + size > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, new ParsingException("Unexpected end of input", state.Index));
                }

                var bytes = new byte[maxSize];

                var offset = maxSize - size;

                Array.Copy(state.Input, state.Index, bytes, offset, size);

                var bits = state.Input[state.Index] & mask;

                var temp = mask;

                while ((temp & 0x01) == 0)
                {
                    bits >>= 1;
                    temp >>= 1;
                }

                return ParserState<byte[]>.SetResult(state, bits, increment ? state.Index + size : state.Index);
            });
        }

        public enum Endian
        {
            Big,
            Little
        }

        private static byte[] ConvertEndianess(byte[] bytes, Endian endianess)
        {
            if ((BitConverter.IsLittleEndian && endianess == Endian.Big) ||
                (!BitConverter.IsLittleEndian && endianess == Endian.Little))
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        private static byte[] ExtractBytes(ParserState<byte[]> state, int size, Endian endian)
        {
            var bytes = new byte[size];

            Array.Copy(state.Input, state.Index, bytes, 0, size);

            return ConvertEndianess(bytes, endian);
        }

        public static ParserState<byte[]> ParseType(ParserState<byte[]> state, int size, Func<ParserState<byte[]>, object> getResult)
        {
            if (state.IsError) return state;

            if (state.Index + size > state.Input.Length)
            {
                return ParserState<byte[]>.SetError(state, new ParsingException("Got unexpected end of input", state.Index));
            }

            try
            {
                var result = getResult(state);

                return ParserState<byte[]>.SetResult(state, result, state.Index + size);
            }
            catch (Exception e)
            {
                return ParserState<byte[]>.SetError(state, new ParsingException("Error extracting type", state.Index, e));
            }
        }

        public static readonly Parser<byte[]> U8 = new Parser<byte[]>((ParserState<byte[]> state) =>
        {
            return ParseType(state, sizeof(byte), (state) => state.Input[state.Index]);
        });

        public static readonly Parser<byte[]> I8 = new Parser<byte[]>((ParserState<byte[]> state) =>
        {
            return ParseType(state, sizeof(byte), (state) => (sbyte)state.Input[state.Index]);
        });

        public static Parser<byte[]> U16(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                var size = sizeof(ushort);

                return ParseType(state, size, (state) => BitConverter.ToUInt16(ExtractBytes(state, size, endian), 0));
            });
        }

        public static Parser<byte[]> I16(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                var size = sizeof(short);

                return ParseType(state, size, (state) => BitConverter.ToInt16(ExtractBytes(state, size, endian), 0));
            });
        }

        public static Parser<byte[]> U32(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                var size = sizeof(uint);

                return ParseType(state, size, (state) => BitConverter.ToUInt32(ExtractBytes(state, size, endian), 0));
            });
        }

        public static Parser<byte[]> I32(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                var size = sizeof(int);

                return ParseType(state, size, (state) => BitConverter.ToInt32(ExtractBytes(state, size, endian), 0));
            });
        }

        public static Parser<byte[]> Bytes(int length)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                return ParseType(state, length, (state) =>
                {
                    var bytes = new byte[length];

                    Array.Copy(state.Input, state.Index, bytes, 0, length);

                    return bytes;
                });
            });
        }

        public static Parser<byte[]> String(int length)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                return ParseType(state, length, (state) => Encoding.ASCII.GetString(state.Input, state.Index, length));
            });
        }

        public static Parser<byte[]> String(string expected)
        {
            var length = Encoding.ASCII.GetBytes(expected).Length;

            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                var newState = String(length).Apply(state);

                var actual = (string)newState.Result;

                if (actual != expected)
                {
                    return ParserState<byte[]>.SetError(state, new ParsingException($"Expected '{expected}', but got '{actual}'", state.Index));
                }

                return ParserState<byte[]>.SetResult(newState, actual, newState.Index);
            });
        }

        // All Integer Types (u8, i8, u16, i16, ...)
        // - Missing: U64, I64

        // 3 floating point types? (do in future?)
        // - Float, Double, Decimal

        // ASCII and Unicode strings
        // - Missing: Unicode
    }
}
