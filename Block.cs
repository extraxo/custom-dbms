namespace KursovaSAAConsole2
{
    public class Block : IBlock
    {
        readonly byte[] _firstBlock;
        readonly long[] _headerValue = new long[5];
        readonly BlockStorage _storage;
        readonly Stream _stream;
        readonly uint _id;

        public event EventHandler Disposed;

        bool isDisposed = false;
        bool isFirstBlockFull = false;

        public uint Id
        {
            get
            {
                return _id;
            }
        }

        public Block(BlockStorage storage, Stream stream, byte[] firstBlock, uint id)
        {
            _storage = storage;
            _stream = stream;
            _firstBlock = firstBlock;
            _id = id;
        }

        public long GetHeader(int area)
        {
            if (area < _headerValue.Length)
            {
                if (_headerValue[area] == 0)
                {
                    _headerValue[area] = BufferReader.ReadBufferInt64(_firstBlock, area * 8);
                }
                return _headerValue[area];
            }
            else
            {
                return BufferReader.ReadBufferInt64(_firstBlock, area * 8);
            }
        }

        public void SetHeader(int area, long value)
        {
            if (area < _headerValue.Length)
            {
                _headerValue[area] = value;
            }

            BufferReader.WriteBuffer(value, _firstBlock, area * 8);
            isFirstBlockFull = true;
        }

        public void Write(byte[] source, int offset, int destOffset, int count)
        {
            if ((_storage.BlockHeaderSize + destOffset) < _storage.DiskSector)
            {
                var write = Math.Min(count, _storage.DiskSector - _storage.BlockHeaderSize - destOffset);
                Buffer.BlockCopy(src: source, srcOffset: offset, dst: _firstBlock, dstOffset: _storage.BlockHeaderSize + destOffset, count: write);
                isFirstBlockFull = true;
            }

            if ((_storage.BlockHeaderSize + destOffset + count) < _storage.DiskSector)
            {
                _stream.Position = (Id * _storage.BlockSize) + Math.Max(_storage.DiskSector, _storage.BlockHeaderSize - destOffset);

                var d = _storage.DiskSector - (_storage.BlockHeaderSize + destOffset);
                if (d > 0)
                {
                    destOffset += d;
                    offset += d;
                    count -= d;
                }

                var written = 0;
                while (written < count)
                {
                    var bytesToWrite = (int)Math.Min(4096, count - written);
                    _stream.Write(source, offset, bytesToWrite);
                    _stream.Flush();
                    written += bytesToWrite;
                }
            }
        }

        public void Read(byte[] dest, int offset, int srcOffset, int count)
        {
            var dataCopied = 0;
            var copyFromFirstSector = (_storage.BlockHeaderSize + srcOffset) < _storage.DiskSector;
            if (copyFromFirstSector)
            {
                var tobeCopied = Math.Min(_storage.DiskSector - _storage.BlockHeaderSize - srcOffset, count);

                Buffer.BlockCopy(src: _firstBlock, srcOffset: _storage.BlockHeaderSize + srcOffset, dst: dest, dstOffset: offset, count: tobeCopied);

                dataCopied += tobeCopied;
            }

        }
        
        protected virtual void OnDisposed(EventArgs e)
        {
            if (Disposed != null)
            {
                Disposed(this, e);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                isDisposed = true;

                if (isFirstBlockFull)
                {
                    _stream.Position = (Id * _storage.BlockSize);
                    _stream.Write(_firstBlock,0, _storage.BlockSize);
                    _stream.Flush();
                    isFirstBlockFull = false;
                }
                OnDisposed(EventArgs.Empty);
            }
        }
    }
}
