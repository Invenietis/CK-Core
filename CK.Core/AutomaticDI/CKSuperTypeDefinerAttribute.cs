using System;

namespace CK.Core;

/// <summary>
/// Attribute that marks a type as being a "Definer of Definer". When <see cref="CKTypeDefinerAttribute"/> is the "father"
/// of the actual CK type (<see cref="IRealObject"/>, IPoco (defined in CK.StObj.Model) or <see cref="IAutoService"/>), this
/// acts as a "grand father".
/// <para>
/// This attribute, just like <see cref="IRealObject"/>, <see cref="IAutoService"/>, <see cref="IScopedAutoService"/>,
/// <see cref="ISingletonAutoService"/> and <see cref="CKTypeDefinerAttribute"/> can be created anywhere: as long as the
/// name is "CKTypeSuperDefinerAttribute" (regardless of the namespace), it will be honored.
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
public class CKTypeSuperDefinerAttribute : Attribute
{
}
