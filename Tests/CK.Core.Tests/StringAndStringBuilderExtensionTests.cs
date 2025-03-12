using System;
using System.Text;
using NUnit.Framework;
using Shouldly;

namespace CK.Core.Tests;

[TestFixture]
public class StringAndStringBuilderExtensionTests
{
    [Test]
    public void concat_method_uses_String_Join_inside()
    {
        var strings = new string[] { "A", "Hello", "B", "World", null!, "End" };
        var s = strings.Concatenate( "|+|" );
        s.ShouldBe( "A|+|Hello|+|B|+|World|+||+|End" );
    }

    [Test]
    public void StringBuilder_AppendStrings_method_does_not_skip_null_entries()
    {
        var strings = new string[] { "A", "Hello", "B", "World", null!, "End" };
        var b = new StringBuilder();
        b.AppendStrings( strings, "|+|" );
        b.ToString().ShouldBe( "A|+|Hello|+|B|+|World|+||+|End" );
    }

    [Test]
    public void appending_multiple_strings_with_a_repeat_count()
    {
        new StringBuilder().Append( "A", 1 ).ToString().ShouldBe( "A" );
        new StringBuilder().Append( "AB", 2 ).ToString().ShouldBe( "ABAB" );
        new StringBuilder().Append( "|-|", 10 ).ToString().ShouldBe( "|-||-||-||-||-||-||-||-||-||-|" );
    }

    [Test]
    public void appends_multiple_strings_silently_ignores_0_or_negative_RepeatCount()
    {
        new StringBuilder().Append( "A", 0 ).ToString().ShouldBeEmpty();
        new StringBuilder().Append( "A", -1 ).ToString().ShouldBeEmpty();
    }

    [Test]
    public void appends_multiple_strings_silently_ignores_null_or_empty_string_to_repeat()
    {
        new StringBuilder().Append( "", 20 ).ToString().ShouldBeEmpty();
        new StringBuilder().Append( (string?)null!, 20 ).ToString().ShouldBeEmpty();
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
        c.HexDigitValue().ShouldBe( expected );
    }

    [Test]
    public void appending_multi_lines_with_a_prefix_with_null_or_empty_or_one_line()
    {
        {
            StringBuilder b = new StringBuilder();
            string text = @"One line.";
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( @"|One line." );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = @"";
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( @"|" );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = null!;
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( @"|" );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = @"One line.";
            string t = b.AppendMultiLine( "|", text, false ).ToString();
            t.ShouldBe( @"One line." );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = @"";
            string t = b.AppendMultiLine( "|", text, false ).ToString(); ;
            t.ShouldBe( @"" );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = null!;
            string t = b.AppendMultiLine( "|", text, false ).ToString();
            t.ShouldBe( @"" );
        }

    }

    [Test]
    public void appending_multi_lines_to_empty_lines()
    {
        {
            StringBuilder b = new StringBuilder();
            string text = Environment.NewLine;
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( "|" );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = Environment.NewLine + Environment.NewLine;
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( "|" + Environment.NewLine + "|" );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = Environment.NewLine + Environment.NewLine + Environment.NewLine;
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( "|" + Environment.NewLine + "|" + Environment.NewLine + "|" );
        }
        {
            StringBuilder b = new StringBuilder();
            string text = Environment.NewLine + Environment.NewLine + Environment.NewLine + "a";
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( "|" + Environment.NewLine + "|" + Environment.NewLine + "|" + Environment.NewLine + "|a" );
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
            // Our AppendMultiLine normalizes the end of lines to Environment.NewLine.
            string t = b.AppendMultiLine( "|", text, true ).ToString();
            t.ShouldBe( @"|First line.
|Second line.
|    Indented.
|
|    Also indented.
|Last line.".ReplaceLineEndings() );
        }

        {
            StringBuilder b = new StringBuilder();
            string text = @"First line.
Second line.
    Indented.

    Also indented.
Last line.";
            string t = b.AppendMultiLine( "|", text, false ).ToString();
            t.ShouldBe( @"First line.
|Second line.
|    Indented.
|
|    Also indented.
|Last line.".ReplaceLineEndings() );
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
            t.ShouldBe( @"|First line.
|Second line.
|
|".ReplaceLineEndings() );
        }

        {
            StringBuilder b = new StringBuilder();
            string t = b.AppendMultiLine( "|", text, true, prefixLastEmptyLine: true ).ToString();
            t.ShouldBe( @"|First line.
|Second line.
|
|
|".ReplaceLineEndings() );
        }
    }

}
