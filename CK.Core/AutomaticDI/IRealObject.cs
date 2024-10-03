namespace CK.Core;

/// <summary>
/// This interface marker states that a class or an interface instance
/// is a gateway to an external resource or a foundation of the architecture.
/// Such foundations are necessarily unique instance in a "context" that is defined
/// by the "StObjMap".
/// <para>
/// It is not required to be this exact type: any empty interface (no members)
/// named "IRealObject" defined in any namespace will be considered as
/// a valid marker (this duck typing is the same as <see cref="IAutoService"/> markers).
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// If the lifetimes of this IRealObject and <see cref="ISingletonAutoService"/>
/// instances are the same, their roles are different as well as the way they are handled.
/// </para>
/// <para>
/// Real objects must be used to model actual singletons "in real life", typically external resources
/// with which the system interacts such as a data repository or an external service.
/// Singleton services are more classical services that happens to be able to be shared by different
/// activities because of their thread safety and the fact that they depend only on other singleton
/// services or real objects.
/// </para>
/// <para>
/// "Normal" singleton services rely on "normal" constructor injection but real objects cannot have constructors: it is replaced
/// by the <c>StObjConstruct</c> method private method that isolates dependencies between a base class and its specializations.
/// </para>
/// <para>
/// Real objects support the optional, non virtual and private following methods:
/// <list type="number">
///     <item>
///     <term><c>void StObjConstruct(...)</c></term>
///     <decription>is the method that defines the dependencies: its parameters are other IRealObjects on which this object depends, the parameters drive the topological sort
///     and there is no real point to put code in it, except that references to dependencies may be captured here if needed.
///     </decription></item>
///     <item>
///     <term><c>void StObjInitialize( IActivityMonitor, IStObjObjectMap )</c></term>
///     <description>is called at runtime once all the real objects have been instantiated and are available.</description>
///     </item>
///     <item>
///     <term><c>void RegisterStartupServices( IActivityMonitor, SimpleServiceContainer )</c></term>
///     <description>is called at runtime once all the real objects have been initialized and can register any services in the <see cref="SimpleServiceContainer"/> that
///     may be used by the following ConfigureServices.</description>
///     </item>
///     <item>
///     <term><c>void ConfigureServices( StObjContextRoot.ServiceRegister, ... )</c></term>
///     <description>enables real objects to configure the DI (registering new services, configuring things, etc.) based on any number of parameters that
///     can be any other real objects and/or startup services previously registered.</description>
///     </item>
///     <item>
///     <term><c>void/Task/ValueTask OnHostStart/Stop[Async]( ... )</c></term>
///     <description>
///     enables real objects to act like hosted services: <c>OnHostStart[Async]</c> and <c>OnHostStop[Async]</c> are called by an automatically
///     generated Microsoft.Extensions.Hosting.IHostedService on startup (resp. on host stop).
///     <para>
///     The parameters can be any scope or singleton service that may be available: a temporary scope is created to call all the Start (resp. Stop)
///     methods and these methods' execution share this temporary scope.
///     </para>
///     <para>
///     Using these methods requires the https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Abstractions/ package to be available in
///     the final application (otherwise a compilation error of the generated code will be raised).
///     </para>
///     </description>
///     </item>
/// </list>
/// </para>
/// <para>
/// Note that a class that is a IRealObject can perfectly implement a IAutoService: this will be
/// resolved as a singleton (a ISingletonAutoService) and will be a potential implementation of the service
/// (that may be replaced). This applies only to class, not to interface: an interface cannot be both
/// a IAutoService and a IRealObject.
/// </para>
/// </remarks>
public interface IRealObject
{
}
