using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public interface IBlockStorage
    {
        int BlockContentSize
        {
            get;
        }
        int BlockHeaderSize
        {
            get;
        }
        int BlockSize
        {
            get;
        }
        IBlock Find(uint blockId);
        IBlock CreateNew();
    }
}
