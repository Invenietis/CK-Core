using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK.Core.Tests;

[TestFixture]
public partial class ThrowTests
{

    [Test]
    public void NotNullOrEmpty_overloads()
    {
        TestEmptyArg( "string" ).Should().Be( "aString" );
        TestEmptyArg( "enumerable".Select( c => c ) ).Should().Be( "anEnumerable" );
        TestEmptyArg( (System.Collections.IEnumerable)("enumerable".Select( c => c )) ).Should().Be( "aLegacyEnumerable" );
        TestEmptyArg( new char[] { 'a' } ).Should().Be( "aROCollection" );
        TestEmptyArg( new int[] { 1 } ).Should().Be( "aROCollection" );

        Span<char> span = new char[] { 'a' }.AsSpan();
        TestEmptyArg( span ).Should().Be( "aSpan" );

        var roSpan = "Hello".AsSpan();
        TestEmptyArg( roSpan ).Should().Be( "aROSpan" );

        Memory<char> memory = new char[] { 'a' }.AsMemory();
        TestEmptyArg( memory ).Should().Be( "aMemory" );

        var roMemory = "Hello".AsMemory();
        TestEmptyArg( roMemory ).Should().Be( "aROMemory" );
    }

    static string TestEmptyArg( string aString )
    {
        Throw.CheckNotNullOrEmptyArgument( aString );
        return nameof( aString );
    }

    static string TestEmptyArg<T>( IEnumerable<T> anEnumerable )
    {
        Throw.CheckNotNullOrEmptyArgument( anEnumerable );
        return nameof( anEnumerable );
    }

    static string TestEmptyArg( System.Collections.IEnumerable aLegacyEnumerable )
    {
        Throw.CheckNotNullOrEmptyArgument( aLegacyEnumerable );
        return nameof( aLegacyEnumerable );
    }

    static string TestEmptyArg<T>( IReadOnlyCollection<T> aROCollection )
    {
        Throw.CheckNotNullOrEmptyArgument( aROCollection );
        return nameof( aROCollection );
    }

    static string TestEmptyArg<T>( Span<T> aSpan )
    {
        Throw.CheckNotNullOrEmptyArgument( aSpan );
        return nameof( aSpan );
    }

    static string TestEmptyArg<T>( Memory<T> aMemory )
    {
        Throw.CheckNotNullOrEmptyArgument( aMemory );
        return nameof( aMemory );
    }

    static string TestEmptyArg<T>( ReadOnlySpan<T> aROSpan )
    {
        Throw.CheckNotNullOrEmptyArgument( aROSpan );
        return nameof( aROSpan );
    }

    static string TestEmptyArg<T>( ReadOnlyMemory<T> aROMemory )
    {
        Throw.CheckNotNullOrEmptyArgument( aROMemory );
        return nameof( aROMemory );
    }

}
