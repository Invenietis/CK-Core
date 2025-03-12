using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CK.Core.Tests;

[TestFixture]
public partial class ThrowTests
{
    [Test]
    public void CheckNotNullArgument_throws_ArgumentNullException_or_ArgumentException_for_reference_type()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "nullRefType" );

        static void f( object nullRefType )
        {
            Throw.CheckNotNullArgument( nullRefType );
        }
    }

    [Test]
    public void CheckNotNullArgument_throws_ArgumentNullException_or_ArgumentException_for_nullable_value_type()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "nullValueType" );

        static void f( int? nullValueType )
        {
            Throw.CheckNotNullArgument( nullValueType );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_strings()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anInvalidString" );
        FluentActions.Invoking( () => f( "" ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null or empty. (Parameter 'anInvalidString')" )
                                                                                   .Where( ex => ex.ParamName == "anInvalidString" );
        static void f( string anInvalidString )
        {
            Throw.CheckNotNullOrEmptyArgument( anInvalidString );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_enumerable()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anEmptyEnumerable" );
        FluentActions.Invoking( () => f( "" ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null or empty. (Parameter 'anEmptyEnumerable')" )
                                                                                   .Where( ex => ex.ParamName == "anEmptyEnumerable" );
        static void f( IEnumerable<char> anEmptyEnumerable )
        {
            Throw.CheckNotNullOrEmptyArgument( anEmptyEnumerable );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_readonly_collections()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anEmptyCollection" );
        FluentActions.Invoking( () => f( Array.Empty<int>() ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null or empty. (Parameter 'anEmptyCollection')" )
                                                                                   .Where( ex => ex.ParamName == "anEmptyCollection" );
        static void f( IReadOnlyCollection<int> anEmptyCollection )
        {
            Throw.CheckNotNullOrEmptyArgument( anEmptyCollection );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_legacy_IEnumerable()
    {

        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anEmptyLegacyEnumerable" );
        FluentActions.Invoking( () => f( Array.Empty<int>() ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null or empty. (Parameter 'anEmptyLegacyEnumerable')" )
                                                                                   .Where( ex => ex.ParamName == "anEmptyLegacyEnumerable" );
        static void f( System.Collections.IEnumerable anEmptyLegacyEnumerable )
        {
            Throw.CheckNotNullOrEmptyArgument( anEmptyLegacyEnumerable );
        }
    }

    [Test]
    public void CheckNotNullOrWhiteSpaceArgument_throws_ArgumentNullException_or_ArgumentException_for_string()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anInvalidString" );
        FluentActions.Invoking( () => f( "" ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null, empty or whitespace. (Parameter 'anInvalidString')" )
                                                                                   .Where( ex => ex.ParamName == "anInvalidString" );
        FluentActions.Invoking( () => f( "   " ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null, empty or whitespace. (Parameter 'anInvalidString')" )
                                                                                      .Where( ex => ex.ParamName == "anInvalidString" );

        static void f( string anInvalidString )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( anInvalidString );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_Span()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptySpan')" )
                                                                                   .Where( ex => ex.ParamName == "emptySpan" );
        FluentActions.Invoking( () => f( "".ToArray() ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptySpan')" )
                                                                                   .Where( ex => ex.ParamName == "emptySpan" );

        static void f( Span<char> emptySpan )
        {
            Throw.CheckNotNullOrEmptyArgument( emptySpan );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_ReadOnlySpan()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptyROSpan')" )
                                                                                   .Where( ex => ex.ParamName == "emptyROSpan" );
        FluentActions.Invoking( () => f( "".ToArray() ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptyROSpan')" )
                                                                                   .Where( ex => ex.ParamName == "emptyROSpan" );

        static void f( ReadOnlySpan<char> emptyROSpan )
        {
            Throw.CheckNotNullOrEmptyArgument( emptyROSpan );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_Memory()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptyMemory')" )
                                                                                   .Where( ex => ex.ParamName == "emptyMemory" );
        FluentActions.Invoking( () => f( Array.Empty<char>() ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptyMemory')" )
                                                                                   .Where( ex => ex.ParamName == "emptyMemory" );

        static void f( Memory<char> emptyMemory )
        {
            Throw.CheckNotNullOrEmptyArgument( emptyMemory );
        }
    }

    public static void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_ReadOnlyMemory()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptyROMemory')" )
                                                                                   .Where( ex => ex.ParamName == "emptyROMemory" );
        FluentActions.Invoking( () => f( "".ToArray() ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be empty. (Parameter 'emptyROMemory')" )
                                                                                   .Where( ex => ex.ParamName == "emptyROMemory" );

        static void f( ReadOnlyMemory<char> emptyROMemory )
        {
            Throw.CheckNotNullOrEmptyArgument( emptyROMemory );
        }
    }

    [Test]
    public void CheckArgument_throws_ArgumentException_with_the_faulty_expression()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentException>()
                                                  .WithMessage( "Invalid argument: 'o is string[] array && array.Length > 3 && array[0] == \"First\"' should be true." );

        static void f( object o )
        {
            Throw.CheckArgument( o is string[] array && array.Length > 3 && array[0] == "First" );
        }
    }

    [Test]
    public void CheckArgument_with_message_and_faulty_expression()
    {
        FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentException>()
                                                  .WithMessage( "The object must be a non null string array. (Parameter 'o != null && o is string[] array')" );

        static void f( object o )
        {
            Throw.CheckArgument( "The object must be a non null string array.", o != null && o is string[] array );
        }
    }

    [Test]
    public void CheckOutOfRangeArgument_throws_ArgumentOutOfRangeException_with_the_faulty_expression()
    {
        FluentActions.Invoking( () => f( -5 ) ).Should().Throw<ArgumentOutOfRangeException>()
                                                  .WithMessage( "Invalid argument: 'index is >= 0 and <= 15' should be true." );

        static void f( int index )
        {
            Throw.CheckOutOfRangeArgument( index is >= 0 and <= 15 );
        }
    }

    [Test]
    public void CheckOutOfRangeArgument_with_message_and_faulty_expression()
    {
        FluentActions.Invoking( () => f( -5 ) ).Should().Throw<ArgumentOutOfRangeException>()
                                                  .WithMessage( "Must be between 0 and 15. (Parameter 'index is >= 0 and <= 15')" );

        static void f( int index )
        {
            Throw.CheckOutOfRangeArgument( "Must be between 0 and 15.", index is >= 0 and <= 15 );
        }
    }

    [Test]
    public void CheckState_throws_InvalidOperationException_with_the_faulty_expression()
    {
        bool _canRun = false;

        FluentActions.Invoking( () => Run() ).Should().Throw<InvalidOperationException>()
                                                      .WithMessage( "Invalid state: '_canRun' should be true." );

        void Run()
        {
            Throw.CheckState( _canRun );
        }
    }

    [Test]
    public void CheckState_with_message_and_faulty_expression()
    {
        bool _canRun = false;

        FluentActions.Invoking( () => Run() ).Should().Throw<InvalidOperationException>()
                                                      .WithMessage( "This should be able to run. (Expression: '_canRun')" );

        void Run()
        {
            Throw.CheckState( "This should be able to run.", _canRun );
        }
    }

    static readonly string _someData = "";

    [Test]
    public void CheckData_throws_InvalidDataException_with_the_faulty_expression()
    {
        FluentActions.Invoking( () => ProcessData() ).Should().Throw<InvalidDataException>()
                                                      .WithMessage( "Invalid data: '_someData.Length > 0' should be true." );

        static void ProcessData()
        {
            Throw.CheckData( _someData.Length > 0 );
        }
    }


    [Test]
    public void CheckData_with_message_and_faulty_expression()
    {
        FluentActions.Invoking( () => ProcessData() ).Should().Throw<InvalidDataException>()
                                                      .WithMessage( "The data must not be empty. (Expression: '_someData.Length > 0')" );

        static void ProcessData()
        {
            Throw.CheckData( "The data must not be empty.", _someData.Length > 0 );
        }
    }


    [Test]
    public void DebugAssert_test()
    {
        Throw.DebugAssert( 1 == 1 );
#if DEBUG
        FluentActions.Invoking( () => Bug() ).Should().Throw<CKException>().WithMessage( "*'1 == 0'*ThrowTests.cs@*" );
#else
        FluentActions.Invoking( () => Bug() ).Should().NotThrow();
#endif

        static void Bug() => Throw.DebugAssert( 1 == 0 );
    }

    [Test]
    public void DebugAssert_with_message_test()
    {
        Throw.DebugAssert( "Always true.", 1 == 1 );
#if DEBUG
        FluentActions.Invoking( () => Bug() ).Should().Throw<CKException>().WithMessage( "* This can't be true. - '1 == 0'*ThrowTests.cs@*" );
#else
        FluentActions.Invoking( () => Bug() ).Should().NotThrow();
#endif

        static void Bug() => Throw.DebugAssert( "This can't be true.", 1 == 0 );
    }

}
