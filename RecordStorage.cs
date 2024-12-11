using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class RecordStorage : IRecordStorage
    {
        readonly IBlockStorage _blockStorage;

        const int _maxRecordSize = 4194304;
        const int _nextBlockId = 0;
        const int _recordLength = 1;
        const int _contentLength = 2;
        const int _previousBlockId = 3;
        const int _isDeleted = 4;

        public RecordStorage(IBlockStorage blockStorage)
        {
            _blockStorage = blockStorage;
        }

        public virtual byte[] Find(uint recordId)
        {
            using (var block = _blockStorage.Find(recordId))
            {
                if (block == null)
                {
                    return null;
                }

                if (1L == block.GetHeader(_isDeleted))
                {
                    return null;
                }

                if (0L == block.GetHeader(_previousBlockId))
                {
                    return null;
                }

                var _totalSize = block.GetHeader(_recordLength);

                var data = new byte[_totalSize];
                var bytesRead = 0;

                IBlock _currentBlock = block;
                while (true)
                {
                    int _nextBlock;

                    using (_currentBlock)
                    {
                        var _blockContentLength = _currentBlock.GetHeader(_contentLength);
                        _currentBlock.Read(data, bytesRead, 0, (int)_blockContentLength);

                        bytesRead += (int)_blockContentLength;

                        _nextBlock = (int)_currentBlock.GetHeader(_nextBlockId);

                        if (_nextBlock == 0)
                        {
                            return data;
                        }
                    }

                    _currentBlock = _blockStorage.Find(_nextBlockId);
                    if (_currentBlock == null)
                    {
                        throw new InvalidDataException("Block not found by ID: " + _nextBlockId);
                    }
                }
            }
        }
        public virtual uint Create(Func<uint, byte[]> dataCreator)
        {
            using (var _firstBlock = AllocateBlock())
            {
                var _returnId = _firstBlock.Id;

                var _data = dataCreator(_returnId);
                var _dataCreated = 0;
                var _notCreatedData = _data.Length;
                _firstBlock.SetHeader(_recordLength, _notCreatedData);

                if (_notCreatedData == 0)
                {
                    return _returnId;
                }

                IBlock _currentBlock = _firstBlock;
                while (_dataCreated < _notCreatedData)
                {
                    IBlock _nextBlock = null;

                    using (_currentBlock)
                    {
                        var _writeToBlock = (int)Math.Min(_blockStorage.BlockContentSize, _notCreatedData - _dataCreated);
                        _currentBlock.Write(_data, _dataCreated, 0, _writeToBlock);
                        _currentBlock.SetHeader(_contentLength, (long)_writeToBlock);
                        _dataCreated += _writeToBlock;

                        if (_dataCreated < _notCreatedData)
                        {
                            _nextBlock = AllocateBlock();
                            var _success = false;

                            try
                            {
                                _nextBlock.SetHeader(_previousBlockId, _currentBlock.Id);
                                _currentBlock.SetHeader(_nextBlockId, _nextBlock.Id);
                                _success = true;
                            }
                            finally
                            {
                                if (_success == false && _nextBlock != null)
                                {
                                    _nextBlock.Dispose();
                                    _nextBlock = null;
                                }

                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (_nextBlock != null)
                    {
                        _currentBlock = _nextBlock;
                    }
                }
                return _returnId;
            }
        }
        public virtual uint Create(byte[] data)
        {
            return Create(_recordId => data);
        }

        public virtual uint Create()
        {
            using (var _firstBlock = AllocateBlock())
            {
                return _firstBlock.Id;
            }
        }
        public virtual void Update(uint recordId, byte[] data)
        {
            var _written = 0;
            var _totalData = data.Length;
            var _blocks = FindBlocks(recordId);
            var _blocksUsed = 0;
            var _previousBlock = (IBlock)null;

            try
            {
                while (_written < _totalData)
                {
                    var _bytesToWrite = Math.Min(_totalData - _written, _blockStorage.BlockContentSize);

                    var _blockIndex = (int)Math.Floor((double)_written / (double)_blockStorage.BlockContentSize);

                    var _targetBlock = (IBlock)null;

                    if(_blockIndex < _blocks.Count)
                    {
                        _targetBlock = _blocks[_blockIndex];
                    }
                    else
                    {
                        _targetBlock = AllocateBlock();
                        if(_targetBlock == null)
                        {
                            throw new Exception("Failed to allocate new block");
                        }
                        _blocks.Add(_targetBlock);
                    }
                    if(_previousBlock != null)
                    {
                        _previousBlock.SetHeader(_nextBlockId, _targetBlock.Id);
                        _targetBlock.SetHeader(_previousBlockId, _previousBlock.Id);
                    }

                    _targetBlock.Write(data, _written, 0, _bytesToWrite);
                    _targetBlock.SetHeader(_contentLength, _bytesToWrite);
                    _targetBlock.SetHeader(_nextBlockId, 0);
                    if(_written == 0)
                    {
                        _targetBlock.SetHeader(_recordLength, _totalData);
                    }

                    _blocksUsed++;
                    _written += _bytesToWrite;
                    _previousBlock = _targetBlock;
                }

                if(_blocksUsed < _blocks.Count)
                {
                    for(var i = _blocksUsed; i < _blocks.Count; i++)
                    {
                        FreeBlock(_blocks[i].Id);
                    }
                }
            }
            finally
            {
                foreach(var block in _blocks)
                {
                    block.Dispose();
                }
            }

        } 

        public virtual void Delete(uint recordId)
        {
            using (var block = _blockStorage.Find(recordId))
            {
                IBlock _currentBlock = block;
                while (true)
                {
                    IBlock _nextBlock = null;

                    using (_currentBlock)
                    {
                        FreeBlock(_currentBlock.Id);
                        _currentBlock.SetHeader(_isDeleted, 1L);

                        var nextBlockId = (int)_currentBlock.GetHeader(_nextBlockId);
                        if(nextBlockId < 0)
                        {
                            break;
                        }
                        else
                        {
                            _nextBlock = _blockStorage.Find(_nextBlockId);
                            if(_currentBlock == null)
                            {
                                throw new InvalidDataException("Block not found by id: " + nextBlockId);
                            }
                        }
                    }
                    if(_nextBlock != null)
                    {
                        _currentBlock = _nextBlock;
                    }
                }
            }
        }

        CustomList<IBlock> FindBlocks(uint recordId)
        {
            var _blocks = new CustomList<IBlock>();
            var _success = false;

            try
            {
                var _currentBlockId = recordId;

                do
                {
                    var _block = _blockStorage.Find(_currentBlockId);
                    if (_block == null)
                    {
                        if (_currentBlockId == 0)
                        {
                            _block = _blockStorage.CreateNew();
                        }
                        else
                        {
                            throw new Exception("Block not found by ID: " + _currentBlockId);
                        }
                    }
                    _blocks.Add(_block);

                    if (1L == _block.GetHeader(_isDeleted))
                    {
                        throw new InvalidDataException("Block not found " + _currentBlockId);
                    }

                    _currentBlockId = (uint)_block.GetHeader(_nextBlockId);
                } while (_currentBlockId != 0);

                _success = true;
                return _blocks;
            }
            finally
            {
                if (false == _success)
                {
                    foreach (var _block in _blocks)
                    {
                        _block.Dispose();
                    }
                }
            }
        }
        IBlock AllocateBlock()
        {
            uint _reusableBlockId;
            IBlock _newBlock;

            if (FindFreeBlock(out _reusableBlockId))
            {
                _newBlock = _blockStorage.CreateNew();
                if (_newBlock == null)
                {
                    throw new Exception("Failed to create a new block");
                }
            }
            else
            {
                _newBlock = _blockStorage.Find(_reusableBlockId);
                if (_newBlock == null)
                {
                    throw new InvalidDataException("Block not found by ID: " + _reusableBlockId);
                }

                _newBlock.SetHeader(_contentLength, 0L);
                _newBlock.SetHeader(_nextBlockId, 0L);
                _newBlock.SetHeader(_previousBlockId, 0L);
                _newBlock.SetHeader(_recordLength, 0L);
                _newBlock.SetHeader(_isDeleted, 0L);
            }
            return _newBlock;
        }

        bool FindFreeBlock(out uint _blockId)
        {
            _blockId = 0;
            IBlock _lastBlock, _secondLastBlock;
            SpaceTracker(out _lastBlock, out _secondLastBlock);

            using (_lastBlock)
            using (_secondLastBlock)
            {
                var _currentContentLength = _lastBlock.GetHeader(_contentLength);
                if (_currentContentLength == 0)
                {
                    if (_secondLastBlock == null)
                    {
                        return false;
                    }

                    _blockId = ReadUint32FromContent(_secondLastBlock);
                    _secondLastBlock.SetHeader(_contentLength, _secondLastBlock.GetHeader(_contentLength));
                    AppendUint32ToContent(_secondLastBlock, _lastBlock.Id);

                    _secondLastBlock.SetHeader(_contentLength, _secondLastBlock.GetHeader(_contentLength) + 4);
                    _secondLastBlock.SetHeader(_nextBlockId, 0);
                    _lastBlock.SetHeader(_previousBlockId, 0);

                    return true;


                }
                else
                {
                    _blockId = ReadUint32FromContent(_lastBlock);
                    _lastBlock.SetHeader(_contentLength, _currentContentLength);

                    return true;
                }
            }
        }
        uint ReadUint32FromContent(IBlock block)
        {
            var _buffer = new byte[4];
            var contentLength = block.GetHeader(_contentLength);

            block.Read(_buffer, 0, (int)contentLength - 4, 4);

            return LittleEndianByteOrder.GetUInt32(_buffer);
        }
        void AppendUint32ToContent(IBlock block, uint value)
        {
            var contentLength = block.GetHeader(_contentLength);

            if (contentLength % 4 != 0)
            {
                throw new DataMisalignedException("Block content length not %4: " + contentLength);

            }

            block.Write(LittleEndianByteOrder.GetBytes(value), 0, (int)_contentLength, 4);
        }
        void FreeBlock(uint blockId)
        {
            IBlock _lastBlock = null;
            IBlock _secondLastBlock = null;
            IBlock _targetBlock = null;

            SpaceTracker(out _lastBlock, out _secondLastBlock);

            using (_lastBlock)
            using (_secondLastBlock)
            {
                try
                {
                    var contentLength = _lastBlock.GetHeader(_contentLength);
                    if ((contentLength + 4) <= _blockStorage.BlockContentSize)
                    {
                        _targetBlock = _lastBlock;
                    }

                    else
                    {
                        _targetBlock = _blockStorage.CreateNew();
                        _targetBlock.SetHeader(_previousBlockId, _lastBlock.Id);

                        _lastBlock.SetHeader(_nextBlockId, _targetBlock.Id);
                        contentLength = 0;
                    }

                    _targetBlock.SetHeader(_contentLength, _contentLength + 4);
                }
                finally
                {
                    if (_targetBlock != null)
                    {
                        _targetBlock.Dispose();
                    }
                }
            }

        }
        void SpaceTracker(out IBlock lastBlock, out IBlock secondLastBlock)
        {
            lastBlock = null;
            secondLastBlock = null;

            var _blocks = FindBlocks(0);

            try
            {
                lastBlock = _blocks[_blocks.Count - 1];
                if (_blocks.Count > 1)
                {
                    secondLastBlock = _blocks[_blocks.Count - 2];
                }
            }
            finally
            {
                if (_blocks != null)
                {
                    foreach (var block in _blocks)
                    {
                        if ((lastBlock == null || block != lastBlock && secondLastBlock == null || block != secondLastBlock))
                        {
                            block.Dispose();
                        }
                    }
                }
            }
        }
    }
}
