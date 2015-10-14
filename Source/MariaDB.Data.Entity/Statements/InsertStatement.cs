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
    internal class InsertStatement : SqlFragment
    {
        public InsertStatement()
        {
            Sets = new List<SqlFragment>();
            Values = new List<SqlFragment>();
        }

        public InputFragment Target { get; set; }
        public List<SqlFragment> Sets { get; private set; }
        public List<SqlFragment> Values { get; private set; }
        public SelectStatement ReturningSelect;

        public override void WriteSql(StringBuilder sql)
        {
            sql.Append("INSERT INTO ");
            Target.WriteSql(sql);
            if (Sets.Count > 0)
            {
                sql.Append("(");
                WriteList(Sets, sql);
                sql.Append(")");
            }
            sql.Append(" VALUES ");
            sql.Append("(");
            WriteList(Values, sql);
            sql.Append(")");

            if (ReturningSelect != null)
            {
                sql.Append(";\r\n");
                ReturningSelect.WriteSql(sql);
            }
        }
    }
}