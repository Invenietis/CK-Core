#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\WeakReference.cs) is part of CiviKey. 
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
using System.Runtime.Serialization;

namespace CK.Core
{
    /// <summary>
    /// A generic weak reference, which references an object while still allowing  
    /// that object to be reclaimed by garbage collection.   
    /// </summary>   
    /// <typeparam name="T">The type of the object that is referenced.</typeparam>
    /// <remarks>
    /// An implicit cast exists from <typeparamref name="T"/> to <see cref="WeakReference{T}"/> 
    /// BUT NOT the opposite, and it is asolutely normal!
    /// </remarks>
    [Serializable]
    public class WeakReference<T> : WeakReference
        where T : class
    {
        /// <summary>       
        /// Initializes a new instance of a weak reference to 
        /// the specified object.       
        /// </summary>       
        /// <param name="target">The object to reference.</param>       
        public WeakReference( T target )
            : base( target )
        {
        }

        /// <summary>       
        /// Initializes a new instance of the WeakReference{T} class, referencing
        /// the specified object and using the specified resurrection tracking.
        /// </summary>       
        /// <param name="target">An object to track.</param>
        /// <param name="trackResurrection">Indicates when to stop tracking the object. If true, the object is tracked
        /// after finalization; if false, the object is only tracked until finalization.</param>
        public WeakReference( T target, bool trackResurrection )
            : base( target, trackResurrection )
        {
        }

        /// <summary>
        /// Required serialization constructor.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The <see cref="StreamingContext"/>.</param>
        protected WeakReference( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        /// <summary>
        /// Gets or sets the object (the target) referenced by this weak reference.
        /// </summary>
        public new T Target
        {
            get { return (T)base.Target; }
            set { base.Target = value; }
        }

        /// <summary>
        /// Casts an object of the type T to a weak reference of T.
        /// </summary>
        public static implicit operator WeakReference<T>( T target )
        {
            if( target == null ) throw new ArgumentNullException( "target" );
            return new WeakReference<T>( target );
        }

    }

}
