using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// Service container (that is a <see cref="IServiceProvider"/>) subordinated to an optional base IServiceProvider 
    /// that acts as a fallback if the service is not found at this level.
    /// Service creation may be deferred thanks to callback registration and an optional remove callback can be registered
    /// with each entry.
    /// </summary>
    /// <remarks>
    /// This container is registered as the service associated to <see cref="IServiceProvider"/> and <see cref="ISimpleServiceContainer"/>
    /// thanks to the overridable <see cref="GetDirectService"/>. This method may be overridden to return other built-in services: these services
    /// take precedence over the registered services.
    /// </remarks>
    public class SimpleServiceContainer : ISimpleServiceContainer, IDisposable
	{
        struct ServiceEntry
        {
            public object? Instance;
            public Func<Object>? Creator;
            public Action<Object>? OnRemove;
        }

        readonly Dictionary<Type,ServiceEntry> _services;
        IServiceProvider? _baseProvider;

        /// <summary>
        /// Initializes a new <see cref="SimpleServiceContainer"/>.
        /// </summary>
        public SimpleServiceContainer()
            : this( null )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SimpleServiceContainer"/> with a <see cref="BaseProvider"/>.
        /// </summary>
        /// <param name="baseProvider">Base <see cref="IServiceProvider"/> provider.</param>
        public SimpleServiceContainer( IServiceProvider? baseProvider )
        {
            _baseProvider = baseProvider;
            _services = new Dictionary<Type, ServiceEntry>();
        }

        /// <summary>
        /// Gets or sets the <see cref="IServiceProvider"/> that is queried whenever a service
        /// is not found in this container.
        /// </summary>
        public IServiceProvider? BaseProvider
        {
            get { return _baseProvider; }
            set 
            {
                SimpleServiceContainer? v = value as SimpleServiceContainer;
                while( v != null )
                {
                    if( v == this ) throw new Exception( "BaseProvider circle detected" );
                    v = v.BaseProvider as SimpleServiceContainer;
                }
                _baseProvider = value; 
            }
        }

        /// <summary>
        /// Registers a service associated to a callback (and an optional callback that will be called when the service will be removed).
        /// The <paramref name="serviceInstance"/> is called as long as no service has been obtained (serviceInstance returns null). 
        /// Once the actual service has been obtained, it is kept and serviceInstance is not called anymore.
        /// </summary>
        /// <param name="serviceType">Service type to register. It must not already exist in this container otherwise an exception is thrown.</param>
        /// <param name="serviceInstance">Delegate to call when needed. Can not be null.</param>
        /// <param name="onRemove">Optional action that will be called whenever <see cref="Remove"/>, <see cref="Clear"/> or <see cref="IDisposable.Dispose"/>
        /// is called and a service as been successfully obtained.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public ISimpleServiceContainer Add( Type serviceType, Func<Object> serviceInstance, Action<Object>? onRemove = null )
        {
            if( serviceType is null ) throw new ArgumentNullException( nameof( serviceType ) );
            if( serviceInstance == null ) throw new ArgumentNullException( nameof( serviceInstance ) );
            if( GetDirectService( serviceType ) != null ) throw new Exception( String.Format( CoreResources.ServiceAlreadyDirectlySupported, serviceType.FullName ) );
            DoAdd( serviceType, new ServiceEntry() { Instance = null, Creator = serviceInstance, OnRemove = onRemove } );
            return this;
        }

        /// <summary>
        /// Registers a service with its implementation (and an optional callback that will be called when the service will be removed).
        /// </summary>
        /// <param name="serviceType">Service type to register. It must not already exist in this container otherwise an exception is thrown.</param>
        /// <param name="serviceInstance">Implementation of the service. Can not be null.</param>
        /// <param name="onRemove">Optional action that will be called whenever <see cref="Remove"/>, <see cref="Clear"/> or <see cref="IDisposable.Dispose"/>.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public ISimpleServiceContainer Add( Type serviceType, object serviceInstance, Action<Object>? onRemove = null )
        {
            if( serviceType is null ) throw new ArgumentNullException( nameof( serviceType ) );
            if( serviceInstance == null ) throw new ArgumentNullException( nameof( serviceInstance ) );
            if( GetDirectService( serviceType ) != null ) throw new Exception( String.Format( CoreResources.ServiceAlreadyDirectlySupported, serviceType.FullName ) );
            if( !serviceType.IsAssignableFrom( serviceInstance.GetType() ) ) throw new Exception( String.Format( CoreResources.ServiceImplTypeMismatch, serviceType.FullName, serviceInstance.GetType().FullName ) );
            DoAdd( serviceType, new ServiceEntry() { Instance = serviceInstance, Creator = null, OnRemove = onRemove } );
            return this;
        }

        /// <summary>
        /// Disables a service: null will always be returned by this <see cref="IServiceProvider.GetService"/>
        /// regardless of any fallbacks from <see cref="BaseProvider"/>. 
        /// Direct services returned by <see cref="GetDirectService"/> can not be disabled.
        /// </summary>
        /// <remarks>
        /// This is not the same as calling <see cref="Add(Type,Func{Object},Action{Object})"/> with a null instance.
        /// A null instance for a service (a callback that always returns null) is nearly the same as
        /// calling <see cref="Remove"/>: any fallbacks (to a base <see cref="IServiceProvider"/> for example) can occur.
        /// This is stronger since this prevents fallbacks.
        /// </remarks>
        /// <param name="serviceType">Service type to disable. It must not already exist in this container otherwise an exception is thrown.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public ISimpleServiceContainer AddDisabled( Type serviceType )
        {
            if( serviceType is null ) throw new ArgumentNullException( nameof( serviceType ) );
            if( GetDirectService( serviceType ) != null ) throw new Exception( String.Format( CoreResources.DirectServicesCanNotBeDisabled, serviceType.FullName ) );
            DoAdd( serviceType, new ServiceEntry() );
            return this;
        }

        /// <summary>
        /// Unregisters a service. Can be called even if the service does not exist.
        /// The service is first removed, then the OnRemove associated action is called if it exists
        /// and if the instance is <see cref="IDisposable"/>, its Dispose method is called.
        /// </summary>
        /// <param name="serviceType">Service type to unregister.</param>
        /// <param name="autoCallDispose">
        /// False to not call Dispose if the instance exists and is disposable.
        /// OnRemove action if it exists is always executed.
        /// </param>
        /// <returns>This object to enable fluent syntax.</returns>
        public ISimpleServiceContainer Remove( Type serviceType, bool autoCallDispose = true )
        {
            if( _services.TryGetValue( serviceType, out ServiceEntry e ) )
            {
                _services.Remove( serviceType );
                if( e.Instance != null )
                {
                    e.OnRemove?.Invoke( e.Instance );
                    if( e.Instance is IDisposable d ) d.Dispose();
                }
            }
            return this;
        }

        /// <summary>
        /// Unregisters all the services. Any "on remove" actions are executed and
        /// IDisposable instances are automatically disposed.
        /// </summary>
        /// <returns>This object to enable fluent syntax.</returns>
        public ISimpleServiceContainer Clear()
        {
            Type[] entries = new Type[ _services.Count ];
            _services.Keys.CopyTo( entries, 0 );

            for (int i = 0 ; i < entries.Length ; i++)
            {
                Remove( entries[i] );
            }

            return this;
        }

        /// <summary>
        /// Implements <see cref="IServiceProvider.GetService"/>.
        /// </summary>
        /// <param name="serviceType">Type of the service to obtain.</param>
        /// <returns>Built-in service, registered service, service from <see cref="BaseProvider"/> or null.</returns>
        public object? GetService( Type serviceType )
        {
            if( serviceType is null ) throw new ArgumentNullException( nameof( serviceType ) );
            object? result = GetDirectService( serviceType );
            if( result == null )
            {
                if( _services.TryGetValue( serviceType, out ServiceEntry e ) )
                {
                    result = e.Instance;
                    if( result == null )
                    {
                        // Disabled service: returns null immediately.
                        if( e.Creator == null ) return null;

                        result = e.Creator();
                        if( result != null )
                        {
                            if( !serviceType.IsAssignableFrom( result.GetType() ) ) throw new Exception( string.Format( CoreResources.ServiceImplCallbackTypeMismatch, serviceType.FullName, result.GetType().FullName ) );
                            // Release Creator reference to minimize (subtle) leaks.
                            e.Creator = null;
                            e.Instance = result;
                            // Since ServiceEntry is a value type: we need to update it back.
                            _services[serviceType] = e;
                        }
                        // If result is still null, we fallback (and Creator will be called again the next time).
                    }
                }
                if( result == null && _baseProvider != null ) result = _baseProvider.GetService( serviceType );
            }
            return result;
        }

        /// <summary>
        /// Must return built-in services if any. These services take precedence over any registered services.
        /// This base implementation returns this object for <see cref="IServiceProvider"/> and <see cref="ISimpleServiceContainer"/>.
        /// </summary>
        /// <param name="serviceType">Type of the service to obtain.</param>
        /// <returns>A built-in service or null.</returns>
        protected virtual object? GetDirectService( Type serviceType )
        {
            if( serviceType.Equals( typeof( IServiceProvider ) ) || serviceType.Equals( typeof( ISimpleServiceContainer ) ) ) return this;
            return null;
        }

        /// <summary>
        /// Ensures that potential unmanaged resources are correctly released by calling <see cref="Dispose(bool)"/> with false.
        /// </summary>
        ~SimpleServiceContainer()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposing calls <see cref="Clear"/> to unregister all services. Any "on remove" actions are executed.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// When <paramref name="disposing"/> is true, calls <see cref="Clear"/> to unregister all services. 
        /// Any "on remove" actions are executed.
        /// </summary>
        /// <param name="disposing">
        /// True if <see cref="Dispose()"/> has been called: managed resources can be disposed.
        /// False if we are finalizing: only unmanaged resources must be considered.
        /// </param>
        protected virtual void Dispose( bool disposing )
        {
            if( disposing ) Clear();
        }

        /// <summary>
        /// Corrects ArgumentException throw by a Dictionary when adding an existing key. 
        /// </summary>
        void DoAdd( Type s, ServiceEntry e )
        {
            Debug.Assert( s is not null );
            try
            {
                _services.Add( s, e );
            }
            catch( ArgumentException ex )
            {
                if( _services.ContainsKey( s ) )
                    throw new Exception( String.Format( CoreResources.ServiceAlreadyRegistered, s.FullName ), ex );
                throw;
            }
        }

    }

}
