using System;
using FluentAssertions;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class StringMatcherTests
    {
        [Test]
        public void simple_char_matching()
        {
            string s = "ABCD";
            var m = new StringMatcher( s );
            m.MatchChar( 'a' ).Should().BeFalse();
            m.MatchChar( 'A' ).Should().BeTrue();
            m.StartIndex.Should().Be( 1 );
            m.MatchChar( 'A' ).Should().BeFalse();
            m.MatchChar( 'B' ).Should().BeTrue();
            m.MatchChar( 'C' ).Should().BeTrue();
            m.IsEnd.Should().BeFalse();
            m.MatchChar( 'D' ).Should().BeTrue();
            m.MatchChar( 'D' ).Should().BeFalse();
            m.IsEnd.Should().BeTrue();
        }

        [TestCase( "abcdef", 0xABCDEFUL )]
        [TestCase( "12abcdef", 0x12ABCDEFUL )]
        [TestCase( "12abcdef12abcdef", 0x12abcdef12abcdefUL )]
        [TestCase( "00000000FFFFFFFF", 0xFFFFFFFFUL )]
        [TestCase( "FFFFFFFFFFFFFFFF", 0xFFFFFFFFFFFFFFFFUL )]
        public void matching_hex_number( string s, ulong v )
        {
            var m = new StringMatcher( s );
            m.TryMatchHexNumber( out ulong value ).Should().BeTrue();
            value.Should().Be( v );
            m.IsEnd.Should().BeTrue();
        }

        [TestCase( "0|", 0x0UL, '|' )]
        [TestCase( "AG", 0xAUL, 'G' )]
        [TestCase( "cd", 0xCUL, 'd' )]
        public void matching_hex_number_one_digit( string s, ulong v, char end )
        {
            var m = new StringMatcher( s );
            m.TryMatchHexNumber( out ulong value, 1, 1 ).Should().BeTrue();
            value.Should().Be( v );
            m.IsEnd.Should().BeFalse();
            m.Head.Should().Be( end );
        }

        [TestCase( "not a hex." )]
        [TestCase( "FA12 but we want 5 digits min." )]
        public void matching_hex_number_failures( string s )
        {
            var m = new StringMatcher( s );
            m.TryMatchHexNumber( out ulong value, 5, 5 ).Should().BeFalse();
            m.IsEnd.Should().BeFalse();
            m.StartIndex.Should().Be( 0 );
        }

        [Test]
        public void matching_texts_and_whitespaces()
        {
            string s = " AB  \t\r C";
            var m = new StringMatcher( s );
            Action a;
            m.MatchText( "A" ).Should().BeFalse();
            m.StartIndex.Should().Be( 0 );
            m.MatchWhiteSpaces().Should().BeTrue();
            m.StartIndex.Should().Be( 1 );
            m.MatchText( "A" ).Should().BeTrue();
            m.MatchText( "B" ).Should().BeTrue();
            m.StartIndex.Should().Be( 3 );
            m.MatchWhiteSpaces( 6 ).Should().BeFalse();
            m.MatchWhiteSpaces( 5 ).Should().BeTrue();
            m.StartIndex.Should().Be( 8 );
            m.MatchWhiteSpaces().Should().BeFalse();
            m.StartIndex.Should().Be( 8 );
            m.MatchText( "c" ).Should().BeTrue();
            m.StartIndex.Should().Be( s.Length );
            m.IsEnd.Should().BeTrue();

            a = () => m.MatchText( "c" ); a.Should().NotThrow();
            a = () => m.MatchWhiteSpaces(); a.Should().NotThrow();
            m.MatchText( "A" ).Should().BeFalse();
            m.MatchWhiteSpaces().Should().BeFalse();
        }

        [Test]
        public void matching_integers()
        {
            var m = new StringMatcher( "X3712Y" );
            m.MatchChar( 'X' ).Should().BeTrue();
            int i;
            m.MatchInt32( out i ).Should().BeTrue();
            i.Should().Be( 3712 );
            m.MatchChar( 'Y' ).Should().BeTrue();
        }

        [Test]
        public void matching_integers_with_min_max_values()
        {
            var m = new StringMatcher( "3712 -435 56" );
            int i;
            m.MatchInt32( out i, -500, -400 ).Should().BeFalse();
            m.MatchInt32( out i, 0, 3712 ).Should().BeTrue();
            i.Should().Be( 3712 );
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchInt32( out i, 0 ).Should().BeFalse();
            m.MatchInt32( out i, -500, -400 ).Should().BeTrue();
            i.Should().Be( -435 );
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchInt32( out i, 1000, 2000 ).Should().BeFalse();
            m.MatchInt32( out i, 56, 56 ).Should().BeTrue();
            i.Should().Be( 56 );
            m.IsEnd.Should().BeTrue();
        }

        public void match_methods_must_set_an_error()
        {
            var m = new StringMatcher( "A" );

            CheckMatchError( m, () => m.MatchChar( 'B' ) );
            int i;
            CheckMatchError( m, () => m.MatchInt32( out i ) );
            CheckMatchError( m, () => m.MatchText( "PP" ) );
            CheckMatchError( m, () => m.MatchText( "B" ) );
            CheckMatchError( m, () => m.MatchWhiteSpaces() );
        }

        private static void CheckMatchError( StringMatcher m, Func<bool> fail )
        {
            int idx = m.StartIndex;
            int len = m.Length;
            fail().Should().BeFalse();
            m.IsError.Should().BeTrue();
            m.ErrorMessage.Should().NotBeNullOrEmpty();
            m.StartIndex.Should().Be( idx );
            m.Length.Should().Be( len );
            m.ClearError();
        }

        [Test]
        public void ToString_constains_the_text_and_the_error()
        {
            var m = new StringMatcher( "The Text" );
            m.SetError( "Plouf..." );
            m.ToString().Contains( "The Text" );
            m.ToString().Contains( "Plouf..." );
        }

        [TestCase( @"null, true", null, ", true" )]
        [TestCase( @"""""X", "", "X" )]
        [TestCase( @"""a""X", "a", "X" )]
        [TestCase( @"""\\""X", @"\", "X" )]
        [TestCase( @"""A\\B""X", @"A\B", "X" )]
        [TestCase( @"""A\\B\r""X", "A\\B\r", "X" )]
        [TestCase( @"""A\\B\r\""""X", "A\\B\r\"", "X" )]
        [TestCase( @"""\u8976""X", "\u8976", "X" )]
        [TestCase( @"""\uABCD\u07FC""X", "\uABCD\u07FC", "X" )]
        [TestCase( @"""\uabCd\u07fC""X", "\uABCD\u07FC", "X" )]
        public void matching_JSONQuotedString( string s, string parsed, string textAfter )
        {
            var m = new StringMatcher( s );
            string result;
            m.TryMatchJSONQuotedString( out result, true ).Should().BeTrue();
            result.Should().Be( parsed );
            m.TryMatchText( textAfter ).Should().BeTrue();

            m = new StringMatcher( s );
            m.TryMatchJSONQuotedString( true ).Should().BeTrue();
            m.TryMatchText( textAfter ).Should().BeTrue();
        }

        [Test]
        public void simple_json_test()
        {
            string s = @"
{ 
    ""p1"": ""n"", 
    ""p2""  : 
    { 
        ""p3"": 
        [ 
            ""p4"": 
            { 
                ""p5"" : 0.989, 
                ""p6"": [],
                ""p7"": {}
            }
        ] 
    } 
}  ";
            var m = new StringMatcher( s );
            string pName;
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '{' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString( out pName ).Should().BeTrue();
            pName.Should().Be( "p1" );
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ':' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString( out pName ).Should().BeTrue();
            pName.Should().Be( "n" );
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ',' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString( out pName ).Should().BeTrue();
            pName.Should().Be( "p2" );
            m.MatchWhiteSpaces( 2 ).Should().BeTrue();
            m.MatchChar( ':' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '{' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString( out pName ).Should().BeTrue();
            pName.Should().Be( "p3" );
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ':' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '[' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString( out pName ).Should().BeTrue();
            pName.Should().Be( "p4" );
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ':' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '{' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString().Should().BeTrue();
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ':' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchDoubleValue().Should().BeTrue();
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ',' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString( out pName ).Should().BeTrue();
            pName.Should().Be( "p6" );
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ':' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '[' ).Should().BeTrue();
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ']' ).Should().BeTrue();
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ',' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.TryMatchJSONQuotedString().Should().BeTrue();
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( ':' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '{' ).Should().BeTrue();
            m.MatchWhiteSpaces( 0 ).Should().BeTrue();
            m.MatchChar( '}' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '}' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( ']' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '}' ).Should().BeTrue();
            m.MatchWhiteSpaces().Should().BeTrue();
            m.MatchChar( '}' ).Should().BeTrue();
            m.MatchWhiteSpaces( 2 ).Should().BeTrue();
            m.IsEnd.Should().BeTrue();
        }

        [TestCase( "0", 0 )]
        [TestCase( "9876978", 9876978 )]
        [TestCase( "-9876978", -9876978 )]
        [TestCase( "0.0", 0 )]
        [TestCase( "0.00", 0 )]
        [TestCase( "0.34", 0.34 )]
        [TestCase( "4e5", 4e5 )]
        [TestCase( "4E5", 4E5 )]
        [TestCase( "29380.34e98", 29380.34e98 )]
        [TestCase( "29380.34E98", 29380.34E98 )]
        [TestCase( "-80.34e-98", -80.34e-98 )]
        [TestCase( "-80.34E-98", -80.34E-98 )]
        public void matching_double_values( string s, double d )
        {
            StringMatcher m = new StringMatcher( "P" + s + "S" );
            double parsed;
            m.MatchChar( 'P' ).Should().BeTrue();
            int idx = m.StartIndex;
            m.TryMatchDoubleValue().Should().BeTrue();
            m.UncheckedMove( idx - m.StartIndex );
            m.TryMatchDoubleValue( out parsed ).Should().BeTrue();
            parsed.Should().BeApproximately( d, 1f );
            m.MatchChar( 'S' ).Should().BeTrue();
            m.IsEnd.Should().BeTrue();
        }

        [TestCase( "N" )]
        [TestCase( "D" )]
        [TestCase( "B" )]
        [TestCase( "P" )]
        [TestCase( "X" )]
        public void matching_the_5_forms_of_guid( string form )
        {
            var id = Guid.NewGuid();
            string sId = id.ToString( form );
            {
                string s = sId;
                var m = new StringMatcher( s );
                Guid readId;
                m.TryMatchGuid( out readId ).Should().BeTrue();
                readId.Should().Be( id );
            }
            {
                string s = "S" + sId;
                var m = new StringMatcher( s );
                Guid readId;
                m.TryMatchChar( 'S' ).Should().BeTrue();
                m.TryMatchGuid( out readId ).Should().BeTrue();
                readId.Should().Be( id );
            }
            {
                string s = "S" + sId + "T";
                var m = new StringMatcher( s );
                Guid readId;
                m.MatchChar( 'S' ).Should().BeTrue();
                m.TryMatchGuid( out readId ).Should().BeTrue();
                readId.Should().Be( id );
                m.MatchChar( 'T' ).Should().BeTrue();
            }
            sId = sId.Remove( sId.Length - 1 );
            {
                string s = sId;
                var m = new StringMatcher( s );
                Guid readId;
                m.TryMatchGuid( out readId ).Should().BeFalse();
                m.StartIndex.Should().Be( 0 );
            }
            sId = id.ToString().Insert( 3, "K" ).Remove( 4 );
            {
                string s = sId;
                var m = new StringMatcher( s );
                Guid readId;
                m.TryMatchGuid( out readId ).Should().BeFalse();
                m.StartIndex.Should().Be( 0 );
            }
        }

    }
}
