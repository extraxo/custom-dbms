using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class BlockStorage : IBlockStorage
    {
        readonly Stream _stream;
        readonly int _blockSize;
        readonly int _blockContentSize;
        readonly int _blockHeaderSize;
        readonly int _unitOfWork;

        readonly CustomDictionary<uint, Block> blocks = new CustomDictionary<uint, Block>();

        public int DiskSector
        {
            get
            {
                return _unitOfWork;
            }
        }

        public int BlockSize
        {
            get
            {
                return _blockSize;
            }
        }

        public int BlockHeaderSize
        {
            get
            {
                return _blockHeaderSize;
            }
        }

        public int BlockContentSize
        {
            get
            {
                return _blockContentSize;
            }
        }

        public BlockStorage(Stream storage, int blockSize = 40960, int blockHeaderSize = 48)
        {
            if (blockSize >= 4096)
            {
                _unitOfWork = 4096;
            }
            else
            {
                _unitOfWork = 128;
            }

            _blockSize = blockSize;
            _blockContentSize = blockHeaderSize;
            _blockHeaderSize = blockHeaderSize;
            _stream = storage;
        }

        public IBlock Find(uint blockId)
        {
            if (true == blocks.ContainsKey(blockId))
            {
                return blocks[blockId];
            }

            var blockPosition = blockId * BlockSize;
            if ((blockPosition + BlockSize) > _stream.Length)
            {
                return null;
            }

            var firstBlock = new byte[DiskSector];
            
            _stream.Position = blockId * BlockSize;
            _stream.Read(firstBlock, 0, DiskSector);

            var block = new Block(this, _stream, firstBlock, blockId);
            
            OnBlockInitialized(block);
            
            return block;
        }

        public IBlock CreateNew()
        {
            var blockId = (uint)Math.Ceiling(_stream.Length / (double)_blockSize);

            _stream.SetLength((blockId * _blockSize) + _blockSize);
            _stream.Flush();

            var firstBlock = new byte[DiskSector];  
            var block = new Block(this, _stream, firstBlock , blockId);
            OnBlockInitialized(block);
            return block;
            
        }

        protected void OnBlockInitialized(Block block)
        {
            blocks[block.Id] = block;

            block.Disposed += HandleBlockDisposed;
        }

        protected virtual void HandleBlockDisposed(object sender, EventArgs e)
        {
            var block = (Block)sender;
            block.Disposed -= HandleBlockDisposed;

            blocks.Remove(block.Id);
        }
    }
}
