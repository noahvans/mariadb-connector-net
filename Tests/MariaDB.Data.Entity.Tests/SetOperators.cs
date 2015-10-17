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
using System.Linq;
using MariaDB.Data.Entity.Tests.Properties;

namespace MariaDB.Data.Entity.Tests
{
    [TestFixture]
    public class SetOperators : BaseEdmTest
    {
        [Test]
        public void Any()
        {
            MySqlDataAdapter da = new MySqlDataAdapter(
                @"SELECT a.id FROM authors a WHERE NOT EXISTS(SELECT * FROM books b WHERE b.author_id=a.id)", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            int i = 0;
            // find all authors that are in our db with no books
            using (testEntities context = new testEntities())
            {
                var authors = from a in context.Authors where !a.Books.Any() select a;

                string sql = authors.ToTraceString();
                CheckSql(sql, SQLSyntax.Any);
                
                foreach (Author a in authors)
                    Assert.AreEqual(dt.Rows[i++]["id"], a.Id);
            }
        }

        [Test]
        public void FirstSimple()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT id FROM orders", conn);
            int id = (int)cmd.ExecuteScalar();

            using (testEntities context = new testEntities())
            {
                var q = from o in context.Orders 
                            select o;
                Order order = q.First() as Order;

                Assert.AreEqual(id, order.Id);
            }
        }

        [Test]
        public void FirstPredicate()
        {
            MySqlCommand cmd = new MySqlCommand("SELECT id FROM orders WHERE freight > 100", conn);
            int id = (int)cmd.ExecuteScalar();

            using (testEntities context = new testEntities())
            {
                var q = from o in context.Orders
                        where o.Freight > 100
                        select o;
                Order order = q.First() as Order;
                Assert.AreEqual(id, order.Id);
            }
        }

        [Test]
        public void Distinct()
        {
            using (testEntities context = new testEntities())
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM Companies LIMIT 2", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                int i = 0;
                var query = context.Companies.Top("2");
                foreach (Company c in query)
                {
                    Assert.AreEqual(dt.Rows[i++]["id"], c.Id);
                }
            }
        }
    }
}