using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public interface IRecordStorage
    {
        void Update(uint recordId, byte[] data);
        byte[] Find(uint recordId);
        uint Create();
        uint Create(byte[] data);
        uint Create(Func<uint, byte[]> dataGenerator);
        void Delete(uint recordId);
    }
}
