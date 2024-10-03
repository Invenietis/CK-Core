using System;

namespace CK.Core;

/// <summary>
/// Attribute that marks a type as being a "Definer" of a <see cref="IRealObject"/>, IPoco (defined in CK.StObj.Model) or <see cref="IAutoService"/>. 
/// This defines the decorated object as a "base type" of the CK type: it is not itself a IAutoService, IPoco or IRealObject type but
/// its specializations are (unless they also are decorated with this attribute).
/// This is a little bit like an 'abstract' type regarding Auto Services, Poco or Real Objects.
/// <para>
/// This attribute, just like <see cref="IRealObject"/>, <see cref="IAutoService"/>, <see cref="IScopedAutoService"/>
/// and <see cref="ISingletonAutoService"/> can be created anywhere: as long as the name is "CKTypeDefinerAttribute" 
/// (regardless of the namespace), it will be honored.
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
public class CKTypeDefinerAttribute : Attribute
{
}
