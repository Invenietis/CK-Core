namespace CK.Core;

/// <summary>
/// Companion service that must be available for any <see cref="IAmbientAutoService"/>.
/// <para>
/// This is a singleton service because the default value of an ambient service must not
/// depend on the resolution context.
/// </para>
/// </summary>
/// <typeparam name="T">
/// The ambient service type. This is not not constrained to <see cref="IAmbientAutoService"/>
/// because <see cref="ScopedContainerConfiguredServiceAttribute"/> can also be used to declare
/// an ambient service.
/// </typeparam>
public interface IAmbientServiceDefaultProvider<out T> : ISingletonAutoService where T : class
{
    /// <summary>
    /// Gets a default value for the ambient service <typeparamref name="T"/>.
    /// </summary>
    T Default { get; }
}
