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
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Diagnostics;
using System.Resources;
using MariaDB.Data.MySqlClient;
using MariaDB.Web.Properties;

namespace MariaDB.Web.Common
{
    /// <summary>
    ///
    /// </summary>
    public static class SchemaManager
    {
        private const int schemaVersion = 6;

        /// <summary>
        /// Gets the most recent version of the schema.
        /// </summary>
        /// <value>The most recent version number of the schema.</value>
        public static int Version
        {
            get { return schemaVersion; }
        }

        internal static void CheckSchema(string connectionString, NameValueCollection config)
        {
            int ver = GetSchemaVersion(connectionString);
            if (ver == Version) return;

            try
            {
                if (String.Compare(config["autogenerateschema"], "true", true) == 0)
                    UpgradeToCurrent(connectionString, ver);
                else
                    throw new ProviderException(Resources.MissingOrWrongSchema);
            }
            catch (Exception ex)
            {
                throw new ProviderException(Resources.MissingOrWrongSchema, ex);
            }
        }

        private static void UpgradeToCurrent(string connectionString, int version)
        {
            ResourceManager r = new ResourceManager("MySql.Web.Properties.Resources",
                typeof(SchemaManager).Assembly);

            if (version == Version) return;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                for (int ver = version + 1; ver <= Version; ver++)
                {
                    string schema = r.GetString(String.Format("schema{0}", ver));
                    MySqlScript script = new MySqlScript(connection);
                    script.Query = schema;
                    script.Execute();
                }
            }
        }

        private static int GetSchemaVersion(string connectionString)
        {
            // retrieve the current schema version
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM my_aspnet_SchemaVersion", conn);
                try
                {
                    object ver = cmd.ExecuteScalar();
                    if (ver != null)
                        return (int)ver;
                }
                catch (MySqlException ex)
                {
                    if (ex.Number != (int)MySqlErrorCode.NoSuchTable)
                        throw;
                    string[] restrictions = new string[4];
                    restrictions[2] = "mysql_Membership";
                    DataTable dt = conn.GetSchema("Tables", restrictions);
                    if (dt.Rows.Count == 1)
                        return Convert.ToInt32(dt.Rows[0]["TABLE_COMMENT"]);
                }
                return 0;
            }
        }

        /// <summary>
        /// Creates the or fetch user id.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="username">The username.</param>
        /// <param name="applicationId">The application id.</param>
        /// <param name="authenticated">if set to <c>true</c> [authenticated].</param>
        /// <returns></returns>
        internal static int CreateOrFetchUserId(MySqlConnection connection, string username,
            int applicationId, bool authenticated)
        {
            Debug.Assert(applicationId > 0);

            // first attempt to fetch an existing user id
            MySqlCommand cmd = new MySqlCommand(@"SELECT id FROM my_aspnet_Users
                WHERE applicationId = @appId AND name = @name", connection);
            cmd.Parameters.AddWithValue("@appId", applicationId);
            cmd.Parameters.AddWithValue("@name", username);
            object userId = cmd.ExecuteScalar();
            if (userId != null) return (int)userId;

            cmd.CommandText = @"INSERT INTO my_aspnet_Users VALUES
                (NULL, @appId, @name, @isAnon, Now())";
            cmd.Parameters.AddWithValue("@isAnon", !authenticated);
            int recordsAffected = cmd.ExecuteNonQuery();
            if (recordsAffected != 1)
                throw new ProviderException(Resources.UnableToCreateUser);

            cmd.CommandText = "SELECT LAST_INSERT_ID()";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}