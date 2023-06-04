using System;

namespace CK.Core
{
    /// <summary>
    /// States that the decorated class or interface is a scoped endpoint service.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false )]
    public sealed class EndpointScopedServiceAttribute : Attribute
    {
    }
}
