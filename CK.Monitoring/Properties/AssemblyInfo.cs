#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Properties\AssemblyInfo.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("CK.Monitoring")]
[assembly: AssemblyDescription("ActivityMonitor related implementations.")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage( "en-US" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("dde3882c-4796-40c3-b767-bb39f0082187")]

// Allow CK.Monitoring.Tests assembly to access to internals of CK.Monitoring.
[assembly: InternalsVisibleTo( "CK.Monitoring.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001008513230ff392395f63671e73e033e89ed6ad7542c0c63d2f9cd8ad15f057fafdce68aa9cae81959aa1c00697d5ff5cb6ec2e4138b5c89e9d62d873d4aa4f4620314c00d4fcb3866f2fe5c01506dfc278546256ada0fcd9c5b043db0ea11ee45509a79fd988f1775a2b31f0558a079ef6fd87a8d9475601d4072b880ef90df29a" )]
