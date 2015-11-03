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
using System.Data.Common;
using MariaDB.Data.Types;
using NUnit.Framework;

namespace MariaDB.Data.MySqlClient.Tests
{
	/// <summary>
	/// Summary description for ConnectionTests.
	/// </summary>
	[TestFixture]
	public class DataTypeTests : BaseTest
	{
		[Test]
		public void BytesAndBooleans()
		{
			InternalBytesAndBooleans(false);
		}

		[Test]
		public void BytesAndBooleansPrepared()
		{
			if (Version < new Version(4, 1)) return;

			InternalBytesAndBooleans(true);
		}

		private void InternalBytesAndBooleans(bool prepare)
		{
			execSQL("CREATE TABLE Test (id TINYINT, idu TINYINT UNSIGNED, i INT UNSIGNED)");
			execSQL("INSERT INTO Test VALUES (-98, 140, 20)");
			execSQL("INSERT INTO Test VALUES (0, 0, 0)");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", conn);
			if (prepare) cmd.Prepare();
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(-98, (sbyte)reader.GetByte(0));
				Assert.AreEqual(140, reader.GetByte(1));
				Assert.IsTrue(reader.GetBoolean(1));
				Assert.AreEqual(20, reader.GetUInt32(2));
				Assert.AreEqual(20, reader.GetInt32(2));

				Assert.IsTrue(reader.Read());
				Assert.AreEqual(0, reader.GetByte(0));
				Assert.AreEqual(0, reader.GetByte(1));
				Assert.IsFalse(reader.GetBoolean(1));

				Assert.IsFalse(reader.Read());
			}
		}

		/// <summary>
		/// Bug#46205 - tinyint as boolean does not work for utf8 default database character set.
		/// </summary>
		///<remarks>
		/// Original bug occured only with mysqld started with --default-character-set=utf8.
		/// It does not seem  possible to reproduce the original buggy behavior´otherwise
		/// Neither "set global character_set_server = utf8" , nor  "create table /database with character set "
		/// were sufficient.
		///</remarks>
		[Test]
		public void TreatTinyAsBool()
		{
			if (version < new Version(4, 1)) return;
			execSQL("CREATE TABLE Test2(i TINYINT(1))");
			execSQL("INSERT INTO Test2 VALUES(1)");
			execSQL("INSERT INTO Test2 VALUES(0)");
			execSQL("INSERT INTO Test2 VALUES(2)");
			MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(conn.ConnectionString);
			Assert.IsTrue(builder.TreatTinyAsBoolean);

			MySqlCommand cmd = new MySqlCommand("SELECT * from Test2", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				bool b;
				Assert.IsTrue(reader.Read());
				b = (bool)reader.GetValue(0);
				Assert.IsTrue(b);
				Assert.IsTrue(reader.Read());
				b = (bool)reader.GetValue(0);
				Assert.IsFalse(b);
				Assert.IsTrue(reader.Read());
				b = (bool)reader.GetValue(0);
				Assert.IsTrue(b);
			}
		}

		[Test]
		public void TestFloat()
		{
			InternalTestFloats(false);
		}

		[Test]
		public void TestFloatPrepared()
		{
			if (Version < new Version(4, 1)) return;

			InternalTestFloats(true);
		}

		private void InternalTestFloats(bool prepared)
		{
			execSQL("CREATE TABLE Test (fl FLOAT, db DOUBLE, dec1 DECIMAL(5,2))");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (?fl, ?db, ?dec)", conn);
			cmd.Parameters.Add("?fl", MySqlDbType.Float);
			cmd.Parameters.Add("?db", MySqlDbType.Double);
			cmd.Parameters.Add("?dec", MySqlDbType.Decimal);
			cmd.Parameters[0].Value = 2.3;
			cmd.Parameters[1].Value = 4.6;
			cmd.Parameters[2].Value = 23.82;
			if (prepared)
				cmd.Prepare();
			int count = cmd.ExecuteNonQuery();
			Assert.AreEqual(1, count);

			cmd.Parameters[0].Value = 1.5;
			cmd.Parameters[1].Value = 47.85;
			cmd.Parameters[2].Value = 123.85;
			count = cmd.ExecuteNonQuery();
			Assert.AreEqual(1, count);

			cmd.CommandText = "SELECT * FROM Test";
			if (prepared)
				cmd.Prepare();
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2.3, (decimal)reader.GetFloat(0));
				Assert.AreEqual(4.6, reader.GetDouble(1));
				Assert.AreEqual(23.82, reader.GetDecimal(2));

				Assert.IsTrue(reader.Read());
				Assert.AreEqual(1.5, (decimal)reader.GetFloat(0));
				Assert.AreEqual(47.85, reader.GetDouble(1));
				Assert.AreEqual(123.85, reader.GetDecimal(2));
			}
		}

		[Test]
		public void TestTime()
		{
			execSQL("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(100), d DATE, dt DATETIME, tm TIME,  PRIMARY KEY(id))");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test (id, tm) VALUES (1, '00:00')", conn);
			cmd.ExecuteNonQuery();
			cmd.CommandText = "INSERT INTO Test (id, tm) VALUES (2, '512:45:17')";
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT * FROM Test";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();

				object value = reader["tm"];
				Assert.AreEqual(value.GetType(), typeof(TimeSpan));
				TimeSpan ts = (TimeSpan)reader["tm"];
				Assert.AreEqual(0, ts.Hours);
				Assert.AreEqual(0, ts.Minutes);
				Assert.AreEqual(0, ts.Seconds);

				reader.Read();
				value = reader["tm"];
				Assert.AreEqual(value.GetType(), typeof(TimeSpan));
				ts = (TimeSpan)reader["tm"];
				Assert.AreEqual(21, ts.Days);
				Assert.AreEqual(8, ts.Hours);
				Assert.AreEqual(45, ts.Minutes);
				Assert.AreEqual(17, ts.Seconds);
			}
		}

		[Test]
		public void YearType()
		{
			execSQL("CREATE TABLE Test (yr YEAR)");
			execSQL("INSERT INTO Test VALUES (98)");
			execSQL("INSERT INTO Test VALUES (1990)");
			execSQL("INSERT INTO Test VALUES (2004)");
			execSQL("SET SQL_MODE=''");
			execSQL("INSERT INTO Test VALUES (111111111111111111111)");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				Assert.AreEqual(1998, reader.GetUInt32(0));
				reader.Read();
				Assert.AreEqual(1990, reader.GetUInt32(0));
				reader.Read();
				Assert.AreEqual(2004, reader.GetUInt32(0));
				reader.Read();
				Assert.AreEqual(0, reader.GetUInt32(0));
			}
		}

		[Test]
		public void TypeCoercion()
		{
			MySqlParameter p = new MySqlParameter("?test", 1);
			Assert.AreEqual(DbType.Int32, p.DbType);
			Assert.AreEqual(MySqlDbType.Int32, p.MySqlDbType);

			p.DbType = DbType.Int64;
			Assert.AreEqual(DbType.Int64, p.DbType);
			Assert.AreEqual(MySqlDbType.Int64, p.MySqlDbType);

			p.MySqlDbType = MySqlDbType.Int16;
			Assert.AreEqual(DbType.Int16, p.DbType);
			Assert.AreEqual(MySqlDbType.Int16, p.MySqlDbType);
		}

		[Test]
		public void AggregateTypesTest()
		{
			execSQL("CREATE TABLE foo (abigint bigint, aint int)");
			execSQL("INSERT INTO foo VALUES (1, 2)");
			execSQL("INSERT INTO foo VALUES (2, 3)");
			execSQL("INSERT INTO foo VALUES (3, 4)");
			execSQL("INSERT INTO foo VALUES (3, 5)");

			// Try a normal query
			string NORMAL_QRY = "SELECT abigint, aint FROM foo WHERE abigint = {0}";
			string qry = String.Format(NORMAL_QRY, 3);
			MySqlCommand cmd = new MySqlCommand(qry, conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					reader.GetInt64(0);
					reader.GetInt32(1); // <--- aint... this succeeds
				}
			}

			cmd.CommandText = "SELECT abigint, max(aint) FROM foo GROUP BY abigint";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					reader.GetInt64(0);
					reader.GetInt64(1); // <--- max(aint)... this fails
				}
			}
		}

		/*		[Test]
				public void TypeBoundaries()
				{
					execSQL("CREATE TABLE Test ( MaxDouble DOUBLE, MinDouble DOUBLE, MaxFloat FLOAT, MinFloat FLOAT )");

					MySqlCommand cmd = new MySqlCommand(
						"INSERT Test (MaxDouble, MinDouble, MaxFloat, MinFloat) VALUES " +
						"(?maxDouble, ?minDouble, ?maxFloat, ?minFloat)", conn);
					cmd.Parameters.Add("?maxDouble", MySqlDouble.MaxValue);
					cmd.Parameters.Add("?minDouble", MySqlDouble.MinValue);
					cmd.Parameters.Add("?maxFloat", MySqlFloat.MaxValue);
					cmd.Parameters.Add("?minFloat", MySqlFloat.MinValue);
					cmd.ExecuteNonQuery();

					cmd.CommandText = "SELECT * FROM Test";
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						reader.Read();
						Assert.AreEqual(MySqlDouble.MaxValue, reader.GetDouble(0));
						Assert.AreEqual(MySqlDouble.MinValue, reader.GetDouble(1));
						Assert.AreEqual(MySqlFloat.MaxValue, reader.GetFloat(2));
						Assert.AreEqual(MySqlFloat.MinValue, reader.GetFloat(3));
					}
				}*/

		[Test]
		public void BitAndDecimal()
		{
			execSQL("CREATE TABLE Test (bt1 BIT(2), bt4 BIT(4), bt11 BIT(11), bt23 BIT(23), bt32 BIT(32)) engine=myisam");
			execSQL("INSERT INTO Test VALUES (2, 3, 120, 240, 1000)");
			execSQL("INSERT INTO Test VALUES (NULL, NULL, 100, NULL, NULL)");

			string connStr = GetConnectionString(true) + ";treat tiny as boolean=false";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();

				MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", c);
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					Assert.IsTrue(reader.Read());
					Assert.AreEqual(2, reader.GetInt32(0));
					Assert.AreEqual(3, reader.GetInt32(1));
					Assert.AreEqual(120, reader.GetInt32(2));
					if (Version >= new Version(5, 0))
					{
						Assert.AreEqual(240, reader.GetInt32(3));
						Assert.AreEqual(1000, reader.GetInt32(4));
					}
					else
					{
						Assert.AreEqual(127, reader.GetInt32(3));
						Assert.AreEqual(127, reader.GetInt32(4));
					}

					Assert.IsTrue(reader.Read());
					Assert.IsTrue(reader.IsDBNull(0));
					Assert.IsTrue(reader.IsDBNull(1));
					Assert.AreEqual(100, reader.GetInt32(2));
					Assert.IsTrue(reader.IsDBNull(3));
					Assert.IsTrue(reader.IsDBNull(4));
				}
			}
		}

		[Test]
		public void DecimalTests()
		{
			execSQL("CREATE TABLE Test (val decimal(10,1))");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES(?dec)", conn);
			cmd.Parameters.AddWithValue("?dec", (decimal)2.4);
			Assert.AreEqual(1, cmd.ExecuteNonQuery());

			cmd.Prepare();
			Assert.AreEqual(1, cmd.ExecuteNonQuery());

			cmd.CommandText = "SELECT * FROM Test";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				Assert.IsTrue(reader.Read());
				Assert.IsTrue(reader[0] is Decimal);
				Assert.AreEqual(2.4, reader[0]);

				Assert.IsTrue(reader.Read());
				Assert.IsTrue(reader[0] is Decimal);
				Assert.AreEqual(2.4, reader[0]);

				Assert.IsFalse(reader.Read());
				Assert.IsFalse(reader.NextResult());
			}
		}

		[Test]
		public void DecimalTests2()
		{
			execSQL("CREATE TABLE Test (val decimal(10,1))");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES(?dec)", conn);
			cmd.Parameters.AddWithValue("?dec", (decimal)2.4);
			Assert.AreEqual(1, cmd.ExecuteNonQuery());

			cmd.Prepare();
			Assert.AreEqual(1, cmd.ExecuteNonQuery());

			cmd.CommandText = "SELECT * FROM Test";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				Assert.IsTrue(reader.Read());
				Assert.IsTrue(reader[0] is Decimal);
				Assert.AreEqual(2.4, reader[0]);

				Assert.IsTrue(reader.Read());
				Assert.IsTrue(reader[0] is Decimal);
				Assert.AreEqual(2.4, reader[0]);

				Assert.IsFalse(reader.Read());
				Assert.IsFalse(reader.NextResult());
			}
		}

		[Test]
		public void Bit()
		{
			if (Version < new Version(5, 0)) return;

			execSQL("CREATE TABLE Test (bit1 BIT, bit2 BIT(5), bit3 BIT(10))");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (?b1, ?b2, ?b3)", conn);
			cmd.Parameters.Add(new MySqlParameter("?b1", MySqlDbType.Bit));
			cmd.Parameters.Add(new MySqlParameter("?b2", MySqlDbType.Bit));
			cmd.Parameters.Add(new MySqlParameter("?b3", MySqlDbType.Bit));
			cmd.Prepare();
			cmd.Parameters[0].Value = 1;
			cmd.Parameters[1].Value = 2;
			cmd.Parameters[2].Value = 3;
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT * FROM Test";
			cmd.Prepare();
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(1, reader[0]);
				Assert.AreEqual(2, reader[1]);
				Assert.AreEqual(3, reader[2]);
			}
		}

		/// <summary>
		/// Bug #25605 BINARY and VARBINARY is returned as a string
		/// </summary>
		[Test]
		public void BinaryAndVarBinary()
		{
			MySqlCommand cmd = new MySqlCommand("SELECT BINARY 'something' AS BinaryData", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				byte[] buffer = new byte[2];
				long read = reader.GetBytes(0, 0, buffer, 0, 2);
				Assert.AreEqual('s', buffer[0]);
				Assert.AreEqual('o', buffer[1]);
				Assert.AreEqual(2, read);

				string s = reader.GetString(0);
				Assert.AreEqual("something", s);
			}
		}

		[Test]
		public void NumericAsBinary()
		{
			MySqlCommand cmd = new MySqlCommand("SELECT IFNULL(NULL,0) AS MyServerID", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				Assert.AreEqual("BIGINT", reader.GetDataTypeName(0));
				Assert.AreEqual(typeof(Int64), reader.GetFieldType(0));
				Assert.AreEqual("System.Int64", reader.GetValue(0).GetType().FullName);
				Assert.AreEqual(0, reader.GetValue(0));
			}
		}

		[Test]
		public void Binary16AsGuid()
		{
			if (Version < new Version(5, 0)) return;

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, g BINARY(16), c VARBINARY(16), c1 BINARY(255))");

			string connStr = GetConnectionString(true) + ";old guids=true";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();
				Guid g = Guid.NewGuid();
				byte[] bytes = g.ToByteArray();

				MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (1, @g, @c, @c1)", c);
				cmd.Parameters.AddWithValue("@g", bytes);
				cmd.Parameters.AddWithValue("@c", bytes);
				cmd.Parameters.AddWithValue("@c1", g.ToString());
				cmd.ExecuteNonQuery();

				string s = BitConverter.ToString(bytes);

				s = s.Replace("-", "");
				string sql = String.Format("TRUNCATE TABLE Test;INSERT INTO Test VALUES(1,0x{0},NULL,NULL)", s);
				execSQL(sql);

				cmd.CommandText = "SELECT * FROM Test";
				cmd.Parameters.Clear();
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					reader.Read();
					Guid g1 = reader.GetGuid(1);
					Assert.AreEqual(g, g1);
				}
			}
		}

		/// <summary>
		/// Bug #35041 'Binary(16) as GUID' - columns lose IsGuid value after a NULL value found
		/// </summary>
		[Test]
		public void Binary16AsGuidWithNull()
		{
			execSQL(@"CREATE TABLE Test (id int(10) NOT NULL AUTO_INCREMENT,
						AGUID binary(16), PRIMARY KEY (id))");
			Guid g = new Guid();
			byte[] guid = g.ToByteArray();
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (NULL, @g)", conn);
			cmd.Parameters.AddWithValue("@g", guid);
			cmd.ExecuteNonQuery();
			execSQL("insert into Test (AGUID) values (NULL)");
			cmd.ExecuteNonQuery();
		}

		/// <summary>
		/// Bug #36313 BIT result is lost in the left outer join
		/// </summary>
		[Test]
		public void BitInLeftOuterJoin()
		{
			if (Version < new Version(5, 0)) return;

			execSQL(@"CREATE TABLE Main (Id int(10) unsigned NOT NULL AUTO_INCREMENT,
				Descr varchar(45) NOT NULL, PRIMARY KEY (`Id`))
				ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1");
			execSQL(@"INSERT INTO Main (Id,Descr) VALUES (1,'AAA'), (2,'BBB'), (3, 'CCC')");

			execSQL(@"CREATE TABLE Child (Id int(10) unsigned NOT NULL AUTO_INCREMENT,
				MainId int(10) unsigned NOT NULL, Value int(10) unsigned NOT NULL,
				Enabled bit(1) NOT NULL, PRIMARY KEY (`Id`))
				ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1");
			execSQL(@"INSERT INTO Child (Id, MainId, Value, Enabled) VALUES (1,2,12345,0x01)");
		}

		/// <summary>
		/// Bug #36081 Get Unknown Datatype in C# .Net
		/// </summary>
		[Test]
		public void GeometryType()
		{
			if (Version < new Version(5, 0)) return;

			execSQL(@"CREATE TABLE Test (ID int(11) NOT NULL, ogc_geom geometry NOT NULL,
				PRIMARY KEY  (`ID`))");
			execSQL(@"INSERT INTO Test VALUES (1,
				GeomFromText('GeometryCollection(Point(1 1), LineString(2 2, 3 3))'))");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
			}
		}

		/// <summary>
		/// Bug #33322 Incorrect Double/Single value saved to MySQL database using MySQL Connector for
		/// </summary>
		[Test]
		public void StoringAndRetrievingDouble()
		{
			if (version.Major < 5) return;

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (v DOUBLE(25,20) NOT NULL)");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (?v)", conn);
			cmd.Parameters.Add("?v", MySqlDbType.Double);
			cmd.Parameters[0].Value = Math.PI;
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT * FROM Test";
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				double d = reader.GetDouble(0);
				Assert.AreEqual(Math.PI, d);
			}
		}

		/// <summary>
		/// Bug #40571  	Add GetSByte to the list of public methods supported by MySqlDataReader
		/// </summary>
		[Test]
		public void SByteFromReader()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (c1 TINYINT, c2 TINYINT UNSIGNED)");
			execSQL("INSERT INTO Test VALUES (99, 217)");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				Assert.AreEqual(99, reader.GetSByte(0));
				Assert.AreEqual(217, reader.GetByte(1));
				Assert.AreEqual(99, reader.GetByte(0));
			}
		}

		[Test]
		public void NewGuidDataType()
		{
			execSQL("CREATE TABLE Test(id INT, g BINARY(16))");

			string connStr = GetConnectionString(true) + ";old guids=true";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();
				Guid guid = Guid.NewGuid();
				MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES(1, @g)", c);
				cmd.Parameters.Add(new MySqlParameter("@g", MySqlDbType.Guid));
				cmd.Parameters[0].Value = guid;
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Bug #44507 Binary(16) considered as Guid
		/// </summary>
		[Test]
		public void ReadBinary16AsBinary()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, guid BINARY(16))");

			string connStr = GetConnectionString(true) + ";old guids=true";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();

				Guid g = new Guid("32A48AC5-285A-46c6-A0D4-158E6E39729C");
				MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (1, ?guid)", c);
				//MySqlParameter p = new MySqlParameter();
				//p.ParameterName = "guid";
				//p.Value = Guid.NewGuid();
				cmd.Parameters.AddWithValue("guid", Guid.NewGuid());
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";
				cmd.Parameters.Clear();
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					reader.Read();

					object o = reader.GetValue(1);
					Assert.IsTrue(o is Guid);

					byte[] bytes = new byte[16];
					long size = reader.GetBytes(1, 0, bytes, 0, 16);
					Assert.AreEqual(16, size);
				}
			}
		}

		[Test]
		public void ReadingUUIDAsGuid()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, guid CHAR(36))");
			execSQL("INSERT INTO Test VALUES (1, UUID())");

			MySqlCommand cmd = new MySqlCommand("SELECT CONCAT('A', guid) FROM Test", conn);
			string serverGuidStr = cmd.ExecuteScalar().ToString().Substring(1);
			Guid serverGuid = new Guid(serverGuidStr);

			cmd.CommandText = "SELECT guid FROM Test";
			Guid g = (Guid)cmd.ExecuteScalar();
			Assert.AreEqual(serverGuid, g);
		}

		[Test]
		public void NewGuidType()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, guid CHAR(36))");

			Guid g = Guid.NewGuid();
			MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES(1, @g)", conn);
			cmd.Parameters.AddWithValue("@g", g);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT guid FROM Test";
			Guid readG = (Guid)cmd.ExecuteScalar();
			Assert.AreEqual(g, readG);
		}

		/// <summary>
		/// Bug #47928 Old Guids=true setting is lost after null value is
		/// encountered in a Binary(16)
		/// </summary>
		[Test]
		public void OldGuidsWithNull()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, guid BINARY(16))");

			string connStr = GetConnectionString(true) + ";old guids=true";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();

				MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES (1, ?guid)", c);
				cmd.Parameters.AddWithValue("guid", Guid.NewGuid());
				cmd.ExecuteNonQuery();

				cmd.Parameters["guid"].Value = null;
				cmd.ExecuteNonQuery();
				cmd.Parameters["guid"].Value = Guid.NewGuid();
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT guid FROM Test";
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					//In Bug #47928, following loop will crash after encountering
					// null value.
					while (reader.Read())
					{
						object o = reader.GetValue(0);
					}
				}
			}
		}

		/// <summary>
		/// Bug #47985	UTF-8 String Length Issue (guids etc)
		/// </summary>
		[Test]
		public void UTF8Char12AsGuid()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, name CHAR(12) CHARSET utf8)");
			execSQL("INSERT INTO Test VALUES (1, 'Name')");

			string connStr = GetConnectionString(true) + ";charset=utf8";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();

				MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", c);
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
					reader.Read();
					string s = reader.GetString(1);
					Assert.AreEqual("Name", s);
				}
			}
		}

		/// <summary>
		/// Bug #48100	Impossible to retrieve decimal value if it doesn't fit into .Net System.Decimal
		/// </summary>
		[Test]
		public void MySqlDecimal()
		{
			if (Version < new Version(5, 0)) return;

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, dec1 DECIMAL(36,2))");
			execSQL("INSERT INTO Test VALUES (1, 9999999999999999999999999999999999.99)");

			MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				MySqlDecimal dec = reader.GetMySqlDecimal(1);
				string s = dec.ToString();
				Assert.AreEqual(9999999999999999999999999999999999.99, dec.ToDouble());
				Assert.AreEqual("9999999999999999999999999999999999.99", dec.ToString());
				try
				{
					decimal d = dec.Value;
					Assert.Fail("this should have failed");
				}
				catch (Exception)
				{
				}
			}
		}

		/// <summary>
		/// Bug #55644 Value was either too large or too small for a Double
		/// </summary>
		[Test]
		public void DoubleMinValue()
		{
			execSQL("DROP TABLE IF EXISTS test");
			execSQL("CREATE TABLE test(dbl double)");
			MySqlCommand cmd = new MySqlCommand("insert into test values(?param1)");
			cmd.Connection = conn;
			cmd.Parameters.Add(new MySqlParameter("?param1", MySqlDbType.Double));
			cmd.Parameters["?param1"].Value = Double.MinValue;
			cmd.ExecuteNonQuery();
			cmd.Parameters["?param1"].Value = Double.MaxValue;
			cmd.ExecuteNonQuery();

			cmd = new MySqlCommand("SELECT * FROM test", conn);
			using (MySqlDataReader reader = cmd.ExecuteReader())
			{
				reader.Read();
				double d = reader.GetDouble(0);
				Assert.AreEqual(d, double.MinValue);
				reader.Read();
				d = reader.GetDouble(0);
				Assert.AreEqual(d, double.MaxValue);
			}
		}

		/// <summary>
		/// Bug #58373	ReadInteger problem
		/// </summary>
		[Test]
		public void BigIntAutoInc()
		{
			execSQL("DROP TABLE IF EXISTS test");
			execSQL("CREATE TABLE test(ID bigint unsigned AUTO_INCREMENT NOT NULL PRIMARY KEY, name VARCHAR(20))");

			MySqlCommand cmd = new MySqlCommand("INSERT INTO test VALUES (@id, 'boo')", conn);
			ulong val = UInt64.MaxValue;
			val -= 100;
			cmd.Parameters.AddWithValue("@id", val);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "INSERT INTO test (name) VALUES ('boo2')";
			cmd.ExecuteNonQuery();
		}
	}
}