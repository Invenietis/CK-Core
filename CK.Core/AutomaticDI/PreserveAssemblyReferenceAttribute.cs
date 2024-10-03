using System;
using System.Reflection;

namespace CK.Core;

/// <summary>
/// Simple assembly attribute that forces a reference to an assembly to be preserved
/// and not trimmed out to be used when there is no direct use of the target library.
/// This replaces any hidden static or other tricks by a more explicit declaration.
/// </summary>
[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true, Inherited = false )]
public sealed class PreserveAssemblyReferenceAttribute : Attribute
{
    readonly Type _t;

    /// <summary>
    /// Initializes this with a type from the assembly to reference.
    /// </summary>
    /// <param name="t">A public type from the target assembly.</param>
    public PreserveAssemblyReferenceAttribute( Type t ) => _t = t;

    /// <summary>
    /// Gets the referenced assembly.
    /// </summary>
    public Assembly Target => _t.Assembly;
}
