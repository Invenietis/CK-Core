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
    public class ExploringTests
    {
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
                string t = b.AppendMultiLine( "|", text, true ).ToString();
                Assert.That( t, Is.EqualTo( @"|First line.
|Second line.
|    Indented.
|
|    Also indented.
|Last line." ) );
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
|Last line." ) );
            }

        }

        [Test]
        public void appending_multi_lines_is_slower_than_naive_implementation()
        {
            string f = File.ReadAllText( Path.Combine( TestHelper.SolutionFolder, "Tests/CK.Text.Tests/StringAndStringBuilderExtensionTests.cs" ) );
            Stopwatch w = new Stopwatch();
            string[] results = new string[1000];
            long naive = PrefixWithNaiveReplace( w, f, results );
            string aNaive = results[0];
            long better = PrefixWithOurExtension( w, f, results );
            Assert.That( results[0], Is.EqualTo( aNaive ) );
            for( int i = 0; i < 10; ++i )
            {
                naive += PrefixWithNaiveReplace( w, f, results );
                better += PrefixWithOurExtension( w, f, results );
            }
            double factor = (double)better / naive;
            Console.WriteLine( $"Naive:{naive}, Extension:{better}. Factor: {factor}" );
            Assert.That( factor > 1 );
        }

        static readonly string prefix = "-";

        long PrefixWithNaiveReplace( Stopwatch w, string f, string[] results )
        {
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
            w.Restart();
            StringBuilder b = new StringBuilder();
            for( int i = 0; i < results.Length; ++i )
            {
                results[i] = b.AppendMultiLine( prefix, f, false, true ).ToString();
                b.Clear();
            }
            w.Stop();
            return w.ElapsedTicks;
        }
    }

    public static class SlowMethods
    {
        public static StringBuilder AppendMultiLine(
            this StringBuilder @this,
            string prefix,
            string text,
            bool prefixOnFirstLine,
            bool prefixLastEmptyLine = false )
        {
            if( string.IsNullOrEmpty( prefix ) ) return @this.Append( text ?? string.Empty );
            if( string.IsNullOrEmpty( text ) ) return prefixOnFirstLine
                                                        ? @this.Append( prefix ?? string.Empty )
                                                        : @this;
            bool lastIsNewLine = false;
            int lenLine, prevIndex = 0, i = 0;
            while( i < text.Length )
            {
                char c = text[i];
                if( c == '\r' || c == '\n' )
                {
                    lenLine = i - prevIndex;
                    if( prevIndex > 0 || prefixOnFirstLine )
                    {
                        if( prevIndex > 0 ) @this.AppendLine();
                        @this.Append( prefix );
                    }
                    @this.Append( text, prevIndex, lenLine );
                    if( ++i < text.Length && c == '\r' && text[i] == '\n' ) ++i;
                    prevIndex = i;
                    lastIsNewLine = true;
                }
                else
                {
                    ++i;
                    lastIsNewLine = false;
                }
            }
            lenLine = i - prevIndex;
            if( lenLine > 0 || (lastIsNewLine && prefixLastEmptyLine) )
            {
                if( prevIndex == 0 )
                {
                    if( prefixOnFirstLine ) @this.Append( prefix );
                    @this.Append( text );
                }
                else
                {
                    @this.AppendLine();
                    @this.Append( prefix ).Append( text, prevIndex, lenLine );
                }
            }
            return @this;
        }


    }
}
