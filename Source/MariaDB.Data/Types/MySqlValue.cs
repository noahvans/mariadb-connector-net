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
	internal interface IMySqlValue
	{
		bool IsNull { get; }
		MySqlDbType MySqlDbType { get; }
		DbType DbType { get; }
		object Value { get; /*set;*/ }
		Type SystemType { get; }
		string MySqlTypeName { get; }

		void WriteValue(MySqlPacket packet, bool binary, object value, int length);
		IMySqlValue ReadValue(MySqlPacket packet, long length, bool isNull);
		void SkipValue(MySqlPacket packet);
	}
}
