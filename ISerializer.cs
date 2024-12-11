using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public interface ISerializer<Key>
    {
        byte[] Serialize(Key value);

        Key Deserialize(byte[] buffer, int offset, int length);

        bool IsFixedSize
        {
            get;
        }

        int Length
        {
            get;
        }
    }
}
