# Mutable & ImmutableConfigurationSection

These 2 helpers implement [IConfigurationSection](https://learn.microsoft.com/fr-fr/dotnet/api/microsoft.extensions.configuration.iconfigurationsection).

The [MutableConfigurationSection](MutableConfigurationSection.cs) can be changed and JSON configuration can easily
be merged. It acts as a builder for immutable configuration.

[ImmutableConfigurationSection](ImmutableConfigurationSection.cs) captures once for all the content and path of any
other `IConfigurationSection`.

## The path/key ambiguity.
We use `path` instead of `key` parameter name to remind you that a relative path
is always available in the conffiguration API to address sub sections.

## The non existing section issue.
A configuration section may not `Exists()`: it has no value nor children.
This is weird but this is how it has been designed. We respect this behavior:
`ImmutableConfigurationSection` captures such "non existing" sections.

Note that the .Net JSON binder (just like our `MutableConfigurationSection.AddJson`
implementation) totally ignores empty JSON object:
```jsonc
{
  // We want the Options with its default values...
  "Option": {}
}
```
Since reading from JSON totally skips the path, this doesn't work.
We must have a way to say "I want to activate this Options with its default configuration".

We recommend to apply the following pattern:
- Allow the section to support a "true" or "false" boolean value (when it has no children).
- Opt-out: when a section must exist "by default" (with a default configuration):
  - When the section has children, consider them as the configuration.
  - a "false" value skips the section.
  - a "true" or a non existing section applies the defaults.
- Opt-in: when a section must not exist "by default" (but still has a default configuration):
  - When the section has children, consider them as the configuration.
  - a "false" value or a a non existing section skips the section.
  - a "true" value applies the defaults.

The `ShouldApplyConfiguration` extension methods (in [ConfigurationSectionExtension](ConfigurationSectionExtension.cs))
implements this once for all.


