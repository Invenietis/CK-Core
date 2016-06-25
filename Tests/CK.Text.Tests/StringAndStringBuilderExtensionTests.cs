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

        [Test]
        public void appending_multi_lines_with_a_prefix_with_null_or_empty_or_one_line()
        {
            {
                StringBuilder b = new StringBuilder();
                string text = @"One line.";
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( @"|One line." ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = @"";
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( @"|" ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = null;
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( @"|" ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = @"One line.";
                string t = b.AppendMultiLine( "|", text, false ).ToString();
                Assert.That( t, Is.EqualTo( @"One line." ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = @"";
                string t = b.AppendMultiLine( "|", text, false ).ToString();
                Assert.That( t, Is.EqualTo( @"" ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = null;
                string t = b.AppendMultiLine( "|", text, false ).ToString();
                Assert.That( t, Is.EqualTo( @"" ) );
            }

        }

        [Test]
        public void appending_multi_lines_to_empty_lines()
        {
            {
                StringBuilder b = new StringBuilder();
                string text = Environment.NewLine;
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( "|" ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = Environment.NewLine + Environment.NewLine;
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( "|" + Environment.NewLine + "|" ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = Environment.NewLine + Environment.NewLine + Environment.NewLine;
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( "|" + Environment.NewLine + "|" + Environment.NewLine + "|" ) );
            }
            {
                StringBuilder b = new StringBuilder();
                string text = Environment.NewLine + Environment.NewLine + Environment.NewLine + "a";
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( "|" + Environment.NewLine + "|" + Environment.NewLine + "|" + Environment.NewLine + "|a" ) );
            }
        }

        [Test]
        public void appending_multi_lines_with_a_prefix()
        {
            {
                StringBuilder b = new StringBuilder();
                string text = @"First line.
Second line.
    Indented.

    Also indented.
Last line.";
                // Here, normalizing the source embedded string is to support 
                // git clone with LF in files instead of CRLF. 
                // Our (slow) AppendMultiLine normalizes the end of lines to Environment.NewLine.
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( @"|First line.
|Second line.
|    Indented.
|
|    Also indented.
|Last line.".NormalizeEOL() ) );
            }

            {
                StringBuilder b = new StringBuilder();
                string text = @"First line.
Second line.
    Indented.

    Also indented.
Last line.";
                string t = b.AppendMultiLine( "|", text, false ).ToString();
                Assert.That( t, Is.EqualTo( @"First line.
|Second line.
|    Indented.
|
|    Also indented.
|Last line.".NormalizeEOL() ) );
            }

        }

        [Test]
        public void appending_multi_lines_with_prefixLastEmptyLine()
        {
            string text = @"First line.
Second line.


";
            {
                StringBuilder b = new StringBuilder();
                string t = b.AppendMultiLine( "|", text, true, prefixLastEmptyLine: false ).ToString();
                Assert.That( t, Is.EqualTo( @"|First line.
|Second line.
|
|".NormalizeEOL() ) );
            }

            {
                StringBuilder b = new StringBuilder();
                string t = b.AppendMultiLine( "|", text, true, prefixLastEmptyLine: true ).ToString();
                Assert.That( t, Is.EqualTo( @"|First line.
|Second line.
|
|
|".NormalizeEOL() ) );
            }
        }

        [Test]
        public void our_appending_multi_lines_is_better_than_naive_implementation_in_release_but_not_in_debug()
        {
            string text = File.ReadAllText( Path.Combine( TestHelper.SolutionFolder, "Tests/CK.Text.Tests/StringAndStringBuilderExtensionTests.cs" ) );
            text = text.NormalizeEOL();
            TestPerf( text, 10 );
            TestPerf( "Small text may behave differently", 100 );
            TestPerf( "Small text may"+Environment.NewLine + "behave differently" +Environment.NewLine, 100 );
        }

        void TestPerf( string text, int count )
        {
            Stopwatch w = new Stopwatch();
            string[] results = new string[2000];
            long naive = PrefixWithNaiveReplace( w, text, results );
            string aNaive = results[0];
            long better = PrefixWithOurExtension( w, text, results );
            Assert.That( results[0], Is.EqualTo( aNaive ) );
            for( int i = 0; i < count; ++i )
            {
                naive += PrefixWithNaiveReplace( w, text, results );
                better += PrefixWithOurExtension( w, text, results );
            }
            double factor = (double)better / naive;
            Console.WriteLine( $"Naive:{naive}, Extension:{better}. Factor: {factor}" );
#if DEBUG
            Assert.That( factor > 1 );
#else
            Assert.That( factor < 1 );
#endif
        }

        static readonly string prefix = "-!-";

        long PrefixWithNaiveReplace( Stopwatch w, string f, string[] results )
        {
            GC.Collect();
            w.Restart();
            for( int i = 0; i < results.Length; ++i )
            {
                results[i] = f.Replace( Environment.NewLine, Environment.NewLine + prefix );
            }
            w.Stop();
            return w.ElapsedTicks;
        }

        long PrefixWithOurExtension( Stopwatch w, string f, string[] results )
        {
            GC.Collect();
            w.Restart();
            StringBuilder b = new StringBuilder();
            for( int i = 0; i < results.Length; ++i )
            {
                // We must use the prefixLastEmptyLine to match the way the naive implementation works.
                results[i] = b.AppendMultiLine( prefix, f, false, prefixLastEmptyLine: true ).ToString();
                b.Clear();
            }
            w.Stop();
            return w.ElapsedTicks;
        }

    }

}
