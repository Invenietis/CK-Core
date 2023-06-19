using System;

namespace CK.Core
{
    /// <summary>
    /// States that the decorated class or interface is a scoped endpoint service and optionally
    /// an ubiquitous information service.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false )]
    public sealed class EndpointScopedServiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="EndpointScopedServiceAttribute"/>.
        /// </summary>
        /// <param name="isUbiquitousEndpointInfo">See <see cref="IsUbiquitousEndpointInfo"/>.</param>
        public EndpointScopedServiceAttribute( bool isUbiquitousEndpointInfo = false )
        {
            IsUbiquitousEndpointInfo = isUbiquitousEndpointInfo;
        }

        /// <summary>
        /// Gets whether this endpoint scoped service is a ubiquitous service that carries
        /// information that are available from all endpoints.
        /// <para>
        /// Very few services are (or can be) such ubiquitous information:
        /// <list type="bullet">
        ///     <item>It must carry information of general interest.</item>
        ///     <item>It should be immutable (at least thread safe but there's little use of mutability here).</item>
        ///     <item>It must obviously not depend on any other service.</item>
        ///     <item>A <see cref="IEndpointUbiquitousServiceDefault{T}"/> must exist for it.</item>
        /// </list>
        /// Examples of such ubiquitous services are concepts like <c>IAuthenticationInfo</c>, <c>ITenantInfo</c>,
        /// <c>ICultureInfo</c>, etc.
        /// </para>
        /// </summary>
        public bool IsUbiquitousEndpointInfo { get; }
    }
}
