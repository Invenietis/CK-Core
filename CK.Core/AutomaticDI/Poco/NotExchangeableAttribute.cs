using System;

namespace CK.Core;

/// <summary>
/// Marks a type to be non exchangeable. This applies to IPoco interfaces, record structs and enums.
/// </summary>
[AttributeUsage( AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false )]
public sealed class NotExchangeableAttribute : Attribute, INotExchangeableAttribute
{
}
