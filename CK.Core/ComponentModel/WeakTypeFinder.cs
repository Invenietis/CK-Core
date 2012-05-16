#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ComponentModel\SimpleTypeFinder.cs) is part of CiviKey. 
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
    /// The implementation of <see cref="ISimpleTypeFinder"/> can be used to load types regardeless of the versions of the input/output assemblies.
    /// </summary>
    public class WeakTypeFinder : ISimpleTypeFinder
    {
        /// <summary>
        /// This implementation returns the assembly qualified name without its version and PublicKeyToken info.
        /// Used to be able to map types regardeless of the strong-named dll's version
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name of a type.</param>
        /// <returns>The assembly qualified name, in the following format : "TypeNamespace.TypeName, AssemblyName".</returns>
        public virtual string MapType( string assemblyQualifiedName )
        {
            string simpleName = assemblyQualifiedName;
            string assemblyName;
            string fullTypeName;
            SplitTypeAndAssemblyName( assemblyQualifiedName, out assemblyName, out fullTypeName );
            //IList<String> stringArray = assemblyQualifiedName.Split(',');
            //if( stringArray.Count > 1)
            //    simpleName = stringArray[0] + "," + stringArray[1];
            return fullTypeName + "," + assemblyName;
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
        /// Helper method to split the assembly qualified name into its assembly name and type name. truncates versio, culture and publicTokenKey
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly qualified name to split.</param>
        /// <param name="assemblyName">Assembly name on output or an empty string.</param>
        /// <param name="fullTypeName">Full type name on output or an empty string.</param>
        /// <returns>True if the split has been successfully done. False otherwise.</returns>
        static public bool SplitTypeAndAssemblyName( string assemblyQualifiedName, out string assemblyName, out string typeName )
        {
            int i = assemblyQualifiedName.IndexOf( ',' );
            int j = assemblyQualifiedName.IndexOf( ',', i + 1);
            if( i > 0 && j > 0 && i < assemblyQualifiedName.Length - 1 && j < assemblyQualifiedName.Length - 1 )
            {
                typeName = assemblyQualifiedName.Substring( 0, i ).Trim();
                assemblyName = assemblyQualifiedName.Substring( i + 1, j - i - 1 ).Trim();
                return assemblyName.Length > 0 && typeName.Length > 0;
            }
            assemblyName = typeName = String.Empty;
            return false;
        }
    }
}
