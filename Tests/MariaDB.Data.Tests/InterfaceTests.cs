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

using System.Data;
using System.Data.Common;

namespace MariaDB.Data.MySqlClient.Tests
{
    [TestFixture]
    public class InterfaceTests : BaseTest
    {
#if !CF

        [Test]
        public void ClientFactory()
        {
            DbProviderFactory f = new MySqlClientFactory();
            using (DbConnection c = f.CreateConnection())
            {
                DbConnectionStringBuilder cb = f.CreateConnectionStringBuilder();
                cb.ConnectionString = GetConnectionString(true);
                c.ConnectionString = cb.ConnectionString;
                c.Open();

                DbCommand cmd = f.CreateCommand();
                cmd.Connection = c;
                cmd.CommandText = "SELECT 1";
                cmd.CommandType = CommandType.Text;
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                }
            }
        }

#endif
    }
}