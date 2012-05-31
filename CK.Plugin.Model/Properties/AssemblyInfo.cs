#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Properties\AssemblyInfo.cs) is part of CiviKey. 
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

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Les informations générales relatives à un assembly dépendent de 
// l'ensemble d'attributs suivant. Changez les valeurs de ces attributs pour modifier les informations
// associées à un assembly.
[assembly: AssemblyTitle( "CK.Plugin.Model" )]
[assembly: AssemblyDescription( "" )]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany( "" )]
[assembly: AssemblyProduct( "CK.Plugin.Model" )]
[assembly: AssemblyCopyright( "Copyright ©  2010" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]
[assembly: CLSCompliant( true )]


// L'affectation de la valeur false à ComVisible rend les types invisibles dans cet assembly 
// aux composants COM. Si vous devez accéder à un type dans cet assembly à partir de 
// COM, affectez la valeur true à l'attribut ComVisible sur ce type.
[assembly: ComVisible( false )]

// Le GUID suivant est pour l'ID de la typelib si ce projet est exposé à COM
[assembly: Guid( "c5031102-eda7-4846-a465-7247fb3b009f" )]

// Les informations de version pour un assembly se composent des quatre valeurs suivantes :
//
//      Version principale
//      Version secondaire 
//      Numéro de build
//      Révision
//
// Vous pouvez spécifier toutes les valeurs ou indiquer les numéros de build et de révision par défaut 
// en utilisant '*', comme indiqué ci-dessous :
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( "1.0.0.0" )]
[assembly: AssemblyFileVersion( "1.0.0.0" )]

// Allow CK.Tests assembly to acces to Internals of CK.Plugin.Config.
// Here to ease the set up of NUnit tests.
[assembly: InternalsVisibleTo( "CK.Plugin.Config.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002badda7c6774254194bd7d7b264aa7be4622e8a0105acfe1b2edc239b3389a317e008862dd5c62b61298042874b8bf08c4ad18a71dcbae5234066d3f6ef159bc9f8014c89d5be68f4d5b59af4169f15784af3eb2fa02e312e480ea123f383c09bab56a016b46519cc830fa17bd6ccff7260cc8d20ece42745cef70b98e3c70d9" )]
