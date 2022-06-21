# CK.Core

This single assembly contains basic helpers and useful tools that should ideally not exist:
they (or their equivalent) should be in the .Net core framework.


## Throw and Guard
See [CK.Core/Throw](CK.Core/Throw/).

## Completable & Completion
See [CK.Core/Completable](CK.Core/Completable/).

## "Match and Forward" pattern
See [CK.Core/Matcher](CK.Core/Matcher/).

## CKBinaryReader/Writer, Simple/VersionedSerializable and IUtf8JsonWritable
See [CK.Core/BinaryReaderWriter/](CK.Core/SimpleSerialization/).

## CoreApplicationIdentity
See [CK.Core/CoreApplicationIdentity/](CK.Core/CoreApplicationIdentity/).

## Automatic Dependency Injection "duck typed" basic contracts
See [CK.Core/AutomaticDI/](CK.Core/AutomaticDI/).


## CKTrait

CKTrait handle the combination of different tags (strings) in a deterministic an thread safe manner. 
Traits are normalized and ordered strings combinations (*"Sql|DB access|Subscription" == "DB access|Sql|Subscription"* and *"DB access|Sql"* is greater than *"Sql"*):
a total order exists on the set of traits combinations based on lexicographical order for atomic
trait and the number of traits in a composite.
They support union, intersect, except and symmetric except in O(n).

Traits exist in a `CKTraitContext` that defines their separator (typically ',', '+' or '|') and,
thanks to their name, can be defined independently but resolves to the same context (this allows 
references to the same context to be defined and used transparently from totally independent modules/assemblies)

## DatetimeStamp

Very simple readonly struct that is a DateTime and a byte uniquifier.

## FastUniqueIdGenerator

Simple thread safe unique identifier generator with 64 bits (8 bytes) of entropy
that generates 11 characters long strings encoded in base 64 url.
Used as a very fast replacement of Guid (with less entropy but still enough for a lot
of usages).

## NormalizedPath

Immutable encapsulation of a `Path` string ('\\' are mapped to '/') and its `Parts` as an array of strings.
This implements a path closer to Unix than Windows (forward slashes '/' and case sensitivity) but works perfectly well
on Windows.
A NormalizedPath can be relative (supports '..' or '.' parts) or be rooted: 5 kind of
[roots](CK.Core/NormalizedPathRootKind.cs) are supported:
  - None (relative path)
  - '/' (RootedBySeparator), 
  - 'X:' or ':' or '~' (RootedByFirstPart), 
  - '//' (RootedByDoubleSeparator), 
  - or 'xx://' (RootedByURIScheme).

## SimpleServiceContainer

Very basic and simple `IServiceProvider` implementation.

# Hash

`SHA1Value`, `SHA256Value` and `SHA512Value` encapsulate in a readonly struct
the hexadecimal string and the binary value of SHA values.

`HashStream` is a simple wrapper around an `IncrementalHash` instance to compute
the hash of a stream. It can be used in read or write mode and as a terminal stream
or as a decorator.

## ISystemClock

Yet another system clock. See https://github.com/dotnet/extensions/issues/151.

