using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using CK.Core.Tests;

// This is outside the CK.Core namespace to avoid extension method ambiguity resolution by the closest namespace.
[TestFixture]
public class CollectionAmbiguityExtensionTests
{
    public void Dictionary_GetValueOrDefault_is_not_ambiguous_because_it_is_defined_on_IDictionary_and_Dictionary()
    {
        var e = new Dictionary<string, int>();
        var x = e.GetValueOrDefault( "a" );
        var y = ((IDictionary<string, int>)e).GetValueOrDefault( "a" );
        var z = ((IReadOnlyDictionary<string, int>)e).GetValueOrDefault( "a" );
    }

    [Test]
    public void AsIReadOnlyDictionary_is_not_ambiguous_because_it_is_defined_on_IDictionary_and_Dictionary()
    {
        var puce = new Canidae( "Puce" );
        var e = new Dictionary<string, Canidae>() { { "TooCute", puce } };
        var x = e.AsIReadOnlyDictionary<string, Canidae, Animal>();
        var y = ((IDictionary<string, Canidae>)e).AsIReadOnlyDictionary<string, Canidae, Animal>();
        var z = ((IReadOnlyDictionary<string, Canidae>)e).AsIReadOnlyDictionary<string, Canidae, Animal>();

        e.GetEnumerator().Should().BeOfType<Dictionary<string, Canidae>.Enumerator>();
        x["TooCute"].Should().BeSameAs( puce );
        y["TooCute"].Should().BeSameAs( puce );
        z["TooCute"].Should().BeSameAs( puce );
        x.GetEnumerator().Should().NotBeOfType<Dictionary<string, Canidae>.Enumerator>();
        y.GetEnumerator().Should().NotBeOfType<Dictionary<string, Canidae>.Enumerator>();
        z.GetEnumerator().Should().NotBeOfType<Dictionary<string, Canidae>.Enumerator>();
    }
}

#pragma warning disable IDE0161 // Convert to file-scoped namespace

namespace CK.Core.Tests
{
    public class CollectionExtensionTests
    {
        [Test]
        public void testing_RemoveWhereAndReturnsRemoved_extension_method()
        {
            {
                List<int> l = new List<int>();
                l.AddRangeArray( 12, 15, 12, 13, 14 );
                var r = l.RemoveWhereAndReturnsRemoved( x => x == 12 );
                l.Count.Should().Be( 5 );
                r.Count().Should().Be( 2 );
                l.Count.Should().Be( 3 );
            }
        }


        [Test]
        public void CKEnumeratorMono_works_and_throws_InvalidOperationException()
        {
            var e = new CKEnumeratorMono<int>( 9 );
            Action a = () => Console.WriteLine( e.Current );
            a.Should().Throw<InvalidOperationException>();
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be( 9 );
            e.MoveNext().Should().BeFalse();
            a = () => Console.WriteLine( e.Current );
            a.Should().Throw<InvalidOperationException>();
            e.Reset();
            a = () => Console.WriteLine( e.Current );
            a.Should().Throw<InvalidOperationException>();
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be( 9 );
            e.MoveNext().Should().BeFalse();
            a = () => Console.WriteLine( e.Current );
            a.Should().Throw<InvalidOperationException>();
        }


        [Test]
        public void Dictionary_GetValueOrDefault_is_not_ambiguous()
        {
            var e = new Dictionary<string, int>();
            var x = e.GetValueOrDefault( "a" );
            var y = ((IDictionary<string, int>)e).GetValueOrDefault( "a" );
            var z = ((IReadOnlyDictionary<string, int>)e).GetValueOrDefault( "a" );
        }
    }
}
