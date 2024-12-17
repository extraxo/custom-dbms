using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeStringSerializer : ISerializer<string>
    {
        public byte[] Serialize(string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        public string Deserialize(byte[] buffer, int offset, int length)
        {
            return System.Text.Encoding.UTF8.GetString(buffer, offset, length);
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public int Length
        {
            get
            {
                throw new InvalidOperationException();
            }
        }
    }
}
