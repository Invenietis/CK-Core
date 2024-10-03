namespace CK.Core;

/// <summary>
/// This interface marker states that a class or an interface instance
/// is a service that will participate in automatic dependency injection.
/// <para>
/// It is not required to be this exact type: any empty interface (no members)
/// named "IAutoService" defined in any namespace will be considered as
/// a valid marker.
/// </para>
/// <para>
/// This marker doesn't indicate the scoped vs. singleton lifetime. The actual
/// lifetime depends on the final implementation that may be marked with the more
/// specific <see cref="ISingletonAutoService"/> or <see cref="IScopedAutoService"/>
/// (but not both) or by analyzing its constructor parameters: if and only if all parameters
/// are known to be singletons, the service will be singleton otherwise it will be
/// considered as a Scoped one.
/// </para>
/// </summary>
public interface IAutoService
{
}
