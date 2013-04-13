#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKComponentModelExtension.cs) is part of CiviKey. 
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
    /// Provides extension methods for system/component model interfaces.
    /// </summary>
    public static class CKComponentModelExtension
    {
        /// <summary>
        /// Strongly typed version of <see cref="IServiceProvider.GetService"/>.
        /// </summary>
        /// <param name="source">This service provider.</param>
        /// <param name="throwOnNull">True to throw an exception if the service can not be provided (otherwise null is returned).</param>
        /// <returns>A service object of the required type or null if not found and <paramref name="throwOnNull"/> is false.</returns>
        public static T GetService<T>( this IServiceProvider source, bool throwOnNull )
        {
            T s = (T)source.GetService( typeof( T ) );
            if( throwOnNull && s == null ) throw new CKException( R.UnregisteredServiceInServiceProvider, typeof( T ).FullName );
            return s;
        }

        /// <summary>
        /// Strongly typed version of <see cref="IServiceProvider.GetService"/> that returns null if service is not found.
        /// (Same behavior as <see cref="IServiceProvider.GetService"/>.)
        /// </summary>
        /// <param name="source">This service provider.</param>
        /// <returns>A service object of the required type or null if not found.</returns>
        public static T GetService<T>( this IServiceProvider source )
        {
            return (T)source.GetService( typeof( T ) );
        }

        /// <summary>
        /// Type safe version to register a service implementation (type of the service is the type of the implementation).
        /// </summary>
        /// <param name="c">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Implementation of the service. Can not be null.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer c, T serviceInstance )
        {
            c.Add( typeof( T ), serviceInstance, null );
            return c;
        }

        /// <summary>
        /// Type safe version to register a service implementation (type of the service is the type of the implementation), 
        /// and an optional callback that will be called when the service will be removed.
        /// </summary>
        /// <param name="c">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Implementation of the service. Can not be null.</param>
        /// <param name="onRemove">Optional action that will be called whenever <see cref="ISimpleServiceContainer.Remove"/>, <see cref="ISimpleServiceContainer.Clear"/> or <see cref="IDisposable.Dispose"/>.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer c, T serviceInstance, Action<T> onRemove )
        {
            c.Add( typeof( T ), serviceInstance, o => onRemove( (T)o ) );
            return c;
        }

        /// <summary>
        /// Type safe version to register a service associated to a callback.
        /// The <paramref name="serviceInstance"/> is called as long as no service has been obtained (serviceInstance returns null). 
        /// Once the actual service has been obtained, it is kept and serviceInstance is not called anymore.
        /// </summary>
        /// <param name="c">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Delegate to call when needed. Can not be null.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer c, Func<T> serviceInstance ) where T : class
        {
            // It is the overloaded version that takes a Func<object> serviceInstance 
            // that is called (unit tests asserts this).
            // To allow the covariance, we MUST constrain the type T to be a reference class (hence the where clause).
            c.Add( typeof( T ), serviceInstance, null );
            return c;
        }

        /// <summary>
        /// Type safe version to register a service associated to a callback (and an optional callback that will be called when the service will be removed).
        /// The <paramref name="serviceInstance"/> is called as long as no service has been obtained (serviceInstance returns null). 
        /// Once the actual service has been obtained, it is kept and serviceInstance is not called anymore.
        /// </summary>
        /// <param name="c">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Delegate to call when needed. Can not be null.</param>
        /// <param name="onRemove">Optional action that will be called whenever <see cref="ISimpleServiceContainer.Remove"/>, <see cref="ISimpleServiceContainer.Clear"/> or <see cref="IDisposable.Dispose"/>
        /// is called and a service as been successfuly obtained.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer c, Func<T> serviceInstance, Action<T> onRemove ) where T : class
        {
            // It is the overloaded version that takes a Func<object> serviceInstance 
            // that is called (unit tests asserts this).
            // To allow the covariance, we MUST constrain the type T to be a reference class (hence the where clause).
            //
            // On the other hand, for the onRemove action we can not do any miracle: we need to adapt the call.
            //
            if( onRemove == null ) throw new ArgumentNullException( "onRemove" );
            c.Add( typeof( T ), serviceInstance, o => onRemove( (T)o ) );
            return c;
        }

    }
}
