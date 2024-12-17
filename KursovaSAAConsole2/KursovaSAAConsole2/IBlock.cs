using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public interface IBlock : IDisposable
    {
        uint Id
        {
            get;
        }
        long GetHeader(int field);
        void SetHeader(int field, long value);
        void Read(byte[] dst, int dstOffset, int srcOffset, int count);
        void Write(byte[] src, int srcOffset, int dstOffset, int count);
    }
}
