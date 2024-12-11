using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class TreeIntSerializer: ISerializer<int>
    {
        public byte[] Serialize(int value)
        {
            return LittleEndianByteOrder.GetBytes(value);
        }
        public int Deserialize(byte[] buffer, int offset, int length)
        {
            if(length != 4)
            {
                throw new ArgumentException("Invalid length: " + length);
            }
            return BufferReader.ReadBufferInt32(buffer, offset);
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
                return 4;
            }
        }

        public class TreeUIntSerializer : ISerializer<uint>
        {
            public byte[] Serialize(uint value)
            {
                return LittleEndianByteOrder.GetBytes(value);
            }

            public uint Deserialize(byte[] buffer, int offset, int length)
            {
                if (length != 4)
                {
                    throw new ArgumentException("Invalid length: " + length);
                }

                return BufferReader.ReadBufferUInt32(buffer, offset);
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
                    return 4;
                }
            }
        }
    }
}
