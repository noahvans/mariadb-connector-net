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

using System.Text;

namespace MariaDB.Data.Entity
{
    internal class JoinFragment : InputFragment
    {
        public SqlFragment Condition;
        public string JoinType;

        public override void WriteInnerSql(StringBuilder sql)
        {
            Left.WriteSql(sql);
            sql.AppendFormat(" {0} ", JoinType);
            Right.WriteSql(sql);
            if (Condition != null)
            {
                sql.Append(" ON ");
                Condition.WriteSql(sql);
            }
        }

        //public override void WriteSql(StringBuilder sql)
        //{
        //    // we don't want our join to write out its name
        //    string name = Name;
        //    Name = null;
        //    base.WriteSql(sql);
        //    Name = name;
        //}
    }
}