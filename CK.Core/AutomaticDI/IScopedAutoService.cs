namespace CK.Core;

/// <summary>
/// This interface marker states that a class or an interface instance
/// must be a unique Service in a scope.
/// <para>
/// It is not required to be this exact type: any empty interface (no members)
/// named "IScopedAutoService" defined in any namespace will be considered as
/// a valid marker, regardless of the fact that it specializes any interface
/// named "IAutoService".
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Note that even if an implementation only relies on other singletons or objects,
/// this interface forces the service to be scoped.
/// </para>
/// <para>
/// If there is no specific constraint, the <see cref="IAutoService"/> marker
/// should be used for abstractions so that its scoped vs. singleton lifetime is
/// either determined by the final, actual, implementation that can be automatically
/// detected based on its constructor dependencies and/or by the way this Service is
/// used referenced by the other participants.
/// </para>
/// </remarks>
public interface IScopedAutoService : IAutoService
{
}
