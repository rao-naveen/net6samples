// System.Net.DelegatedStream
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace S3StreamUnzip
{

    public class StreamWrapper2 : Stream
    {
        private readonly Stream stream;
        private readonly long length;
        private long position;

        public StreamWrapper2(Stream stream, long length) 
        {
            this.stream = stream;
            this.length = length;
        }
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position { get => position; set => throw new NotImplementedException(); }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = stream.Read(buffer, offset, count);
            position+= bytesRead;
            return bytesRead;
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
