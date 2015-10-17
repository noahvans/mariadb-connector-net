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
using NUnit.Framework;
using MariaDB.Data.MySqlClient.Tests;
using System.Data.EntityClient;
using System.Data.Common;
using System.Data.Objects;

namespace MariaDB.Data.Entity.Tests
{
    [TestFixture]
    public class InsertTests : BaseEdmTest
    {
        [Test]
        public void InsertSingleRow()
        {
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM companies", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DataRow lastRow = dt.Rows[dt.Rows.Count - 1];
            int lastId = (int)lastRow["id"];
            DateTime dateBegan = DateTime.Now;

            using (testEntities context = new testEntities())
            {
                Company c = new Company();
                c.Id = 23;
                c.Name = "Yoyo";
                c.NumEmployees = 486;
                c.DateBegan = dateBegan;
                c.Address.Address = "212 My Street.";
                c.Address.City = "Helena";
                c.Address.State = "MT";
                c.Address.ZipCode = "44558";

                context.AddToCompanies(c);
                int result = context.SaveChanges();

                DataTable afterInsert = new DataTable();
                da.Fill(afterInsert);
                lastRow = afterInsert.Rows[afterInsert.Rows.Count - 1];

                Assert.AreEqual(dt.Rows.Count + 1, afterInsert.Rows.Count);
                Assert.AreEqual(lastId+1, lastRow["id"]);
                Assert.AreEqual("Yoyo", lastRow["name"]);
                Assert.AreEqual(486, lastRow["numemployees"]);
                DateTime insertedDT = (DateTime)lastRow["dateBegan"];
                Assert.AreEqual(dateBegan.Date, insertedDT.Date);
                Assert.AreEqual("212 My Street.", lastRow["address"]);
                Assert.AreEqual("Helena", lastRow["city"]);
                Assert.AreEqual("MT", lastRow["state"]);
                Assert.AreEqual("44558", lastRow["zipcode"]);
            }
        }

        [Test]
        public void CanInsertRowWithDefaultTimeStamp()
        {
            using (testEntities context = new testEntities())
            {
                // The default timestamp is in the CreatedDate column.
                Product product = new Product();
                product.Name = "Coca Cola";

                context.AddToProducts(product);
                context.SaveChanges();

                Assert.AreEqual(DateTime.Today.Day, product.CreatedDate.Day);
            }
        }
    }
}