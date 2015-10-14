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

using System.Data.Common.CommandTrees;

namespace MariaDB.Data.Entity
{
    internal class DeleteGenerator : SqlGenerator
    {
        public override string GenerateSQL(DbCommandTree tree)
        {
            DbDeleteCommandTree commandTree = tree as DbDeleteCommandTree;

            DeleteStatement statement = new DeleteStatement();
            //scope.Push(null);
            statement.Target = commandTree.Target.Expression.Accept(this);

            statement.Where = commandTree.Predicate.Accept(this);

            return statement.ToString();
        }
    }
}