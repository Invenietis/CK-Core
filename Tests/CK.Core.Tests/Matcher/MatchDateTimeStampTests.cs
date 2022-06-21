using FluentAssertions;
using System;
using NUnit.Framework;
using System.Linq;

namespace CK.Core.Tests
{

    public class MatchDateTimeStampTests
    {
        [Test]
        public void matching_FileNameUniqueTimeUtcFormat()
        {
            DateTime t = DateTime.UtcNow;
            var s = ("X" + t.ToString( FileUtil.FileNameUniqueTimeUtcFormat ) + "Y").AsSpan();

            s.TryMatch( 'X' ).Should().BeTrue();
            DateTime parsed;
            s.TryMatchFileNameUniqueTimeUtcFormat( out parsed ).Should().BeTrue();
            parsed.Should().Be( t );
            s.TryMatch( 'Y' ).Should().BeTrue();

            s = t.ToString( FileUtil.FileNameUniqueTimeUtcFormat ).Insert( 2, "X" ).AsSpan();
            s.TryMatchFileNameUniqueTimeUtcFormat( out _ ).Should().BeFalse();
            s.TryMatchInt32( out int i ).Should().BeTrue();
            i.Should().Be( 20 );
        }

        [Test]
        public void matching_DateTimeStamp()
        {
            DateTimeStamp t = DateTimeStamp.UtcNow;
            CheckDateTimeStamp( t );
            CheckDateTimeStamp( new DateTimeStamp( t.TimeUtc, 67 ) );
        }

        private static void CheckDateTimeStamp( DateTimeStamp t )
        {
            string s = t.ToString();
            var m = new ROSpanCharMatcher( "X" + s + "Y" );
            m.TryMatch( 'X' ).Should().BeTrue();
            DateTimeStamp parsed;
            m.TryMatchDateTimeStamp( out parsed ).Should().BeTrue();
            parsed.Should().Be( t );
            m.TryMatch( 'Y' ).Should().BeTrue();

            m = new ROSpanCharMatcher( s.Insert( 2, "X" ) );
            m.TryMatchDateTimeStamp( out parsed ).Should().BeFalse();
            m.TryMatchInt32( out int i ).Should().BeTrue();
            i.Should().Be( 20 );

            m = new ROSpanCharMatcher( s.Insert( s.Length - 2, "X" ) );
            m.TryMatchDateTimeStamp( out parsed ).Should().BeFalse();
            m.Head.Length.Should().Be( m.AllText.Length );

            m = new ROSpanCharMatcher( s.Insert( s.Length - 1, "X" ) );
            m.TryMatchDateTimeStamp( out parsed ).Should().BeFalse();
            m.Head.Length.Should().Be( m.AllText.Length );
        }

        [TestCase( "A", "@0-DateTimeStamp|@0--UTC time" )]
        [TestCase( "2021-12-21 14h37.18.5195853(3", "@0-DateTimeStamp|@29--Character ')'" )]
        [TestCase( "2021-12-21 14h37.18.5195853(X", "@0-DateTimeStamp|@28--Integer between 0 and 255 (without leading zeros)" )]
        [TestCase( "2021-12-21 14h37.18.5195853(", "@0-DateTimeStamp|@28--Integer between 0 and 255 (without leading zeros)" )]
        public void match_methods_must_set_an_error( string s, string errors )
        {
            var m = new ROSpanCharMatcher( s );
            m.TryMatchDateTimeStamp( out _ ).Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.GetRawErrors().Select( e => $"@{e.Pos}{new string( '-', e.Depth + 1 )}{e.Expectation}" ).Concatenate( '|' ).Should().Be( errors );
            m.Head.Length.Should().Be( m.AllText.Length );
        }

    }
}
