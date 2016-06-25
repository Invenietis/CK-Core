using System;
using NUnit.Framework;

namespace CK.Text.Tests
{
    [TestFixture]
    public class StringMatcherTests
    {
        [Test]
        public void simple_char_matching()
        {
            string s = "ABCD";
            var m = new StringMatcher( s );
            Assert.That( m.MatchChar( 'a' ), Is.False );
            Assert.That( m.MatchChar( 'A' ), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( 1 ) );
            Assert.That( m.MatchChar( 'A' ), Is.False );
            Assert.That( m.MatchChar( 'B' ), Is.True );
            Assert.That( m.MatchChar( 'C' ), Is.True );
            Assert.That( m.IsEnd, Is.False );
            Assert.That( m.MatchChar( 'D' ), Is.True );
            Assert.That( m.MatchChar( 'D' ), Is.False );
            Assert.That( m.IsEnd, Is.True );
        }

        [Test]
        public void matching_texts_and_whitespaces()
        {
            string s = " AB  \t\r C";
            var m = new StringMatcher( s );
            Assert.That( m.MatchText( "A" ), Is.False );
            Assert.That( m.StartIndex, Is.EqualTo( 0 ) );
            Assert.That( m.MatchWhiteSpaces(), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( 1 ) );
            Assert.That( m.MatchText( "A" ), Is.True );
            Assert.That( m.MatchText( "B" ), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( 3 ) );
            Assert.That( m.MatchWhiteSpaces( 6 ), Is.False );
            Assert.That( m.MatchWhiteSpaces( 5 ), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( 8 ) );
            Assert.That( m.MatchWhiteSpaces(), Is.False );
            Assert.That( m.StartIndex, Is.EqualTo( 8 ) );
            Assert.That( m.MatchText( "c" ), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( s.Length ) );
            Assert.That( m.IsEnd, Is.True );

            Assert.DoesNotThrow( () => m.MatchText( "c" ) );
            Assert.DoesNotThrow( () => m.MatchWhiteSpaces() );
            Assert.That( m.MatchText( "A" ), Is.False );
            Assert.That( m.MatchWhiteSpaces(), Is.False );
        }

        [Test]
        public void matching_integers()
        {
            var m = new StringMatcher( "X3712Y" );
            Assert.That( m.MatchChar( 'X' ) );
            int i;
            Assert.That( m.MatchInt32( out i ) && i == 3712 );
            Assert.That( m.MatchChar( 'Y' ) );
        }

        [Test]
        public void matching_integers_with_min_max_values()
        {
            var m = new StringMatcher( "3712 -435 56" );
            int i;
            Assert.That( m.MatchInt32( out i, -500, -400 ), Is.False );
            Assert.That( m.MatchInt32( out i, 0, 3712 ) && i == 3712 );
            Assert.That( m.MatchWhiteSpaces() );
            Assert.That( m.MatchInt32( out i, 0 ), Is.False );
            Assert.That( m.MatchInt32( out i, -500, -400 ) && i == -435 );
            Assert.That( m.MatchWhiteSpaces() );
            Assert.That( m.MatchInt32( out i, 1000, 2000 ), Is.False );
            Assert.That( m.MatchInt32( out i, 56, 56 ) && i == 56 );
            Assert.That( m.IsEnd );
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
            Assert.That( fail(), Is.False );
            Assert.That( m.IsError );
            Assert.That( m.ErrorMessage, Is.Not.Null.Or.Empty );
            Assert.That( m.StartIndex == idx, "Head must not move on error." );
            Assert.That( m.Length == len, "Length must not change on error." );
            m.SetSuccess();
        }

        [Test]
        public void ToString_constains_the_text_and_the_error()
        {
            var m = new StringMatcher( "The Text" );
            m.SetError( "Plouf..." );
            Assert.That( m.ToString(), Does.Contain( "The Text" ).And.Contain( "Plouf..." ) );
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
            Assert.That( m.TryMatchJSONQuotedString( out result, true ) );
            Assert.That( result, Is.EqualTo( parsed ) );
            Assert.That( m.TryMatchText( textAfter ), "Should be followed by: " + textAfter );

            m = new StringMatcher( s );
            Assert.That( m.TryMatchJSONQuotedString( true ) );
            Assert.That( m.TryMatchText( textAfter ), "Should be followed by: " + textAfter );
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
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '{' ) );
            string pName;
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString( out pName ) && pName == "p1" );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ':' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString( out pName ) && pName == "n" );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ',' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString( out pName ) && pName == "p2" );
            Assert.That( m.MatchWhiteSpaces( 2 ) && m.MatchChar( ':' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '{' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString( out pName ) && pName == "p3" );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ':' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '[' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString( out pName ) && pName == "p4" );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ':' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '{' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString() );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ':' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchDoubleValue() );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ',' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString( out pName ) && pName == "p6" );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ':' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '[' ) );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ']' ) );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ',' ) );
            Assert.That( m.MatchWhiteSpaces() && m.TryMatchJSONQuotedString() );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( ':' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '{' ) );
            Assert.That( m.MatchWhiteSpaces( 0 ) && m.MatchChar( '}' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '}' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( ']' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '}' ) );
            Assert.That( m.MatchWhiteSpaces() && m.MatchChar( '}' ) );
            Assert.That( m.MatchWhiteSpaces( 2 ) && m.IsEnd );
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
            Assert.That( m.MatchChar( 'P' ) );
            int idx = m.StartIndex;
            Assert.That( m.TryMatchDoubleValue() );
            m.UncheckedMove( idx - m.StartIndex );
            double parsed;
            Assert.That( m.TryMatchDoubleValue( out parsed ) );
            Assert.That( parsed, Is.EqualTo( d ).Within( 1 ).Ulps );
            Assert.That( m.MatchChar( 'S' ) );
            Assert.That( m.IsEnd );
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
                Assert.That( m.TryMatchGuid( out readId ) && readId == id );
            }
            {
                string s = "S" + sId;
                var m = new StringMatcher( s );
                Guid readId;
                Assert.That( m.MatchChar( 'S' ) );
                Assert.That( m.TryMatchGuid( out readId ) && readId == id );
            }
            {
                string s = "S" + sId + "T";
                var m = new StringMatcher( s );
                Guid readId;
                Assert.That( m.MatchChar( 'S' ) );
                Assert.That( m.TryMatchGuid( out readId ) && readId == id );
                Assert.That( m.MatchChar( 'T' ) );
            }
            sId = sId.Remove( sId.Length - 1 );
            {
                string s = sId;
                var m = new StringMatcher( s );
                Guid readId;
                Assert.That( m.TryMatchGuid( out readId ), Is.False );
                Assert.That( m.StartIndex, Is.EqualTo( 0 ) );
            }
            sId = id.ToString().Insert( 3, "K" ).Remove( 4 );
            {
                string s = sId;
                var m = new StringMatcher( s );
                Guid readId;
                Assert.That( m.TryMatchGuid( out readId ), Is.False );
                Assert.That( m.StartIndex, Is.EqualTo( 0 ) );
            }
        }

    }
}