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
	[TestFixture]
	public class ProcedureParameterTests : BaseTest
	{
		/// <summary>
		/// Bug #48586	Expose defined possible enum values
		/// </summary>
		[Test]
		public void PossibleValues()
		{
			if (Version < new Version(5, 0)) return;

			execSQL(@"CREATE  PROCEDURE spTest (id INT UNSIGNED ZEROFILL,
					dec1 DECIMAL(10,2),
					name VARCHAR(20) /* this is a comment */ CHARACTER SET ascii,
					t1 TINYTEXT BINARY, t2 ENUM('a','b','c'),
					t3 /* comment */ SET(/* comment */'1','2','3'))
					BEGIN SELECT name; END");
			MySqlCommand cmd = new MySqlCommand("spTest", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			//MySqlCommandBuilder.DeriveParameters(cmd);
			Assert.IsNull(cmd.Parameters["@id"].PossibleValues);
			Assert.IsNull(cmd.Parameters["@dec1"].PossibleValues);
			Assert.IsNull(cmd.Parameters["@name"].PossibleValues);
			Assert.IsNull(cmd.Parameters["@t1"].PossibleValues);
			MySqlParameter t2 = cmd.Parameters["@t2"];
			Assert.IsNotNull(t2.PossibleValues);
			Assert.AreEqual("a", t2.PossibleValues[0]);
			Assert.AreEqual("b", t2.PossibleValues[1]);
			Assert.AreEqual("c", t2.PossibleValues[2]);
			MySqlParameter t3 = cmd.Parameters["@t3"];
			Assert.IsNotNull(t3.PossibleValues);
			Assert.AreEqual("1", t3.PossibleValues[0]);
			Assert.AreEqual("2", t3.PossibleValues[1]);
			Assert.AreEqual("3", t3.PossibleValues[2]);
		}
	}
}