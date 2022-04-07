# Simple serialization

CK.Core defines 2 interfaces that supports "simple" binary serialization based on
the [ICKBinaryReader](ICKBinaryReader.cs) and [ICKBinaryWriter](ICKBinaryWriter.cs).

For more complex support (support of object references, type mutations and other features), the
CK.BinarySerialization package with its `BinarySerializer and `BinarySerializer` must be used.

## The ICKBinaryReader/Writer

These interfaces extends the standard .Net [BinaryReader](https://docs.microsoft.com/en-us/dotnet/api/system.io.binaryreader)
and [BinaryWriter](https://docs.microsoft.com/en-us/dotnet/api/system.io.binarywriter) API with nullable support,
more read/write of standard types and optional pools to share values.


## Simple serialization: ICKSimpleBinarySerializable (any type)
This is the simplest pattern where versioning must be handled explicitly ant that applies
to any POCO-like type since there is no support for object graphs: the only allowed references
are hierarchical ones and data structures like lists, arrays, sets of any kind, including 
dictionaries must be manually handled.

It may seem cumbersome... It is. However it's rather easy to implement and, when the versioning pattern is 
properly applied, a type can freely mute from version to version.

```c#
/// <summary>
/// Basic interface for simple binary serialization support.
/// A deserialization constructor must be implemented (that accepts a <see cref="ICKBinaryReader"/>).
/// <para>
/// Simple serialization means that there is no support for object graph (no reference management),
/// no support for polymorphism (the exact type must be known) and that versions must be manually managed.
/// </para>
/// </summary>
public interface ICKSimpleBinarySerializable
{
    /// <summary>
    /// Serializes this object into the writer.
    /// There should be a version written first (typically a byte): the deserialization
    /// constructor must read this version first.
    /// </summary>
    /// <param name="w">The writer to use.</param>
    void Write( ICKBinaryWriter w );
}
```

Such "simple serializable" objects are automatically handled by the CK.BinarySerialization package.

The version has to be manually handled (typically by a first byte - at least until the version 254: versions 255
up to 64534 should use 2 bytes, etc.).

```c#
readonly struct Sample : ICKSimpleBinarySerializable
{
    public readonly int Power;
    public readonly string Name;
    public readonly short? Age;

    public Sample( int power, string name, short? age )
    {
        Throw.CheckNotNullOrEmptyArgument( name );
        Power = power;
        Name = name;
        Age = age;
    }

    public Sample( ICKBinaryReader r )
    {
        r.ReadByte(); // Version
        Power = r.ReadInt32();
        Name = r.ReadString();
        Age = r.ReadNullableInt16();
    }

    public void Write( ICKBinaryWriter w )
    {
        w.Write( (byte)0 ); // Version
        w.Write( Power );
        w.Write( Name );
        w.WriteNullableInt16( Age );
    }
}
```
> Try to always use the same names: **r** for the reader, **w** for the writer.

If thousands of instances of a `ICKSimpleBinarySerializable` must be saved, you obviously have useless bytes of
information in the serialized data: here comes the `ICKVersionedBinarySerializable` that shares, once for all,
its version number.

## About the IUtf8JsonWritable

The [IUtf8JsonWritable](IUtf8JsonWritable.cs) mimics the `ICKSimpleBinarySerializable` interface but with a
`Utf8JsonWriter` instead of a binary writer. It's named "Writable" because it is not intended to support the associated
constructor that takes a `ref Utf8JsonReader`: reading Json back (and being able to support versioning) requires a
"protocol" that defines where the version should appear (an array may use its first cell to contain the version, or
you may choose to wrap any object in an object with its version and its data). This choice impacts the shape of the data
and that may not be desirable.

That said, nothing prevents a specific type to implement the constructor that takes a `ref Utf8JsonReader`, and
either:
- uses the same pattern as the simple serializable with a version that will be visible in the output;
- or uses a more complex algorithm to read back the data (more like a "model binding" approach).

## Sharing version: ICKVersionedBinarySerializable (struct & sealed classes only)

This `ICKVersionedBinarySerializable` works like the simple one, except that the
version is kindly handled once for all at the type level (the version is written only once per type even if thousands of
objects are serialized) and the current version is specified by the `[SerializationVersion( 42 )]` [attribute](SerializationVersionAttribute.cs):
```c#
/// <summary>
/// Interface for versioned binary serialization that uses an externally 
/// stored or known version number. This should be used only on sealed classes or value types 
/// (since inheritance or any other traits or composite objects will have to share the same version).
/// <para>
/// The version must be defined by a <see cref="SerializationVersionAttribute"/> on the type and
/// should be written once for all instances of the type.
/// </para>
/// <para>
/// A deserialization constructor must be implemented (that accepts a <see cref="ICKBinaryReader"/> and a int version).
/// </para>
/// <para>
/// This is for "simple object" serialization where "simple" means that there is no support for object graph (no reference
/// management).
/// </para>
/// </summary>
public interface ICKVersionedBinarySerializable
{
    /// <summary>
    /// Must write the binary layout only, without the version number that must be handled externally.
    /// This binary layout will be read by a deserialization constructor that takes a <see cref="ICKBinaryReader"/> 
    /// and a int version.
    /// </summary>
    /// <remarks>
    /// This method is named WriteData so that an object can implement both <see cref="ICKSimpleBinarySerializable"/>
    /// (with its <see cref="ICKSimpleBinarySerializable.Write(ICKBinaryWriter)"/> that must write its version) and
    /// this <see cref="ICKVersionedBinarySerializable"/> where the version number must be externally managed.
    /// </remarks>
    /// <param name="w">The writer to use.</param>
    void WriteData( ICKBinaryWriter w );
}
``` 

Just like "simple serializable", "versioned serializable" objects are automatically handled by the CK.BinarySerialization package.

```c#
[SerializationVersion( 42 )]
struct ThingStruct : ICKVersionedBinarySerializable
{
    // Name was nullable before v42.
    // Now it is necessarily not null, empty or white space.
    public readonly string Name;

    public ThingStruct( string name )
    {
        Throw.CheckNotNullOrWhiteSpaceArgument( name );
        Name = name;
    }

    public ThingStruct( ICKBinaryReader r, int version )
    {
        if( version < 42 )
        {
            Name = r.ReadNullableString() ?? "(no name)";
        }
        else
        {
            Name = r.ReadString();
        }
    }

    public void WriteData( ICKBinaryWriter w )
    {
        w.Write( Name );
    }
}
```

This can only be applied to value types or sealed classes. Unless a Code Analyzer is written one day to check this,
absolutely no check is done in CK.Core. But this check is implemented in CK.BinarySerialization package: an `InvalidOperationException` will
be raised  if non sealed class appears in a serialization or deserialization session.

To overcome this limitation, a more complex model is required: this is what the CK.BinarySerialization.Sliced package
brings to the table.

## Implementing both Simple & Versioned (struct & sealed classes only)

Implementing both interfaces is possible. The following pattern should be used:
```c#
/// <summary>
/// Supporting both interfaces enables simple scenario to use the embedded version
/// (to be used when not too many instances must be serialized) or use the shared version
/// (when many instances must be serialized).
/// </summary>
[SerializationVersion( 3712 )]
sealed class CanSupportBothSimpleSerialization : ICKSimpleBinarySerializable, ICKVersionedBinarySerializable
{
    public string? Data { get; set; }

    /// <summary>
    /// Simple deserialization constructor.
    /// </summary>
    /// <param name="r">The reader.</param>
    public CanSupportBothSimpleSerialization( ICKBinaryReader r )
        : this( r, r.ReadSmallInt32() )
    {
    }

    /// <summary>
    /// Versioned deserialization constructor.
    /// </summary>
    /// <param name="r">The reader.</param>
    /// <param name="version">The saved version number.</param>
    public CanSupportBothSimpleSerialization( ICKBinaryReader r, int version )
    {
        // Use the version as usual.
        Data = r.ReadNullableString();
    }

    public void Write( ICKBinaryWriter w )
    {
        // Using a Debug.Assert here avoids the cost of the reflexion.
        Debug.Assert( SerializationVersionAttribute.GetRequiredVersion( GetType() ) == 3712 );
        w.WriteSmallInt32( 3712 );
        WriteData( w );
    }

    public void WriteData( ICKBinaryWriter w )
    {
        // The version is externally managed.
        w.WriteNullableString( Data );
    }
}
```

CK.BinarySerialization always uses the Versioned interface (since it automatically handles the type information
and the version is a part of this information).

## SimpleSerializable helper class
This static class exposes 4 basic helpers that can de/serialize simple and versioned objects from/to a byte array.

Two deep clone static methods are also available:

```c#
/// <summary>
/// Deep clones a <see cref="ICKSimpleBinarySerializable"/> by serializing/deserializing it.
/// </summary>
/// <typeparam name="T">The object's type.</typeparam>
/// <param name="o">The object to clone.</param>
/// <returns>The clone or null if the object to clone is null.</returns>
[return: NotNullIfNotNull( "o" )]
public static T? DeepCloneSimple<T>( T? o ) where T : ICKSimpleBinarySerializable
{
    if( o is null ) return default;
    using( var s = new MemoryStream() )
    using( var w = new CKBinaryWriter( s, Encoding.UTF8, true ) )
    {
        o.Write( w );
        w.Flush();
        s.Position = 0;
        using( var r = new CKBinaryReader( s, Encoding.UTF8, true ) )
        {
            return (T)Activator.CreateInstance( typeof( T ), r )!;
        }
    }
}

/// <summary>
/// Deep clones a <see cref="ICKVersionedBinarySerializable"/> by serializing/deserializing it.
/// </summary>
/// <typeparam name="T">The object's type.</typeparam>
/// <param name="o">The object to clone.</param>
/// <returns>The clone or null if the object to clone is null.</returns>
[return: NotNullIfNotNull( "o" )]
public static T? DeepCloneVersioned<T>( T? o ) where T : ICKVersionedBinarySerializable
{
    if( o is null ) return default;
    using( var s = new MemoryStream() )
    using( var w = new CKBinaryWriter( s, Encoding.UTF8, true ) )
    {
        o.WriteData( w );
        w.Flush();
        s.Position = 0;
        using( var r = new CKBinaryReader( s, Encoding.UTF8, true ) )
        {
            return (T)Activator.CreateInstance( typeof( T ), r, SerializationVersionAttribute.GetRequiredVersion( o.GetType() ) )!;
        }
    }
}


```
