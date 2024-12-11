using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public static class CustomIndexOf
    {
        public static int IndexOf(string input, char searchChar)
        {
            if (string.IsNullOrEmpty(input))
            {
                return -1;
            }

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == searchChar)
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

    }
}
