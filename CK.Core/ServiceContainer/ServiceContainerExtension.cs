using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for system/component model interfaces.
    /// </summary>
    public static class ServiceContainerExtension
    {
        /// <summary>
        /// Strongly typed version of <see cref="IServiceProvider.GetService"/>.
        /// Unfortunately there is no way (as of today) to condition the return nullability to the <paramref name="throwOnNull"/> value:
        /// this declares that returned object is not null even if it can be null when throwOnNull is false.
        /// </summary>
        /// <param name="this">This service provider.</param>
        /// <param name="throwOnNull">True to throw an exception if the service can not be provided (otherwise null is returned).</param>
        /// <returns>A service object of the required type or null if not found and <paramref name="throwOnNull"/> is false.</returns>
        public static T GetService<T>( this IServiceProvider @this, bool throwOnNull )
        {
            T? s = (T?)@this.GetService( typeof( T ) );
            if( throwOnNull && s == null ) ThrowUnregistered<T>();
            return s!;
        }

        /// <summary>
        /// Strongly typed version of <see cref="IServiceProvider.GetService"/>.
        /// </summary>
        /// <param name="this">This service provider.</param>
        /// <param name="service">The service if it can be resolved.</param>
        /// <param name="throwOnNull">True to throw an exception if the service can not be provided (otherwise null is returned).</param>
        /// <returns>A service object of the required type or null if not found and <paramref name="throwOnNull"/> is false.</returns>
        public static bool TryGetService<T>( this IServiceProvider @this, [NotNullWhen(true)]out T? service, bool throwOnNull )
        {
            service = (T?)@this.GetService( typeof( T ) );
            if( throwOnNull && service == null ) ThrowUnregistered<T>();
            return service != null;
        }

        [DoesNotReturn]
        static void ThrowUnregistered<T>()
        {
            throw new Exception( String.Format( Impl.CoreResources.UnregisteredServiceInServiceProvider, typeof( T ).FullName ) );
        }

        /// <summary>
        /// Type safe version to remove a registered type.
        /// </summary>
        /// <param name="this">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Remove<T>( this ISimpleServiceContainer @this )
        {
            return @this.Remove( typeof( T ) );
        }

        /// <summary>
        /// Type safe version to register a service implementation (type of the service is the type of the implementation).
        /// </summary>
        /// <param name="this">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Implementation of the service. Can not be null.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer @this, T serviceInstance )
        {
            if( serviceInstance == null ) ThrowArgumentNull( nameof(serviceInstance) );
            return @this.Add( typeof( T ), serviceInstance, null );
        }

        [DoesNotReturn]
        static void ThrowArgumentNull( string parameterName )
        {
            throw new ArgumentNullException( parameterName );
        }

        /// <summary>
        /// Type safe version to register a service implementation (type of the service is the type of the implementation), 
        /// and a callback that will be called when the service is eventually removed.
        /// </summary>
        /// <param name="this">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Implementation of the service. Can not be null.</param>
        /// <param name="onRemove">Action that will be called whenever <see cref="ISimpleServiceContainer.Remove"/>, <see cref="ISimpleServiceContainer.Clear"/> or <see cref="IDisposable.Dispose"/>.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer @this, T serviceInstance, Action<T> onRemove )
        {
            if( onRemove == null ) ThrowArgumentNull( nameof( onRemove ) );
            if( serviceInstance == null ) ThrowArgumentNull( nameof( serviceInstance ) );
            return @this.Add( typeof( T ), serviceInstance, o => onRemove( (T)o ) );
        }

        /// <summary>
        /// Type safe version to register a service associated to a callback.
        /// The <paramref name="serviceInstance"/> is called as long as no service has been obtained (serviceInstance returns null). 
        /// Once the actual service has been obtained, it is kept and serviceInstance is not called anymore.
        /// </summary>
        /// <param name="this">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Delegate to call when needed. Can not be null.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer @this, Func<T> serviceInstance ) where T : class
        {
            // It is the overloaded version that takes a Func<object> serviceInstance 
            // that is called (unit tests asserts this).
            // To allow the covariance, we MUST constrain the type T to be a reference class (hence the where clause).
            return @this.Add( typeof( T ), serviceInstance, null );
        }

        /// <summary>
        /// Type safe version to register a service associated to a callback (and a callback that will be called when the service is eventually removed).
        /// The <paramref name="serviceInstance"/> is called as long as no service has been obtained (serviceInstance returns null). 
        /// Once the actual service has been obtained, it is kept and serviceInstance is not called anymore.
        /// </summary>
        /// <param name="this">This <see cref="ISimpleServiceContainer"/> object.</param>
        /// <param name="serviceInstance">Delegate to call when needed. Can not be null.</param>
        /// <param name="onRemove">Action that will be called whenever <see cref="ISimpleServiceContainer.Remove"/>, <see cref="ISimpleServiceContainer.Clear"/> or <see cref="IDisposable.Dispose"/>
        /// is called and a service as been successfully obtained.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public static ISimpleServiceContainer Add<T>( this ISimpleServiceContainer @this, Func<T> serviceInstance, Action<T> onRemove ) where T : class
        {
            // It is the overloaded version that takes a Func<object> serviceInstance 
            // that is called (unit tests asserts this).
            // To allow the covariance, we MUST constrain the type T to be a reference class (hence the where clause).
            //
            // On the other hand, for the onRemove action we cannot do any miracle: we need to adapt the call.
            //
            if( onRemove == null ) ThrowArgumentNull( nameof( onRemove ) );
            return @this.Add( typeof( T ), serviceInstance, o => onRemove( (T)o ) );
        }

        /// <summary>
        /// Gets whether a service is available.
        /// (This simply calls <see cref="IServiceProvider.GetService(Type)"/> and checks for a non null value.)
        /// </summary>
        /// <typeparam name="T">Type of the service.</typeparam>
        /// <param name="this">This container.</param>
        /// <returns>True if the service is available, false otherwise.</returns>
        public static bool IsAvailable<T>( this ISimpleServiceContainer @this ) => IsAvailable( @this, typeof( T ) );

        /// <summary>
        /// Gets whether a service is available.
        /// (This simply calls <see cref="IServiceProvider.GetService(Type)"/> and checks for a non null value.)
        /// </summary>
        /// <param name="this">This container.</param>
        /// <param name="serviceType">Service type.</param>
        /// <returns>True if the service is available, false otherwise.</returns>
        public static bool IsAvailable( this ISimpleServiceContainer @this, Type serviceType ) => @this.GetService( serviceType ) != null;

    }
}
