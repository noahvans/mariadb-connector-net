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
using System.Globalization;

namespace MariaDB.Data.Types
{

	public struct MySqlDecimal : IMySqlValue
	{
		private byte precision;
		private byte scale;
		private string mValue;
		private bool isNull;

		internal MySqlDecimal(bool isNull)
		{
			this.isNull = isNull;
			mValue = null;
			precision = scale = 0;
		}

		internal MySqlDecimal(string val)
		{
			this.isNull = false;
			precision = scale = 0;
			mValue = val;
		}

		#region IMySqlValue Members

		public bool IsNull
		{
			get { return isNull; }
		}

		MySqlDbType IMySqlValue.MySqlDbType
		{
			get { return MySqlDbType.Decimal; }
		}

		public byte Precision
		{
			get { return precision; }
			set { precision = value; }
		}

		public byte Scale
		{
			get { return scale; }
			set { scale = value; }
		}


		DbType IMySqlValue.DbType
		{
			get { return DbType.Decimal; }
		}

		object IMySqlValue.Value
		{
			get { return this.Value; }
		}

		public decimal Value
		{
			get { return Convert.ToDecimal(mValue, CultureInfo.InvariantCulture); }
		}

		public double ToDouble()
		{
			return Double.Parse(mValue);
		}

		public override string ToString()
		{
			return mValue;
		}

		Type IMySqlValue.SystemType
		{
			get { return typeof(decimal); }
		}

		string IMySqlValue.MySqlTypeName
		{
			get { return "DECIMAL"; }
		}

		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			decimal v = (val is decimal) ? (decimal)val : Convert.ToDecimal(val);
			string valStr = v.ToString(CultureInfo.InvariantCulture);
			if (binary)
				packet.WriteLenString(valStr);
			else
				packet.WriteStringNoNull(valStr);
		}

		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
				return new MySqlDecimal(true);

			string s = String.Empty;
			if (length == -1)
				s = packet.ReadLenString();
			else 
				s = packet.ReadString(length);
			return new MySqlDecimal(s);
		}

		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			int len = (int)packet.ReadFieldLength();
			packet.Position += len;
		}

		#endregion

		internal static void SetDSInfo(DataTable dsTable)
		{
			// we use name indexing because this method will only be called
			// when GetSchema is called for the DataSourceInformation 
			// collection and then it wil be cached.
			DataRow row = dsTable.NewRow();
			row["TypeName"] = "DECIMAL";
			row["ProviderDbType"] = MySqlDbType.NewDecimal;
			row["ColumnSize"] = 0;
			row["CreateFormat"] = "DECIMAL({0},{1})";
			row["CreateParameters"] = "precision,scale";
			row["DataType"] = "System.Decimal";
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
