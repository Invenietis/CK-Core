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
            {
                // Removes from and add in the same list!
                List<int> l = new List<int>();
                l.AddRangeArray(12, 15, 12, 13, 14, 12);
                Should.Throw<ArgumentOutOfRangeException>(() => l.AddRange(l.RemoveWhereAndReturnsRemoved(x => x == 12)));
            }
        }


        [Test]
        public void CKEnumeratorMono_works()
        {
            var e = new CKEnumeratorMono<int>(9);
            Should.Throw<InvalidOperationException>(() => Console.WriteLine(e.Current));
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(9);
            e.MoveNext().Should().BeFalse();
            Should.Throw<InvalidOperationException>(() => Console.WriteLine(e.Current));
            e.Reset();
            Should.Throw<InvalidOperationException>(() => Console.WriteLine(e.Current));
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(9);
            e.MoveNext().Should().BeFalse();
            Should.Throw<InvalidOperationException>(() => Console.WriteLine(e.Current));
        }
    }
}
