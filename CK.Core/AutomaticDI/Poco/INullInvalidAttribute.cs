using System;

namespace CK.Core;

/// <summary>
/// Marker interface for <see cref="Attribute"/> that states that a null nullable Poco property
/// is actually not valid: null is allowed temporarily but the property must eventually be not be
/// null for the Poco to be valid.
/// </summary>
public interface INullInvalidAttribute
{
}
