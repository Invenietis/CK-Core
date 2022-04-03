using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            m.GetErrorMessage().Should().Be( message.ReplaceLineEndings().Trim() );
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

        [TestCase( "Primero /*a comment*/ , ", @"
@1,25 - Expected: String 'Last' (TryMatch)
              Or: String 'Dernier' (TryMatch)
              Or: String 'Última' (TryMatch)
              Or: String 'Letzter' (TryMatch)
" )]

        [TestCase( "Erste,Última", null )]
        public void with_Or_Expected( string text, string? message )
        {
            var m = new ROSpanCharMatcher( text );
            m.SkipWhiteSpaces();
            if( !TryMatchFirstAndLast( ref m ) )
            {
                Debug.Assert( message != null );
                m.Head.Length.Should().Be( m.AllText.Length );
                m.HasError.Should().BeTrue();
                m.GetErrorMessage().Should().Be( message.ReplaceLineEndings().Trim() );
            }
            else
            {
                Debug.Assert( message == null );
                m.HasError.Should().BeFalse();
                m.Head.IsEmpty.Should().BeTrue();
                m.GetErrorMessage().Should().BeEmpty();
            }
        }

        static bool TryMatchFirstAndLast( ref ROSpanCharMatcher m )
        {
            var savedHead = m.Head;
            if( (m.TryMatch( "First" ) || m.TryMatch( "Premier" ) || m.TryMatch( "Primero" ) || m.TryMatch( "Erste" ))
                && m.SkipWhiteSpacesAndJSComments() && m.TryMatch( ',' ) && m.SkipWhiteSpacesAndJSComments()
                && (m.TryMatch( "Last" ) || m.TryMatch( "Dernier" ) || m.TryMatch( "Última" ) || m.TryMatch( "Letzter" )) )
            {
                return m.SetSuccess();
            }
            m.Head = savedHead;
            return false;
        }

        [TestCase( "", @"
@1,1 - Expected: First,Last (in English, French, Spanish or German) (TryMatchFirstAndLastWithExpectation)
                   String 'First' (TryMatch)
             Or:   String 'Premier' (TryMatch)
             Or:   String 'Primero' (TryMatch)
             Or:   String 'Erste' (TryMatch)

" )]

        [TestCase( "Erste", @"
@1,1 - Expected: First,Last (in English, French, Spanish or German) (TryMatchFirstAndLastWithExpectation)
  @1,6 - Expected: Character ',' (TryMatch)" )]

        [TestCase( "Erste,", @"
@1,1 - Expected: First,Last (in English, French, Spanish or German) (TryMatchFirstAndLastWithExpectation)
  @1,7 - Expected: String 'Last' (TryMatch)
               Or: String 'Dernier' (TryMatch)
               Or: String 'Última' (TryMatch)
               Or: String 'Letzter' (TryMatch)" )]

        [TestCase( "Primero /*a comment*/ , ", @"
@1,1 - Expected: First,Last (in English, French, Spanish or German) (TryMatchFirstAndLastWithExpectation)
  @1,25 - Expected: String 'Last' (TryMatch)
                Or: String 'Dernier' (TryMatch)
                Or: String 'Última' (TryMatch)
                Or: String 'Letzter' (TryMatch)
" )]
        [TestCase( "Erste,Última", null )]
        public void with_Or_Expected_and_OpenExpectations( string text, string? message )
        {
            var m = new ROSpanCharMatcher( text );
            m.SkipWhiteSpaces();
            if( !TryMatchFirstAndLastWithExpectation( ref m ) )
            {
                Debug.Assert( message != null );
                m.Head.Length.Should().Be( m.AllText.Length );
                m.HasError.Should().BeTrue();
                m.GetErrorMessage().Should().Be( message.ReplaceLineEndings().Trim() );
            }
            else
            {
                Debug.Assert( message == null );
                m.HasError.Should().BeFalse();
                m.Head.IsEmpty.Should().BeTrue();
                m.GetErrorMessage().Should().BeEmpty();
            }
        }

        static bool TryMatchFirstAndLastWithExpectation( ref ROSpanCharMatcher m )
        {
            var savedHead = m.Head;
            using( m.OpenExpectations( "First,Last (in English, French, Spanish or German)" ) )
            {
                if( (m.TryMatch( "First" ) || m.TryMatch( "Premier" ) || m.TryMatch( "Primero" ) || m.TryMatch( "Erste" ))
                && m.SkipWhiteSpacesAndJSComments() && m.TryMatch( ',' ) && m.SkipWhiteSpacesAndJSComments()
                && (m.TryMatch( "Last" ) || m.TryMatch( "Dernier" ) || m.TryMatch( "Última" ) || m.TryMatch( "Letzter" )) )
                {
                    return m.SetSuccess();
                }
            }
            m.Head = savedHead;
            return false;
        }

        [TestCase( "{", @"
@1,1 - Expected: Any JSON token or object (TryMatchAnyJSON)
  @1,2 - Expected: JSON object properties (TryMatchJSONObjectContent)" )]
        [TestCase( "[", @"
@1,1 - Expected: Any JSON token or object (TryMatchAnyJSON)
  @1,2 - Expected: JSON array values (TryMatchJSONArrayContent)" )]
        [TestCase( "[null,,]", @"
@1,1 - Expected: Any JSON token or object (TryMatchAnyJSON)
  @1,2 - Expected: JSON array values (TryMatchJSONArrayContent)
    @1,7 - Expected: Any JSON token or object (TryMatchAnyJSON)
                       String 'true' (TryMatch)
                 Or:   String 'false' (TryMatch)
                 Or:   JSON string or null (TryMatchJSONQuotedString)
                 Or:   Floating number (TryMatchDouble)" )]
        public void TryMatchAnyJSON_errors( string s, string message )
        {
            var m = new ROSpanCharMatcher( s );
            m.TryMatchAnyJSON( out _ ).Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.GetErrorMessage().Should().Be( message.ReplaceLineEndings().Trim() );
        }

    }
}
