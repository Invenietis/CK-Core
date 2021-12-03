using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    [TestFixture]
    public class ThrowTests
    {
        [Test]
        public void OnNullOrEmptyArgument_throws_ArgumentNullException_or_ArgumentException()
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

    }
}
