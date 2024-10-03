using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using System.Diagnostics;

namespace CK.Core.Tests;

[TestFixture]
public class MatchJSONTests
{
    [Test]
    public void match_JSON_objects()
    {
        {
            var j = @"{""A"":1,""B"":2}";
            var m = new ROSpanCharMatcher( j );
            m.TryMatchAnyJSON( out object? o ).Should().BeTrue();
            var list = o as List<(string Name, object Value)>;
            Debug.Assert( list != null );
            list.Select( k => k.Name + '|' + k.Value ).Concatenate().Should().Be( "A|1, B|2" );
        }
        {
            var j = @"{ ""A"" : 1.0, ""B"" : 2 }";
            var m = new ROSpanCharMatcher( j );
            m.TryMatchAnyJSON( out object? o ).Should().BeTrue();
            var list = o as List<(string Name, object Value)>;
            Debug.Assert( list != null );
            list.Select( k => k.Name + '|' + k.Value ).Concatenate().Should().Be( "A|1, B|2" );
        }
        {
            var j = @"{ ""A"" : [ ""a"" , 3 , null , 6], ""B"" : [ 2, 3, ""XX"" ] }";
            var m = new ROSpanCharMatcher( j );
            m.TryMatchAnyJSON( out object? o ).Should().BeTrue();
            var list = o as List<(string Name, object Value)>;
            Debug.Assert( list != null );
            list.Select( k => k.Name
                              + '|'
                              + ((List<object?>)k.Value).Select( v => v?.ToString() ).Concatenate( "+" ) )
                .Concatenate().Should().Be( "A|a+3++6, B|2+3+XX" );
        }
    }

    [Test]
    public void match_JSON_empty_array_or_objects()
    {
        {
            var j = @"{}";
            var m = new ROSpanCharMatcher( j );
            m.TryMatchAnyJSON( out object? o ).Should().BeTrue();
            var list = o as List<(string Name, object Value)>;
            Debug.Assert( list != null );
            list.Should().BeEmpty();
        }
        {
            var j = @"[]";
            var m = new ROSpanCharMatcher( j );
            m.TryMatchAnyJSON( out object? o ).Should().BeTrue();
            var list = o as List<object?>;
            Debug.Assert( list != null );
            list.Should().BeEmpty();
        }
    }

    [TestCase( "1.2" )]
    [TestCase( "/* ... */1.2// ..." )]
    [TestCase( "1.2/* ..." )]
    [TestCase( "/* ... */1.2// ..." )]
    [TestCase( @"/*
*/  // ...

/* 3 */ 1.2     
//...
/*" )]
    public void match_JSON_skips_JS_comments( string jsonWithComment )
    {
        var m = new ROSpanCharMatcher( jsonWithComment );
        m.TryMatchAnyJSON( out object? o ).Should().BeTrue();
        o.Should().Be( 1.2 );
    }


    [TestCase( "{", "@0-Any JSON token or object|@1--JSON object properties" )]
    [TestCase( "[", "@0-Any JSON token or object|@1--JSON array values" )]
    [TestCase( "[null,,]", "@0-Any JSON token or object|@1--JSON array values|@6---Any JSON token or object|@6----String 'true'|@6----String 'false'|@6----JSON string or null|@6----Floating number" )]
    public void TryMatchAnyJSON_has_detailed_errors( string s, string errors )
    {
        var m = new ROSpanCharMatcher( s );
        m.TryMatchAnyJSON( out _ ).Should().BeFalse();
        m.HasError.Should().BeTrue();
        m.GetRawErrors().Select( e => $"@{e.Pos}{new string( '-', e.Depth + 1 )}{e.Expectation}" ).Concatenate( '|' ).Should().Be( errors );

        m.SetSuccess();
        m.SingleExpectationMode = true;
        m.TryMatchAnyJSON( out _ ).Should().BeFalse();
        m.HasError.Should().BeTrue();
        m.GetRawErrors().Single().Expectation.Should().Be( errors.Split( '|' )[0].Remove( 0, 3 ) );
    }
}
