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
@1,1 - simple_error_list
 @1,1 - Character 'A' (TryMatch)" )]
        [TestCase( @"A B", @"
@1,1 - simple_error_list
 @1,3 - First (simple_error_list)
  @1,4 - Second (simple_error_list)
   @1,4 - Character 'C' (TryMatch)" )]
        [TestCase( @"A
    B
        C
            D", @"
@1,1 - simple_error_list
 @2,5 - First (simple_error_list)
  @3,9 - Second (simple_error_list)
   @4,13 - Third (simple_error_list)
    @4,14 - Character 'X' (TryMatch)" )]
        [TestCase( @"
A
B
C
D  NOTX", @"
@2,1 - simple_error_list
 @3,1 - First (simple_error_list)
  @4,1 - Second (simple_error_list)
   @5,1 - Third (simple_error_list)
    @5,4 - Character 'X' (TryMatch)" )]
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
    }
}
