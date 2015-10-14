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
using System.Data;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;

namespace MariaDB.Data.Entity
{
    internal class FunctionGenerator : SqlGenerator
    {
        public CommandType CommandType { get; private set; }

        public override string GenerateSQL(DbCommandTree commandTree)
        {
            DbFunctionCommandTree tree = (commandTree as DbFunctionCommandTree);
            EdmFunction function = tree.EdmFunction;
            CommandType = CommandType.StoredProcedure;

            string cmdText = (string)function.MetadataProperties["CommandTextAttribute"].Value;
            if (String.IsNullOrEmpty(cmdText))
            {
                string schema = (string)function.MetadataProperties["Schema"].Value;
                if (String.IsNullOrEmpty(schema))
                    schema = function.NamespaceName;

                string functionName = (string)function.MetadataProperties["StoreFunctionNameAttribute"].Value;
                if (String.IsNullOrEmpty(functionName))
                    functionName = function.Name;

                return String.Format("`{0}`", functionName);
            }
            else
            {
                CommandType = CommandType.Text;
                return cmdText;
            }
        }
    }
}