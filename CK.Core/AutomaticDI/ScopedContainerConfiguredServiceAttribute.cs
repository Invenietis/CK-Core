using System;

namespace CK.Core;

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
public sealed class ScopedContainerConfiguredServiceAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="ScopedContainerConfiguredServiceAttribute"/>.
    /// </summary>
    public ScopedContainerConfiguredServiceAttribute()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ScopedContainerConfiguredServiceAttribute"/>.
    /// </summary>
    /// <param name="isAmbientService">See <see cref="IsAmbientService"/>.</param>
    [Obsolete( "Use ExternalTypes registration instead.", error: true )]
    public ScopedContainerConfiguredServiceAttribute( bool isAmbientService = false )
    {
    }

}
