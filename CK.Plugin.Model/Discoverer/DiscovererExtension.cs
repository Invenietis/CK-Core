#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\DiscovererExtension.cs) is part of CiviKey. 
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

using System.Text;

namespace CK.Plugin
{
    public static class DiscovererExtension
    {
        /// <summary>
        /// Gets the method's signature.
        /// </summary>
        /// <param name="m">This <see cref="ISimpleMethodInfo"/>.</param>
        /// <returns>The signature (return type, name and parameter types, types are ).</returns>
        public static string GetSimpleSignature( this ISimpleMethodInfo m )
        {
            return AppendSimpleSignature( m, new StringBuilder() ).ToString();
        }

        /// <summary>
        /// Writes the method's signature into a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="m">This <see cref="ISimpleMethodInfo"/>.</param>
        /// <returns>The string builder (to allow fluent syntax).</returns>
        public static StringBuilder AppendSimpleSignature( this ISimpleMethodInfo m, StringBuilder b )
        {
            b.Append( m.ReturnType ).Append( ' ' ).Append( m.Name ).Append( '(' );
            foreach( var p in m.Parameters ) b.Append( p.ParameterType ).Append( ',' );
            b.Length = b.Length - 1;
            b.Append( ')' );
            return b;
        }
    }
}
