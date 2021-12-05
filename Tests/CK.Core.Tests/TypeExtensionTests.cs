using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class TypeExtensionTests
    {
#pragma warning disable CS0693
        public class Above<T1>
        {
            // This triggers a CS0693 warning:
            // Type parameter 'T1' has the same name as the type parameter from outer type 'ToCSharpNameTests.Above<T1>'
            public class Below<T1>
            {
            }
        }
#pragma warning restore CS0693

        public class G<T>
        {
            public class Nested
            {
                public A<int?>.C<string>? Prop { get; set; }
            }
        }

        public class A<TB>
        {
            public class C<TD> { }
        }

        public class H<T1, T2> { }

        public class Another
        {
            public class I<T3> { }
        }

        [TestCase( typeof( Dictionary<,>.KeyCollection ), "System.Collections.Generic.Dictionary<,>.KeyCollection" )]
        [TestCase( typeof( Dictionary<int, string>.KeyCollection ), "System.Collections.Generic.Dictionary<int,string>.KeyCollection" )]
        [TestCase( typeof( Above<>.Below<> ), "CK.Core.Tests.TypeExtensionTests.Above<>.Below<>" )]
        [TestCase( typeof( Above<long>.Below<int> ), "CK.Core.Tests.TypeExtensionTests.Above<long>.Below<int>" )]
        [TestCase( typeof( TypeExtensionTests ), "CK.Core.Tests.TypeExtensionTests" )]
        [TestCase( typeof( List<string> ), "System.Collections.Generic.List<string>" )]
        [TestCase( typeof( List<Dictionary<int, string>> ), "System.Collections.Generic.List<System.Collections.Generic.Dictionary<int,string>>" )]
        [TestCase( typeof( Nullable<Guid> ), "System.Guid?" )]
        [TestCase( typeof( Guid? ), "System.Guid?" )]
        [TestCase( typeof( Another ), "CK.Core.Tests.TypeExtensionTests.Another" )]
        [TestCase( typeof( G<> ), "CK.Core.Tests.TypeExtensionTests.G<>" )]
        [TestCase( typeof( G<string> ), "CK.Core.Tests.TypeExtensionTests.G<string>" )]
        [TestCase( typeof( G<Another> ), "CK.Core.Tests.TypeExtensionTests.G<CK.Core.Tests.TypeExtensionTests.Another>" )]
        [TestCase( typeof( H<,> ), "CK.Core.Tests.TypeExtensionTests.H<,>" )]
        [TestCase( typeof( H<string, Another> ), "CK.Core.Tests.TypeExtensionTests.H<string,CK.Core.Tests.TypeExtensionTests.Another>" )]
        [TestCase( typeof( Another.I<> ), "CK.Core.Tests.TypeExtensionTests.Another.I<>" )]
        [TestCase( typeof( Another.I<int> ), "CK.Core.Tests.TypeExtensionTests.Another.I<int>" )]
        [TestCase( typeof( G<>.Nested ), "CK.Core.Tests.TypeExtensionTests.G<>.Nested" )]
        [TestCase( typeof( G<string>.Nested ), "CK.Core.Tests.TypeExtensionTests.G<string>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.Core.Tests.TypeExtensionTests.A<>.C<>" )]
        [TestCase( typeof( A<int>.C<string> ), "CK.Core.Tests.TypeExtensionTests.A<int>.C<string>" )]
        public void ToCSharpName_without_generic_parameter_names( Type type, string expected )
        {
            type.ToCSharpName( typeDeclaration: false ).Should().Be( expected );
        }

        [TestCase( typeof( List<IEnumerable<string>> ), "List<IEnumerable<string>>" )]
        [TestCase( typeof( H<string, Another> ), "TypeExtensionTests.H<string,TypeExtensionTests.Another>" )]
        public void ToCSharpName_without_namespace( Type type, string expected )
        {
            type.ToCSharpName( withNamespace: false ).Should().Be( expected );
        }

        [TestCase( typeof( long* ), "long*" )]
        [TestCase( typeof( Guid*** ), "System.Guid***" )]
        public void ToCSharpName_handles_pointers( Type type, string expected )
        {
            type.ToCSharpName().Should().Be( expected );
        }

        [Test]
        public void ToCSharpName_handles_byRef()
        {
            typeof( long ).MakeByRefType().ToCSharpName().Should().Be( "long&" );
            typeof( List<int> ).MakeByRefType().ToCSharpName().Should().Be( "System.Collections.Generic.List<int>&" );
            typeof( long ).MakeByRefType().ToCSharpName().Should().Be( "long&" );
        }

        [TestCase( typeof( Dictionary<,>.KeyCollection ), "System.Collections.Generic.Dictionary<TKey,TValue>.KeyCollection" )]
        [TestCase( typeof( Dictionary<int, string>.KeyCollection ), "System.Collections.Generic.Dictionary<int,string>.KeyCollection" )]
        [TestCase( typeof( Above<>.Below<> ), "CK.Core.Tests.TypeExtensionTests.Above<T1>.Below<T1>" )]
        [TestCase( typeof( Nullable<> ), "System.Nullable<T>" )]
        [TestCase( typeof( G<> ), "CK.Core.Tests.TypeExtensionTests.G<T>" )]
        [TestCase( typeof( H<,> ), "CK.Core.Tests.TypeExtensionTests.H<T1,T2>" )]
        [TestCase( typeof( Another.I<> ), "CK.Core.Tests.TypeExtensionTests.Another.I<T3>" )]
        [TestCase( typeof( G<>.Nested ), "CK.Core.Tests.TypeExtensionTests.G<T>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.Core.Tests.TypeExtensionTests.A<TB>.C<TD>" )]
        [TestCase( typeof( int[] ), "int[]" )]
        [TestCase( typeof( Byte[,,,] ), "byte[,,,]" )]
        [TestCase( typeof( int? ), "int?" )]
        [TestCase( typeof( int?[] ), "int?[]" )]
        public void ToCSharpName_with_default_parameters( Type type, string expected )
        {
            type.ToCSharpName().Should().Be( expected );
        }

        // Cannot use TestCase parameters with tuple strings: parentheses trigger an error that prevents the test to run.
        //
        // An exception occurred while invoking executor 'executor://nunit3testexecutor/': Incorrect format for TestCaseFilter Error:
        // Missing '('. Specify the correct format and try again. Note that the incorrect format can lead to no test getting executed.
        //
        [TestCase( true )]
        [TestCase( false )]
        public void ToCSharpName_for_value_tuples( bool useValueTupleParentheses )
        {
            typeof( (int, string) ).ToCSharpName( useValueTupleParentheses: useValueTupleParentheses )
                .Should().Be( useValueTupleParentheses ? "(int,string)" : "System.ValueTuple<int,string>" );

            typeof( (int, (string, float)) ).ToCSharpName( useValueTupleParentheses: useValueTupleParentheses )
                .Should().Be( useValueTupleParentheses ? "(int,(string,float))" : "System.ValueTuple<int,System.ValueTuple<string,float>>" );

            // 8 Slots: the rest must be handled.
            // Note the trailing ValueTuple<T> (singleton) that is chained.
            typeof( (int, string, float, int, string, float, int, float) ).ToCSharpName( useValueTupleParentheses: useValueTupleParentheses )
                .Should().Be( useValueTupleParentheses
                                ? "(int,string,float,int,string,float,int,float)"
                                : "System.ValueTuple<int,string,float,int,string,float,int,System.ValueTuple<float>>" );
            // And more...
            typeof( (int, string, float, int, string, float, int, string, double, float) ).ToCSharpName( useValueTupleParentheses: useValueTupleParentheses )
                .Should().Be( useValueTupleParentheses
                                ? "(int,string,float,int,string,float,int,string,double,float)"
                                : "System.ValueTuple<int,string,float,int,string,float,int,System.ValueTuple<string,double,float>>" );
        }
    }
}
