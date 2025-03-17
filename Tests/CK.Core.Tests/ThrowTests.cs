using Shouldly;
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
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentNullException>().ParamName.ShouldBe( "nullRefType" );

        static void f( object nullRefType )
        {
            Throw.CheckNotNullArgument( nullRefType );
        }
    }

    [Test]
    public void CheckNotNullArgument_throws_ArgumentNullException_or_ArgumentException_for_nullable_value_type()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentNullException>().ParamName.ShouldBe( "nullValueType" );

        static void f( int? nullValueType )
        {
            Throw.CheckNotNullArgument( nullValueType );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_strings()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentNullException>().ParamName.ShouldBe(  "anInvalidString" );

        var ex = Util.Invokable( () => f( "" ) ).ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe( "Must not be null or empty. (Parameter 'anInvalidString')" );
        ex.ParamName.ShouldBe( "anInvalidString" );

        static void f( string anInvalidString )
        {
            Throw.CheckNotNullOrEmptyArgument( anInvalidString );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_enumerable()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentNullException>().ParamName.ShouldBe(  "anEmptyEnumerable" );
        var ex = Util.Invokable( () => f( "" ) ).ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe( "Must not be null or empty. (Parameter 'anEmptyEnumerable')" );
        ex.ParamName.ShouldBe( "anEmptyEnumerable" );

        static void f( IEnumerable<char> anEmptyEnumerable )
        {
            Throw.CheckNotNullOrEmptyArgument( anEmptyEnumerable );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_readonly_collections()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentNullException>().ParamName.ShouldBe(  "anEmptyCollection" );
        var ex = Util.Invokable( () => f( Array.Empty<int>() ) ).ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe( "Must not be null or empty. (Parameter 'anEmptyCollection')" );
        ex.ParamName.ShouldBe( "anEmptyCollection" );

        static void f( IReadOnlyCollection<int> anEmptyCollection )
        {
            Throw.CheckNotNullOrEmptyArgument( anEmptyCollection );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_legacy_IEnumerable()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentNullException>().ParamName.ShouldBe(  "anEmptyLegacyEnumerable" );
        var ex = Util.Invokable( () => f( Array.Empty<int>() ) ).ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe( "Must not be null or empty. (Parameter 'anEmptyLegacyEnumerable')" );
        ex.ParamName.ShouldBe( "anEmptyLegacyEnumerable" );

        static void f( System.Collections.IEnumerable anEmptyLegacyEnumerable )
        {
            Throw.CheckNotNullOrEmptyArgument( anEmptyLegacyEnumerable );
        }
    }

    [Test]
    public void CheckNotNullOrWhiteSpaceArgument_throws_ArgumentNullException_or_ArgumentException_for_string()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentNullException>().ParamName.ShouldBe( "anInvalidString" );

        Check( Util.Invokable( () => f( "" ) ).ShouldThrow<ArgumentException>() );
        Check( Util.Invokable( () => f( "    " ) ).ShouldThrow<ArgumentException>() );

        static void f( string anInvalidString )
        {
            Throw.CheckNotNullOrWhiteSpaceArgument( anInvalidString );
        }

        static void Check( ArgumentException ex )
        {
            ex.Message.ShouldBe( "Must not be null, empty or whitespace. (Parameter 'anInvalidString')" );
            ex.ParamName.ShouldBe( "anInvalidString" );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_Span()
    {
        Check( Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentException>() );
        Check( Util.Invokable( () => f( "".ToArray() ) ).ShouldThrow<ArgumentException>() );

        static void f( Span<char> emptySpan )
        {
            Throw.CheckNotNullOrEmptyArgument( emptySpan );
        }

        static void Check( ArgumentException ex )
        {
            ex.Message.ShouldBe( "Must not be empty. (Parameter 'emptySpan')" );
            ex.ParamName.ShouldBe( "emptySpan" );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_ReadOnlySpan()
    {
        Check( Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentException>() );
        Check( Util.Invokable( () => f( "".ToArray() ) ).ShouldThrow<ArgumentException>() );

        static void f( ReadOnlySpan<char> emptyROSpan )
        {
            Throw.CheckNotNullOrEmptyArgument( emptyROSpan );
        }

        static void Check( ArgumentException ex )
        {
            ex.Message.ShouldBe( "Must not be empty. (Parameter 'emptyROSpan')" );
            ex.ParamName.ShouldBe( "emptyROSpan" );
        }
    }

    [Test]
    public void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_Memory()
    {
        Check( Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentException>() );
        Check( Util.Invokable( () => f( Array.Empty<char>() ) ).ShouldThrow<ArgumentException>() );

        static void f( Memory<char> emptyMemory )
        {
            Throw.CheckNotNullOrEmptyArgument( emptyMemory );
        }

        static void Check( ArgumentException ex )
        {
            ex.Message.ShouldBe( "Must not be empty. (Parameter 'emptyMemory')" );
            ex.ParamName.ShouldBe( "emptyMemory" );
        }
    }

    public static void CheckNotNullOrEmptyArgument_throws_ArgumentException_for_ReadOnlyMemory()
    {
        Check( Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentException>() );
        Check( Util.Invokable( () => f( "".ToArray() ) ).ShouldThrow<ArgumentException>() );

        static void f( ReadOnlyMemory<char> emptyROMemory )
        {
            Throw.CheckNotNullOrEmptyArgument( emptyROMemory );
        }

        static void Check( ArgumentException ex )
        {
            ex.Message.ShouldBe( "Must not be empty. (Parameter 'emptyROMemory')" );
            ex.ParamName.ShouldBe( "emptyROMemory" );
        }
    }

    [Test]
    public void CheckArgument_throws_ArgumentException_with_the_faulty_expression()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentException>()
                                          .Message.ShouldBe( "Invalid argument: 'o is string[] array && array.Length > 3 && array[0] == \"First\"' should be true." );

        static void f( object o )
        {
            Throw.CheckArgument( o is string[] array && array.Length > 3 && array[0] == "First" );
        }
    }

    [Test]
    public void CheckArgument_with_message_and_faulty_expression()
    {
        Util.Invokable( () => f( null! ) ).ShouldThrow<ArgumentException>()
                                          .Message.ShouldBe( "The object must be a non null string array. (Parameter 'o != null && o is string[] array')" );

        static void f( object o )
        {
            Throw.CheckArgument( "The object must be a non null string array.", o != null && o is string[] array );
        }
    }

    [Test]
    public void CheckOutOfRangeArgument_throws_ArgumentOutOfRangeException_with_the_faulty_expression()
    {
        Util.Invokable( () => f( -5 ) ).ShouldThrow<ArgumentOutOfRangeException>()
                                                  .Message.ShouldBe( "Invalid argument: 'index is >= 0 and <= 15' should be true." );

        static void f( int index )
        {
            Throw.CheckOutOfRangeArgument( index is >= 0 and <= 15 );
        }
    }

    [Test]
    public void CheckOutOfRangeArgument_with_message_and_faulty_expression()
    {
        Util.Invokable( () => f( -5 ) ).ShouldThrow<ArgumentOutOfRangeException>()
                                       .Message.ShouldBe( "Must be between 0 and 15. (Parameter 'index is >= 0 and <= 15')" );

        static void f( int index )
        {
            Throw.CheckOutOfRangeArgument( "Must be between 0 and 15.", index is >= 0 and <= 15 );
        }
    }

    [Test]
    public void CheckState_throws_InvalidOperationException_with_the_faulty_expression()
    {
        bool _canRun = false;

        Util.Invokable( () => Run() ).ShouldThrow<InvalidOperationException>()
                                     .Message.ShouldBe( "Invalid state: '_canRun' should be true." );

        void Run()
        {
            Throw.CheckState( _canRun );
        }
    }

    [Test]
    public void CheckState_with_message_and_faulty_expression()
    {
        bool _canRun = false;

        Util.Invokable( () => Run() ).ShouldThrow<InvalidOperationException>()
                                     .Message.ShouldBe( "This should be able to run. (Expression: '_canRun')" );

        void Run()
        {
            Throw.CheckState( "This should be able to run.", _canRun );
        }
    }

    static readonly string _someData = "";

    [Test]
    public void CheckData_throws_InvalidDataException_with_the_faulty_expression()
    {
        Util.Invokable( () => ProcessData() ).ShouldThrow<InvalidDataException>()
                                             .Message.ShouldBe( "Invalid data: '_someData.Length > 0' should be true." );

        static void ProcessData()
        {
            Throw.CheckData( _someData.Length > 0 );
        }
    }


    [Test]
    public void CheckData_with_message_and_faulty_expression()
    {
        Util.Invokable( () => ProcessData() ).ShouldThrow<InvalidDataException>()
                                             .Message.ShouldBe( "The data must not be empty. (Expression: '_someData.Length > 0')" );

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
        Util.Invokable( () => Bug() ).ShouldThrow<CKException>()
                                     .Message.ShouldMatch( @".*'1 == 0'.*?ThrowTests\.cs@.*" );
#else
        Util.Invokable( () => Bug() ).ShouldNotThrow();
#endif

        static void Bug() => Throw.DebugAssert( 1 == 0 );
    }

    [Test]
    public void DebugAssert_with_message_test()
    {
        Throw.DebugAssert( "Always true.", 1 == 1 );
#if DEBUG
        Util.Invokable( Bug ).ShouldThrow<CKException>().Message.ShouldMatch( @".* This can't be true\. - '1 == 0'.*ThrowTests\.cs@.*" );
#else
        Util.Invokable( Bug ).ShouldNotThrow();
#endif

        static void Bug() => Throw.DebugAssert( "This can't be true.", 1 == 0 );
    }

}
