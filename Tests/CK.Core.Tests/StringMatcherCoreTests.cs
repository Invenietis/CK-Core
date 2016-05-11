using CK.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class StringMatcherCoreTests
    {
        [Test]
        public void matching_FileNameUniqueTimeUtcFormat()
        {
            DateTime t = DateTime.UtcNow;
            string s = t.ToString( FileUtil.FileNameUniqueTimeUtcFormat );
            var m = new StringMatcher( "X" + s + "Y" );
            Assert.That( m.MatchChar( 'X' ) );
            DateTime parsed;
            Assert.That( m.MatchFileNameUniqueTimeUtcFormat( out parsed ) && parsed == t );
            Assert.That( m.MatchChar( 'Y' ) );

            m = new StringMatcher( s.Insert( 2, "X" ) );
            Assert.That( m.MatchFileNameUniqueTimeUtcFormat( out parsed ), Is.False );
            int i;
            Assert.That( m.MatchInt32( out i ) && i == 20 );
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
            var m = new StringMatcher( "X" + s + "Y" );
            Assert.That( m.MatchChar( 'X' ) );
            DateTimeStamp parsed;
            Assert.That( m.MatchDateTimeStamp( out parsed ) && parsed == t );
            Assert.That( m.MatchChar( 'Y' ) );

            m = new StringMatcher( s.Insert( 2, "X" ) );
            Assert.That( m.MatchDateTimeStamp( out parsed ), Is.False );
            Assert.That( m.ErrorMessage, Is.Not.Null );
            int i;
            Assert.That( m.MatchInt32( out i ) && i == 20 );
        }

        public void match_methods_must_set_an_error()
        {
            var m = new StringMatcher( "A" );

            DateTimeStamp ts;
            CheckMatchError( m, () => m.MatchDateTimeStamp( out ts ) );
            DateTime dt;
            CheckMatchError( m, () => m.MatchFileNameUniqueTimeUtcFormat( out dt ) );
            CheckMatchError( m, () => m.MatchText( "B" ) );
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
    }
}
