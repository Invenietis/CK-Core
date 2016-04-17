using System;
using NUnit.Framework;
using CK.Core;

namespace CK.Text.Tests
{
    [TestFixture]
    public class StringMatcherTests
    {
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

    }
}