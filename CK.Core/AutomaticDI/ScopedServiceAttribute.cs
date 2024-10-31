using System;

namespace CK.Core;

/// <summary>
/// Marks a class or an interface with a scoped lifetime, but don't handle automatic DI for it (unlike
/// the <see cref="ISingletonAutoService"/> interface): the service must be manually registered in the
/// global DI container.
/// <para>
/// Unlike <see cref="ScopedContainerConfiguredServiceAttribute"/>, the service will be automatically registered
/// in the other DI containers with the same Service descriptor.
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
public class ScopedServiceAttribute : Attribute
{
}
