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

using Microsoft.Data.Entity.Query.Sql;
using System.Collections.Generic;
using Microsoft.Data.Entity.Storage;
using System;
using System.Data.Common;

namespace MariaDB.Data.Entity
{
    internal class InsertGenerator : ISqlQueryGenerator
    {
        public IRelationalValueBufferFactory CreateValueBufferFactory(IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory, DbDataReader dataReader)
        {
            throw new NotImplementedException();
        }

        public RelationalCommand GenerateSql(IDictionary<string, object> parameterValues)
        {
            DbInsertCommandTree commandTree = tree as DbInsertCommandTree;

            InsertStatement statement = new InsertStatement();

            DbExpressionBinding e = commandTree.Target;
            statement.Target = (InputFragment)e.Expression.Accept(this);

            foreach (DbSetClause setClause in commandTree.SetClauses)
                statement.Sets.Add(setClause.Property.Accept(this));

            foreach (DbSetClause setClause in commandTree.SetClauses)
            {
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

            if (commandTree.Returning != null)
                statement.ReturningSelect = GenerateReturningSql(commandTree, commandTree.Returning);

            return statement.ToString();

        }
    }
}