using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class GuidSerializer : ISerializer<Guid>
    {
        public byte[] Serialize(Guid value)
        {
            return value.ToByteArray();
        }

        public Guid Deserialize(byte[] buffer, int offset, int length)
        {
            if (length != 16)
            {
                throw new ArgumentException("length");
            }

            return BufferReader.ReadBufferGuid(buffer, offset);
        }

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }
        public int Length
        {
            get
            {
                return 16;
            }
        }
    }
}
