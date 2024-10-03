using System;

namespace CK.Core;

/// <summary>
/// Marker interface for <see cref="Attribute"/> that states that a Type is not serializable.
/// This applies to IPoco interfaces, record structs and enums.
/// <para>
/// A non serializable type is by definition non exchangeable.
/// </para>
/// <para>
/// The <see cref="NonSerializedAttribute"/> is the default implementation of this interface.
/// </para>
/// </summary>
public interface INotSerializableAttribute
{
}
