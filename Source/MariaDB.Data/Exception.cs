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
using System.Data.Common;
using System.Runtime.Serialization;

namespace MariaDB.Data.MySqlClient
{
    /// <summary>
    /// The exception that is thrown when MySQL returns an error. This class cannot be inherited.
    /// </summary>
    /// <include file='docs/MySqlException.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
    public sealed class MySqlException : DbException
    {
        private int errorCode;
        private bool isFatal;

        internal MySqlException()
        {
        }

        internal MySqlException(string msg) : base(msg)
        {
        }

        internal MySqlException(string msg, Exception ex) : base(msg, ex)
        {
        }

        internal MySqlException(string msg, bool isFatal, Exception inner) : base(msg, inner)
        {
            this.isFatal = isFatal;
        }

        internal MySqlException(string msg, int errno, Exception inner)
            : this(msg, inner)
        {
            errorCode = errno;
            Data.Add("Server Error Code", errno);
        }

        internal MySqlException(string msg, int errno)
            : this(msg, errno, null)
        {
        }

        /// <summary>
        /// Gets a number that identifies the type of error.
        /// </summary>
        public int Number
        {
            get { return errorCode; }
        }

        /// <summary>
        /// True if this exception was fatal and cause the closing of the connection, false otherwise.
        /// </summary>
        internal bool IsFatal
        {
            get { return isFatal; }
        }

        internal bool IsQueryAborted
        {
            get
            {
                return (errorCode == (int)MySqlErrorCode.QueryInterrupted ||
                    errorCode == (int)MySqlErrorCode.FileSortAborted);
            }
        }
    }
}