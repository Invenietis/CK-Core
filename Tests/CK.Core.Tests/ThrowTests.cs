using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class ThrowTests
    {
        [Test]
        public void OnNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_strings()
        {
            FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anInvalidString" );
            FluentActions.Invoking( () => f( "" ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null or empty. (Parameter 'anInvalidString')" )
                                                                                       .Where( ex => ex.ParamName == "anInvalidString" );
            static void f( string anInvalidString )
            {
                Throw.OnNullOrEmptyArgument( anInvalidString );
            }
        }

        [Test]
        public void OnNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException_for_enumerable()
        {
            FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anEmptyEnumerable" );
            FluentActions.Invoking( () => f( "" ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null or empty. (Parameter 'anEmptyEnumerable')" )
                                                                                       .Where( ex => ex.ParamName == "anEmptyEnumerable" );
            static void f( IEnumerable<char> anEmptyEnumerable )
            {
                Throw.OnNullOrEmptyArgument( anEmptyEnumerable );
            }
        }

        [Test]
        public void OnNullOrWhiteSpaceArgument_throws_ArgumentNullException_or_ArgumentException()
        {
            FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentNullException>().Where( ex => ex.ParamName == "anInvalidString" );
            FluentActions.Invoking( () => f( "" ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null, empty or whitespace. (Parameter 'anInvalidString')" )
                                                                                       .Where( ex => ex.ParamName == "anInvalidString" );
            FluentActions.Invoking( () => f( "   " ) ).Should().Throw<ArgumentException>().WithMessage( "Must not be null, empty or whitespace. (Parameter 'anInvalidString')" )
                                                                                          .Where( ex => ex.ParamName == "anInvalidString" );

            static void f( string anInvalidString )
            {
                Throw.OnNullOrWhiteSpaceArgument( anInvalidString );
            }
        }

        [Test]
        public void CheckArgument_throws_ArgumentException_with_the_faulty_expression()
        {
            FluentActions.Invoking( () => f( null! ) ).Should().Throw<ArgumentException>()
                                                      .WithMessage( "Invalid argument: 'o != null && o is string[] array' should be true." );

            static void f( object o )
            {
                Throw.CheckArgument( o != null && o is string[] array );
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
                                                          .WithMessage( "This should be able to run. (Expression: '_canRun'.)" );

            void Run()
            {
                Throw.CheckState( "This should be able to run.", _canRun );
            }
        }

    }
}
