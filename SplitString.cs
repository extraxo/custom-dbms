using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class CustomSplit
    {
        public static CustomList<string> SplitString(string input, char separator)
        {
            var result = new CustomList<string>();
            var current = new StringBuilder();

            foreach (var c in input)
            {
                if (c == separator)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                }
                else 
                {
                    current.Append(c);
                }
            }
            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }

            return result;
        }
    }
}
