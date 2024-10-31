using System;
using System.Collections.Generic;

namespace CK.Core;

/// <summary>
/// Marks an interface so that all its mappings to concrete classes must be automatically
/// registered, regardless of any existing registrations.
/// <para>
/// Interfaces marked as "Multiple Service" are not compatible with <see cref="IRealObject"/> but can support
/// any other auto service markers like <see cref="IScopedAutoService"/>.
/// This attribute cancels the implicit unicity of the mapping but doesn't impact the lifetime: the <see cref="IEnumerable{T}"/>
/// will be a singleton if all the implementations are singletons otherwise it will be a scoped service.
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Interface|AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public sealed class IsMultipleAttribute : Attribute
{
}
