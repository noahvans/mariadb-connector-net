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
using System.Linq;
using System.Data.EntityClient;
using System.Data.Common;
using NUnit.Framework;
using System.Data.Objects;
using MariaDB.Data.Entity.Tests.Properties;

namespace MariaDB.Data.Entity.Tests
{
    [TestFixture]
    public class Paging : BaseEdmTest
    {
        [Test]
        public void Top()
        {
            using (testEntities context = new testEntities())
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Companies LIMIT 2", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                int i = 0;
                var query = context.Companies.Top("2");
                string sql = query.ToTraceString();
                CheckSql(sql, SQLSyntax.Top);

                foreach (Company c in query)
                {
                    Assert.AreEqual(dt.Rows[i++]["id"], c.Id);
                }
            }
        }

        [Test]
        public void Skip()
        {
            using (testEntities context = new testEntities())
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Companies LIMIT 3,20", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                int i = 0;
                var query = context.Companies.Skip("it.Id", "3");
                string sql = query.ToTraceString();
                CheckSql(sql, SQLSyntax.Skip);

                foreach (Company c in query)
                {
                    Assert.AreEqual(dt.Rows[i++]["id"], c.Id);
                }
            }
        }

        [Test]
        public void SkipAndTakeSimple()
        {
            using (testEntities context = new testEntities())
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Companies LIMIT 2,2", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                int i = 0;
                var query = context.Companies.Skip("it.Id", "2").Top("2");
                string sql = query.ToTraceString();
                CheckSql(sql, SQLSyntax.SkipAndTakeSimple);

                foreach (Company c in query)
                {
                    Assert.AreEqual(dt.Rows[i++]["id"], c.Id);
                }
                Assert.AreEqual(2, i);
            }
        }

        /// <summary>
        /// Bug #45723 Entity Framework DbSortExpression not processed when using Skip & Take  
        /// </summary>
        [Test]
        public void SkipAndTakeWithOrdering()
        {
            using (testEntities context = new testEntities())
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Companies ORDER BY Name DESC LIMIT 2,2", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                int i = 0;
                var query = context.Companies.OrderByDescending(q => q.Name).Skip(2).Take(2);
                string sql = query.ToTraceString();
                CheckSql(sql, SQLSyntax.SkipAndTakeWithOrdering);
                foreach (Company c in query)
                    Assert.AreEqual(dt.Rows[i++]["Name"], c.Name);
            }
        }
    }
}