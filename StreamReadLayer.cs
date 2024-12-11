using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class StreamReadLayer : Stream
    {
        readonly Stream _stream;
        long _position = 0;
        long _readLimit;

        public override long Position
        {
            get
            {
                return _position;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                return _readLimit;
            }
        }

        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public StreamReadLayer(Stream stream, long readLimit)
        {
            _stream = stream;
            _readLimit = readLimit;
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if((_readLimit - _position) == 0)
            {
                return 0;
            }

            var read = _stream.Read(buffer, offset, (int)Math.Min(count, _readLimit - _position));
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) 
        { 
            throw new NotImplementedException(); 
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
