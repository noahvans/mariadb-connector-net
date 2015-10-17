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

namespace MariaDB.Data.MySqlClient.Tests
{
    /// <summary>
    /// Summary description for BlobTests.
    /// </summary>
    [TestFixture]
    public class SqlServerMode : BaseTest
    {
        public SqlServerMode()
        {
            csAdditions += ";sqlservermode=yes;";
        }

        [Test]
        public void Simple()
        {
            execSQL("CREATE TABLE Test (id INT, name VARCHAR(20))");
            execSQL("INSERT INTO Test VALUES (1, 'A')");

            MySqlCommand cmd = new MySqlCommand("SELECT [id], [name] FROM [Test]", conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                Assert.AreEqual(1, reader.GetInt32(0));
                Assert.AreEqual("A", reader.GetString(1));
            }
        }
    }
}