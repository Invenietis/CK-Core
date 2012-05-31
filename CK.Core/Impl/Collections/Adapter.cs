#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Collections\Adapter.cs) is part of CiviKey. 
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "CK.Core.Adapter.#AlwaysFalse`1(System.Action`1<!!0>)", MessageId = "a" )]
[module: SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope = "member", Target = "CK.Core.Adapter.#AlwaysTrue`1(System.Action`1<!!0>)", MessageId = "a" )]

namespace CK.Core
{
   
    /// <summary>
	/// </summary>
	public static class Adapter 
    {

        /// <summary>
        /// Wraps an action in a predicate that returns always the provided result.
        /// </summary>
        /// <typeparam name="T">The type of the action's parameter.</typeparam>
        /// <param name="a">The action (a method that accepts <typeparamref name="T"/> as its only argument).</param>
        /// <param name="result">result that will be returned.</param>
        /// <returns>A predicate that performs the action and returns true.</returns>
        static public Predicate<T> ToPredicate<T>( this Action<T> a, bool result )
        {
            return delegate( T o ) { a( o ); return result; };
        }

        /// <summary>
        /// Wraps an action in a predicate that returns always true.
        /// </summary>
        /// <typeparam name="T">The type of the action's parameter.</typeparam>
        /// <param name="a">The action (a method that accepts <typeparamref name="T"/> as its only argument).</param>
        /// <returns>A predicate that performs the action and returns true.</returns>
        static public Predicate<T> AlwaysTrue<T>( Action<T> a )
        {
            return delegate( T o ) { a( o ); return true; };
        }

        /// <summary>
        /// Wraps an action in a predicate that returns always false.
        /// </summary>
        /// <typeparam name="T">The type of the action's parameter.</typeparam>
        /// <param name="a">The action (a method that accepts <typeparamref name="T"/> as its only argument).</param>
        /// <returns>A predicate that performs the action and returns false.</returns>
        static public Predicate<T> AlwaysFalse<T>( Action<T> a )
        {
            return delegate( T o ) { a( o ); return false; };
        }
        
    }

}
