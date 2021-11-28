using System;

namespace CK.Core
{
    /// <summary>
    /// Defines a container for services.
    /// </summary>
    public interface ISimpleServiceContainer : IServiceProvider
    {
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
        ISimpleServiceContainer Add( Type serviceType, Func<Object> serviceInstance, Action<Object>? onRemove = null );

        /// <summary>
        /// Registers a service with its implementation (and an optional callback that will be called when the service will be removed).
        /// </summary>
        /// <param name="serviceType">Service type to register. It must not already exist in this container otherwise an exception is thrown.</param>
        /// <param name="serviceInstance">Implementation of the service. Can not be null.</param>
        /// <param name="onRemove">Optional action that will be called whenever <see cref="Remove"/>, <see cref="Clear"/> or <see cref="IDisposable.Dispose"/>.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        ISimpleServiceContainer Add( Type serviceType, object serviceInstance, Action<Object>? onRemove = null );

        /// <summary>
        /// Unregisters a service. Can be called even if the service does not exist.
        /// The service is first removed and then the OnRemove associated action is called if it exists:
        /// this enables OnRemove action to be bound to a method that safely calls back this Remove method.
        /// </summary>
        /// <param name="serviceType">Service type to unregister.</param>
        /// <param name="autoCallDispose">
        /// False to not call Dispose if the instance exists and is disposable.
        /// OnRemove action if it exists is always executed.
        /// </param>
        /// <returns>This object to enable fluent syntax.</returns>
        ISimpleServiceContainer Remove( Type serviceType, bool autoCallDispose = true );
              
        /// <summary>
        /// Unregisters all the services. Any "on remove" actions are executed.
        /// </summary>
        /// <returns>This object to enable fluent syntax.</returns>
        ISimpleServiceContainer Clear();
 
        /// <summary>
        /// Disables a service: null will always be returned by this <see cref="IServiceProvider.GetService"/>
        /// regardless of any fallbacks that may exist for this container.
        /// </summary>
        /// <remarks>
        /// This is not the same as calling <see cref="Add(Type,Func{Object},Action{Object})"/> with a null instance. A null instance for a service (a callback that always returns null)
        /// is nearly the same as calling <see cref="Remove"/>: any fallbacks (to a base <see cref="IServiceProvider"/> for example) can occur.
        /// This one is stronger since this must prevent fallbacks.
        /// </remarks>
        /// <param name="serviceType">Service type to disable. It must not already exist in this container otherwise an exception is thrown.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        ISimpleServiceContainer AddDisabled( Type serviceType );
        
    }
}
