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
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;

namespace MariaDB.Data.Entity
{
    internal class UpdateGenerator : SqlGenerator
    {
        public override string GenerateSQL(DbCommandTree tree)
        {
            DbUpdateCommandTree commandTree = tree as DbUpdateCommandTree;

            UpdateStatement statement = new UpdateStatement();

            //scope.Push(commandTree.Target.VariableName);
            statement.Target = commandTree.Target.Expression.Accept(this);

            foreach (DbSetClause setClause in commandTree.SetClauses)
            {
                statement.Properties.Add(setClause.Property.Accept(this));
                DbExpression value = setClause.Value;
                SqlFragment valueFragment = value.Accept(this);
                statement.Values.Add(valueFragment);

                if (values == null)
                    values = new Dictionary<EdmMember, SqlFragment>();

                if (value.ExpressionKind != DbExpressionKind.Null)
                {
                    EdmMember property = ((DbPropertyExpression)setClause.Property).Property;
                    values.Add(property, valueFragment);
                }
            }

            statement.Where = commandTree.Predicate.Accept(this);

            if (commandTree.Returning != null)
                statement.ReturningSelect = GenerateReturningSql(commandTree, commandTree.Returning);

            return statement.ToString();
        }
    }
}