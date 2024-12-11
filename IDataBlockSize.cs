using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public interface IDataBlockSize : IDisposable
    {
        uint Id
        {
            get;
        }

        long GetHeader(int field);

        void SetHeader(int field, long value);

        void Read(byte[] dest, int dstOffset, int srcOffset, int count);

        void Write(byte[] source, int srcOffset, int destOffset, int count);
    }
}
