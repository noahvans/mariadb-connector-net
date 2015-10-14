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
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MariaDB.Data.MySqlClient.Properties;
using Microsoft.Win32.SafeHandles;

namespace MariaDB.Data.Common
{
    /// <summary>
    /// Summary description for API.
    /// </summary>
    internal class NamedPipeStream : Stream
    {
        private SafeFileHandle handle;
        private Stream fileStream;
        private int readTimeout = Timeout.Infinite;
        private int writeTimeout = Timeout.Infinite;
        private const int ERROR_PIPE_BUSY = 231;
        private const int ERROR_SEM_TIMEOUT = 121;

        public NamedPipeStream(string path, FileAccess mode, uint timeout)
        {
            Open(path, mode, timeout);
        }

        private void CancelIo()
        {
            bool ok = NativeMethods.CancelIo(handle.DangerousGetHandle());
            if (!ok)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        public void Open(string path, FileAccess mode, uint timeout)
        {
            IntPtr nativeHandle;
            for (;;)
            {
                nativeHandle = NativeMethods.CreateFile(path, NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                             0, null, NativeMethods.OPEN_EXISTING, NativeMethods.FILE_FLAG_OVERLAPPED, 0);
                if (nativeHandle != IntPtr.Zero)
                    break;

                if (Marshal.GetLastWin32Error() != ERROR_PIPE_BUSY)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Error opening pipe");
                }
                LowResolutionStopwatch sw = LowResolutionStopwatch.StartNew();
                bool success = NativeMethods.WaitNamedPipe(path, timeout);
                sw.Stop();
                if (!success)
                {
                    if (timeout < sw.ElapsedMilliseconds ||
                        Marshal.GetLastWin32Error() == ERROR_SEM_TIMEOUT)
                    {
                        throw new TimeoutException("Timeout waiting for named pipe");
                    }
                    else
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(),
                            "Error waiting for pipe");
                    }
                }
                timeout -= (uint)sw.ElapsedMilliseconds;
            }
            handle = new SafeFileHandle(nativeHandle, true);
            fileStream = new FileStream(handle, mode, 4096, true);
        }

        public override bool CanRead
        {
            get { return fileStream.CanRead; }
        }

        public override bool CanWrite
        {
            get { return fileStream.CanWrite; }
        }

        public override bool CanSeek
        {
            get { throw new NotSupportedException(Resources.NamedPipeNoSeek); }
        }

        public override long Length
        {
            get { throw new NotSupportedException(Resources.NamedPipeNoSeek); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(Resources.NamedPipeNoSeek); }
            set { }
        }

        public override void Flush()
        {
            fileStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (readTimeout == Timeout.Infinite)
            {
                return fileStream.Read(buffer, offset, count);
            }
            IAsyncResult result = fileStream.BeginRead(buffer, offset, count, null, null);
            if (result.CompletedSynchronously)
                return fileStream.EndRead(result);

            if (!result.AsyncWaitHandle.WaitOne(readTimeout))
            {
                CancelIo();
                throw new TimeoutException("Timeout in named pipe read");
            }
            return fileStream.EndRead(result);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (writeTimeout == Timeout.Infinite)
            {
                fileStream.Write(buffer, offset, count);
                return;
            }
            IAsyncResult result = fileStream.BeginWrite(buffer, offset, count, null, null);
            if (result.CompletedSynchronously)
            {
                fileStream.EndWrite(result);
            }

            if (!result.AsyncWaitHandle.WaitOne(readTimeout))
            {
                CancelIo();
                throw new TimeoutException("Timeout in named pipe write");
            }
            fileStream.EndWrite(result);
        }

        public override void Close()
        {
            if (handle != null && !handle.IsInvalid && !handle.IsClosed)
            {
                fileStream.Close();
                try
                {
                    handle.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        public override void SetLength(long length)
        {
            throw new NotSupportedException(Resources.NamedPipeNoSetLength);
        }

        public override bool CanTimeout
        {
            get
            {
                return true;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return readTimeout;
            }
            set
            {
                readTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return writeTimeout;
            }
            set
            {
                writeTimeout = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(Resources.NamedPipeNoSeek);
        }

        internal static Stream Create(string pipeName, string hostname, uint timeout)
        {
            string pipePath;
            if (0 == String.Compare(hostname, "localhost", true))
                pipePath = @"\\.\pipe\" + pipeName;
            else
                pipePath = String.Format(@"\\{0}\pipe\{1}", hostname, pipeName);
            return new NamedPipeStream(pipePath, FileAccess.ReadWrite, timeout);
        }
    }
}