# Throw: *throw* and *Guard*

## Some words about Guards

We all use Guards (any method accessible from another code than its assembly - when it is public and often also when it is protected - must be "guarded").
Example:
```csharp
public ObjectPool( ICKBinaryWriter w, IEqualityComparer<T>? comparer = null )
{
    if( w == null ) throw new ArgumentNullException( "w" );
    // …
}
```
Recently (C#6), the `nameof` operator has been introduced:

```csharp
if( w == null ) throw new ArgumentNullException( nameof( w ) );
```
With this, at least, when you rename the parameter, the name follows... But it's still annoying.
And there is another hidden impact: a small method that could be inlined (https://en.wikipedia.org/wiki/Inline_expansion) will not be because of the throw. 

*Note:* A little start to dig what's going on in C# if you're interested: https://www.graymatterdeveloper.com/2020/03/07/csharp-inlining-rules/ (with links to the .Net runtime in C++).

That's why in the vast majority of "serious" code bases, you'll find small ThrowHelper internal and static: 83 similar
classes in .Net, just for "ThrowHelper" but there are others (so go and see them here: https://source.dot.net/#q=ThrowHelper).

Lately, a static method has appeared on `ArgumentNullException`:

```csharp
ArgumentNullException.ThrowIfNull( w, nameof( w ) );
```
This helps... for null, but we still have to use `nameof`.

## Existing library

Numerous library exist, for instance this library that is used by the people who make Azure and other internal Microsoft
stuff: https://github.com/CommunityToolkit/dotnet/tree/main/CommunityToolkit.Diagnostics.
Here you have centralized static "ThrowHelper" (with a `ThrowXXX` for a lot of exceptions) and "Guard" class
with a lot of Guards like `HasSizeGreaterThan`, `IsLessThanOrEqualTo`, `IsBetween`, `IsInRange`, etc. (you have 10 seconds
to understand the difference between `IsBetween` and `IsInRange`).

It's not super easy to pick the right one and sometimes the constraint cannot be expressed with one call...
Could there be a simpler solution?

## A simpler approach in Net6

There's a newcomer in Net6 (C#10 actually): the `CallerArgumentExpressionAttribute`.
Thanks to it, and after grinding several options and inspired by CommunityToolkit.Diagnostics implementation,
CK.Core now offers a ThrowHelper AND Guards in a single static class `Throw`:

- For simple checks, there is no more `nameof` to put: the parameter name is the expression itself.
```csharp
Throw.CheckNotNullArgument( w );
```

- For all the `HasSizeGreaterThan`, `IsBetween` and other `MustHaveAtLeastItems`... `CheckArgument` is the only one guard:  

```csharp
static void f( object o )
{
    Throw.CheckArgument( o is string[] array && array.Length > 3 && array[0] == "First" );
}
```
__Raises `ArgumentException`__: `Invalid argument: 'o is string[] array && array.Length > 3 && array[0] == "First"' should be true.`

- However, some classics are specifically supported:
  - `Throw.CheckOutOfRangeArgument` is like `CheckArgument` except that it is an `ArgumentOutOfRangeException` that is thrown: 
  `Throw.CheckOutOfRangeArgument( index is >= 0 and <= 15 );`

  - Those guards offer an optional message in addition, if you want to enrich the information:
```csharp
Throw.CheckOutOfRangeArgument( "Come on!", index is >= 0 and <= 15 );
```
__Raises `OutOfRangeArgumentException`__: `Come on! (Parameter 'index is >= 0 and <= 15')`

  - For strings, `Throw.CheckNotNullOrEmptyArgument` and `Throw.CheckNotNullOrWhiteSpaceArgument` raise an `ArgumentNullException`
  (if the value is null) and an `ArgumentException` otherwise.

  - `Throw.CheckNotNullOrEmptyArgument` is also available for "collections": all `IEnumerable`, `IEnumerable<T>` but also `Span<T>`, 
  `Memory<T>` and their ReadOnly versions (technically, Span and Memory are structs - Value type - 
  and cannot be "null", but I found it more convenient to use the same overload than a specific "CheckNotEmpty").

Arguments are not the only things that can go wrong. Your code has a *State* that can be invalid regarding the requested operation:
this is the `InvalidOperationException`:

```csharp
public void Run()
{
  Throw.CheckState( CanRun );
  //...
}
```
__Raises `InvalidOperationException`__: `Invalid state: 'CanRun' should be true.`

The third and last aspect that can go wrong that is not related to an argument or an internal state is about *data* (that can be
understood as a kind of "external state"). `CheckData`, just like `CheckState`, accepts an explicit message:

```csharp
void FillFile()
{
    Throw.CheckData( "This file must be empty.", File.ReadAllText( ThisFile() ).Length == 0 );
}
```
__Raises `InvalidDataException`__: `This file must be empty. (Expression: 'File.ReadAllText( ThisFile() ).Length == 0')`


Of course, this `static class Throw`, as its name indicates, is also a "ThrowHelper" that supports the most common exceptions. 
(This will be extended as needed.)




