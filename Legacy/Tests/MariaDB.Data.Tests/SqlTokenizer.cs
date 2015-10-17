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

using System.Reflection;
using NUnit.Framework;

namespace MariaDB.Data.MySqlClient.Tests
{
    /// <summary>
    ///
    /// </summary>
    internal class SqlTokenizer
    {
        private object tokenizer;

        public SqlTokenizer(string sql)
        {
            tokenizer = typeof(MySqlConnection).Assembly.CreateInstance("MySql.Data.MySqlClient.MySqlTokenizer",
                false, System.Reflection.BindingFlags.CreateInstance, null,
                    new object[] { sql }, null, null);
        }

        public bool ReturnComments
        {
            set
            {
                PropertyInfo pi = tokenizer.GetType().GetProperty("ReturnComments");
                pi.SetValue(tokenizer, value, null);
            }
        }

        public bool AnsiQuotes
        {
            set
            {
                PropertyInfo pi = tokenizer.GetType().GetProperty("AnsiQuotes");
                pi.SetValue(tokenizer, value, null);
            }
        }

        public bool SqlServerMode
        {
            set
            {
                PropertyInfo pi = tokenizer.GetType().GetProperty("SqlServerMode");
                pi.SetValue(tokenizer, value, null);
            }
        }

        public bool Quoted
        {
            get
            {
                PropertyInfo pi = tokenizer.GetType().GetProperty("Quoted");
                return (bool)pi.GetValue(tokenizer, null);
            }
        }

        public string NextToken()
        {
            return (string)tokenizer.GetType().InvokeMember("NextToken",
                System.Reflection.BindingFlags.InvokeMethod,
                null, tokenizer, null);
        }

        public string NextParameter()
        {
            return (string)tokenizer.GetType().InvokeMember("NextParameter",
                System.Reflection.BindingFlags.InvokeMethod,
                null, tokenizer, null);
        }
    }
}