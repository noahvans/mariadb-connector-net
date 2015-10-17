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
using System.Runtime.InteropServices;

namespace MariaDB.Data.Common
{
    internal class Platform
    {
        private static bool inited;
        private static bool isMono;

        /// <summary>
        /// By creating a private ctor, we keep the compiler from creating a default ctor
        /// </summary>
        private Platform() { }

        public static bool IsWindows()
        {
            OperatingSystem os = Environment.OSVersion;
            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    return true;
            }
            return false;
        }

        public static bool IsUnix()
        {
            OperatingSystem os = Environment.OSVersion;
            switch (os.Platform)
            {
                case PlatformID.Unix:
                    return true;
            }
            return false;
        }

        public static bool IsMono()
        {
            if (!inited)
                Init();
            return isMono;
        }
        
        private static void Init()
        {
            inited = true;
            Type t = Type.GetType("Mono.Runtime");
            isMono = t != null;
        }
    }
}