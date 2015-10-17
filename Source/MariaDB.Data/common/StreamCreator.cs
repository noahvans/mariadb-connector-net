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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using MariaDB.Data.MySqlClient.Properties;

namespace MariaDB.Data.Common
{
	/// <summary>
	/// Summary description for StreamCreator.
	/// </summary>
	internal class StreamCreator
	{
		private string hostList;
		private uint port;
		private uint timeOut;
		private uint keepalive;

#if !DNX
		private string pipeName;

		public StreamCreator(string hosts, uint port, string pipeName, uint keepalive)
#else
		public StreamCreator(string hosts, uint port, uint keepalive)
#endif
		{
			hostList = hosts;
			if (hostList == null || hostList.Length == 0)
				hostList = "localhost";
			this.port = port;
#if !DNX
			this.pipeName = pipeName;
#endif
			this.keepalive = keepalive;
		}

#if !DNX
		private Stream GetStreamFromHost(string pipeName, string hostName, uint timeout)
		{
			if (pipeName != null && pipeName.Length != 0)
				return NamedPipeStream.Create(pipeName, hostName, timeout);
#else
		private Stream GetStreamFromHost(string hostName, uint timeout)
		{
#endif
			Stream stream = null;
			IPHostEntry ipHE = GetHostEntry(hostName);
			foreach (IPAddress address in ipHE.AddressList)
			{
				try
				{
					stream = CreateSocketStream(address, false);
					if (stream != null) break;
				}
				catch (Exception ex)
				{
					SocketException socketException = ex as SocketException;
					// if the exception is a ConnectionRefused then we eat it as we may have other address
					// to attempt
					if (socketException == null) throw;
					if (socketException.SocketErrorCode != SocketError.ConnectionRefused) throw;
				}
			}

			return stream;
		}

		public Stream GetStream(uint timeout)
		{
			timeOut = timeout;

			if (hostList.StartsWith("/", StringComparison.OrdinalIgnoreCase))
				return CreateSocketStream(null, true);

			string[] dnsHosts = hostList.Split(',');

			Random random = new Random((int)DateTime.Now.Ticks);
			int index = random.Next(dnsHosts.Length);
			int pos = 0;
			Stream stream = null;

			while (stream == null && pos < dnsHosts.Length)
			{
#if !DNX
				stream = GetStreamFromHost(pipeName, dnsHosts[index++], timeout);
#else
				stream = GetStreamFromHost(dnsHosts[index++], timeout);
#endif
				if (index == dnsHosts.Length) index = 0;
				pos++;
			}
			return stream;
		}

		private IPHostEntry ParseIPAddress(string hostname)
		{
			IPHostEntry ipHE = null;
			IPAddress addr;
			if (IPAddress.TryParse(hostname, out addr))
			{
				ipHE = new IPHostEntry();
				ipHE.AddressList = new IPAddress[1];
				ipHE.AddressList[0] = addr;
			}
			return ipHE;
		}

		private IPHostEntry GetDnsHostEntry(string hostname)
		{
			LowResolutionStopwatch stopwatch = new LowResolutionStopwatch();

			try
			{
				stopwatch.Start();
				return Dns.GetHostEntry(hostname);
			}
			catch (SocketException ex)
			{
				string message = String.Format(Resources.GetHostEntryFailed,
				stopwatch.Elapsed, hostname, ex.SocketErrorCode,
				ex.ErrorCode, ex.NativeErrorCode);
				throw new Exception(message, ex);
			}
			finally
			{
				stopwatch.Stop();
			}
		}

		private IPHostEntry GetHostEntry(string hostname)
		{
			IPHostEntry ipHE = ParseIPAddress(hostname);
			if (ipHE != null) return ipHE;
			return GetDnsHostEntry(hostname);
		}

		private static EndPoint CreateUnixEndPoint(string host)
		{
#if !DNX
			// first we need to load the Mono.posix assembly
			Assembly a = Assembly.Load(@"Mono.Posix, Version=2.0.0.0,
				Culture=neutral, PublicKeyToken=0738eb9f132ed756");

			// then we need to construct a UnixEndPoint object
			EndPoint ep = (EndPoint)a.CreateInstance("Mono.Posix.UnixEndPoint",
				false, BindingFlags.CreateInstance, null,
				new object[1] { host }, null, null);
			return ep;
#else
			return null;
#endif
		}

		private Stream CreateSocketStream(IPAddress ip, bool unix)
		{
			EndPoint endPoint;
			if (!Platform.IsWindows() && unix)
				endPoint = CreateUnixEndPoint(hostList);
			else
				endPoint = new IPEndPoint(ip, (int)port);

			Socket socket = unix ?
				new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP) :
				new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			if (keepalive > 0)
			{
				SetKeepAlive(socket, keepalive);
			}
			IAsyncResult ias = socket.BeginConnect(endPoint, null, null);
			if (!ias.AsyncWaitHandle.WaitOne((int)timeOut * 1000, false))
			{
				socket.Close();
				return null;
			}
			try
			{
				socket.EndConnect(ias);
			}
			catch (Exception)
			{
				socket.Close();
				throw;
			}
			MyNetworkStream stream = new MyNetworkStream(socket, true);
			GC.SuppressFinalize(socket);
			GC.SuppressFinalize(stream);
			return stream;
		}

		/// <summary>
		/// Set keep-alive + timeout on socket.
		/// </summary>
		/// <param name="s">socket</param>
		/// <param name="time">keep-alive timeout, in seconds</param>
		private static void SetKeepAlive(Socket s, uint time)
		{
			uint on = 1;
			uint interval = 1000; // default interval = 1 sec

			uint timeMilliseconds;
			if (time > UInt32.MaxValue / 1000)
				timeMilliseconds = UInt32.MaxValue;
			else
				timeMilliseconds = time * 1000;

			byte[] inOptionValues = new byte[12];
			BitConverter.GetBytes(on).CopyTo(inOptionValues, 0);
			BitConverter.GetBytes(time).CopyTo(inOptionValues, 4);
			BitConverter.GetBytes(interval).CopyTo(inOptionValues, 8);
			try
			{
				// call WSAIoctl via IOControl
				s.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
				return;
			}
			catch (NotImplementedException)
			{
				// Mono throws not implemented currently
			}
			// Fall-back if Socket.IOControl is not available ( Compact Framework )
			// or not implemented ( Mono ). Keep-alive option will still be set, but
			// with timeout is kept default.
			s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
		}
	}
}