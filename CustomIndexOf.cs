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

        public static int IndexOf(string source, char target, int startIndex)
        {
            for (int i = startIndex; i < source.Length; i++)
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

        public static string IndexOfSubstring(string input, int startIndex)
        {
            return IndexOfSubstring(input, startIndex, input.Length - startIndex);
        }

        public static bool StartsWith(string source, string prefix)
        {
            if (prefix.Length > source.Length)
                return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (source[i] != prefix[i])
                    return false;
            }

            return true;
        }
        public static bool EqualsIgnoreCase(string str1, string str2)
        {
            if (str1.Length != str2.Length)
                return false;

            for (int i = 0; i < str1.Length; i++)
            {
                if (char.ToLowerInvariant(str1[i]) != char.ToLowerInvariant(str2[i]))
                    return false;
            }

            return true;
        }
    }
}
