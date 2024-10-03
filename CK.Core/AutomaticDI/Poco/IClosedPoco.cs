namespace CK.Core;

/// <summary>
/// Marker interface for "Closed Poco".
/// A "Closed Poco" is a IPoco for which an interface that unifies all the IPoco interfaces of its family
/// must be defined.
/// <para>
/// This introduces a constraint similar to the "unique leaf" of the <see cref="IRealObject"/>: Closed Poco
/// can (and should) be handled through this "unique leaf". 
/// </para>
/// </summary>
[CKTypeDefiner]
public interface IClosedPoco : IPoco
{
}
