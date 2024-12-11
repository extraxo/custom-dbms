using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeLongSerializer : ISerializer<long>
    {
        public byte[] Serialize(long value)
        {
            return LittleEndianByteOrder.GetBytes(value);
        }

        public long Deserialize(byte[] buffer, int offset, int length)
        {
            if (length != 8)
            {
                throw new ArgumentException("Invalid length: " + length);
            }

            return BufferReader.ReadBufferInt64(buffer, offset);
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
                return 8;
            }
        }
    }
}
