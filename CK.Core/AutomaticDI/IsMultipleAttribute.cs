using System;

namespace CK.Core
{
    /// <summary>
    /// Marks an interface so that all its mappings to concrete classes must be automatically
    /// registered, regardless of any existing registrations.
    /// <para>
    /// It is not required to be this exact type: any attribute named "IMultipleAutoService" defined in any
    /// namespace will be considered as a valid marker.
    /// </para>
    /// <para>
    /// Interfaces marked as "Multiple Service" are not compatible with <see cref="IRealObject"/> but can support
    /// any other auto service markers like <see cref="IFrontAutoService"/> or <see cref="IScopedAutoService"/>.
    /// This attribute cancels the implicit unicity of the mapping but doesn't impact the lifetime or the "front" related
    /// aspect: lifetime and "front aspects" apply eventually to the implementation.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
    public sealed class IsMultipleAttribute : Attribute
    {
    }

}
