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

namespace MariaDB.Web.Security
{
    internal static class Runtime
    {
        private static bool inited;
        private static bool isMono;

        public static bool IsMono
        {
            get
            {
                if (!inited)
                    Init();
                return isMono;
            }
        }

        private static void Init()
        {
            inited = true;
            Type t = Type.GetType("Mono.Runtime");
            isMono = t != null;
        }
    }
}