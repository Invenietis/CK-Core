#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Properties\AssemblyInfo.cs) is part of CiviKey. 
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
[assembly: AssemblyTitle( "CK.Plugin.Host" )]
[assembly: AssemblyDescription( "" )]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany( "Invenietis" )]
[assembly: AssemblyProduct( "CK.Plugin.Hosting" )]
[assembly: AssemblyCopyright( "Copyright © Invenietis 2011" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

[assembly: InternalsVisibleTo( "CKProxyAssembly, PublicKey=00240000048000009400000006020000002400005253413100040000010001009fbf2868f04bdf33df4c8c0517bb4c3d743b5b27fcd94009d42d6607446c1887a837e66545221788ecfff8786e85564c839ff56267fe1a3225cd9d8d9caa5aae3ba5d8f67f86ff9dbc5d66f16ba95bacde6d0e02f452fae20022edaea26d31e52870358d0dda69e592ea5cef609a054dac4dbbaa02edc32fb7652df9c0e8e9cd" )]
[assembly: InternalsVisibleTo( "CK.Plugin.Host.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002badda7c6774254194bd7d7b264aa7be4622e8a0105acfe1b2edc239b3389a317e008862dd5c62b61298042874b8bf08c4ad18a71dcbae5234066d3f6ef159bc9f8014c89d5be68f4d5b59af4169f15784af3eb2fa02e312e480ea123f383c09bab56a016b46519cc830fa17bd6ccff7260cc8d20ece42745cef70b98e3c70d9" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]
[assembly: CLSCompliant(true) ]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "f6208078-5cca-44ed-9ae4-1414643efce0" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( "1.0.0.0" )]
[assembly: AssemblyFileVersion( "1.0.0.0" )]
