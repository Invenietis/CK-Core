using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Xunit;

namespace CK.Core.Tests
{

    public class EnumerableExtensionTests
    {
        [Fact]
        public void test_IsSortedStrict_and_IsSortedLarge_extension_methods()
        {
            List<int> listWithDuplicate = new List<int>();
            listWithDuplicate.AddRangeArray<int>(1, 2, 2, 3, 3, 5);

            List<int> listWithoutDuplicate = new List<int>();
            listWithoutDuplicate.AddRangeArray<int>(1, 2, 3, 5);

            listWithDuplicate.IsSortedStrict().Should().BeFalse();
            listWithDuplicate.IsSortedLarge().Should().BeTrue();
            Should.Throw<ArgumentNullException>(() => listWithDuplicate.IsSortedLarge(null));
            Should.Throw<ArgumentNullException>(() => listWithDuplicate.IsSortedStrict(null));

            listWithoutDuplicate.IsSortedStrict().Should().BeTrue();
            listWithoutDuplicate.IsSortedLarge().Should().BeTrue();
            Should.Throw<ArgumentNullException>(() => listWithoutDuplicate.IsSortedLarge(null));
            Should.Throw<ArgumentNullException>(() => listWithoutDuplicate.IsSortedStrict(null));

            listWithDuplicate.Reverse(0, listWithDuplicate.Count);
            listWithoutDuplicate.Reverse(0, listWithoutDuplicate.Count);

            listWithDuplicate.IsSortedStrict().Should().BeFalse();
            listWithDuplicate.IsSortedLarge().Should().BeFalse();

            listWithoutDuplicate.IsSortedStrict().Should().BeFalse();
            listWithoutDuplicate.IsSortedLarge().Should().BeFalse();

            //test with 2 items
            listWithoutDuplicate = new List<int>();
            listWithoutDuplicate.AddRangeArray<int>(1, 5);

            listWithoutDuplicate.IsSortedStrict().Should().BeTrue();
            listWithoutDuplicate.IsSortedLarge().Should().BeTrue();


            listWithDuplicate = new List<int>();
            listWithDuplicate.AddRangeArray<int>(5, 5);

            listWithDuplicate.IsSortedStrict().Should().BeFalse();
            listWithDuplicate.IsSortedLarge().Should().BeTrue();

            listWithDuplicate.Reverse(0, listWithDuplicate.Count);
            listWithoutDuplicate.Reverse(0, listWithoutDuplicate.Count);

            listWithDuplicate.IsSortedStrict().Should().BeFalse();
            listWithDuplicate.IsSortedLarge().Should().BeTrue();

            listWithoutDuplicate.IsSortedStrict().Should().BeFalse();
            listWithoutDuplicate.IsSortedLarge().Should().BeFalse();

            //test with 1 items
            listWithoutDuplicate = new List<int>();
            listWithoutDuplicate.Add(1);

            listWithoutDuplicate.IsSortedStrict().Should().BeTrue();
            listWithoutDuplicate.IsSortedLarge().Should().BeTrue();

            //Empty test
            listWithoutDuplicate = new List<int>();

            listWithoutDuplicate.IsSortedStrict().Should().BeTrue();
            listWithoutDuplicate.IsSortedLarge().Should().BeTrue();

            listWithDuplicate = new List<int>();

            listWithoutDuplicate.IsSortedStrict().Should().BeTrue();
            listWithoutDuplicate.IsSortedLarge().Should().BeTrue();

            listWithDuplicate = null;
            Should.Throw<NullReferenceException>(() => listWithDuplicate.IsSortedLarge());
            Should.Throw<NullReferenceException>(() => listWithDuplicate.IsSortedStrict());
        }

        [Fact]
        public void test_IndexOf_extension_method()
        {
            List<int> listToTest = new List<int>();
            listToTest.AddRangeArray<int>(1, 2);

            listToTest.IndexOf(a => a == 0).Should().Be(-1);
            listToTest.IndexOf(a => a == 1).Should().Be(0);
            listToTest.IndexOf(a => a == 2).Should().Be(1);
            listToTest.IndexOf((a, b) => b == listToTest.IndexOf<int>(c => c == 0) && a == 0).Should().Be(-1);
            listToTest.IndexOf((a, b) => b == listToTest.IndexOf<int>(c => c == 1) && a == 1).Should().Be(0);
            listToTest.IndexOf((a, b) => b == listToTest.IndexOf<int>(c => c == 2) && a == 2).Should().Be(1);

            listToTest.Add(2);

            listToTest.IndexOf(a => a == 2).Should().Be(1);
            listToTest.IndexOf((a, idx) => idx == 2 && a == 2).Should().Be(2);

            Func<int, bool> nullFunc = null;
            Func<int, int, bool> nullFuncWithIndex = null;
            Should.Throw<ArgumentNullException>(() => listToTest.IndexOf(nullFunc));
            Should.Throw<ArgumentNullException>(() => listToTest.IndexOf(nullFuncWithIndex));
            listToTest = null;
            Should.Throw<NullReferenceException>(() => listToTest.IndexOf(a => a == 0));
            Should.Throw<NullReferenceException>(() => listToTest.IndexOf((a, idx) => a == 0));
        }

        // See: https://github.com/dotnet/corefx/issues/15716
        [Fact]
        public void buggy_behavior_of_Append_extension_method()
        {
            {
                int[] t = new int[0];
                var e = t.GetEnumerator();
                Should.Throw<InvalidOperationException>(() => Console.Write(e.Current));
            }
            {
                int[] tWithItems = new int[] { 7 };
                var e = tWithItems.GetEnumerator();
                Should.Throw<InvalidOperationException>(() => Console.Write(e.Current));
            }

            {
                int[] t = new int[0];
                var e = t.Append( 3712 ).GetEnumerator();
                // This fails: e.Current is 0, the default(int)...
                //Should.Throw<InvalidOperationException>(() => Console.Write(e.Current));
            }
        }

        [Fact]
        public void test_Append_extension_method()
        {
            int[] t = new int[0];
            t.Append(5).Should().BeEquivalentTo(5);
            t.Append(2).Append(5).Should().BeEquivalentTo(2, 5);
            t.Append(2).Append(3).Append(5).Should().BeEquivalentTo(2, 3, 5);

            var tX = t.Append(2).Append(3).Append(4).Append(5);
            tX.Should().BeEquivalentTo(2, 3, 4, 5);

            var e = tX.GetEnumerator();
#if !NETCOREAPP1_0
            // See: https://github.com/dotnet/corefx/issues/15716
            Should.Throw<InvalidOperationException>(() => Console.Write(e.Current));
#endif
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(2);
#if !NETCOREAPP1_0
            e.Reset();
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(2);
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(3);
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(4);
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(5);
            e.MoveNext().Should().BeFalse();
            Should.Throw<InvalidOperationException>(() => Console.Write(e.Current));
            Should.Throw<InvalidOperationException>(() => e.MoveNext());
            t = null;
            Should.Throw<NullReferenceException>(() => t.Append(5));
#endif

        }

        [Fact]
        public void MaxBy_throws_InvalidOperationException_on_empty_sequence()
        {
            Should.Throw<InvalidOperationException>(() => new int[0].MaxBy(i => -i));
            Should.Throw<InvalidOperationException>(() => new int[0].MaxBy(i => -i, null));
        }

        [Fact]
        public void test_MaxBy_extension_method()
        {
            int[] t = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            t.MaxBy(Util.FuncIdentity).Should().Be(12);
            t.MaxBy(i => -i).Should().Be(0);
            t.MaxBy(i => (i + 1) % 6 == 0).Should().Be(5);
            t.MaxBy(i => i.ToString()).Should().Be(9, "Lexicographical ordering.");

            t.MaxBy(i => i, (x, y) => x - y).Should().Be(12);

            Should.Throw<ArgumentNullException>(() => t.MaxBy<int, int>(null));
            t = null;
            Should.Throw<NullReferenceException>(() => t.MaxBy(Util.FuncIdentity));
        }
    }
}
