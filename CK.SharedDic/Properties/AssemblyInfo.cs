#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Properties\AssemblyInfo.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "CK.SharedDic" )]
[assembly: AssemblyDescription("This is a keyboard to help people with disabilities")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Invenietis")]
[assembly: AssemblyProduct("Custom Virtual Keyboard")]
[assembly: AssemblyCopyright("Copyright © Invenietis - In’Tech INFO 2007-2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]	

[assembly: ComVisible( false )]
[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "6b180c10-75d6-48e5-ae3b-c01df6b41fcb" )]

// Allow CK.Tests assembly to acces to Internals of CK.SharedDic.
// Here to ease the set up of NUnit tests.
[assembly: InternalsVisibleTo( "CK.SharedDic.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002badda7c6774254194bd7d7b264aa7be4622e8a0105acfe1b2edc239b3389a317e008862dd5c62b61298042874b8bf08c4ad18a71dcbae5234066d3f6ef159bc9f8014c89d5be68f4d5b59af4169f15784af3eb2fa02e312e480ea123f383c09bab56a016b46519cc830fa17bd6ccff7260cc8d20ece42745cef70b98e3c70d9" )]


