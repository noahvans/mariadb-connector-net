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

using System.Collections.Generic;
using System.Text;

namespace MariaDB.Data.Entity
{
    internal class UpdateStatement : SqlFragment
    {
        public UpdateStatement()
        {
            Properties = new List<SqlFragment>();
            Values = new List<SqlFragment>();
        }

        public SqlFragment Target { get; set; }
        public List<SqlFragment> Properties { get; private set; }
        public List<SqlFragment> Values { get; private set; }
        public SqlFragment Where { get; set; }
        public SelectStatement ReturningSelect;

        public override void WriteSql(StringBuilder sql)
        {
            sql.Append("UPDATE ");
            Target.WriteSql(sql);
            sql.Append(" SET ");

            string seperator = "";
            for (int i = 0; i < Properties.Count; i++)
            {
                sql.Append(seperator);
                Properties[i].WriteSql(sql);
                sql.Append("=");
                Values[i].WriteSql(sql);
                seperator = ", ";
            }
            if (Where != null)
            {
                sql.Append(" WHERE ");
                Where.WriteSql(sql);
            }
            if (ReturningSelect != null)
            {
                sql.Append(";\r\n");
                ReturningSelect.WriteSql(sql);
            }
        }
    }
}