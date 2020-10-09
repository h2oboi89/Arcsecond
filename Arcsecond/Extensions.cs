namespace Arcsecond
{
    public static class StringExtensions
    {
        private const int END_OF_STRING = -1;

        public static string Slice(this string s, int index, int length = END_OF_STRING)
        {
            if (index >= s.Length) return string.Empty;

            if (length == END_OF_STRING || index + length > s.Length)
            {
                length = s.Length - index;
            }

            return s.Substring(index, length);
        }
    }
}
