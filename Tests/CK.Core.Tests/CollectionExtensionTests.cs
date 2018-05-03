using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    public class CollectionExtensionTests
    {
        [Test]
        public void testing_RemoveWhereAndReturnsRemoved_extension_method()
        {
            {
                List<int> l = new List<int>();
                l.AddRangeArray(12, 15, 12, 13, 14);
                var r = l.RemoveWhereAndReturnsRemoved(x => x == 12);
                l.Count.Should().Be(5);
                r.Count().Should().Be(2);
                l.Count.Should().Be(3);
            }
        }


        [Test]
        public void CKEnumeratorMono_works()
        {
            var e = new CKEnumeratorMono<int>(9);
            Action a = () => Console.WriteLine(e.Current);
            a.Should().Throw<InvalidOperationException>();
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(9);
            e.MoveNext().Should().BeFalse();
            a = () => Console.WriteLine(e.Current);
            a.Should().Throw<InvalidOperationException>();
            e.Reset();
            a = () => Console.WriteLine(e.Current);
            a.Should().Throw<InvalidOperationException>();
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(9);
            e.MoveNext().Should().BeFalse();
            a = () => Console.WriteLine(e.Current);
            a.Should().Throw<InvalidOperationException>();
        }
    }
}
