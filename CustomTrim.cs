using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public static class CustomTrimFunc
    {
        public static string CustomTrim(string input)
        {
            if (input == null)
                return null;

            int startIndex = 0;
            int endIndex = input.Length - 1;

            while (startIndex <= endIndex && char.IsWhiteSpace(input[startIndex]))
            {
                startIndex++;
            }

            while (endIndex >= startIndex && char.IsWhiteSpace(input[endIndex]))
            {
                endIndex--;
            }

            if (startIndex > endIndex)
            {
                return string.Empty;
            }

            return CustomIndexOf.IndexOfSubstring(input, startIndex, endIndex - startIndex + 1);
        }
    }
}
