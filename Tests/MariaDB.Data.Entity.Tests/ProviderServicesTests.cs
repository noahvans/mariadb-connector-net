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

namespace MariaDB.Data.Entity.Tests
{
    [TestFixture]
    public class ProviderServicesTests : BaseEdmTest
    {
        #if CLR4
        [Test]
        public void CreateDatabase()
        {
            suExecSQL("GRANT ALL ON `modeldb`.* to 'test'@'localhost'");
            suExecSQL("FLUSH PRIVILEGES");

            using (Model1Container ctx = new Model1Container())
            {
                Assert.IsFalse(ctx.DatabaseExists());
                ctx.CreateDatabase();
                Assert.IsTrue(ctx.DatabaseExists());
            }
        }

        [Test]
        public void CreateDatabaseScript()
        {
            using (testEntities ctx = new testEntities())
            {
                string s = ctx.CreateDatabaseScript();
            }
        }

        [Test]
        public void DeleteDatabase()
        {
            suExecSQL("GRANT ALL ON `modeldb`.* to 'test'@'localhost'");
            suExecSQL("FLUSH PRIVILEGES");

            using (Model1Container ctx = new Model1Container())
            {
                Assert.IsFalse(ctx.DatabaseExists());
                ctx.CreateDatabase();
                Assert.IsTrue(ctx.DatabaseExists());
                ctx.DeleteDatabase();
                Assert.IsFalse(ctx.DatabaseExists());
            }
        }

        [Test]
        public void DatabaseExists()
        {
            suExecSQL("GRANT ALL ON `modeldb`.* to 'test'@'localhost'");
            suExecSQL("FLUSH PRIVILEGES");

            using (Model1Container ctx = new Model1Container())
            {
                Assert.IsFalse(ctx.DatabaseExists());
                ctx.CreateDatabase();
                Assert.IsTrue(ctx.DatabaseExists());
                ctx.DeleteDatabase();
                Assert.IsFalse(ctx.DatabaseExists());
            }
        }
#endif
    }
}