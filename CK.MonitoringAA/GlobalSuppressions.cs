#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GlobalSuppressions.cs) is part of CiviKey. 
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

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", 
    Scope = "member", 
    Target = "CK.Monitoring.GrandOutputHandlers.BinaryFile.#System.IDisposable.Dispose()",
    Justification="I want hide Dispose from the public interface: Close should be used (with its parameters)." )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", 
    MessageId = "_writer", 
    Scope = "member", 
    Target = "CK.Monitoring.GrandOutputHandlers.BinaryFile.#System.IDisposable.Dispose()",
    Justification="Close does the job correctly." )]


