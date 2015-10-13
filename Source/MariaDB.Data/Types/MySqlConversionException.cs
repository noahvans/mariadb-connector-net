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

namespace MariaDB.Data.Types
{
	/// <summary>
	/// Summary description for MySqlConversionException.
	/// </summary>
#if !CF
	[Serializable]
#endif
	public class MySqlConversionException : Exception
	{
		/// <summary>Ctor</summary>
		public MySqlConversionException(string msg)
			: base(msg)
		{
		}
	}
}
