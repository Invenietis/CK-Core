namespace CK.Core;

/// <summary>
/// This interface marker states that a class or an interface instance
/// must be a globally unique Service in a context, just like <see cref="IRealObject"/>.
/// <para>
/// It is not required to be this exact type: any empty interface (no members)
/// named "ISingletonAutoService" defined in any namespace will be considered as
/// a valid marker, regardless of the fact that it specializes any interface
/// named "IAutoService".
/// </para>
/// </summary>
public interface ISingletonAutoService : IAutoService
{
}
