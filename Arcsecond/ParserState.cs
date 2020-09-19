﻿using System;
using System.Collections.Generic;

namespace Arcsecond
{
    public struct ParserState
    {
        public readonly string Input;
        public readonly int Index;
        public readonly object Result; // can this be same type as Input?
        public readonly object Error; // what type is this? (ParseException?)

        public ParserState(string input, int index, object result, object error)
        {
            Input = input;
            Index = index;
            Result = result;
            Error = error;
        }

        public static ParserState Initialize(string input)
        {
            return new ParserState(input, 0, null, null);
        }

        public static ParserState SetError(ParserState state, object error)
        {
            return new ParserState(state.Input, state.Index, state.Result, error);
        }

        public static ParserState SetResult(ParserState state, object result)
        {
            return new ParserState(state.Input, state.Index, result, state.Error);
        }

        public static ParserState SetResult(ParserState state, object result, int index)
        {
            return new ParserState(state.Input, index, result, state.Error);
        }

        public bool IsError => Error != null;
    }
}
