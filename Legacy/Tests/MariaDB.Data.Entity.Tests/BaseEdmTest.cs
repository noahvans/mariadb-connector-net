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

//  This code was contributed by Sean Wright (srwright@alcor.concordia.ca) on 2007-01-12
//  The copyright was assigned and transferred under the terms of
//  the MySQL Contributor License Agreement (CLA)

using MariaDB.Data.MySqlClient;
using System.Data;
using System.Configuration;
using System.Reflection;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using MariaDB.Data.MySqlClient.Tests;
using System.Resources;
using System.Xml;
using System.IO;
using NUnit.Framework;
using System.Text;
using System.Data.EntityClient;
using MariaDB.Data.Entity.Tests.Properties;

namespace MariaDB.Data.Entity.Tests
{
    public class BaseEdmTest : BaseTest
    {
        protected override void LoadStaticConfiguration()
        {
            base.LoadStaticConfiguration();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string filename = config.FilePath;

            database0 = database1 = "test";

            XmlDocument configDoc = new XmlDocument();
            configDoc.PreserveWhitespace = true;
            configDoc.Load(filename);
            XmlElement configNode = configDoc["configuration"];
            configNode.RemoveAll();

            XmlElement systemData = (XmlElement)configDoc.CreateNode(XmlNodeType.Element, "system.data", "");
            XmlElement dbFactories = (XmlElement)configDoc.CreateNode(XmlNodeType.Element, "DbProviderFactories", "");
            XmlElement provider = (XmlElement)configDoc.CreateNode(XmlNodeType.Element, "add", "");
            provider.SetAttribute("name", "MariaDB Data Provider");
            provider.SetAttribute("description", ".Net Framework Data Provider for MariaDB");
            provider.SetAttribute("invariant", "MariaDB.Data.MySqlClient");

            string fullname = String.Format("MariaDB.Data.MySqlClient.MySqlClientFactory, {0}",
                typeof(MySqlConnection).Assembly.FullName);
            provider.SetAttribute("type", fullname);

            dbFactories.AppendChild(provider);
            systemData.AppendChild(dbFactories);
            configNode.AppendChild(systemData);
            configDoc.Save(filename);

            ConfigurationManager.RefreshSection("system.data");
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            ResourceManager r = new ResourceManager("MariaDB.Data.Entity.Tests.Properties.Resources", typeof(BaseEdmTest).Assembly);
            string schema = r.GetString("schema");
            MySqlScript script = new MySqlScript(conn);
            script.Query = schema;
            script.Execute();

            // now create our procs
            schema = r.GetString("procs");
            script = new MySqlScript(conn);
            script.Delimiter = "$$";
            script.Query = schema;
            script.Execute();

            MySqlCommand cmd = new MySqlCommand("DROP DATABASE IF EXISTS `modeldb`", rootConn);
            cmd.ExecuteNonQuery();
        }

        [TearDown]
        public override void Teardown()
        {
            MySqlCommand cmd = new MySqlCommand("DROP DATABASE IF EXISTS `modeldb`", rootConn);
            cmd.ExecuteNonQuery();

            base.Teardown();            
        }

        private EntityConnection GetEntityConnection()
        {
            string connectionString = String.Format(
                "metadata=TestDB.csdl|TestDB.msl|TestDB.ssdl;provider=MariaDB.Data.MySqlClient; provider connection string=\"{0}\"", GetConnectionString(true));
            EntityConnection connection = new EntityConnection(connectionString);
            return connection;
        }

        protected void CheckSql(string sql, string refSql)
        {
            StringBuilder str1 = new StringBuilder();
            StringBuilder str2 = new StringBuilder();
            foreach (char c in sql)
                if (!Char.IsWhiteSpace(c))
                    str1.Append(c);
            foreach (char c in refSql)
                if (!Char.IsWhiteSpace(c))
                    str2.Append(c);
            Assert.AreEqual(0, String.Compare(str1.ToString(), str2.ToString(), true));
        }
    }
}
