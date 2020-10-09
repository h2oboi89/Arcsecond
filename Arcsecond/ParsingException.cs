using System;

namespace Arcsecond
{
    public class ParsingException : Exception
    {
        public ParsingException(string message, int index) : 
            this(message, index, null) { }

        public ParsingException(string message, int index, Exception innerException) : 
            base($"{message} at index {index}", innerException) { }
    }
}