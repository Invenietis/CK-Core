using Shouldly;
using System;
using NUnit.Framework;

namespace CK.Core.Tests;


public class UtilInterlockedTests
{
    [Test]
    public void InterlockedAdd_atomically_adds_an_item_to_an_array()
    {
        int[] a = Array.Empty<int>();
        Util.InterlockedAdd( ref a, 1 );
        a.ShouldNotBeNull();
        a.ShouldBe( new[] { 1 } );
        Util.InterlockedAdd( ref a, 2 );
        a.ShouldBe( new[] { 1, 2 } );
        Util.InterlockedAdd( ref a, 3 );
        a.ShouldBe( new[] { 1, 2, 3 } );
    }

    [Test]
    public void InterlockedAdd_can_add_an_item_in_front_of_an_array()
    {
        int[] a = Array.Empty<int>();
        Util.InterlockedAdd( ref a, 1, true );
        a.ShouldNotBeNull();
        a.ShouldBe( new[] { 1 } );
        Util.InterlockedAdd( ref a, 2, true );
        a.ShouldBe( new[] { 2, 1 } );
        Util.InterlockedAdd( ref a, 3, true );
        a.ShouldBe( new[] { 3, 2, 1 } );
    }

    [Test]
    public void InterlockedAddUnique_tests_the_occurrence_of_the_item()
    {
        {
            // Prepend
            int[] a = Array.Empty<int>();
            Util.InterlockedAddUnique( ref a, 1, true );
            a.ShouldBe( new[] { 1 } );
            var theA = a;
            Util.InterlockedAddUnique( ref a, 1, true );
            a.ShouldBeSameAs( theA );
            Util.InterlockedAddUnique( ref a, 2, true );
            a.ShouldBe( new[] { 2, 1 } );
            theA = a;
            Util.InterlockedAddUnique( ref a, 2, true );
            a.ShouldBeSameAs( theA );
        }
        {
            // Append
            int[] a = Array.Empty<int>();
            Util.InterlockedAddUnique( ref a, 1 );
            a.ShouldBe( new[] { 1 } );
            var theA = a;
            Util.InterlockedAddUnique( ref a, 1 );
            a.ShouldBeSameAs( theA );
            Util.InterlockedAddUnique( ref a, 2 );
            a.ShouldBe( new[] { 1, 2 } );
            theA = a;
            Util.InterlockedAddUnique( ref a, 2 );
            a.ShouldBeSameAs( theA );
        }
    }

    [Test]
    public void InterlockedRemove_an_item_from_an_array()
    {
        int[] a = new[] { 1, 2, 3, 4, 5, 6, 7 };
        Util.InterlockedRemove( ref a, 1 );
        a.ShouldBe( new[] { 2, 3, 4, 5, 6, 7 } );
        Util.InterlockedRemove( ref a, 4 );
        a.ShouldBe( new[] { 2, 3, 5, 6, 7 } );
        Util.InterlockedRemove( ref a, 3712 );
        a.ShouldBe( new[] { 2, 3, 5, 6, 7 } );
        Util.InterlockedRemove( ref a, 7 );
        a.ShouldBe( new[] { 2, 3, 5, 6 } );
        Util.InterlockedRemove( ref a, 3 );
        a.ShouldBe( new[] { 2, 5, 6 } );
        Util.InterlockedRemove( ref a, 5 );
        a.ShouldBe( new[] { 2, 6 } );
        Util.InterlockedRemove( ref a, 3712 );
        a.ShouldBe( new[] { 2, 6 } );
        Util.InterlockedRemove( ref a, 6 );
        a.ShouldBe( new[] { 2 } );
        Util.InterlockedRemove( ref a, 2 );
        a.ShouldBeEmpty();

        var aEmpty = a;
        Util.InterlockedRemove( ref a, 2 );
        a.ShouldBeSameAs( aEmpty );

        Util.InterlockedRemove( ref a, 3712 );
        a.ShouldBeSameAs( aEmpty );
    }

    [Test]
    public void InterlockedRemoveAll_items_that_match_a_condition()
    {
        int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Util.InterlockedRemoveAll( ref a, i => i % 2 == 0 );
        a.ShouldBe( new[] { 1, 3, 5, 7, 9 } );
        Util.InterlockedRemoveAll( ref a, i => i % 2 != 0 );
        a.ShouldBeEmpty();

        Util.InterlockedRemoveAll( ref a, i => i % 2 != 0 );
        a.ShouldBeEmpty();
    }

    [Test]
    public void InterlockedRemove_removes_the_first_item_that_matches_a_condition()
    {
        int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Util.InterlockedRemove( ref a, i => i % 2 == 0 );
        a.ShouldBe( new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 } );
        Util.InterlockedRemove( ref a, i => i > 7 );
        a.ShouldBe( new[] { 1, 2, 3, 4, 5, 6, 7, 9 } );
    }

    [Test]
    public void InterlockedAdd_item_under_condition()
    {
        int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var theA = a;
        Util.InterlockedAdd( ref a, i => i == 3, () => 3 );
        a.ShouldBeSameAs( theA );

        Util.InterlockedAdd( ref a, i => i == 10, () => 10 );
        a.ShouldBe( new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } );

        Util.InterlockedAdd( ref a, i => i == -1, () => -1, true );
        a.ShouldBe( new[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } );

        Action call = () => Util.InterlockedAdd( ref a, i => i == 11, () => 10 );
        call.ShouldThrow<InvalidOperationException>();

        a = Array.Empty<int>();
        Util.InterlockedAdd( ref a, i => i == 3, () => 3 );
        a.ShouldBe( new[] { 3 } );
        Util.InterlockedAdd( ref a, i => i == 4, () => 4 );
        a.ShouldBe( new[] { 3, 4 } );

        a = new int[0];
        Util.InterlockedAdd( ref a, i => i == 3, () => 3 );
        a.ShouldBe( new[] { 3 } );
        Util.InterlockedAdd( ref a, i => i == 4, () => 4 );
        a.ShouldBe( new[] { 3, 4 } );
    }
}
