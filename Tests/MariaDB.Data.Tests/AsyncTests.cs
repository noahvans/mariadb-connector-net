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
using System.Data;
using NUnit.Framework;

namespace MariaDB.Data.MySqlClient.Tests
{
        public class AsyncTests : BaseTest
    {           
        public void ExecuteNonQuery()
        {
            if (Version < new Version(5, 0)) return;

            execSQL("CREATE TABLE test (id int)");
            execSQL("CREATE PROCEDURE spTest() BEGIN SET @x=0; REPEAT INSERT INTO test VALUES(@x); " +
                "SET @x=@x+1; UNTIL @x = 300 END REPEAT; END");

            MySqlCommand proc = new MySqlCommand("spTest", conn);
            proc.CommandType = CommandType.StoredProcedure;
            IAsyncResult iar = proc.BeginExecuteNonQuery();
            int count = 0;
            while (!iar.IsCompleted)
            {
                count++;
                System.Threading.Thread.Sleep(20);
            }
            proc.EndExecuteNonQuery(iar);
            Assert.IsTrue(count > 0);

            proc.CommandType = CommandType.Text;
            proc.CommandText = "SELECT COUNT(*) FROM test";
            object cnt = proc.ExecuteScalar();
            Assert.AreEqual(300, cnt);
        }
                
        public void ExecuteReader()
        {
            if (Version < new Version(5, 0)) return;

            execSQL("CREATE TABLE test (id int)");
            execSQL("CREATE PROCEDURE spTest() BEGIN INSERT INTO test VALUES(1); " +
                "SELECT SLEEP(2); SELECT 'done'; END");

            MySqlCommand proc = new MySqlCommand("spTest", conn);
            proc.CommandType = CommandType.StoredProcedure;
            IAsyncResult iar = proc.BeginExecuteReader();
            int count = 0;
            while (!iar.IsCompleted)
            {
                count++;
                System.Threading.Thread.Sleep(20);
            }

            using (MySqlDataReader reader = proc.EndExecuteReader(iar))
            {
                Assert.IsNotNull(reader);
                Assert.IsTrue(count > 0, "count > 0");
                Assert.IsTrue(reader.Read(), "can read");
                Assert.IsTrue(reader.NextResult());
                Assert.IsTrue(reader.Read());
                Assert.AreEqual("done", reader.GetString(0));
                reader.Close();

                proc.CommandType = CommandType.Text;
                proc.CommandText = "SELECT COUNT(*) FROM test";
                object cnt = proc.ExecuteScalar();
                Assert.AreEqual(1, cnt);
            }
        }

        [Test]
        public void ThrowingExceptions()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT xxx", conn);
            IAsyncResult r = cmd.BeginExecuteReader();
            try
            {
                using (MySqlDataReader reader = cmd.EndExecuteReader(r))
                {
                    Assert.Fail("EndExecuteReader should have thrown an exception");
                }
            }
            catch (MySqlException)
            {
            }
        }
    }
}