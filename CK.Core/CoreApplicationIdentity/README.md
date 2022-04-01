# CoreApplicationIdentity

## Goal

The goal of this small static class is to capture once for all the notion of a process identity. And this is not a simple
issue.

The idea here is to as simple as possible:

- A process is rooted in a "domain", that is a family of applications that typically work together. A domain 
is like a "realm" in security frameworks, a "tenant" or an "organization" in a cloud infrastructure. 
- Then comes an optional "environment" that aims to identify a deployment context like "Production" or "Staging".
- Finally the `PartyName` identifies the "application" itself, both the code base and its role in the architecture.

The first usage of this `DomainName/PartyName` or `DomainName/EnvironmentName/PartyName` simple path is to group/route logs
emitted by processes.

This doesn't imply any form of uniqueness, "process uniqueness" is a complex matter. A process can only be truly unique (at
any point in time) by using synchronization primitives like Mutex or specific coordination infrastructure (election algorithms)
in distributed systems. Even a "single web application" may not be truly unique: when its host does an "application recycling",
for a short period of time, two instances of the "same" application co-exist.

This small class will never solve this issue but it aims to capture and expose the necessary and sufficient information to reason
(or help reasoning) about it:

- The "running instance" itself is necessarily unique because of the `InstanceId` that is a unique random string.
- Other "identity" can be captured by the `ContextIdentifier` that can use process arguments, working directory
or any other contextual information that helps identify a process.

## Initialization and usage

The usage can hardly be simpler: `CoreApplicationIdentity.Instance` is a singleton withe the 5 basic informations.

Initializations is a little bit trickier since this must be an immutable information: one shouldn't be able to use it before
it is built and one shouldn't modify it once built.

To configure the identity `Configure` or `TryConfigure` before its very first use (first call to `Instance` property) can be used:

```csharp
  /// <summary>
  /// Configure the application identity if it's not yet initialized or throws an <see cref="InvalidOperationException"/> otherwise.
  /// </summary>
  /// <param name="configurator">The configuration action.</param>
  public static void Configure( Action<Builder> configurator )
  {
    // ...
  }

  /// <summary>
  /// Tries to configure the application identity if it's not yet initialized.
  /// </summary>
  /// <param name="configurator">The configuration action.</param>
  /// <returns>True if the <paramref name="configurator"/> has been called, false if the <see cref="Instance"/> is already available.</returns>
  public static bool TryConfigure( Action<Builder> configurator )
  {
    // ...
  }
``` 

For code that wants to be safe regarding any deferred initialization of the identity, they can use the `IsInitialized` property
and/or the `OnInitialized` method that handles callbacks.

```csharp
  /// <summary>
  /// Gets whether the <see cref="Instance"/> has been initialized or
  /// can still be configured.
  /// </summary>
  public static bool IsInitialized { get; }

  /// <summary>
  /// Registers a callback that will be called when the <see cref="Instance"/> will be available
  /// or immediately if the instance has already been configured.
  /// </summary>
  /// <param name="action">Any action that requires the application's identity to be available.</param>
  public static void OnInitialized( Action action )
  {
    // ...
  }
```

