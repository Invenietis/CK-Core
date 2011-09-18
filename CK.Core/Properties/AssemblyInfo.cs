#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Properties\AssemblyInfo.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "CK.Core" )]
[assembly: AssemblyDescription("Core library.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Invenietis")]
[assembly: AssemblyProduct("Civikey")]
[assembly: AssemblyCopyright("Copyright © Invenietis - In’Tech INFO 2007-2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]	
[assembly: CLSCompliant(true)]	

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "042d54e7-cce5-4e76-8229-39af5f82da30" )]

// Allow CK.Core.Tests assembly to acces to internals of CK.Core.
// Here to allow white testing of CK.Core.
[assembly: InternalsVisibleTo( "CK.Core.Tests" )]
