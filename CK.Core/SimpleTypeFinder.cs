#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\SimpleTypeFinder.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Very simple default implementation of the <see cref="ISimpleTypeFinder"/>: it can be used as a base class.
    /// A static <see cref="Default"/> is available.
    /// </summary>
    public class SimpleTypeFinder : ISimpleTypeFinder
    {
        /// <summary>
        /// Default implementation for <see cref="ISimpleTypeFinder"/>.
        /// </summary>
        public static readonly ISimpleTypeFinder Default = new SimpleTypeFinder();

        /// <summary>
        /// Default implementation returns exactly its <paramref name="assemblyQualifiedName"/> parameter.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name of a type.</param>
        /// <returns>The assembly qualified name to use.</returns>
        public virtual string MapType( string assemblyQualifiedName )
        {
            return assemblyQualifiedName;
        }

        /// <summary>
        /// Simple implementation that calls <see cref="MapType"/> and then <see cref="Type.GetType(string,bool)"/>.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name of a type.</param>
        /// <param name="throwOnError">
        /// True to ALWAYS throw a <see cref="TypeLoadException"/> if the type is not found.
        /// False prevents any exception to be thrown and simply returns null.
        /// </param>
        /// <returns>The type or null if not found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception cref="TypeLoadException">
        /// When <paramref name="throwOnError"/> is true, always throws this kind of exception.
        /// The original error (not a <see cref="TypeLoadException"/>) is available in the <see cref="Exception.InnerException"/>.
        /// </exception>
        public virtual Type ResolveType( string assemblyQualifiedName, bool throwOnError )
        {
            assemblyQualifiedName = MapType( assemblyQualifiedName );
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
        /// Helper method to split the assembly qualified name into its assembly name and full type name.
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name to split.</param>
        /// <param name="assemblyName">Assembly name on output or an empty string.</param>
        /// <param name="fullTypeName">Full type name on output or an empty string.</param>
        /// <returns>True if the split has been successfully done. False otherwise.</returns>
        static public bool SplitNames( string assemblyQualifiedName, out string assemblyName, out string fullTypeName )
        {
            int i = assemblyQualifiedName.IndexOf( ',' );
            if( i > 0 && i < assemblyQualifiedName.Length-1 )
            {
                assemblyName = assemblyQualifiedName.Substring( Char.IsWhiteSpace( assemblyQualifiedName, i+1 ) ? i + 2 : i + 1 ).Trim();
                fullTypeName = assemblyQualifiedName.Substring( 0, i ).Trim();
                return assemblyName.Length > 0 && fullTypeName.Length > 0;
            }
            assemblyName = fullTypeName = String.Empty;
            return false;
        }

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
