#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\UtilMatcherTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
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
        public void matching_strings_and_whitespaces()
        {
            string s = " AB  \t\r C";
            var m = new StringMatcher( s );
            Assert.That( m.MatchString( "A" ), Is.False );
            Assert.That( m.StartIndex, Is.EqualTo( 0 ) );
            Assert.That( m.MatchWhiteSpaces(), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( 1 ) );
            Assert.That( m.MatchString( "A" ), Is.True );
            Assert.That( m.MatchString( "B" ), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( 3 ) );
            Assert.That( m.MatchWhiteSpaces( 6 ), Is.False );
            Assert.That( m.MatchWhiteSpaces( 5 ), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( 8 ) );
            Assert.That( m.MatchWhiteSpaces(), Is.False );
            Assert.That( m.StartIndex, Is.EqualTo( 8 ) );
            Assert.That( m.MatchString( "c" ), Is.True );
            Assert.That( m.StartIndex, Is.EqualTo( s.Length ) );
            Assert.That( m.IsEnd, Is.True );

            Assert.DoesNotThrow( () => m.MatchString( "c" ) );
            Assert.DoesNotThrow( () => m.MatchWhiteSpaces() );
            Assert.That( m.MatchString( "A" ), Is.False );
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
    }
}
