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
using System.IO;
using NUnit.Framework;

namespace MariaDB.Data.MySqlClient.Tests
{
	[TestFixture]
	public class Syntax : BaseTest
	{
		[Test]
		public void ProblemCharsInSQLUTF8()
		{
			if (Version < new Version(4, 1)) return;

			execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), mt MEDIUMTEXT, " +
					  "PRIMARY KEY(id)) CHAR SET utf8");

			using (MySqlConnection c = new MySqlConnection(GetConnectionString(true) + ";charset=utf8"))
			{
				c.Open();

				MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (?id, ?text, ?mt)", c);
				cmd.Parameters.AddWithValue("?id", 1);
				cmd.Parameters.AddWithValue("?text", "This is my;test ? string");
				cmd.Parameters.AddWithValue("?mt", "My MT string: ?");
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(1, reader.GetInt32(0));
					Assert.AreEqual("This is my;test ? string", reader.GetString(1));
					Assert.AreEqual("My MT string: ?", reader.GetString(2));
				}
			}
		}

		[Test]
		public void ProblemCharsInSQL()
		{
			execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), mt MEDIUMTEXT, " +
					  "PRIMARY KEY(id))");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (?id, ?text, ?mt)", conn);
			cmd.Parameters.AddWithValue("?id", 1);
			cmd.Parameters.AddWithValue("?text", "This is my;test ? string-'''\"\".");
			cmd.Parameters.AddWithValue("?mt", "My MT string: ?");
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT * FROM Test";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(1, reader.GetInt32(0));
				Assert.AreEqual("This is my;test ? string-'''\"\".", reader.GetString(1));
				Assert.AreEqual("My MT string: ?", reader.GetString(2));
			}
		}

		[Test]
		public void LoadDataLocalInfile()
		{
			execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");
			string connString = conn.ConnectionString + ";pooling=false";
			MySqlConnection c = new MySqlConnection(connString);
			c.Open();

			string path = Path.GetTempFileName();
			StreamWriter sw = new StreamWriter(File.OpenWrite(path));
			for (int i = 0; i < 2000000; i++)
				sw.WriteLine(i + ",'Test'");
			sw.Flush();
			sw.Dispose();

			path = path.Replace(@"\", @"\\");
			MySqlCommand cmd = new MySqlCommand(
				"LOAD DATA LOCAL INFILE '" + path + "' INTO TABLE Test FIELDS TERMINATED BY ','", conn);
			cmd.CommandTimeout = 0;

			object cnt = 0;
			cnt = cmd.ExecuteNonQuery();
			Assert.AreEqual(2000000, cnt);

			cmd.CommandText = "SELECT COUNT(*) FROM Test";
			cnt = cmd.ExecuteScalar();
			Assert.AreEqual(2000000, cnt);

			c.Close();
		}

		[Test]
		public void ShowTablesInNonExistentDb()
		{
			MySqlCommand cmd = new MySqlCommand("SHOW TABLES FROM dummy", conn);
			try
			{
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					Assert.Fail("ExecuteReader should not succeed");
				}
			}
			catch (MySqlException)
			{
				Assert.AreEqual(ConnectionState.Open, conn.State);
			}
		}

		[Test]
		public void Bug6135()
		{
			string sql = @"CREATE TABLE `KLANT` (`KlantNummer` int(11) NOT NULL auto_increment,
				`Username` varchar(50) NOT NULL default '', `Password` varchar(100) NOT NULL default '',
				`Naam` varchar(100) NOT NULL default '', `Voornaam` varchar(100) NOT NULL default '',
				`Straat` varchar(100) NOT NULL default '', `StraatNr` varchar(10) NOT NULL default '',
				`Gemeente` varchar(100) NOT NULL default '', `Postcode` varchar(10) NOT NULL default '',
				`DefaultMail` varchar(255) default '', 	`BtwNr` varchar(50) default '',
				`ReceiveMail` tinyint(1) NOT NULL default '0',	`Online` tinyint(1) NOT NULL default '0',
				`LastVisit` timestamp NOT NULL, `Categorie` int(11) NOT NULL default '0',
				PRIMARY KEY  (`KlantNummer`),	UNIQUE KEY `UniqueUsername` (`Username`),
				UNIQUE KEY `UniqueDefaultMail` (`DefaultMail`)	)";
			createTable(sql, "MyISAM");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM KLANT", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read()) { }
			}
		}

		[Test]
		public void Sum()
		{
			execSQL("CREATE TABLE Test (field1 mediumint(9) default '0', field2 float(9,3) " +
				"default '0.000', field3 double(15,3) default '0.000') engine=innodb ");
			execSQL("INSERT INTO Test values (1,1,1)");

			MySqlCommand cmd2 = new MySqlCommand("SELECT sum(field2) FROM Test", conn);
			using (MySqlDataReader reader = cmd2.ExecuteReader())
			{
				reader.Read();
				object o = reader[0];
				Assert.AreEqual(1, o);
			}
		}

		[Test]
		public void Sum2()
		{
			execSQL("CREATE TABLE Test (id int, count int)");
			execSQL("INSERT INTO Test VALUES (1, 21)");
			execSQL("INSERT INTO Test VALUES (1, 33)");
			execSQL("INSERT INTO Test VALUES (1, 16)");
			execSQL("INSERT INTO Test VALUES (1, 40)");

			MySqlCommand cmd = new MySqlCommand("SELECT id, SUM(count) FROM Test GROUP BY id", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				Assert.AreEqual(1, reader.GetInt32(0));
				Assert.AreEqual(110, reader.GetDouble(1));
			}
		}

		[Test]
		public void ForceWarnings()
		{
			if (Version < new Version(4, 1)) return;

			execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(250), PRIMARY KEY(id))");
			MySqlCommand cmd = new MySqlCommand(
				"SELECT * FROM Test; DROP TABLE IF EXISTS test2; SELECT * FROM Test", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.NextResult()) { }
			}
		}

		[Test]
		public void SettingAutoIncrementColumns()
		{
			execSQL("CREATE TABLE Test (id int auto_increment, name varchar(100), primary key(id))");
			execSQL("INSERT INTO Test VALUES (1, 'One')");
			execSQL("INSERT INTO Test VALUES (3, 'Two')");

			MySqlCommand cmd = new MySqlCommand("SELECT name FROM Test WHERE id=1", conn);
			object name = cmd.ExecuteScalar();
			Assert.AreEqual("One", name);

			cmd.CommandText = "SELECT name FROM Test WHERE id=3";
			name = cmd.ExecuteScalar();
			Assert.AreEqual("Two", name);

			try
			{
				execSQL("INSERT INTO Test (id, name2) values (5, 'Three')");
				Assert.Fail("This should have failed");
			}
			catch (MySqlException)
			{
			}
		}

		/// <summary>
		/// Bug #16645 FOUND_ROWS() Bug
		/// </summary>
		[Test]
		public void FoundRows()
		{
			execSQL("CREATE TABLE Test (testID int(11) NOT NULL auto_increment, testName varchar(100) default '', " +
					"PRIMARY KEY  (testID)) ENGINE=InnoDB DEFAULT CHARSET=latin1");
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (NULL, 'test')", conn);
			for (int i = 0; i < 1000; i++)
				cmd.ExecuteNonQuery();
			cmd.CommandText = "SELECT SQL_CALC_FOUND_ROWS * FROM Test LIMIT 0, 10";
			cmd.ExecuteNonQuery();
			cmd.CommandText = "SELECT FOUND_ROWS()";
			object cnt = cmd.ExecuteScalar();
			Assert.AreEqual(1000, cnt);
		}

		[Test]
		public void AutoIncrement()
		{
			execSQL("CREATE TABLE Test (testID int(11) NOT NULL auto_increment, testName varchar(100) default '', " +
					"PRIMARY KEY  (testID)) ENGINE=InnoDB DEFAULT CHARSET=latin1");
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (NULL, 'test')", conn);
			cmd.ExecuteNonQuery();
			cmd.CommandText = "SELECT @@IDENTITY as 'Identity'";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				int ident = Int32.Parse(reader.GetValue(0).ToString());
				Assert.AreEqual(1, ident);
			}
		}

		/// <summary>
		/// Bug #25178 Addition message in error
		/// </summary>
		[Test]
		public void ErrorMessage()
		{
			MySqlCommand cmd = new MySqlCommand("SELEKT NOW() as theTime", conn);
			try
			{
				cmd.ExecuteScalar();
			}
			catch (MySqlException ex)
			{
				string s = ex.Message;
				Assert.IsFalse(s.StartsWith("#"));
			}
		}

		[Test]
		public void SpaceInDatabaseName()
		{
			string dbName = System.IO.Path.GetFileNameWithoutExtension(
				System.IO.Path.GetTempFileName()) + " x";
			try
			{
				suExecSQL(String.Format("CREATE DATABASE `{0}`", dbName));
				suExecSQL(String.Format("GRANT ALL ON `{0}`.* to 'test'@'localhost' identified by 'test'",
					dbName));
				suExecSQL(String.Format("GRANT ALL ON `{0}`.* to 'test'@'%' identified by 'test'",
					dbName));
				suExecSQL("FLUSH PRIVILEGES");

				string connStr = GetConnectionString(false) + ";database=" + dbName;
				MySqlConnection c = new MySqlConnection(connStr);
				c.Open();
				c.Close();
			}
			finally
			{
				suExecSQL(String.Format("DROP DATABASE `{0}`", dbName));
			}
		}

		[Test]
		public void SemisAtStartAndEnd()
		{
			using (MySqlCommand cmd = new MySqlCommand(";;SELECT 1;;;", conn))
			{
				Assert.AreEqual(1, cmd.ExecuteScalar());
			}
		}

		/// <summary>
		/// Bug #51610	Exception thrown inside Connector.NET
		/// </summary>
		[Test]
		public void Bug51610()
		{
			MySqlCommand cmd = new MySqlCommand("SELECT 'ABC', (0/`QOH`) from (SELECT 1 as `QOH`) `d1`", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				Assert.AreEqual("ABC", reader.GetString(0));
				Assert.AreEqual(0, reader.GetInt32(1));
			}

			cmd.CommandText = "SELECT 'ABC', (0-`QOH`) from (SELECT 1 as `QOH`) `d1`";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				Assert.AreEqual("ABC", reader.GetString(0));
				Assert.AreEqual(-1, reader.GetInt32(1));
			}

			cmd.CommandText = "SELECT 'test 2010-03-04 @ 10:14'";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				Assert.AreEqual("test 2010-03-04 @ 10:14", reader.GetString(0));
			}
		}

		/// <summary>
		/// Bug #51788	Error in SQL syntax not reported. A CLR exception was thrown instead,
		/// </summary>
		[Test]
		public void NonTerminatedString()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test(id INT, name1 VARCHAR(20), name2 VARCHAR(20))");

			try
			{
				MySqlCommand cmd = new MySqlCommand(
					"INSERT INTO test VALUES (1, 'test 2010-03-04 @ 10:14, name2=' joe')", conn);
				cmd.ExecuteNonQuery();
			}
			catch (MySqlException)
			{
			}
		}

		/// <summary>
		/// Bug #53865 : crash in QueryNormalizer, "IN" clause incomplete
		/// </summary>
		[Test]
		public void QueryNormalizerCrash1()
		{
			execSQL(
				"CREATE TABLE  extable_1 (x_coord int, y_coord int, z_coord int," +
				"edge_id int, life_id int)");
			execSQL("CREATE TABLE  extable_2 (daset_id int, sect_id int, " +
			  "xyz_id  int, edge_id int, life_id int, another_id int, yetanother_id int)");

			string connStr = GetConnectionString(true) + ";logging=true";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();
				MySqlCommand cmd = new MySqlCommand(
					"SELECT tblb.x_coord, tblb.y_coord, tblb.z_coord, " +
					"tbl_c.x_coord, tbl_c.y_coord, tbl_c.z_coord, tbl_a.edge_id, " +
					"tbl_a.life_id, tbl_a.daset_id, tbl_a.sect_id, tbl_a.yetanother_id," +
					" IFNULL(tbl_a.xyz_id,0) FROM extable_2 tbl_a, extable_1 tblb, " +
					"extable_1 tbl_c WHERE tbl_a.daset_id=208 AND tbl_a.sect_id IN " +
					"(1,2,3,4,5,6,7)",
					c);
				//Console.WriteLine(cmd.ExecuteScalar());
			}
		}

		/// <summary>
		/// Bug #54152 : Crash in QueryNormalizer, VALUES incomplete
		/// </summary>
		[Test]
		public void QueryNormalizerCrash2()
		{
			execSQL("CREATE TABLE bug54152 (id INT, expr INT,name VARCHAR(20)," +
				"fld4 VARCHAR(10), fld5 VARCHAR(10), fld6 VARCHAR(10)," +
				"fld7 VARCHAR(10), fld8 VARCHAR(10), fld9 VARCHAR(10)," +
				"fld10 VARCHAR(10), PRIMARY KEY(id))");

			string connStr = GetConnectionString(true) + ";logging=true";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();
				string query =
					"INSERT INTO bug54152 VALUES " +
					"(1,1, 'name 1',1,1,1,1,1,1,1)," +
					"(2,2,'name 2',2,2,2,2,2,2,2)," +
					"(3,3,'name 3',3,3,3,3,3,3,3)," +
					"(4,4,'name 4',4,4,4,4,4,4,4)," +
					"(5,5,'name 5',5,5,5,5,5,5,5)," +
					"(6,6,'name 6',6,6,6,6,6,6,6)," +
					"(7,7,'name 7',7,7,7,7,7,7,7)," +
					"(8,8,'name 8',8,8,8,8,8,8,8)," +
					"(9,9,'name 9',9,9,9,9,9,9,9)," +
					"(10,10,'name 10',10,10,10,10,10,10,10)";
				MySqlCommand cmd = new MySqlCommand(query, c);
				cmd.ExecuteNonQuery();
			}
		}
	}
}