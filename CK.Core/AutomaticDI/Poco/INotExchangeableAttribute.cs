using System;

namespace CK.Core;

/// <summary>
/// Marker interface for <see cref="Attribute"/> that states that a type is not exchangeable.
/// This applies to IPoco interfaces, record structs and enums.
/// <para>
/// The <see cref="NotExchangeableAttribute"/> is the default implementation of this interface.
/// </para>
/// </summary>
public interface INotExchangeableAttribute
{
}
