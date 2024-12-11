using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class DataBlockSize : IDataBlockSize
    {
        readonly byte[] _data;
        readonly long[] _cachedValue;
        readonly Stream _dataStream;
        readonly BlockStorage _storage;
        readonly uint _id;

        bool isSectorFull = false;
        bool isDisposed = false;

        public event EventHandler Disposed;

        public uint Id
        {
            get
            {
                return _id;
            }
        }

        public DataBlockSize(BlockStorage storage, uint id, byte[] data, Stream stream)
        {
            _storage = storage;
            _id = id;
            _data = data;
            _dataStream = stream;
        }

        public long GetHeader(int field)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("Block");
            }

            if (field < 0)
            {
                throw new IndexOutOfRangeException();
            }
            if (field >= (_storage.BlockHeaderSize / 8))
            {
                throw new ArgumentException("Invalid field: " + field);
            }

            if (field < _cachedValue.Length)
            {
                if (_cachedValue[field] == null)
                {
                    _cachedValue[field] = BufferReader.ReadBufferInt64(_data, field * 8);
                }
                return _cachedValue[field];
            }
            else
            {
                return BufferReader.ReadBufferInt64(_data, field * 8);
            }


        }
        public void SetHeader(int field, long value)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("Block");
            }

            if (field < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (field < _cachedValue.Length)
            {
                _cachedValue[field] = value;
            }

            BufferReader.WriteBuffer((long)value, _data, field * 8);
            isSectorFull = true;
        }

        public void Read(byte[] destination, int destOffset, int srcOffset, int count)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("Block");
            }

            if (false == ((count >= 0) && ((count + srcOffset) <= _storage.BlockContentSize)))
            {
                throw new ArgumentOutOfRangeException("Requested count is outside of src bounds: Count=" + count, "count");
            }

            if (false == ((count + destOffset) <= destination.Length))
            {
                throw new ArgumentOutOfRangeException("Requested count is outside of dest bounds: Count=" + count);
            }


            var dataCopied = 0;
            var copyFromFirstSector = (_storage.BlockHeaderSize + srcOffset) < _storage.DiskSector;
            if (copyFromFirstSector)
            {
                var tobeCopied = Math.Min(_storage.DiskSector - _storage.BlockHeaderSize - srcOffset, count);

                Buffer.BlockCopy(src: _data
                    , srcOffset: _storage.BlockHeaderSize + srcOffset
                    , dst: destination
                    , dstOffset: destOffset
                    , count: tobeCopied);

                dataCopied += tobeCopied;
            }

            if (dataCopied < count)
            {
                if (copyFromFirstSector)
                {
                    _dataStream.Position = (Id * _storage.BlockSize) + _storage.DiskSector;
                }
                else
                {
                    _dataStream.Position = (Id * _storage.BlockSize) + _storage.BlockHeaderSize + srcOffset;
                }
            }

            while (dataCopied < count)
            {
                var bytesToRead = Math.Min(_storage.DiskSector, count - dataCopied);
                var thisRead = _dataStream.Read(destination, destOffset + dataCopied, bytesToRead);
                if (thisRead == 0)
                {
                    throw new EndOfStreamException();
                }
                dataCopied += thisRead;
            }
        }

        public void Write(byte[] source, int srcOffset, int destOffset, int count)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("Block");
            }

            if (false == ((destOffset >= 0) && ((destOffset + count) <= _storage.BlockContentSize)))
            {
                throw new ArgumentOutOfRangeException("Count argument is outside of dest bounds: Count=" + count
                    , "count");
            }

            if (false == ((srcOffset >= 0) && ((srcOffset + count) <= source.Length)))
            {
                throw new ArgumentOutOfRangeException("Count argument is outside of src bounds: Count=" + count
                    , "count");
            }

            if ((_storage.BlockHeaderSize + destOffset) < _storage.DiskSector)
            {
                var thisWrite = Math.Min(count, _storage.DiskSector - _storage.BlockHeaderSize - destOffset);
                Buffer.BlockCopy(src: source
                    , srcOffset: srcOffset
                    , dst: _data
                    , dstOffset: _storage.BlockHeaderSize + destOffset
                    , count: thisWrite);
                isSectorFull = true;
            }
            if ((_storage.BlockHeaderSize + destOffset + count) > _storage.DiskSector)
            {
                _dataStream.Position = (Id * _storage.BlockSize)
                + Math.Max(_storage.DiskSector, _storage.BlockHeaderSize + destOffset);

                var d = _storage.DiskSector - (_storage.BlockHeaderSize + destOffset);
                if (d > 0)
                {
                    destOffset += d;
                    srcOffset += d;
                    count -= d;
                }

                var written = 0;
                while (written < count)
                {
                    var bytesToWrite = (int)Math.Min(4096, count - written);
                    _dataStream.Write(source, srcOffset + written, bytesToWrite);
                    _dataStream.Flush();
                    written += bytesToWrite;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("[Block: Id={0}, ContentLength={1}, Prev={2}, Next={3}]"
                , Id
                , GetHeader(2)
                , GetHeader(3)
                , GetHeader(0));
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

                if (isSectorFull)
                {
                    _dataStream.Position = (Id * _storage.BlockSize);
                    _dataStream.Write(_data, 0, 4096);
                    _dataStream.Flush();
                    isSectorFull = false;
                }

                OnDisposed(EventArgs.Empty);
            }
        }

    }
}
