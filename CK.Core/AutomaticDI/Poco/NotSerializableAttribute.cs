using System;

namespace CK.Core;

/// <summary>
/// Marks a type to be non serializable. This applies to IPoco interfaces, record structs and enums.
/// <para>
/// A non serializable type is by definition non exchangeable.
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false )]
public sealed class NotSerializableAttribute : Attribute, INotSerializableAttribute
{
}
