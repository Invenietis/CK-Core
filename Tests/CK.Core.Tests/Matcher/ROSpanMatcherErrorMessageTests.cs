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
    public class ROSpanMatcherErrorMessageTests
    {

        [TestCase( @"", @"
@1,1 - Expected: simple_error_list
                  Character 'A' (TryMatch)" )]

        [TestCase( @"A B", @"
@1,1 - Expected: simple_error_list
 @1,3 - Expected: First (simple_error_list)
  @1,4 - Expected: Second (simple_error_list)
                    Character 'C' (TryMatch)" )]
        [TestCase( @"A
    B
        C
            D", @"
@1,1 - Expected: simple_error_list
 @2,5 - Expected: First (simple_error_list)
  @3,9 - Expected: Second (simple_error_list)
   @4,13 - Expected: Third (simple_error_list)
    @4,14 - Expected: Character 'X' (TryMatch)" )]
        [TestCase( @"
A
B
C
D  NOTX", @"
@2,1 - Expected: simple_error_list
 @3,1 - Expected: First (simple_error_list)
  @4,1 - Expected: Second (simple_error_list)
   @5,1 - Expected: Third (simple_error_list)
    @5,4 - Expected: Character 'X' (TryMatch)" )]
        public void simple_error_list( string text, string message )
        {
            var m = new ROSpanCharMatcher( text );
            m.SkipWhiteSpaces();
            using( m.OpenExpectations() )
            {
                if( m.TryMatch( 'A' ) )
                {
                    m.SkipWhiteSpaces();
                    using( m.OpenExpectations( "First" ) )
                    {
                        if( m.TryMatch( 'B' ) )
                        {
                            m.SkipWhiteSpaces();
                            using( m.OpenExpectations( "Second" ) )
                            {
                                if( m.TryMatch( 'C' ) )
                                {
                                    m.SkipWhiteSpaces();
                                    using( m.OpenExpectations( "Third" ) )
                                    {
                                        if( m.TryMatch( 'D' ) )
                                        {
                                            m.SkipWhiteSpaces();
                                            m.TryMatch( 'X' );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            m.HasError.Should().BeTrue();
            m.GetErrorMessage().Should().Be( message.NormalizeEOL().Trim() );
        }

        [TestCase( "", @"
@1,1 - Expected: String 'First' (TryMatch)
             Or: String 'Premier' (TryMatch)
             Or: String 'Primero' (TryMatch)
             Or: String 'Erste' (TryMatch)" )]
        [TestCase( "Erste", @"@1,6 - Expected: Character ',' (TryMatch)" )]
        [TestCase( "Erste,", @"
@1,7 - Expected: String 'Last' (TryMatch)
             Or: String 'Dernier' (TryMatch)
             Or: String 'Última' (TryMatch)
             Or: String 'Letzter' (TryMatch)
" )]
        public void with_Or_Expected( string text, string message )
        {
            var m = new ROSpanCharMatcher( text );
            m.SkipWhiteSpaces();
            if( !TryMatchFirstLast( m ) )
            {
                m.HasError.Should().BeTrue();
                m.GetErrorMessage().Should().Be( message.NormalizeEOL().Trim() );
            }
            else
            {
                m.HasError.Should().BeFalse();
            }
        }

        private static bool TryMatchFirstLast( ROSpanCharMatcher m )
        {
            if( m.TryMatch( "First" ) || m.TryMatch( "Premier" ) || m.TryMatch( "Primero" ) || m.TryMatch( "Erste" ) )
            {
                if( m.TryMatch( ',' ) )
                {
                    if( m.TryMatch( "Last" ) || m.TryMatch( "Dernier" ) || m.TryMatch( "Última" ) || m.TryMatch( "Letzter" ) )
                    {
                        return m.ClearExpectations();
                    }
                }
            }
            return false;
        }
    }
}
