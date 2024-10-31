using System;

namespace CK.Core;

/// <summary>
/// Marks a class or an interface with a singleton lifetime, but don't handle automatic DI for it (unlike
/// the <see cref="ISingletonAutoService"/> interface): the service must be manually registered in the
/// global DI container.
/// <para>
/// Unlike <see cref="SingletonContainerConfiguredServiceAttribute"/>, the service will be automatically registered
/// in the other DI containers.
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
public class SingletonServiceAttribute : Attribute
{
}
