using System;
using System.IO;

namespace Server.Core
{
    public class DisposableStreamWriter : IDisposable
    {
        public readonly Stream _stream;
        public readonly StreamWriter _writer;
        private bool _disposed;

        public DisposableStreamWriter(string path, bool append)
        {
            _stream = new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_stream);
        }

        public void WriteLine(string line)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DisposableStreamWriter));
            _writer.WriteLine(line);
            _writer.Flush();
        }

        public void Dispose(bool disposing) 
        {
            if (_disposed) return;
            if (disposing) { _writer?.Flush(); _writer?.Dispose(); _stream?.Dispose(); }
            _disposed = true;
        }

        DisposableStreamWriter() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    }
}
