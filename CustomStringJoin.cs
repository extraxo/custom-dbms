using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class CustomStringJoin
    {
        public static string Join(string delimiter, IEnumerable<string> strings)
        {
            StringBuilder result = new StringBuilder();
            bool isFirst = true;

            foreach (var str in strings)
            {
                if (!isFirst)
                {
                    result.Append(delimiter);
                }

                result.Append(str);
                isFirst = false;
            }

            return result.ToString();
        }
    }

}
