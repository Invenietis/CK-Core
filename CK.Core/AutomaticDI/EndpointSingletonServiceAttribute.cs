using System;

namespace CK.Core
{
    /// <summary>
    /// States that the decorated class or interface is a singleton endpoint service.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
    public sealed class EndpointSingletonServiceAttribute : Attribute
    {
    }
}
