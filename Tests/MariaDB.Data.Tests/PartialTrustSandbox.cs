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
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Security.Permissions;
using System.Net;

namespace MariaDB.Data.MySqlClient.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public class PartialTrustSandbox : MarshalByRefObject
    {
        public static AppDomain CreatePartialTrustDomain()
        {
            AppDomainSetup setup = new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory, PrivateBinPath = AppDomain.CurrentDomain.RelativeSearchPath };
            PermissionSet permissions = new PermissionSet(null);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissions.AddPermission(new DnsPermission(PermissionState.Unrestricted));
            permissions.AddPermission(new SocketPermission(PermissionState.Unrestricted));

            return AppDomain.CreateDomain("Partial Trust Sandbox", AppDomain.CurrentDomain.Evidence, setup, permissions);
        }

        public MySqlConnection TryOpenConnection(string connectionString)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
