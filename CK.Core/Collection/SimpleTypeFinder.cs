#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\SimpleTypeFinder.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Very simple default implementation of the <see cref="ISimpleTypeFinder"/>: it can be used as a base class.
    /// Static <see cref="Default"/> and <see cref="WeakDefault"/> are available.
    /// </summary>
    public class SimpleTypeFinder : ISimpleTypeFinder
    {
        /// <summary>
        /// Default implementation for <see cref="ISimpleTypeFinder"/>.
        /// </summary>
        public static readonly ISimpleTypeFinder Default = new SimpleTypeFinder();

        class WeakTypeFinder : SimpleTypeFinder
        {
            public override Type ResolveType( string assemblyQualifiedName, bool throwOnError )
            {
                Type done = base.ResolveType( assemblyQualifiedName, false );
                if( done == null )
                {
                    string weakTypeName;
                    if( !WeakenAssemblyQualifiedName( assemblyQualifiedName, out weakTypeName ) && throwOnError )
                    {
                        throw new ArgumentException( String.Format( R.InvalidAssemblyQualifiedName, assemblyQualifiedName ), "assemblyQualifiedName" );
                    }
                    done = base.ResolveType( weakTypeName, throwOnError );
                }
                return done;
            }
        }
        /// <summary>
        /// An implementation of <see cref="ISimpleTypeFinder"/> that can be used to load types regardless of 
        /// the version, culture, architecture and public key token (strongly-named assemblies) of the type names.
        /// (See <see cref="WeakenAssemblyQualifiedName"/>.)
        /// </summary>
        /// <remarks>
        /// The type name used is in the following format: "TypeNamespace.TypeName, AssemblyName".
        /// </remarks>
        public static readonly ISimpleTypeFinder WeakDefault = new WeakTypeFinder();

        private static void CheckAssemblyQualifiedNameValid( string assemblyQualifiedName )
        {
            if( assemblyQualifiedName == null ) throw new ArgumentNullException( "assemblyQualifiedName" );
            if( assemblyQualifiedName.Length == 0 || !assemblyQualifiedName.Contains( "," ) )
            {
                throw new ArgumentException( String.Format( R.InvalidAssemblyQualifiedName, assemblyQualifiedName ), "assemblyQualifiedName" );
            }
        }

        /// <summary>
        /// Simple implementation that checks that the assembly qualified name set as parameter is valid, then calls <see cref="Type.GetType(string,bool)"/>.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name of a type.</param>
        /// <param name="throwOnError">
        /// True to ALWAYS throw a <see cref="TypeLoadException"/> if the type is not found.
        /// It may also throw <see cref="ArgumentNullException"/> and <see cref="ArgumentException"/> when the assembly qualified name is not valid
        /// False prevents any exception from being thrown and simply returns null.
        /// </param>
        /// <returns>The type or null if not found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception cref="TypeLoadException">
        /// When <paramref name="throwOnError"/> is true, always throws this kind of exception.
        /// The original error (not a <see cref="TypeLoadException"/>) is available in the <see cref="Exception.InnerException"/>.
        /// </exception>
        public virtual Type ResolveType( string assemblyQualifiedName, bool throwOnError )
        {
            if( throwOnError ) CheckAssemblyQualifiedNameValid( assemblyQualifiedName );
            try
            {
                return Type.GetType( assemblyQualifiedName, throwOnError );
            }
            catch( Exception ex )
            {
                if( !throwOnError ) return null;
                if( ex is TypeLoadException ) throw;
                throw new TypeLoadException( String.Format( R.ExceptionWhileResolvingType, assemblyQualifiedName ), ex );
            }
        }

        /// <summary>
        /// Obsolete version of <see cref="WeakenAssemblyQualifiedName"/>
        /// </summary>
        [Obsolete( "Use SplitAssemblyQualifiedName (and INVERT the 2 output parameters!!).", true )]
        static public bool SplitNames( string assemblyQualifiedName, out string assemblyFullName, out string fullTypeName )
        {
            int i = assemblyQualifiedName.IndexOf( ',' );
            if( i > 0 && i < assemblyQualifiedName.Length - 1 )
            {
                assemblyFullName = assemblyQualifiedName.Substring( Char.IsWhiteSpace( assemblyQualifiedName, i + 1 ) ? i + 2 : i + 1 ).Trim();
                fullTypeName = assemblyQualifiedName.Substring( 0, i ).Trim();
                return assemblyFullName.Length > 0 && fullTypeName.Length > 0;
            }
            assemblyFullName = fullTypeName = String.Empty;
            return false;
        }

        /// <summary>
        /// Helper method to remove version, architecture, publiTokenKey and culture from the assembly qualified name into its assembly name passed as parameter.
        /// "CK.Core.SimpleTypeFinder, CK.Core, version=1.0.0, culture='fr-FR'" gives "CK.Core.SimpleTypeFinder, CK.Core".
        /// Used to remove strong name from an strongly-named assembly qualified name
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name to weaken.</param>
        /// <param name="weakTypeName">The weakened assembly qualified name on output or an empty string.</param>
        /// <returns>True if the split has been successfully done. False otherwise.</returns>
        static public bool WeakenAssemblyQualifiedName( string assemblyQualifiedName, out string weakTypeName )
        {
            weakTypeName = String.Empty;
            string fullTypeName, assemblyFullName, assemblyName, versionCultureAndPublicKeyToken;
            if( SplitAssemblyQualifiedName( assemblyQualifiedName, out fullTypeName, out assemblyFullName )
                && SplitAssemblyFullName( assemblyFullName, out assemblyName, out versionCultureAndPublicKeyToken ) )
            {
                weakTypeName = fullTypeName + ", " + assemblyName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method to split the assembly qualified name into its assembly name and full type name.
        /// "CK.Core.SimpleTypeFinder, CK.Core, version=1.0.0, culture='fr-FR'" gives "CK.Core.SimpleTypeFinder" and "CK.Core, version=1.0.0, culture='fr-FR'".
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name to split.</param>
        /// <param name="fullTypeName">Full type name on output or an empty string.</param>
        /// <param name="assemblyFullName">Weaken type name on output or an empty string if the weaking hasn't work.</param>
        /// <returns>True if the weakening has been successfully done. False otherwise.</returns>
        static public bool SplitAssemblyQualifiedName( string assemblyQualifiedName, out string fullTypeName, out string assemblyFullName )
        {
            int i = assemblyQualifiedName.IndexOf( ',' );
            if( i > 0 && i < assemblyQualifiedName.Length - 1 )
            {
                assemblyFullName = assemblyQualifiedName.Substring( Char.IsWhiteSpace( assemblyQualifiedName, i + 1 ) ? i + 2 : i + 1 ).Trim();
                fullTypeName = assemblyQualifiedName.Substring( 0, i ).Trim();
                return assemblyFullName.Length > 0 && fullTypeName.Length > 0;
            }
            assemblyFullName = fullTypeName = String.Empty;
            return false;
        }

        /// <summary>
        /// Helper method to split an assembly full name in two parts: 
        /// "CK.Core, version=1.0.0, culture='fr-FR'" gives "CK.Core" and "version=1.0.0, culture='fr-FR'".
        /// </summary>
        /// <param name="assemblyFullName">The assembly full name.</param>
        /// <param name="assemblyName">Set to assembly name only.</param>
        /// <param name="versionCultureAndPublicKeyToken">Set to extra information.</param>
        /// <returns>True if the split worked. False otherwise.</returns>
        static public bool SplitAssemblyFullName( string assemblyFullName, out string assemblyName, out string versionCultureAndPublicKeyToken )
        {
            versionCultureAndPublicKeyToken = assemblyName = String.Empty;
            int i = assemblyFullName.IndexOf( ',' );
            if( i < 0 ) assemblyName = assemblyFullName;
            else if( i > 0 )
            {
                if( i < assemblyFullName.Length - 1 )
                {
                    versionCultureAndPublicKeyToken = assemblyFullName.Substring( Char.IsWhiteSpace( assemblyFullName, i + 1 ) ? i + 2 : i + 1 ).Trim();
                }
                assemblyName = assemblyFullName.Substring( 0, i ).Trim();
            }
            return assemblyName.Length > 0;
        }

    }
}
