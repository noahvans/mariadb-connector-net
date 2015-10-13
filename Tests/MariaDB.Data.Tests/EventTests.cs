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
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace MariaDB.Data.MySqlClient.Tests
{
	[TestFixture]
	public class EventTests : BaseTest
	{
		[Test]
		public void Warnings()
		{
			if (Version < new Version(4, 1)) return;

			execSQL("CREATE TABLE Test (name VARCHAR(10))");

			string connStr = GetConnectionString(true);
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();

				MySqlCommand cmd = new MySqlCommand("SET SQL_MODE=''", c);
				cmd.ExecuteNonQuery();

				c.InfoMessage += new MySqlInfoMessageEventHandler(WarningsInfoMessage);

				cmd.CommandText = "INSERT INTO Test VALUES ('12345678901')";
				using (MySqlDataReader reader = cmd.ExecuteReader())
				{
				}
			}
		}

		private void WarningsInfoMessage(object sender, MySqlInfoMessageEventArgs args)
		{
			Assert.AreEqual(1, args.errors.Length);
		}
		
		[Test]
		public void StateChange() 
		{
			MySqlConnection c = new MySqlConnection(GetConnectionString(true));
			c.StateChange += new StateChangeEventHandler(StateChangeHandler);
			c.Open();
			c.Close();
		}

		private void StateChangeHandler(object sender, StateChangeEventArgs e)
		{
		}		
	}
}
