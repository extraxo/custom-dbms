using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public static class StreamMain
    {
        public static StreamReadLayer ExpectStream(Stream stream, long length)
        {
            return new StreamReadLayer(stream, length);
        }

        public static void Write(Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public static async Task<int> Read(Stream source, byte[] buffer)
        {
            var filled = 0;
            var lastRead = 0;
            while (filled < buffer.Length)
            {
                lastRead = source.Read(buffer, filled, buffer.Length - filled);
                if (lastRead == 0)
                {
                    break;
                }
            }
            return filled;
        }

        public static float ExpectFloat(this Stream target)
        {
            var buff = new byte[4];
            if (target.Read(buff) == 4)
            {
                return LittleEndianByteOrder.GetSingle(buff);
            }
            else
            {
                throw new EndOfStreamException();
            }
        }
        public static int ExpectInt32(this Stream target)
        {
            var buff = new byte[4];
            if (target.Read(buff) == 4)
            {
                return LittleEndianByteOrder.GetInt32(buff);
            }
            else
            {
                throw new EndOfStreamException();
            }
        }
        public static uint ExpectUInt32(this Stream target)
        {
            var buff = new byte[4];
            if (target.Read(buff) == 4)
            {
                return LittleEndianByteOrder.GetUInt32(buff);
            }
            else
            {
                throw new EndOfStreamException();
            }
        }
        public static long ExpectInt64(this Stream target)
        {
            var buff = new byte[8];
            if (target.Read(buff) == 8)
            {
                return LittleEndianByteOrder.GetInt64(buff);
            }
            else
            {
                throw new EndOfStreamException();
            }
        }

        public static double ExpectDouble(this Stream target)
        {
            var buff = new byte[8];
            if (target.Read(buff) == 8)
            {
                return LittleEndianByteOrder.GetDouble(buff);
            }
            else
            {
                throw new EndOfStreamException();
            }
        }
        public static bool ExpectBool(this Stream target)
        {
            return Convert.ToBoolean(target.ReadByte());
        }
        public static Guid ExpectGuid(this Stream target)
        {
            var buff = new byte[16];
            if (target.Read(buff) == 16)
            {
                return new Guid(buff);
            }
            else
            {
                throw new EndOfStreamException();
            }
        }
        public static async Task CopyTo(Stream source, Stream destination, int bufSize = 4096, Func<long,bool> feedback = null, long maxLength = 0)
        {
            var buffer = new byte[bufSize];
            var totalRead = 0L;

            while (totalRead < maxLength)
            {
                var bytesToRead = (int)Math.Min(maxLength -  totalRead, buffer.Length);
                var Read = source.Read(buffer, 0, bytesToRead);

                if(Read == 0)
                {
                    throw new EndOfStreamException();
                }

                totalRead += Read;

                destination.Write(buffer, 0, Read);
                destination.Flush();
            }
        }
    }
}
