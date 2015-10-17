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
using System.Threading;
using MariaDB.Data.MySqlClient;
using MariaDB.Data.MySqlClient.Tests;
using System.Data.EntityClient;
using System.Data.Common;
using NUnit.Framework;
using System.Data.Objects;
using MariaDB.Data.Entity.Tests.Properties;
using System.Linq;

namespace MariaDB.Data.Entity.Tests
{
    [TestFixture]
    public class RelationalOperators : BaseEdmTest
    {
        [Test]
        public void Except()
        {
/*            using (TestDB.TestDB db = new TestDB.TestDB())
            {
                var q = from c in db.Companies where 
                var query = from o in db.Orders
                            where o.StoreId = 3
                            select o;

                var result = query.First();
            }*/
        }

        [Test]
        public void Intersect()
        {
        }

        [Test]
        public void CrossJoin()
        {
        }

        [Test]
        public void Union()
        {
        }

        [Test]
        public void UnionAll()
        {
            using (testEntities context = new testEntities())
            {
                MySqlDataAdapter da = new MySqlDataAdapter(
                    "SELECT t.Id FROM Toys t UNION ALL SELECT c.Id FROM Companies c", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                string entitySQL = @"(SELECT t.Id, t.Name FROM Toys AS t) 
                UNION ALL (SELECT c.Id, c.Name FROM Companies AS c)";
                ObjectQuery<DbDataRecord> query = context.CreateQuery<DbDataRecord>(entitySQL);

                string sql = query.ToTraceString();
                CheckSql(sql, SQLSyntax.UnionAll);

                int i = 0;
                foreach (DbDataRecord r in query)
                {
                    i++;
                }
                Assert.AreEqual(dt.Rows.Count, i);
            }
        }

        /// <summary>
        /// Bug #60652	Query returns BLOB type but no BLOBs are in the database.        
        /// </summary>
        [Test]
        public void UnionAllWithBitColumnsDoesNotThrow()
        {
            using (testEntities entities = new testEntities())
            {
                // Here, Computer is the base type of DesktopComputer, LaptopComputer and TabletComputer. 
                // LaptopComputer and TabletComputer include the bit fields that would provoke
                // an InvalidCastException (byte[] to bool) when participating in a UNION 
                // created internally by the Connector/Net entity framework provider.
                var computers = from c in entities.Computers
                                select c;

                foreach (Computer computer in computers)
                {
                    Assert.NotNull(computer);
                    Assert.IsTrue(computer.Id > 0);
                }
            }
        }
    }
}