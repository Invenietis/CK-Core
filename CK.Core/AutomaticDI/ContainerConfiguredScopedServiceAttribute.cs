using System;

namespace CK.Core
{
    /// <summary>
    /// States that the decorated class or interface is a scoped service that is registered specifically
    /// by Dependency Injection container.
    /// <para>
    /// This optionally allow to define the class or interface to be an ambient service. It is simpler to use
    /// the <see cref="IAmbientAutoService"/> interface marker: this defines an auto service that automatically
    /// handles specialization.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false )]
    public sealed class ContainerConfiguredScopedServiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="ContainerConfiguredScopedServiceAttribute"/>.
        /// </summary>
        /// <param name="isAmbientService">See <see cref="IsAmbientService"/>.</param>
        public ContainerConfiguredScopedServiceAttribute( bool isAmbientService = false )
        {
            IsAmbientService = isAmbientService;
        }

        /// <summary>
        /// Gets whether this scoped service is an ambient service.
        /// <para>
        /// Very few services are (or can be) ambient services:
        /// <list type="bullet">
        ///     <item>It must carry information of general interest.</item>
        ///     <item>It should be immutable (at least thread safe but there's little use of mutability here).</item>
        ///     <item>It must obviously not depend on any other service.</item>
        ///     <item>A <see cref="IAmbientServiceDefaultProvider{T}"/> must exist for it.</item>
        /// </list>
        /// Examples of such ambient services are concepts like <c>IAuthenticationInfo</c>, <c>ITenantInfo</c>,
        /// <c>CurrentCultureInfo</c>, etc.
        /// </para>
        /// </summary>
        public bool IsAmbientService { get; }
    }
}
