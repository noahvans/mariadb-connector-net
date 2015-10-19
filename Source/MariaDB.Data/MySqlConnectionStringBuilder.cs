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

using MariaDB.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MariaDB.Data.MySqlClient
{
    public class MySqlConnectionStringBuilder : DbConnectionStringBuilder
    {
        private static Dictionary<string, string> validKeywords =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, PropertyDefaultValue> defaultValues =
            new Dictionary<string, PropertyDefaultValue>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, object> values =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private bool hasProcAccess = true;

        static MySqlConnectionStringBuilder()
        {
            // load up our valid keywords and default values only once
            Initialize();
        }

        public MySqlConnectionStringBuilder()
        {
            Clear();
        }

        public MySqlConnectionStringBuilder(string connStr)
            : this()
        {
            ConnectionString = connStr;
        }

        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        /// <value>The server.</value>
        [DefaultValue("")]
        [ValidKeywords("host, data source, datasource, address, addr, network address")]
        public string Server
        {
            get { return values["server"] as string; }
            set { SetValue("server", value); }
        }

        /// <summary>
        /// Gets or sets the name of the database the connection should
        /// initially connect to.
        /// </summary>
        [DefaultValue("")]
        [ValidKeywords("initial catalog")]
        public string Database
        {
            get { return values["database"] as string; }
            set { SetValue("database", value); }
        }

        /// <summary>
        /// Gets or sets the protocol that should be used for communicating
        /// with MySQL.
        /// </summary>
        [DefaultValue(MySqlConnectionProtocol.Sockets)]
        [ValidKeywords("protocol")]
        public MySqlConnectionProtocol ConnectionProtocol
        {
            get { return (MySqlConnectionProtocol)values["Connection Protocol"]; }
            set { SetValue("Connection Protocol", value); }
        }

        /// <summary>
        /// Gets or sets the name of the named pipe that should be used
        /// for communicating with MySQL.
        /// </summary>
        [DefaultValue("MYSQL")]
        [ValidKeywords("pipe")]
        public string PipeName
        {
            get { return (string)values["Pipe Name"]; }
            set { SetValue("Pipe Name", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether this connection
        /// should use compression.
        /// </summary>
        [DefaultValue(false)]
        [ValidKeywords("compress")]
        public bool UseCompression
        {
            get { return (bool)values["Use Compression"]; }
            set { SetValue("Use Compression", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether this connection will allow
        /// commands to send multiple SQL statements in one execution.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowBatch
        {
            get { return (bool)values["Allow Batch"]; }
            set { SetValue("Allow Batch", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether logging is enabled.
        /// </summary>
        [DefaultValue(false)]
        public bool Logging
        {
            get { return (bool)values["Logging"]; }
            set { SetValue("Logging", value); }
        }

        /// <summary>
        /// Gets or sets the base name of the shared memory objects used to
        /// communicate with MySQL when the shared memory protocol is being used.
        /// </summary>
        [DefaultValue("MYSQL")]
        public string SharedMemoryName
        {
            get { return (string)values["Shared Memory Name"]; }
            set { SetValue("Shared Memory Name", value); }
        }

        /// <summary>
        /// Gets or sets the port number that is used when the socket
        /// protocol is being used.
        /// </summary>
        [DefaultValue(3306)]
        public uint Port
        {
            get { return (uint)values["Port"]; }
            set { SetValue("Port", value); }
        }

        /// <summary>
        /// Gets or sets the connection timeout.
        /// </summary>
        [DefaultValue(15)]
        [ValidKeywords("connection timeout")]
        public uint ConnectionTimeout
        {
            get { return (uint)values["Connect Timeout"]; }

            set
            {
                // Timeout in milliseconds should not exceed maximum for 32 bit
                // signed integer (~24 days). We truncate the value if it exceeds
                // maximum (MySqlCommand.CommandTimeout uses the same technique
                uint timeout = Math.Min(value, Int32.MaxValue / 1000);
                SetValue("Connect Timeout", timeout);
            }
        }

        /// <summary>
        /// Gets or sets the default command timeout.
        /// </summary>
        [DefaultValue(30)]
        [ValidKeywords("command timeout")]
        public uint DefaultCommandTimeout
        {
            get { return (uint)values["Default Command Timeout"]; }
            set { SetValue("Default Command Timeout", value); }
        }

        /// <summary>
        /// Gets or sets the user id that should be used to connect with.
        /// </summary>
        [DefaultValue("")]
        [ValidKeywords("uid, username, user name, user")]
        public string UserID
        {
            get { return (string)values["User Id"]; }
            set { SetValue("User Id", value); }
        }

        /// <summary>
        /// Gets or sets the password that should be used to connect with.
        /// </summary>
        [DefaultValue("")]
        [ValidKeywords("pwd")]
        public string Password
        {
            get { return (string)values["Password"]; }
            set { SetValue("Password", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates if the password should be persisted
        /// in the connection string.
        /// </summary>
        [DefaultValue(false)]
        public bool PersistSecurityInfo
        {
            get { return (bool)values["Persist Security Info"]; }
            set { SetValue("Persist Security Info", value); }
        }

        [DefaultValue(null)]
        public string CertificateFile
        {
            get { return (string)values["Certificate File"]; }
            set
            {
                SetValue("Certificate File", value);
            }
        }

        [DefaultValue(null)]
        public string CertificatePassword
        {
            get { return (string)values["Certificate Password"]; }
            set
            {
                SetValue("Certificate Password", value);
            }
        }

        [DefaultValue(MySqlCertificateStoreLocation.None)]
        public MySqlCertificateStoreLocation CertificateStoreLocation
        {
            get { return (MySqlCertificateStoreLocation)values["Certificate Store Location"]; }
            set
            {
                SetValue("Certificate Store Location", value);
            }
        }

        [DefaultValue(null)]
        public string CertificateThumbprint
        {
            get { return (string)values["Certificate Thumbprint"]; }
            set
            {
                SetValue("Certificate Thumbprint", value);
            }
        }

        [DefaultValue(false)]
        public bool IntegratedSecurity
        {
            get
            {
                object val = values["Integrated Security"];
                return (bool)val;
            }
            set
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    throw new MySqlException("IntegratedSecurity is supported on Windows only");

                SetValue("Integrated Security", value);
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates if zero date time values are supported.
        /// </summary>
        [DefaultValue(false)]
        public bool AllowZeroDateTime
        {
            get { return (bool)values["Allow Zero Datetime"]; }
            set { SetValue("Allow Zero DateTime", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if zero datetime values should be
        /// converted to DateTime.MinValue.
        /// </summary>
        [DefaultValue(false)]
        public bool ConvertZeroDateTime
        {
            get { return (bool)values["Convert Zero Datetime"]; }
            set { SetValue("Convert Zero DateTime", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if the Usage Advisor should be enabled.
        /// </summary>
        [DefaultValue(false)]
        [ValidKeywords("usage advisor")]
        public bool UseUsageAdvisor
        {
            get { return (bool)values["Use Usage Advisor"]; }
            set { SetValue("Use Usage Advisor", value); }
        }

        /// <summary>
        /// Gets or sets the size of the stored procedure cache.
        /// </summary>
        [DefaultValue(25)]
        [ValidKeywords("procedure cache, procedurecache")]
        public uint ProcedureCacheSize
        {
            get { return (uint)values["Procedure Cache Size"]; }
            set { SetValue("Procedure Cache Size", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if the perfmon hooks should be enabled.
        /// </summary>
        [DefaultValue(false)]
        [ValidKeywords("userperfmon, perfmon")]
        public bool UsePerformanceMonitor
        {
            get { return (bool)values["Use Performance Monitor"]; }
            set { SetValue("Use Performance Monitor", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if calls to Prepare() should be ignored.
        /// </summary>
        [DefaultValue(true)]
        public bool IgnorePrepare
        {
            get { return (bool)values["Ignore Prepare"]; }
            set { SetValue("Ignore Prepare", value); }
        }

        [DefaultValue(true)]
        public bool AutoEnlist
        {
            get { return (bool)values["Auto Enlist"]; }
            set { SetValue("Auto Enlist", value); }
        }

        [DefaultValue(true)]
        public bool RespectBinaryFlags
        {
            get { return (bool)values["Respect Binary Flags"]; }
            set { SetValue("Respect Binary Flags", value); }
        }

        [DefaultValue(true)]
        public bool TreatTinyAsBoolean
        {
            get { return (bool)values["Treat Tiny As Boolean"]; }
            set { SetValue("Treat Tiny As Boolean", value); }
        }

        [DefaultValue(false)]
        public bool AllowUserVariables
        {
            get { return (bool)values["Allow User Variables"]; }
            set { SetValue("Allow User Variables", value); }
        }

        [DefaultValue(false)]
        [ValidKeywords("interactive")]
        public bool InteractiveSession
        {
            get { return (bool)values["Interactive Session"]; }
            set { SetValue("Interactive Session", value); }
        }

        [DefaultValue(false)]
        public bool FunctionsReturnString
        {
            get { return (bool)values["Functions Return String"]; }
            set { SetValue("Functions Return String", value); }
        }

        [DefaultValue(false)]
        public bool UseAffectedRows
        {
            get { return (bool)values["Use Affected Rows"]; }
            set { SetValue("Use Affected Rows", value); }
        }

        [DefaultValue(false)]
        public bool OldGuids
        {
            get { return (bool)values["Old Guids"]; }
            set { SetValue("Old Guids", value); }
        }

        [DefaultValue(0)]
        public uint Keepalive
        {
            get { return (uint)values["Keep Alive"]; }
            set { SetValue("Keep Alive", value); }
        }

        [DefaultValue(false)]
        [ValidKeywords("sqlservermode, sql server mode")]
        public bool SqlServerMode
        {
            get { return (bool)values["Sql Server Mode"]; }
            set { SetValue("Sql Server Mode", value); }
        }

        [DefaultValue(false)]
        [ValidKeywords("tablecache, table cache")]
        public bool TableCaching
        {
            get { return (bool)values["Table Cache"]; }
            set { SetValue("Table Cache", value); }
        }

        [DefaultValue(60)]
        public int DefaultTableCacheAge
        {
            get { return (int)values["Default Table Cache Age"]; }
            set { SetValue("Default Table Cache Age", value); }
        }

        [DefaultValue(true)]
        public bool CheckParameters
        {
            get { return (bool)values["Check Parameters"]; }
            set { SetValue("Check Parameters", value); }
        }

        [DefaultValue(false)]
        public bool Replication
        {
            get { return (bool)values["Replication"]; }
            set { SetValue("Replication", value); }
        }

        /// <summary>
        /// Gets or sets the lifetime of a pooled connection.
        /// </summary>
        [DefaultValue(0)]
        public uint ConnectionLifeTime
        {
            get { return (uint)values["Connection LifeTime"]; }
            set { SetValue("Connection LifeTime", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if connection pooling is enabled.
        /// </summary>
        [DefaultValue(true)]
        public bool Pooling
        {
            get { return (bool)values["Pooling"]; }
            set { SetValue("Pooling", value); }
        }

        /// <summary>
        /// Gets the minimum connection pool size.
        /// </summary>
        [DefaultValue(0)]
        [ValidKeywords("min pool size")]
        public uint MinimumPoolSize
        {
            get { return (uint)values["Minimum Pool Size"]; }
            set { SetValue("Minimum Pool Size", value); }
        }

        /// <summary>
        /// Gets or sets the maximum connection pool setting.
        /// </summary>
        [DefaultValue(100)]
        [ValidKeywords("max pool size")]
        public uint MaximumPoolSize
        {
            get { return (uint)values["Maximum Pool Size"]; }
            set { SetValue("Maximum Pool Size", value); }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating if the connection should be reset when retrieved
        /// from the pool.
        /// </summary>
        [DefaultValue(false)]
        public bool ConnectionReset
        {
            get { return (bool)values["Connection Reset"]; }
            set { SetValue("Connection Reset", value); }
        }

        [DefaultValue(false)]
        public bool CacheServerProperties
        {
            get { return (bool)values["Cache Server Properties"]; }
            set { SetValue("Cache Server Properties", value); }
        }

        /// <summary>
        /// Gets or sets the character set that should be used for sending queries to the server.
        /// </summary>
        [DefaultValue("")]
        [ValidKeywords("charset")]
        public string CharacterSet
        {
            get { return (string)values["Character Set"]; }
            set { SetValue("Character Set", value); }
        }

        /// <summary>
        /// Indicates whether the driver should treat binary blobs as UTF8
        /// </summary>
        [DefaultValue(false)]
        public bool TreatBlobsAsUTF8
        {
            get { return (bool)values["Treat Blobs As UTF8"]; }
            set { SetValue("Treat Blobs As UTF8", value); }
        }

        /// <summary>
        /// Gets or sets the pattern that matches the columns that should be treated as UTF8
        /// </summary>
        [DefaultValue("")]
        public string BlobAsUTF8IncludePattern
        {
            get { return (string)values["BlobAsUTF8IncludePattern"]; }
            set { SetValue("BlobAsUTF8IncludePattern", value); }
        }

        /// <summary>
        /// Gets or sets the pattern that matches the columns that should not be treated as UTF8
        /// </summary>
        [DefaultValue("")]
        public string BlobAsUTF8ExcludePattern
        {
            get { return (string)values["BlobAsUTF8ExcludePattern"]; }
            set { SetValue("BlobAsUTF8ExcludePattern", value); }
        }

        /// <summary>
        /// Indicates whether to use SSL connections and how to handle server certificate errors.
        /// </summary>
        [DefaultValue(MySqlSslMode.None)]
        public MySqlSslMode SslMode
        {
            get { return (MySqlSslMode)values["Ssl Mode"]; }
            set { SetValue("Ssl Mode", value); }
        }

        internal bool HasProcAccess
        {
            get { return hasProcAccess; }
            set { hasProcAccess = value; }
        }

        internal Regex GetBlobAsUTF8IncludeRegex()
        {
            if (String.IsNullOrEmpty(BlobAsUTF8IncludePattern)) return null;
            return new Regex(BlobAsUTF8IncludePattern);
        }

        internal Regex GetBlobAsUTF8ExcludeRegex()
        {
            if (String.IsNullOrEmpty(BlobAsUTF8ExcludePattern)) return null;
            return new Regex(BlobAsUTF8ExcludePattern);
        }

        public override bool ContainsKey(string keyword)
        {
            try
            {
                object value;
                ValidateKeyword(keyword);
                return values.TryGetValue(validKeywords[keyword], out value);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override object this[string keyword]
        {
            get { return values[validKeywords[keyword]]; }
            set
            {
                ValidateKeyword(keyword);
                if (value == null)
                    Remove(keyword);
                else
                    SetValue(keyword, value);
            }
        }

        public override void Clear()
        {
            base.Clear();

            // make a copy of our default values array
            foreach (string key in defaultValues.Keys)
                values[key] = defaultValues[key].DefaultValue;
        }

        public override bool Remove(string keyword)
        {
            ValidateKeyword(keyword);
            string primaryKey = validKeywords[keyword];

            values.Remove(primaryKey);
            base.Remove(primaryKey);

            values[primaryKey] = defaultValues[primaryKey].DefaultValue;
            return true;
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            ValidateKeyword(keyword);
            return values.TryGetValue(validKeywords[keyword], out value);
        }

        public string GetConnectionString(bool includePass)
        {
            if (includePass) return ConnectionString;

            StringBuilder conn = new StringBuilder();
            string delimiter = "";
            foreach (string key in this.Keys)
            {
                if (String.Compare(key, "password", true) == 0 ||
                    String.Compare(key, "pwd", true) == 0)
                    continue;
                conn.AppendFormat(CultureInfo.CurrentCulture, "{0}{1}={2}",
                    delimiter, key, this[key]);
                delimiter = ";";
            }
            return conn.ToString();
        }

        private void SetValue(string keyword, object value)
        {
            ValidateKeyword(keyword);
            keyword = validKeywords[keyword];

            Remove(keyword);

            NormalizeValue(keyword, ref value);

            object val = null;
            if (value is string && defaultValues[keyword].DefaultValue is Enum)
                val = ParseEnum(defaultValues[keyword].Type, (string)value, keyword);
            else if (value is string && string.IsNullOrEmpty(value.ToString()))
                val = defaultValues[keyword].DefaultValue;
            else
                val = ChangeType(value, defaultValues[keyword].Type);
            HandleObsolete(keyword, val);
            values[keyword] = val;
            base[keyword] = val;
        }

        private static void NormalizeValue(string keyword, ref object value)
        {
            // Handle special case "Integrated Security=SSPI"
            // Integrated Security is a logically bool parameter, SSPI value
            // for it is the same as "true" (SSPI is SQL Server legacy value
            if (keyword == "Integrated Security" && value is string &&
                ((string)value).ToLower() == "sspi")
            {
                value = true;
            }
        }

        private void HandleObsolete(string keyword, object value)
        {
            if (String.Compare(keyword, "Use Procedure Bodies", true) == 0)
                CheckParameters = (bool)value;
        }

        private object ParseEnum(Type t, string requestedValue, string key)
        {
            try
            {
                return Enum.Parse(t, requestedValue, true);
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException(String.Format(
                    ResourceStrings.InvalidConnectionStringValue, requestedValue, key));
            }
        }

        private object ChangeType(object value, Type t)
        {
            if (t == typeof(bool) && value is string)
            {
                string s = value.ToString().ToLower();
                if (s == "yes" || s == "true") return true;
                if (s == "no" || s == "false") return false;

                throw new FormatException(String.Format(ResourceStrings.InvalidValueForBoolean, value));
            }
            else
                return Convert.ChangeType(value, t, CultureInfo.CurrentCulture);
        }

        private void ValidateKeyword(string keyword)
        {
            string key = keyword.ToLower();
            if (!validKeywords.ContainsKey(key))
                throw new ArgumentException(ResourceStrings.KeywordNotSupported, keyword);
        }

        private static void Initialize()
        {
            PropertyInfo[] properties = typeof(MySqlConnectionStringBuilder).GetProperties();
            foreach (PropertyInfo pi in properties)
                AddKeywordFromProperty(pi);
            // remove this starting with 6.4
            PropertyInfo encrypt = typeof(MySqlConnectionStringBuilder).GetProperty(
                "Encrypt", BindingFlags.Instance | BindingFlags.NonPublic);
            AddKeywordFromProperty(encrypt);
        }

        private static void AddKeywordFromProperty(PropertyInfo pi)
        {
            string name = pi.Name.ToLower();
            string displayName = name;
            var attr = pi.GetCustomAttributes(false);
            validKeywords[name] = displayName;
            validKeywords[displayName] = displayName;

            foreach (Attribute a in attr)
            {
                if (a is ValidKeywordsAttribute)
                {
                    foreach (string keyword in (a as ValidKeywordsAttribute).Keywords)
                        validKeywords[keyword.ToLower().Trim()] = displayName;
                }
                else if (a is DefaultValueAttribute)
                {
                    defaultValues[displayName] = new PropertyDefaultValue(pi.PropertyType,
                            Convert.ChangeType((a as DefaultValueAttribute).Value, pi.PropertyType, CultureInfo.CurrentCulture));
                }
            }
        }
    }

    internal struct PropertyDefaultValue
    {
        public PropertyDefaultValue(Type t, object v)
        {
            Type = t;
            DefaultValue = v;
        }

        public Type Type;
        public object DefaultValue;
    }

    internal class ValidKeywordsAttribute : Attribute
    {
        private string keywords;

        public ValidKeywordsAttribute(string keywords)
        {
            this.keywords = keywords.ToLower();
        }

        public string[] Keywords
        {
            get { return keywords.Split(','); }
        }
    }
}