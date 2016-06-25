using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Text.Tests
{
    [TestFixture]
    public class JSONVisitorTests
    {
        class JSONProperties : JSONVisitor
        {
            public List<string> Properties;
            public List<string> Paths;

            public JSONProperties( StringMatcher m )
                : base( m )
            {
                Properties = new List<string>();
                Paths = new List<string>();
            }

            protected override bool VisitObjectProperty( int startPropertyIndex, string propertyName, int propertyIndex )
            {
                Properties.Add( propertyName );
                Paths.Add( string.Join( "|", Path.Select( x => x.Index + "=" + x.PropertyName ) ) + " => " + propertyIndex + "=" + propertyName );
                return base.VisitObjectProperty( startPropertyIndex, propertyName, propertyIndex );
            }
        }

        [Test]
        public void extracting_properties_from_a_JSON()
        {
            string s = @"
{ 
    ""p1"": ""n"", 
    ""p2"": 
    { 
        ""p3"": 
        [ 
            {
                ""p4Before"": [""zero"", ""one"", { ""pSub"": [] }, ""three"" ]
                ""p4"": 
                { 
                    ""p5"" : 0.989, 
                    ""p6"": [],
                    ""p7"": {}
                }
            }
        ] 
    } 
}";
            JSONProperties p = new JSONProperties( new StringMatcher( s ) );
            p.Visit();
            CollectionAssert.AreEqual( new[] { "p1", "p2", "p3", "p4Before", "pSub", "p4", "p5", "p6", "p7" }, p.Properties );
            CollectionAssert.AreEqual( new[] {
                " => 0=p1",
                " => 1=p2",
                "1=p2 => 0=p3",
                "1=p2|0=p3|0= => 0=p4Before",
                "1=p2|0=p3|0=|0=p4Before|2= => 0=pSub",
                "1=p2|0=p3|0= => 1=p4",
                "1=p2|0=p3|0=|1=p4 => 0=p5",
                "1=p2|0=p3|0=|1=p4 => 1=p6",
                "1=p2|0=p3|0=|1=p4 => 2=p7" }, p.Paths );
        }

        class JSONDoubleSum : JSONVisitor
        {
            public double Sum;

            public JSONDoubleSum( string s ) : base( s ) { }

            protected override bool VisitTerminalValue()
            {
                Matcher.MatchWhiteSpaces( 0 );
                double d;
                if( Matcher.TryMatchDoubleValue( out d ) )
                {
                    Sum += d;
                    return true;
                }
                else return base.VisitTerminalValue();
            }
        }

        [Test]
        public void summing_all_doubles_in_a_json()
        {
            string data = @"
{
    ""v"": 9.87e2, 
    ""a"": [ 8.65, true, {}, {""x"" : 45.98, ""y"":12.786}, 874.6324 ]
}";
            var v = new JSONDoubleSum( data );
            v.Visit();
            Assert.That( v.Sum, Is.EqualTo( 9.87e2 + 8.65 + 45.98 + 12.786 + 874.6324 ) );
        }

        class JSONDoubleRewriter : JSONVisitor
        {
            readonly StringBuilder _builder;
            readonly Func<double, string> _rewriter;
            int _lastWriteIdx;

            public JSONDoubleRewriter( string s, Func<double,string> rewriter ) 
                : base( s )
            {
                _rewriter = rewriter;
                _builder = new StringBuilder();
            }

            public string Rewrite()
            {
                _lastWriteIdx = Matcher.StartIndex;
                _builder.Clear();
                Visit();
                Flush( Matcher.StartIndex );
                return _builder.ToString();
            }

            void Flush( int idx )
            {
                int len = idx - _lastWriteIdx;
                _builder.Append( Matcher.Text, _lastWriteIdx, len );
                _lastWriteIdx = idx;
            }

            protected override bool VisitTerminalValue()
            {
                Matcher.MatchWhiteSpaces( 0 );
                int idx = Matcher.StartIndex;
                double d;
                if( Matcher.TryMatchDoubleValue( out d ) )
                {
                    Flush( idx );
                    _builder.Append( _rewriter( d ) );
                    _lastWriteIdx = Matcher.StartIndex;
                    return true;
                }
                else return base.VisitTerminalValue();
            }
        }

        [Test]
        public void using_JSONVisitor_to_transform_all_doubles_in_it()
        {
            string data = @"
{
    ""v"": 9.87e2, 
    ""a"": [ 8.65, true, {}, {""x"" : 45.98, ""y"":12.786}, 874.6324 ]
}";
            var v = new JSONDoubleRewriter( data, d =>
            {
                Console.WriteLine( "{0} => {1}", d, Math.Floor( d ).ToString() );
                return Math.Floor( d ).ToString();
            } );
          
            string rewritten = v.Rewrite();

            var summer = new JSONDoubleSum( rewritten );
            summer.Visit();
            Assert.That( summer.Sum, Is.EqualTo( 987 + 8 + 45 + 12 + 874 ) );
        }


        class JSONMinifier : JSONVisitor
        {
            readonly StringBuilder _builder;
            int _lastWriteIdx;

            public JSONMinifier( string s )
                : base( s )
            {
                _builder = new StringBuilder();
            }

            static public string Minify( string s )
            {
                return new JSONMinifier( s ).Run();
            } 

            string Run()
            {
                _lastWriteIdx = Matcher.StartIndex;
                Visit();
                Flush( Matcher.StartIndex );
                return _builder.ToString();
            }

            void Flush( int idx )
            {
                int len = idx - _lastWriteIdx;
                _builder.Append( Matcher.Text, _lastWriteIdx, len );
                _lastWriteIdx = idx;
            }

            protected override void SkipWhiteSpaces()
            {
                if( char.IsWhiteSpace( Matcher.Head ) )
                {
                    Flush( Matcher.StartIndex );
                    Matcher.MatchWhiteSpaces( 0 );
                    _lastWriteIdx = Matcher.StartIndex;
                }
            }

        }

        [Test]
        public void minifying_JSON()
        {
            string data = @"
{
    ""v"": 9.87e2, 
    ""a"" : 
        [ 8.65, 
            true, 
            { } 
            , { ""x"" : null,           ""y"": 0.0      }
        , 874 
]
}";
            string mini = JSONMinifier.Minify( data );
            Assert.That( mini, Is.EqualTo( @"{""v"":9.87e2,""a"":[8.65,true,{},{""x"":null,""y"":0.0},874]}" ) );
        }


    }
}
