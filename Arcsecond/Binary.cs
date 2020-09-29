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
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
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

        public static readonly Parser<byte[]> U8 = new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                var size = sizeof(byte);

                if (state.Index + size > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                return ParserState<byte[]>.SetResult(state, state.Input[state.Index], state.Index + size);
            });

        public static readonly Parser<byte[]> I8 = new Parser<byte[]>((ParserState<byte[]> state) =>
        {
            if (state.IsError) return state;

            var size = sizeof(sbyte);

            if (state.Index + size > state.Input.Length)
            {
                return ParserState<byte[]>.SetError(state, "Unexpected end of input");
            }

            return ParserState<byte[]>.SetResult(state, (sbyte)state.Input[state.Index], state.Index + size);
        });

        public static Parser<byte[]> U16(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                var size = sizeof(ushort);

                if (state.Index + size > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                var bytes = ExtractBytes(state, size, endian);

                var result = BitConverter.ToUInt16(bytes, 0);

                return ParserState<byte[]>.SetResult(state, result, state.Index + size);
            });
        }

        public static Parser<byte[]> I16(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                var size = sizeof(short);

                if (state.Index + size > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                var bytes = ExtractBytes(state, size, endian);

                var result = BitConverter.ToInt16(bytes, 0);

                return ParserState<byte[]>.SetResult(state, result, state.Index + size);
            });
        }

        public static Parser<byte[]> U32(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                var size = sizeof(uint);

                if (state.Index + size > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                var bytes = ExtractBytes(state, size, endian);

                var result = BitConverter.ToUInt32(bytes, 0);

                return ParserState<byte[]>.SetResult(state, result, state.Index + size);
            });
        }

        public static Parser<byte[]> I32(Endian endian = Endian.Big)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                var size = sizeof(int);

                if (state.Index + size > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                var bytes = ExtractBytes(state, size, endian);

                var result = BitConverter.ToInt32(bytes, 0);

                return ParserState<byte[]>.SetResult(state, result, state.Index + size);
            });
        }

        public static Parser<byte[]> Bytes(int length)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                if (state.Index + length > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                var bytes = new byte[length];

                Array.Copy(state.Input, state.Index, bytes, 0, length);

                return ParserState<byte[]>.SetResult(state, bytes, state.Index + length);
            });
        }

        public static Parser<byte[]> String(int length)
        {
            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                if (state.Index + length > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                var bytes = new byte[length];

                Array.Copy(state.Input, bytes, length);


                // TODO: wrap in try catch?
                var result = Encoding.ASCII.GetString(bytes);

                return ParserState<byte[]>.SetResult(state, result, state.Index + length);
            });
        }

        public static Parser<byte[]> String(string target)
        {
            var length = Encoding.ASCII.GetBytes(target).Length;

            return new Parser<byte[]>((ParserState<byte[]> state) =>
            {
                if (state.IsError) return state;

                if (state.Index + length > state.Input.Length)
                {
                    return ParserState<byte[]>.SetError(state, "Unexpected end of input");
                }

                var bytes = new byte[target.Length];

                Array.Copy(state.Input, 0, bytes, 0, target.Length);

                var result = Encoding.ASCII.GetString(bytes);

                if (result != target)
                {
                    return ParserState<byte[]>.SetError(state, $"Expected '{target}', but got '{result}' at index {state.Index}");
                }

                return ParserState<byte[]>.SetResult(state, result, state.Index + length);
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
