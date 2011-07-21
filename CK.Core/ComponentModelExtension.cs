#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ComponentModelExtension.cs) is part of CiviKey. 
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
    /// Provides extension methods for system/component model interfaces.
    /// </summary>
    public static class ComponentModelExtension
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
        /// Merges this <see cref="IMergeable"/> object without any <see cref="IServiceProvider"/>.
        /// This method should not raise any execption. Instead, false should be returned. 
        /// If an exception is raised, it must be handled as if the method returned false.
        /// </summary>
        /// <param name="this">This mergeable object.</param>
        /// <param name="source">The object to merge. When the object is the same as this, true is returned.</param>
        /// <returns>True if the merge succeeded, false if the merge failed or is not possible.</returns>
        public static bool Merge( this IMergeable @this, object source )
        {
            return @this.Merge( source, null );
        }

        /// <summary>
        /// Registers a service with its implementation.
        /// </summary>
        /// <param name="c">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceType">Service type to register. It must not already exist in this container otherwise an exception is thrown.</param>
        /// <param name="serviceInstance">Implementation of the service. Can not be null.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add( this ISimpleServiceContainer c, Type serviceType, object serviceInstance )
        {
            c.Add( serviceType, serviceInstance, null );
            return c;
        }

        /// <summary>
        /// Registers a service associated to a callback.
        /// The <paramref name="serviceInstance"/> is called as long as no service has been obtained (serviceInstance returns null). 
        /// Once the actual service has been obtained, it is kept and serviceInstance is not called anymore.
        /// </summary>
        /// <param name="c">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceType">Service type to register. It must not already exist in this container otherwise an exception is thrown.</param>
        /// <param name="serviceInstance">Delegate to call when needed. Can not be null.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add( this ISimpleServiceContainer c, Type serviceType, Func<Object> serviceInstance )
        {
            c.Add( serviceType, serviceInstance, null );
            return c;
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
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer c, Func<T> serviceInstance )
        {
            // This wrapping in a new Func<Object> will not be required anymore with the CLR/.Net 4.0.
            c.Add( typeof( T ), () => serviceInstance(), null );
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
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer c, Func<T> serviceInstance, Action<T> onRemove )
        {
            // This wrapping in a new Func<Object> will not be required anymore with the CLR/.Net 4.0.
            c.Add( typeof( T ), () => serviceInstance(), o => onRemove( (T)o ) );
            return c;
        }

    }
}
