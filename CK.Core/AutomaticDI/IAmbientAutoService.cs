namespace CK.Core;

/// <summary>
/// Marker interface for ambient service.
/// An ambient service is a scoped service that is available in all Dependecy Injection containers
/// and whose values can be injected in new "Unit of Work" (scoped service provider).
/// <para>
/// Very few services are (or can be) ambient services:
/// <list type="bullet">
///     <item>It must carry information of general interest.</item>
///     <item>It should be immutable (at least thread safe but there's little use of mutability here).</item>
///     <item>It must obviously not depend on any other service.</item>
///     <item>A <see cref="IAmbientServiceDefaultProvider{T}"/> must exist for it.</item>
/// </list>
/// Examples of such ambient services are concepts like <c>IAuthenticationInfo</c>, <c>ITenantInfo</c>,
/// <c>CurrentCultureInfo</c>, etc.
/// </para>
/// </summary>
[CKTypeDefiner]
public interface IAmbientAutoService : IScopedAutoService
{
}
