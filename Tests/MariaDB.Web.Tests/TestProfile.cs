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
using System.Collections.Generic;
using System.Text;
using System.Web.Profile;
using System.Web.Security;

namespace MariaDB.Web.Tests
{
    public class TestProfile : ProfileBase
    {
        public static TestProfile GetUserProfile(string username, bool auth)
        {
            return Create(username, auth) as TestProfile;
        }

        public static TestProfile GetUserProfile(bool auth) 
        {
            return Create(Membership.GetUser().UserName, auth) as TestProfile; 
        }

        [SettingsAllowAnonymous(false)]
        public string Description 
        { 
            get { return base["Description"] as string; } 
            set { base["Description"] = value; } 
        }

        [SettingsAllowAnonymous(false)]
        public string Location 
        { 
            get { return base["Location"] as string; } 
            set { base["Location"] = value; } 
        }

        [SettingsAllowAnonymous(false)]
        public string FavoriteMovie 
        { 
            get { return base["FavoriteMovie"] as string; } 
            set { base["FavoriteMovie"] = value; } 
        }
    } 
}
