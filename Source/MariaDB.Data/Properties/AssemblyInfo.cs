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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyDescription("ADO.Net driver for MariaDB and MySQL")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
#if !CF
[assembly: AssemblyTitle("MariaDB.Data.dll")]
[assembly: AllowPartiallyTrustedCallers()]
#else
[assembly: AssemblyTitle("MariaDB.Data.CF.dll")]
#endif

#if CLR4
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
[assembly: InternalsVisibleTo("MySql.Data.Tests, PublicKey = 0024000004800000940000000602000000240000525341310004000001000100d973bda91f71752c78294126974a41a08643168271f65fc0fb3cd45f658da01fbca75ac74067d18e7afbf1467d7a519ce0248b13719717281bb4ddd4ecd71a580dfe0912dfc3690b1d24c7e1975bf7eed90e4ab14e10501eedf763bff8ac204f955c9c15c2cf4ebf6563d8320b6ea8d1ea3807623141f4b81ae30a6c886b3ee1")]
[assembly: InternalsVisibleTo("MySql.Data.CF.Tests, PublicKey = 0024000004800000940000000602000000240000525341310004000001000100d973bda91f71752c78294126974a41a08643168271f65fc0fb3cd45f658da01fbca75ac74067d18e7afbf1467d7a519ce0248b13719717281bb4ddd4ecd71a580dfe0912dfc3690b1d24c7e1975bf7eed90e4ab14e10501eedf763bff8ac204f955c9c15c2cf4ebf6563d8320b6ea8d1ea3807623141f4b81ae30a6c886b3ee1")]

#if CF
[assembly: AssemblyFlags(AssemblyNameFlags.Retargetable)]
#endif