// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; version 3 of the License.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License
// for more details.
//
// You should have received a copy of the GNU Lesser General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MariaDB.Data.MySqlClient;
using System.IO.MemoryMappedFiles;
using System.IO;

namespace MariaDB.Data.Common
{
    /// <summary>
    /// Helper class to encapsulate shared memory functionality
    /// Also cares of proper cleanup of file mapping object and cew
    /// </summary>
    internal class SharedMemory : IDisposable
    {
        private MemoryMappedFile mmf;
        private MemoryMappedViewStream mmvs;
        private MemoryMappedViewAccessor mmva;

        public SharedMemory(string name, IntPtr size)
        {
            mmf = MemoryMappedFile.OpenExisting(name + "_CONNECT_DATA");

            mmva = mmf.CreateViewAccessor();
            if (mmf.SafeMemoryMappedFileHandle.IsInvalid)
                throw new MySqlException("Cannot open file mapping " + name);
            mmvs = mmf.CreateViewStream(0L, size.ToInt64());
            mmva = mmf.CreateViewAccessor(0L, size.ToInt64());
        }

        public MemoryMappedViewStream ViewStream
        {
            get { return mmvs; }
        }
        public MemoryMappedViewAccessor ViewAccessor
        {
            get { return mmva; }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mmva != null && !mmva.SafeMemoryMappedViewHandle.IsInvalid && !mmva.SafeMemoryMappedViewHandle.IsClosed)
                {
                    mmva.SafeMemoryMappedViewHandle.ReleasePointer();
                    mmva.SafeMemoryMappedViewHandle.SetHandleAsInvalid();
                    mmva.Dispose();
                }
                if (mmvs != null && !mmvs.SafeMemoryMappedViewHandle.IsInvalid && !mmvs.SafeMemoryMappedViewHandle.IsClosed)
                {
                    mmvs.SafeMemoryMappedViewHandle.ReleasePointer();
                    mmvs.SafeMemoryMappedViewHandle.SetHandleAsInvalid();
                    mmvs.Dispose();
                }
                if (mmf != null && !mmf.SafeMemoryMappedFileHandle.IsInvalid && !mmf.SafeMemoryMappedFileHandle.IsClosed)
                {
                    mmf.SafeMemoryMappedFileHandle.SetHandleAsInvalid();
                    mmf.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Summary description for SharedMemoryStream.
    /// </summary>
    internal class SharedMemoryStream : Stream
    {
        private string memoryName;
        private EventWaitHandle serverRead;
        private EventWaitHandle serverWrote;
        private EventWaitHandle clientRead;
        private EventWaitHandle clientWrote;
        private EventWaitHandle connectionClosed;
        private SharedMemory data;
        private int bytesLeft;
        private int position;
        private int connectNumber;

        private const int BUFFERLENGTH = 16004;

        private int readTimeout = System.Threading.Timeout.Infinite;
        private int writeTimeout = System.Threading.Timeout.Infinite;

        public SharedMemoryStream(string memName)
        {
            memoryName = memName;
        }

        public void Open(uint timeOut)
        {
            if (connectionClosed != null)
            {
                Debug.Assert(false, "Connection is already open");
            }
            GetConnectNumber(timeOut);
            SetupEvents();
        }

        public void Close()
        {
            if (connectionClosed != null)
            {
                bool isClosed = connectionClosed.WaitOne(0);
                if (!isClosed)
                {
                    connectionClosed.Set();
                    connectionClosed.Dispose();
                }
                connectionClosed = null;
                EventWaitHandle[] handles =
                {serverRead, serverWrote, clientRead, clientWrote};

                for (int i = 0; i < handles.Length; i++)
                {
                    if (handles[i] != null)
                        handles[i].Dispose();
                }
                if (data != null)
                {
                    data.Dispose();
                    data = null;
                }
            }
        }

        private void GetConnectNumber(uint timeOut)
        {
            EventWaitHandle connectRequest;
            try
            {
                connectRequest =
                    EventWaitHandle.OpenExisting(memoryName + "_CONNECT_REQUEST");
            }
            catch (Exception)
            {
                // If server runs as service, its shared memory is global
                // And if connector runs in user session, it needs to prefix
                // shared memory name with "Global\"
                string prefixedMemoryName = @"Global\" + memoryName;
                connectRequest =
                    EventWaitHandle.OpenExisting(prefixedMemoryName + "_CONNECT_REQUEST");
                memoryName = prefixedMemoryName;
            }
            EventWaitHandle connectAnswer =
               EventWaitHandle.OpenExisting(memoryName + "_CONNECT_ANSWER");
            using (SharedMemory connectData =
                new SharedMemory(memoryName + "_CONNECT_DATA", (IntPtr)4))
            {
                // now start the connection
                if (!connectRequest.Set())
                    throw new MySqlException("Failed to open shared memory connection");
                if (!connectAnswer.WaitOne((int)(timeOut * 1000)))
                    throw new MySqlException("Timeout during connection");
                connectNumber = connectData.ViewAccessor.ReadInt32(0);
            }
        }

        private void SetupEvents()
        {
            string prefix = memoryName + "_" + connectNumber;
            data = new SharedMemory(prefix + "_DATA", (IntPtr)BUFFERLENGTH);
            serverWrote = EventWaitHandle.OpenExisting(prefix + "_SERVER_WROTE");
            serverRead = EventWaitHandle.OpenExisting(prefix + "_SERVER_READ");
            clientWrote = EventWaitHandle.OpenExisting(prefix + "_CLIENT_WROTE");
            clientRead = EventWaitHandle.OpenExisting(prefix + "_CLIENT_READ");
            connectionClosed = EventWaitHandle.OpenExisting(prefix + "_CONNECTION_CLOSED");

            // tell the server we are ready
            serverRead.Set();
        }

        public override bool CanRead
        {
            get { return data.ViewStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return data.ViewStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return data.ViewStream.CanWrite; }
        }

        public override long Length
        {
            get { return data.ViewStream.Length; }
        }

        public override long Position
        {
            get { return data.ViewStream.Position; }
            set { data.ViewStream.Position = value; }
        }

        public override void Flush()
        {
            // No need to flush anything to disk ,as our shared memory is backed
            // by the page file
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return data.ViewStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return data.ViewStream.Seek(offset, origin);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            data.ViewStream.Write(buffer, offset, count);
        }

        public override void SetLength(long value)
        {
            data.ViewStream.SetLength(value);
        }

        public override bool CanTimeout
        {
            get { return data.ViewStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return data.ViewStream.ReadTimeout; }
            set { data.ViewStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return data.ViewStream.WriteTimeout; }
            set { data.ViewStream.WriteTimeout = value; }
        }
    }
}