# Mutable & ImmutableConfigurationSection

These 2 helpers implement [IConfigurationSection](https://learn.microsoft.com/fr-fr/dotnet/api/microsoft.extensions.configuration.iconfigurationsection).

The [MutableConfigurationSection](MutableConfigurationSection.cs) can be changed and JSON configuration can easily
be merged. It acts as a builder for immutable configuration.

[ImmutableConfigurationSection](ImmutableConfigurationSection.cs) captures once for all the content and path of any
other `IConfigurationSection`.

