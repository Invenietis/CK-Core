using System;

namespace CK.Core;

/// <summary>
/// Specifies that an an empty content is invalid: this
/// applies to string and collections.
/// </summary>
[AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
public sealed class EmptyInvalidAttribute : Attribute, IEmptyInvalid
{
}
