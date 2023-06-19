namespace CK.Core
{
    /// <summary>
    /// Companion service that must be available for any endpoint service that is marked as
    /// a ubiquitous information by <see cref="EndpointScopedServiceAttribute"/> constructor
    /// flag.
    /// <para>
    /// This is a singleton service because the default value of an ubiquitous info should not
    /// depend on the resolution context. However, technically, it could be: if a scope resolution happens to
    /// be required (or simply makes sense for any reason), this can be changed but currently we stick to
    /// the safer and more strict singleton lifetime.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The endpoint ubiquitous information service.</typeparam>
    public interface IEndpointUbiquitousServiceDefault<out T> : ISingletonAutoService where T : class
    {
        /// <summary>
        /// Gets a default value for the ubiquitous information service <typeparamref name="T"/>.
        /// </summary>
        public abstract T Default { get; }
    }
}
