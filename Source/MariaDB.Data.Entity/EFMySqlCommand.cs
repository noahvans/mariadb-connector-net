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
using System.Data.Common;
using System.Data.Metadata.Edm;
using MariaDB.Data.MySqlClient;

namespace MariaDB.Data.Entity
{
    internal class EFMySqlCommand : DbCommand, ICloneable
    {
        private bool designTimeVisible = true;
        private DbConnection connection;
        private MySqlCommand command = new MySqlCommand();

        internal PrimitiveType[] ColumnTypes;

        #region Properties

        public override string CommandText
        {
            get { return command.CommandText; }
            set { command.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return command.CommandTimeout; }
            set { command.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return command.CommandType; }
            set { command.CommandType = value; }
        }

        public override bool DesignTimeVisible
        {
            get { return designTimeVisible; }
            set { designTimeVisible = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return connection; }
            set
            {
                connection = value;
                command.Connection = (MySqlConnection)value;
            }
        }

        protected override DbTransaction DbTransaction
        {
            get { return command.Transaction; }
            set { command.Transaction = (MySqlTransaction)value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return command.Parameters; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return command.UpdatedRowSource; }
            set { command.UpdatedRowSource = value; }
        }

        #endregion Properties

        public override void Cancel()
        {
            command.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return new MySqlParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return new EFMySqlDataReader(this, command.ExecuteReader(behavior));
        }

        public override int ExecuteNonQuery()
        {
            return command.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            return command.ExecuteScalar();
        }

        public override void Prepare()
        {
            command.Prepare();
        }

        #region ICloneable Members

        public object Clone()
        {
            EFMySqlCommand clone = new EFMySqlCommand();

            clone.connection = connection;
            clone.ColumnTypes = ColumnTypes;
            clone.command = (MySqlCommand)((ICloneable)command).Clone();

            return clone;
        }

        #endregion ICloneable Members
    }
}