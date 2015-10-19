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

#if !PocketPC

using System;
using System.Data.Common;
using System.Reflection;

namespace MariaDB.Data.MySqlClient
{
    /// <summary>
    /// DBProviderFactory implementation for MysqlClient.
    /// </summary>
    public sealed class MySqlClientFactory : DbProviderFactory
    {
        /// <summary>
        /// Gets an instance of the <see cref="MySqlClientFactory"/>.
        /// This can be used to retrieve strongly typed data objects.
        /// </summary>
        public static MySqlClientFactory Instance = new MySqlClientFactory();

        private Type dbServicesType;

        /// <summary>
        /// Returns a strongly typed <see cref="DbCommand"/> instance.
        /// </summary>
        /// <returns>A new strongly typed instance of <b>DbCommand</b>.</returns>
        public override DbCommand CreateCommand()
        {
            return new MySqlCommand();
        }

        /// <summary>
        /// Returns a strongly typed <see cref="DbConnection"/> instance.
        /// </summary>
        /// <returns>A new strongly typed instance of <b>DbConnection</b>.</returns>
        public override DbConnection CreateConnection()
        {
            return new MySqlConnection();
        }

        /// <summary>
        /// Returns a strongly typed <see cref="DbParameter"/> instance.
        /// </summary>
        /// <returns>A new strongly typed instance of <b>DbParameter</b>.</returns>
        public override DbParameter CreateParameter()
        {
            return new MySqlParameter();
        }

        /// <summary>
        /// Returns a strongly typed <see cref="DbConnectionStringBuilder"/> instance.
        /// </summary>
        /// <returns>A new strongly typed instance of <b>DbConnectionStringBuilder</b>.</returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new MySqlConnectionStringBuilder();
        }

        /// <summary>
        /// Provide a simple caching layer
        /// </summary>
        private Type DbServicesType
        {
            get
            {
                if (dbServicesType == null)
                {
                    // Get the type this way so we don't have to reference System.Data.Entity
                    // from our core provider
                    dbServicesType = Type.GetType(
                        @"System.Data.Common.DbProviderServices, System.Data.Entity,
                        Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                                                                                          false);
                }
                return dbServicesType;
            }
        }
    }
}

#endif