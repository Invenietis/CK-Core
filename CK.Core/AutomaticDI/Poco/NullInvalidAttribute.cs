using System;

namespace CK.Core;

/// <summary>
/// Marks a nullable Poco property that must be not null to be valid: : null is allowed
/// temporarily but the property must eventually be not be null for the Poco to be valid.
/// </summary>
[AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
public sealed class NullInvalidAttribute : Attribute, INullInvalidAttribute
{
}
