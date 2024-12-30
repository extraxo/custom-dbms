using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public static class CustomIndexOf
    {
        public static int IndexOf(string source, char target)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == target)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int IndexOfSubstring(string input, string target)
        {
            int targetLength = target.Length;
            for (int i = 0; i <= input.Length - targetLength; i++)
            {
                bool match = true;
                for (int j = 0; j < targetLength; j++)
                {
                    if (input[i + j] != target[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i; 
                }
            }
            return -1;
        }

        public static string IndexOfSubstring(string input, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex + length > input.Length)
            {
                throw new ArgumentOutOfRangeException("Invalid start index or length.");
            }

            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = input[startIndex + i];
            }

            return new string(result);
        }
       
    }
}
