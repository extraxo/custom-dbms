using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class StringIntSerializer : ISerializer<CustomTuple<string, int>>
    {
        public byte[] Serialize(CustomTuple<string, int> value)
        {
            var stringBytes = System.Text.Encoding.UTF8.GetBytes(value.Item1);

            var data = new byte[
                4 +                    
                stringBytes.Length +   
                4                      
            ];

            BufferReader.WriteBuffer((int)stringBytes.Length, data, 0);
            Buffer.BlockCopy(src: stringBytes, srcOffset: 0, dst: data, dstOffset: 4, count: stringBytes.Length);
            BufferReader.WriteBuffer((int)value.Item2, data, 4 + stringBytes.Length);
            return data;
        }

        public CustomTuple<string, int> Deserialize(byte[] buffer, int offset, int length)
        {
            var stringLength = BufferReader.ReadBufferInt32(buffer, offset);
            if (stringLength < 0 || stringLength > (16 * 1024))
            {
                throw new Exception("Invalid string length: " + stringLength);
            }
            var stringValue = System.Text.Encoding.UTF8.GetString(buffer, offset + 4, stringLength);
            var integerValue = BufferReader.ReadBufferInt32(buffer, offset + 4 + stringLength);
            return new CustomTuple<string, int>(stringValue, integerValue);
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
