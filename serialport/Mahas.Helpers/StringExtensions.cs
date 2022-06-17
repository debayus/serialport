using System;

namespace serialport.Mahas.Helpers
{
    public static class StringExtensions
    {
        public static string ToLiteral(this string value)
        {
            value = value.Replace("\\r", $"{(char)13}");
            value = value.Replace("\\n", $"{(char)10}");
            return value;
        }
    }
}

