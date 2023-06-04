# Automatic Dependency Injection "duck typed" basic contracts

This set of interfaces and attributes are pure markers that are defined here (in CK.Core) for convenience
but can be copied to any other assembly without a dependency on CK.Core.

These types are the heart of the Automatic DI that is handled by CK.StObj.Model and its code generation engine.

> CK.StObj.Model define more "duck types" that defines "context" for services and "marshalling" of these services across "contexts".
The "context" management is not currently finalized (endpoint context should be defined by a type and a "front" service should be
tied to a endpoint context - currently the "front context" notion is ubiquitous). Once these evolutions will be done, the corresponding
duck types will be available here.

Similar attributes related to CKSetup process are also defined here in CK.Core. See [CKSetup](CKSetup) directory.

## IRealObject

This interface marker states that a class or an interface instance
is a gateway to an external resource or a foundation of the architecture.
Such foundations are necessarily unique instance in a "context" that is defined
by the "StObjMap".

It is not required to be this exact type: any empty interface (no members)
named "IRealObject" defined in any namespace or assembly will be considered as
a valid marker.

If the lifetimes of this [IRealObject](IRealObject.cs) and [ISingletonAutoService](ISingletonAutoService.cs)
instances are the same, their roles are different as well as the way they are handled.

Real objects must be used to model actual singletons "in real life", typically external resources
with which the system interacts such as a data repository or an external service.
Singleton services are more classical services that happens to be able to be shared by different
activities because of their thread safety and the fact that they depend only on other singleton
services or real objects.

"Normal" singleton services rely on "normal" constructor injection but real objects cannot have constructors: it is replaced
by the `StObjConstruct` method private method that isolates dependencies between a base class and its specializations.

Real objects support the optional, non virtual and private following methods:

 - `void StObjConstruct(...)`
is the method that defines the dependencies: its parameters are other IRealObjects on which this object depends, the parameters drive the topological sort
and there is no real point to put code in it, except that references to dependencies may be captured here if needed.

Defining this private method doesn't require any dependency.

 - `void StObjInitialize( IActivityMonitor, IStObjObjectMap )`
is called at runtime once all the real objects have been instantiated and are available.

Defining this private method requires a dependency to CK.ActivityMonitor and CK.StObj.Model packages.

 - `void RegisterStartupServices( IActivityMonitor, SimpleServiceContainer )`
is called at runtime once all the real objects have been initialized and can register any services in the [SimpleServiceContainer](../ServiceContainer/SimpleServiceContainer.cs)
that may be used by the following `ConfigureServices` methods.

Defining this private method requires a dependency to CK.ActivityMonitor package.

- `void ConfigureServices( StObjContextRoot.ServiceRegister, ... )`
enables real objects to configure the DI (registering new services, configuring things, etc.) based on any number of parameters that
can be any other real objects and/or startup services previously registered.

Defining this private method requires a dependency to CK.StObj.Model package.

- `void OnHostStart/Stop( ... )` or `Task/ValueTask OnHostStart/StopAsync( ... )`
enables real objects to act like hosted services: `OnHostStart[Async]` and `OnHostStop[Async]` are called by an automatically
generated Microsoft.Extensions.Hosting.IHostedService on startup (resp. on host stop).

The parameters can be any scope or singleton service that may be available: a temporary scope is created to call all the Start (resp. Stop)
methods and these methods' execution share this temporary scope.

Defining this private method doesn't require any direct dependency but these methods requires the https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Abstractions/
package to be available in the final application (otherwise a compilation error of the generated code will be raised).

> Note that a class that is a IRealObject can perfectly implement a IAutoService: this will be
resolved as a singleton (a ISingletonAutoService) and will be a potential implementation of the service
(that may be replaced). This applies only to class, not to interface: an interface cannot be both
a IAutoService and a IRealObject.

## IAutoService
This interface marker states that a class or an interface instance is a service that will participate in automatic dependency injection.

It is not required to be this exact type: any empty interface (no members)
named "IAutoService" defined in any namespace will be considered as
a valid marker.

This marker doesn't indicate the scoped vs. singleton lifetime. The actual
lifetime depends on the final implementation that may be marked with the more
specific [ISingletonAutoService](ISingletonAutoService.cs) or [IScopedAutoService](IScopedAutoService.cs)
(but not both) or by analyzing its single constructor parameters: if and only if all parameters
are known to be singletons, the service will be singleton otherwise it will be considered as a Scoped one.

## ISingletonAutoService : IAutoService
This interface marker states that a class or an interface instance
must be a globally unique Service in a context, just like IRealObject.

It is not required to be this exact type: any empty interface (no members)
named "ISingletonAutoService" defined in any namespace will be considered as
a valid marker, regardless of the fact that it specializes any interface
named "IAutoService".

## IScopedAutoService : IAutoService

This interface marker states that a class or an interface instance
must be a unique Service in a scope.

It is not required to be this exact type: any empty interface (no members)
named "IScopedAutoService" defined in any namespace will be considered as
a valid marker, regardless of the fact that it specializes any interface
named "IAutoService".

Note that even if an implementation only relies on other singletons or objects,
this interface forces the service to be scoped.

If there is no specific constraint, the IAutoService marker
should be used for abstractions so that its scoped vs. singleton lifetime is
either determined by the final, actual, implementation that can be automatically
detected based on its constructor dependencies and/or by the way this Service is
used referenced by the other participants.

## EndpointScopedServiceAttribute & EndpointSingletonServiceAttribute

Marks a class or an interface to be a endpoint service. A endpoint service is not necessarily
available from all DI containers that may exist in an application. Note that defining a service
as being "endpoint bound" requires to declare its lifetime (hence the 2 attributes).

When a `IAutoService` is marked as an endpoint service, it is available in the global DI container but not
in other endpoints: it is up to the other endpoint to explicitly support the service by registering the
abstractions and/or implementations in its own service collection.

It is not required to be this exact type: any attribute named "EndpointScoped/SingletonServiceAttribute" defined
in any namespace will be considered as a valid marker.

## IsMultipleAttribute
Marks an interface so that all its mappings to concrete classes must be automatically
registered, regardless of any existing registrations.

It is not required to be this exact type: any attribute named "IMultipleAutoService" defined in any
namespace will be considered as a valid marker.

Interfaces marked as "Multiple Service" are not compatible with IRealObject but can support
any other auto service markers like IScopedAutoService.
This attribute cancels the implicit unicity of the mapping but doesn't impact the lifetime (or the "endpoint/process context" related
aspect): lifetime and "context" apply eventually to the implementation.

## ReplaceAutoServiceAttribute

Optional attribute for `IAutoService` **implementation** that
declares that this implementation replaces another one (the replaced implementation
is the single constructor parameter).

Note that this attribute is useless if this implementation specializes the replaced service since
discovering the most precise implementation is one of the key goal of Auto services handling.

It is also useless if the replaced service is used by this implementation: as long as a parameter with the
same type appears in its constructor, this service "covers" (and possibly reuses) the replaced one.

This attribute, just like `IRealObject`, `IAutoService`, `IScopedAutoService`
and `ISingletonAutoService` can be created anywhere: the name must be `ReplaceAutoServiceAttribute`
and a constructor with a Type and/or a constructor with a string must be defined.


## CKTypeDefinerAttribute
Attribute that marks a type as being a "Definer" of a `IRealObject`, IPoco (defined in CK.StObj.Model) or `IAutoService`.
This defines the decorated object as a "base type" of the CK type: it is not itself a `IAutoService`, `IPoco` or `IRealObject` type but
its specializations are (unless they also are decorated with this attribute).
This is a little bit like an 'abstract' type regarding Auto Services, Poco or Real Objects.

This attribute can be created anywhere: as long as the name is `CKTypeDefinerAttribute` 
(regardless of the namespace), it will be honored.

## CKTypeSuperDefinerAttribute
Attribute that marks a type as being a "Definer of Definer". Where `CKTypeDefinerAttribute` is the "father"
of the actual CK type this acts as a "grand father".

This attribute can be created anywhere: as long as the name is "CKTypeSuperDefinerAttribute" (regardless of the namespace), it will be honored.

## ExcludeCKTypeAttribute
Attribute that excludes a type from Automatic DI discovery process. 

This attribute can be created anywhere: as long as the name is "ExcludeCKTypeAttribute" (regardless of the namespace), it will be honored.

## PreserveAssemblyReferenceAttribute

This is a simple helper that avoids a reference to an assembly to be trimmed out by the compiler/linker.
