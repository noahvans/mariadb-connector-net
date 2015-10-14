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
using MariaDB.Data.MySqlClient;

namespace MariaDB.Data.Types
{
    internal struct MySqlUByte : IMySqlValue
    {
        private byte mValue;
        private bool isNull;

        public MySqlUByte(bool isNull)
        {
            this.isNull = isNull;
            mValue = 0;
        }

        public MySqlUByte(byte val)
        {
            this.isNull = false;
            mValue = val;
        }

        #region IMySqlValue Members

        public bool IsNull
        {
            get { return isNull; }
        }

        MySqlDbType IMySqlValue.MySqlDbType
        {
            get { return MySqlDbType.UByte; }
        }

        DbType IMySqlValue.DbType
        {
            get { return DbType.Byte; }
        }

        object IMySqlValue.Value
        {
            get { return mValue; }
        }

        public byte Value
        {
            get { return mValue; }
        }

        Type IMySqlValue.SystemType
        {
            get { return typeof(byte); }
        }

        string IMySqlValue.MySqlTypeName
        {
            get { return "TINYINT"; }
        }

        void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
        {
            byte v = (val is byte) ? (byte)val : Convert.ToByte(val);
            if (binary)
                packet.WriteByte(v);
            else
                packet.WriteStringNoNull(v.ToString());
        }

        IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
        {
            if (nullVal)
                return new MySqlUByte(true);

            if (length == -1)
                return new MySqlUByte((byte)packet.ReadByte());
            else
                return new MySqlUByte(Byte.Parse(packet.ReadString(length)));
        }

        void IMySqlValue.SkipValue(MySqlPacket packet)
        {
            packet.ReadByte();
        }

        #endregion IMySqlValue Members

        internal static void SetDSInfo(DataTable dsTable)
        {
            // we use name indexing because this method will only be called
            // when GetSchema is called for the DataSourceInformation
            // collection and then it wil be cached.
            DataRow row = dsTable.NewRow();
            row["TypeName"] = "TINY INT";
            row["ProviderDbType"] = MySqlDbType.UByte;
            row["ColumnSize"] = 0;
            row["CreateFormat"] = "TINYINT UNSIGNED";
            row["CreateParameters"] = null;
            row["DataType"] = "System.Byte";
            row["IsAutoincrementable"] = true;
            row["IsBestMatch"] = true;
            row["IsCaseSensitive"] = false;
            row["IsFixedLength"] = true;
            row["IsFixedPrecisionScale"] = true;
            row["IsLong"] = false;
            row["IsNullable"] = true;
            row["IsSearchable"] = true;
            row["IsSearchableWithLike"] = false;
            row["IsUnsigned"] = true;
            row["MaximumScale"] = 0;
            row["MinimumScale"] = 0;
            row["IsConcurrencyType"] = DBNull.Value;
            row["IsLiteralSupported"] = false;
            row["LiteralPrefix"] = null;
            row["LiteralSuffix"] = null;
            row["NativeDataType"] = null;
            dsTable.Rows.Add(row);
        }
    }
}