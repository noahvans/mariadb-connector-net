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
using System.Globalization;
using MariaDB.Data.MySqlClient;

namespace MariaDB.Data.Types
{
    internal struct MySqlSingle : IMySqlValue
    {
        private float mValue;
        private bool isNull;

        public MySqlSingle(bool isNull)
        {
            this.isNull = isNull;
            mValue = 0.0f;
        }

        public MySqlSingle(float val)
        {
            this.isNull = false;
            mValue = val;
        }

        public bool IsNull
        {
            get { return isNull; }
        }

        MySqlDbType IMySqlValue.MySqlDbType
        {
            get { return MySqlDbType.Float; }
        }

        DbType IMySqlValue.DbType
        {
            get { return DbType.Single; }
        }

        object IMySqlValue.Value
        {
            get { return mValue; }
        }

        public float Value
        {
            get { return mValue; }
        }

        Type IMySqlValue.SystemType
        {
            get { return typeof(float); }
        }

        string IMySqlValue.MySqlTypeName
        {
            get { return "FLOAT"; }
        }

        void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
        {
            Single v = (val is Single) ? (Single)val : Convert.ToSingle(val);
            if (binary)
                packet.Write(BitConverter.GetBytes(v));
            else
                packet.WriteStringNoNull(v.ToString("R",
                     CultureInfo.InvariantCulture));
        }

        IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
        {
            if (nullVal)
                return new MySqlSingle(true);

            if (length == -1)
            {
                byte[] b = new byte[4];
                packet.Read(b, 0, 4);
                return new MySqlSingle(BitConverter.ToSingle(b, 0));
            }
            return new MySqlSingle(Single.Parse(packet.ReadString(length),
                     CultureInfo.InvariantCulture));
        }

        void IMySqlValue.SkipValue(MySqlPacket packet)
        {
            packet.Position += 4;
        }

        internal static void SetDSInfo(DataTable dsTable)
        {
            // we use name indexing because this method will only be called
            // when GetSchema is called for the DataSourceInformation
            // collection and then it will be cached.
            DataRow row = dsTable.NewRow();
            row["TypeName"] = "FLOAT";
            row["ProviderDbType"] = MySqlDbType.Float;
            row["ColumnSize"] = 0;
            row["CreateFormat"] = "FLOAT";
            row["CreateParameters"] = null;
            row["DataType"] = "System.Single";
            row["IsAutoincrementable"] = false;
            row["IsBestMatch"] = true;
            row["IsCaseSensitive"] = false;
            row["IsFixedLength"] = true;
            row["IsFixedPrecisionScale"] = true;
            row["IsLong"] = false;
            row["IsNullable"] = true;
            row["IsSearchable"] = true;
            row["IsSearchableWithLike"] = false;
            row["IsUnsigned"] = false;
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