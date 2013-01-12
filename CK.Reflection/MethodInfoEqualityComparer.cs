#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Reflection\MethodInfoEqualityComparer.cs) is part of CiviKey. 
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
using System.Reflection;

namespace CK.Reflection
{
    /// <summary>
    /// Provides actual comparison of <see cref="MemberInfo"/> without  considering the <see cref="MemberInfo.ReflectedType"/>.
    /// </summary>
    /// <remarks>
    /// This code comes from re-motion Core Framework (LGPL). It has been slightly modified, but the credits is for them.
    /// <para>
    /// Copyright (c) rubicon IT GmbH, www.rubicon.eu
    /// </para>
    /// </remarks>
    public sealed class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
    {
        /// <summary>
        /// Gets the comparer instance that should be always used.
        /// </summary>
        public static readonly MemberInfoEqualityComparer Default = new MemberInfoEqualityComparer();

        /// <summary>
        /// Checks two <see cref="MemberInfo"/> instances for logical equality, without considering the <see cref="MemberInfo.ReflectedType"/>.
        /// </summary>
        /// <param name="x">First <see cref="MemberInfo"/> to compare.</param>
        /// <param name="y">Second <see cref="MemberInfo"/> to compare.</param>
        /// <returns>
        /// True if the two <see cref="MemberInfo"/> objects are logically equivalent, ie., if they represent the same <see cref="MemberInfo"/>.
        /// This is very similar to the <see cref="object.Equals(object)"/> implementation of <see cref="MemberInfo"/> objects, but it ignores the
        /// <see cref="MemberInfo.ReflectedType"/> property. In effect, two members compare equal if they are declared by the same type and
        /// denote the same member on that type. For generic <see cref="MethodInfo"/> objects, the generic arguments must be the same.
        /// The idea for this method, but not the code, was taken from http://blogs.msdn.com/b/kingces/archive/2005/08/17/452774.aspx.
        /// </returns>
        public bool Equals( MemberInfo x, MemberInfo y )
        {
            if( ReferenceEquals( x, y ) ) return true;
            if( ReferenceEquals( x, null ) || ReferenceEquals( null, y ) ) return false;

            // Types are always reference equals or not equal at all.
            if( x is Type || y is Type ) return false;

            // Methods defined by concrete arrays (int[].Set (...) etc.) will always succeed in the checks above if they are equal; it doesn't seem to be 
            // possible to get two different MethodInfo references for the same array method. Therefore, return false if an array method got through the 
            // check. (Since array methods have no metadata tokens, the checks below wouldn't detect any differences.)

            if( x.DeclaringType != null && x.DeclaringType.IsArray ) return false;

            // Equal members always have the same metadata token
            if( x.MetadataToken != y.MetadataToken ) return false;

            // Equal members always have the same declaring type - if any!
            if( x.DeclaringType != y.DeclaringType ) return false;

            // Equal members always have the same module
            if( x.Module != y.Module ) return false;

            var xM = x as MethodInfo;
            var yM = y as MethodInfo;
            if( xM != null && yM != null && xM.IsGenericMethod )
            {
                var xArgs = xM.GetGenericArguments();
                var yArgs = yM.GetGenericArguments();
                for( int i = 0; i < xArgs.Length; ++i )
                {
                    if( xArgs[i] != yArgs[i] ) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code for the given <see cref="MemberInfo"/>. To calculate the hash code, the hash codes of the declaring type, 
        /// metadata token and module of the <see cref="MemberInfo"/> are combined. If the declaring type is an array, the name of the 
        /// <see cref="MemberInfo"/> is used instead of the metadata token.
        /// </summary>
        /// <param name="m">The <see cref="MemberInfo"/> for which the hash code should be calculated.</param>
        /// <returns>The calculated hash code of the <see cref="MemberInfo"/>.</returns>
        public int GetHashCode( MemberInfo m )
        {
            if( m  == null ) throw new ArgumentNullException( "m" );
            if( m.DeclaringType != null && m.DeclaringType.IsArray )
            {
                return SafeHashCode( m.DeclaringType ) ^ SafeHashCode( m.Name ) ^ SafeHashCode( m.Module );
            }            
            return SafeHashCode( m.DeclaringType ) ^ SafeHashCode( m.MetadataToken ) ^ SafeHashCode( m.Module );
        }

        private int SafeHashCode( object o )
        {
            return o != null ? o.GetHashCode() : 0;
        }
    }
}