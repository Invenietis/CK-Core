using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;

namespace CK.Text.Tests
{
    [TestFixture]
    public class StringAndStringBuilderExtensionTests
    {
        [Test]
        public void concat_method_uses_StringBuilder_AppendStrings_inside()
        {
            var strings = new string[] { "A", "Hello", "B", "World", null, "End" };
            var s = strings.Concatenate( "|+|" );
            Assert.That( s, Is.EqualTo( "A|+|Hello|+|B|+|World|+||+|End" ) );
        }

        [Test]
        public void StringBuilder_AppendStrings_method_does_not_skip_null_entries()
        {
            var strings = new string[] { "A", "Hello", "B", "World", null, "End" };
            var b = new StringBuilder();
            b.AppendStrings( strings, "|+|" );
            Assert.That( b.ToString(), Is.EqualTo( "A|+|Hello|+|B|+|World|+||+|End" ) );
        }

        [Test]
        public void appending_multiple_strings_with_a_repeat_count()
        {
            Assert.That( new StringBuilder().Append( "A", 1 ).ToString(), Is.EqualTo( "A" ) );
            Assert.That( new StringBuilder().Append( "AB", 2 ).ToString(), Is.EqualTo( "ABAB" ) );
            Assert.That( new StringBuilder().Append( "|-|", 10 ).ToString(), Is.EqualTo( "|-||-||-||-||-||-||-||-||-||-|" ) );
        }

        [Test]
        public void appends_multiple_strings_silently_ignores_0_or_negative_RepeatCount()
        {
            Assert.That( new StringBuilder().Append( "A", 0 ).ToString(), Is.Empty );
            Assert.That( new StringBuilder().Append( "A", -1 ).ToString(), Is.Empty );
        }

        [Test]
        public void appends_multiple_strings_silently_ignores_null_or_empty_string_to_repeat()
        {
            Assert.That( new StringBuilder().Append( "", 20 ).ToString(), Is.Empty );
            Assert.That( new StringBuilder().Append( null, 20 ).ToString(), Is.Empty );
        }

        [TestCase( '0', 0 )]
        [TestCase( '1', 1 )]
        [TestCase( '9', 9 )]
        [TestCase( 'a', 10 )]
        [TestCase( 'e', 14 )]
        [TestCase( 'f', 15 )]
        [TestCase( 'A', 10 )]
        [TestCase( 'C', 12 )]
        [TestCase( 'F', 15 )]
        [TestCase( 'm', -1 )]
        [TestCase( '\t', -1 )]
        [TestCase( '\u0000', -1 )]
        [TestCase( 'Z', -1 )]
        public void HexDigitValue_extension_method_on_character( char c, int expected )
        {
            Assert.That( c.HexDigitValue(), Is.EqualTo( expected ) );
        }
    }

}
