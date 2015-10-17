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
using System.Configuration.Provider;
using MariaDB.Data.MySqlClient;
using MariaDB.Web.Properties;

namespace MariaDB.Web.General
{
    internal class Application
    {
        private int _id;
        private string _desc;

        public Application(string name, string desc)
        {
            Id = -1;
            Name = name;
            Description = desc;
        }

        public int Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        public string Name;

        public string Description
        {
            get { return _desc; }
            private set { _desc = value; }
        }

        public int FetchId(MySqlConnection connection)
        {
            if (Id == -1)
            {
                MySqlCommand cmd = new MySqlCommand(
                    @"SELECT id FROM my_aspnet_Applications WHERE name=@name", connection);
                cmd.Parameters.AddWithValue("@name", Name);
                object id = cmd.ExecuteScalar();
                Id = id == null ? -1 : Convert.ToInt32(id);
            }
            return Id;
        }

        /// <summary>
        /// Creates the or fetch application id.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationId">The application id.</param>
        /// <param name="applicationDesc">The application desc.</param>
        /// <param name="connection">The connection.</param>
        public int EnsureId(MySqlConnection connection)
        {
            // first try and retrieve the existing id
            if (FetchId(connection) <= 0)
            {
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO my_aspnet_Applications VALUES (NULL, @appName, @appDesc)", connection);
                cmd.Parameters.AddWithValue("@appName", Name);
                cmd.Parameters.AddWithValue("@appDesc", Description);
                int recordsAffected = cmd.ExecuteNonQuery();
                if (recordsAffected != 1)
                    throw new ProviderException(Resources.UnableToCreateApplication);

                Id = Convert.ToInt32(cmd.LastInsertedId);
            }
            return Id;
        }
    }
}