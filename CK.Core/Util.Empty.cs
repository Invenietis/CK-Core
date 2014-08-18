#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Util.Empty.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// </summary>
	static public partial class Util
	{
        /// <summary>
        /// Empty array centralization.
        /// </summary>
        /// <typeparam name="T">Any type.</typeparam>
        public static class EmptyArray<T>
        {
            /// <summary>
            /// Empty array to use (and reuse!).
            /// </summary>
            public readonly static T[] Empty = new T[0];
        }

        class FakeDisposable : IDisposable { public void Dispose() { } }

        /// <summary>
        /// A void, immutable, <see cref="IDisposable"/> that does absolutely nothing.
        /// </summary>
        public static readonly IDisposable EmptyDisposable = new FakeDisposable();

        /// <summary>
        /// Gets a static empty <see cref="String"/> array.
        /// </summary>
        static public readonly string[] EmptyStringArray = EmptyArray<string>.Empty;

        /// <summary>
        /// The empty version is defined as the Major.Minor.Build.Revision set to "0.0.0.0".
        /// </summary>
        static public readonly Version EmptyVersion = new Version( 0, 0, 0, 0 );


    }
}
