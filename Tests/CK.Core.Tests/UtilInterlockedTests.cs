using FluentAssertions;
using System;
using NUnit.Framework;

namespace CK.Core.Tests
{

    public class UtilInterlockedTests
    {
        [Test]
        public void InterlockedAdd_atomically_adds_an_item_to_an_array()
        {
            int[] a = Array.Empty<int>();
            Util.InterlockedAdd(ref a, 1);
            a.Should().NotBeNull();
            a.Should().BeEquivalentTo(new[] { 1 }, o => o.WithStrictOrdering());
            Util.InterlockedAdd(ref a, 2);
            a.Should().BeEquivalentTo(new[] { 1, 2 }, o => o.WithStrictOrdering());
            Util.InterlockedAdd(ref a, 3);
            a.Should().BeEquivalentTo(new[] { 1, 2, 3 }, o => o.WithStrictOrdering());
        }

        [Test]
        public void InterlockedAdd_can_add_an_item_in_front_of_an_array()
        {
            int[] a = Array.Empty<int>();
            Util.InterlockedAdd(ref a, 1, true);
            a.Should().NotBeNull();
            a.Should().BeEquivalentTo(new[] { 1 }, o => o.WithStrictOrdering());
            Util.InterlockedAdd(ref a, 2, true);
            a.Should().BeEquivalentTo(new[] { 2, 1 }, o => o.WithStrictOrdering());
            Util.InterlockedAdd(ref a, 3, true);
            a.Should().BeEquivalentTo(new[] { 3, 2, 1 }, o => o.WithStrictOrdering());
        }

        [Test]
        public void InterlockedAddUnique_tests_the_occurrence_of_the_item()
        {
            {
                // Prepend
                int[] a = Array.Empty<int>();
                Util.InterlockedAddUnique(ref a, 1, true);
                a.Should().BeEquivalentTo(new[] { 1 }, o => o.WithStrictOrdering());
                var theA = a;
                Util.InterlockedAddUnique(ref a, 1, true);
                a.Should().BeSameAs(theA);
                Util.InterlockedAddUnique(ref a, 2, true);
                a.Should().BeEquivalentTo(new[] { 2, 1 }, o => o.WithStrictOrdering());
                theA = a;
                Util.InterlockedAddUnique(ref a, 2, true);
                a.Should().BeSameAs(theA);
            }
            {
                // Append
                int[] a = Array.Empty<int>();
                Util.InterlockedAddUnique(ref a, 1);
                a.Should().BeEquivalentTo(new[] { 1 }, o => o.WithStrictOrdering());
                var theA = a;
                Util.InterlockedAddUnique(ref a, 1);
                a.Should().BeSameAs(theA);
                Util.InterlockedAddUnique(ref a, 2);
                a.Should().BeEquivalentTo(new[] { 1, 2 }, o => o.WithStrictOrdering());
                theA = a;
                Util.InterlockedAddUnique(ref a, 2);
                a.Should().BeSameAs(theA);
            }
        }

        [Test]
        public void InterlockedRemove_an_item_from_an_array()
        {
            int[] a = new[] { 1, 2, 3, 4, 5, 6, 7 };
            Util.InterlockedRemove(ref a, 1);
            a.Should().BeEquivalentTo(new[] { 2, 3, 4, 5, 6, 7 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 4);
            a.Should().BeEquivalentTo(new[] { 2, 3, 5, 6, 7 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 3712);
            a.Should().BeEquivalentTo(new[] { 2, 3, 5, 6, 7 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 7);
            a.Should().BeEquivalentTo(new[] { 2, 3, 5, 6 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 3);
            a.Should().BeEquivalentTo(new[] { 2, 5, 6 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 5);
            a.Should().BeEquivalentTo(new[] { 2, 6 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 3712);
            a.Should().BeEquivalentTo(new[] { 2, 6 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 6);
            a.Should().BeEquivalentTo(new[] { 2 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, 2);
            a.Should().BeEmpty();

            var aEmpty = a;
            Util.InterlockedRemove(ref a, 2);
            a.Should().BeSameAs(aEmpty);

            Util.InterlockedRemove(ref a, 3712);
            a.Should().BeSameAs(aEmpty);
        }

        [Test]
        public void InterlockedRemoveAll_items_that_match_a_condition()
        {
            int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Util.InterlockedRemoveAll(ref a, i => i % 2 == 0);
            a.Should().BeEquivalentTo(new[] { 1, 3, 5, 7, 9 }, o => o.WithStrictOrdering());
            Util.InterlockedRemoveAll(ref a, i => i % 2 != 0);
            a.Should().BeEmpty();

            Util.InterlockedRemoveAll(ref a, i => i % 2 != 0);
            a.Should().BeEmpty();
        }

        [Test]
        public void InterlockedRemove_removes_the_first_item_that_matches_a_condition()
        {
            int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Util.InterlockedRemove(ref a, i => i % 2 == 0);
            a.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, o => o.WithStrictOrdering());
            Util.InterlockedRemove(ref a, i => i > 7);
            a.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 9 }, o => o.WithStrictOrdering());
        }

        [Test]
        public void InterlockedAdd_item_under_condition()
        {
            int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var theA = a;
            Util.InterlockedAdd(ref a, i => i == 3, () => 3);
            a.Should().BeSameAs(theA);

            Util.InterlockedAdd(ref a, i => i == 10, () => 10);
            a.Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, o => o.WithStrictOrdering());

            Util.InterlockedAdd(ref a, i => i == -1, () => -1, true);
            a.Should().BeEquivalentTo(new[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, o => o.WithStrictOrdering());

            Action call = () => Util.InterlockedAdd(ref a, i => i == 11, () => 10);
            call.Should().Throw<InvalidOperationException>();

            a = Array.Empty<int>();
            Util.InterlockedAdd(ref a, i => i == 3, () => 3);
            a.Should().BeEquivalentTo(new[] { 3 }, o => o.WithStrictOrdering());
            Util.InterlockedAdd(ref a, i => i == 4, () => 4);
            a.Should().BeEquivalentTo(new[] { 3, 4 }, o => o.WithStrictOrdering());

            a = new int[0];
            Util.InterlockedAdd(ref a, i => i == 3, () => 3);
            a.Should().BeEquivalentTo(new[] { 3 }, o => o.WithStrictOrdering());
            Util.InterlockedAdd(ref a, i => i == 4, () => 4);
            a.Should().BeEquivalentTo(new[] { 3, 4 }, o => o.WithStrictOrdering());
        }
    }
}
