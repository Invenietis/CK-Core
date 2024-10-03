using FluentAssertions;
using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace CK.Core.Tests;

public class FIFOTests
{
    #region Helpers

    /// <summary>
    /// Applies Capacity+1 times the same test to the same buffer "logical" content but with 
    /// different internal offsets each time. 
    /// The "logical" initial buffer content is restored each time and at the end of the test.
    /// </summary>
    static void TestWithInternalOffsets<T>( FIFOBuffer<T> f, Action<FIFOBuffer<T>> testPredicate )
    {
        var saved = f.ToArray();
        for( int iTry = 0; iTry <= f.Capacity; ++iTry )
        {
            f.Clear();
            for( int i = 0; i < iTry; ++i ) f.Push( default! );
            foreach( var i in saved ) f.Push( i );
            while( f.Count > saved.Length ) f.Pop();
            f.SequenceEqual( saved ).Should().BeTrue();
            testPredicate( f );
        }
        foreach( var i in saved ) f.Push( i );
        f.Truncate( saved.Length );
    }

    /// <summary>
    /// Applies Capacity times TestWithInternalOffsets.
    /// Can be used only on tests that do not rely on the capacity limit (typically for FIFORemoveAt test).
    /// </summary>
    static void TestWithInternalOffsetsAndGrowingCapacity<T>( FIFOBuffer<T> f, Action<FIFOBuffer<T>> testPredicate )
    {
        int c = f.Capacity;
        for( int i = 0; i < c; ++i )
        {
            TestWithInternalOffsets( f, testPredicate );
            f.Capacity++;
        }
        f.Capacity = c;
    }

    static void AssertContains<T>( FIFOBuffer<T> f, params T[] values )
    {
        f.Count.Should().Be( values.Length );
        f.SequenceEqual( values ).Should().BeTrue();
        f.ToArray().SequenceEqual( values ).Should().BeTrue();
    }

    static void AssertEmpty<T>( FIFOBuffer<T> f )
    {
        f.Should().BeEmpty();
        AssertContains( f );
    }

    static void CheckOneValueType<T>( FIFOBuffer<T> f, T value, T otherValue ) where T : struct
    {
        CheckOnlyOneValueType<T>( f, value, otherValue );
        f.Pop().Should().Be( value );
        AssertEmpty( f );
        f.Push( value );
        CheckOnlyOneValueType<T>( f, value, otherValue );
    }

    static void CheckOnlyOneValueType<T>( FIFOBuffer<T> f, T value, T otherValue ) where T : struct
    {
        f[0].Should().Be( value );
        f.Contains( value ).Should().BeTrue();
        f.Contains( otherValue ).Should().BeFalse();
        f.Contains( null! ).Should().BeFalse();
        f.SequenceEqual( new[] { value } ).Should().BeTrue();
    }

    #endregion

    [Test]
    public void FIFO_ToArray_method()
    {
        int[] initialArray = new int[7];
        initialArray[0] = initialArray[6] = -1;
        FIFOBuffer<int> f = new FIFOBuffer<int>( 5 );
        f.ToArray().Length.Should().Be( 0 );

        TestWithInternalOffsets( f, b =>
            {
                var array = (int[])initialArray.Clone();
                b.Push( 1 );
                b.CopyTo( array.AsSpan( 3, 2 ) );
                array.SequenceEqual( new int[] { -1, 0, 0, 1, 0, 0, -1 } ).Should().BeTrue();
                array[3] = 0;
                b.Push( 2 );
                b.CopyTo( array.AsSpan( 3, 2 ) );
                array.SequenceEqual( new int[] { -1, 0, 0, 1, 2, 0, -1 } ).Should().BeTrue();

                array[3] = 0; array[4] = 0;
                b.Push( 3 );
                b.CopyTo( array.AsSpan( 3, 3 ) );
                array.SequenceEqual( new int[] { -1, 0, 0, 1, 2, 3, -1 } ).Should().BeTrue();

                array[3] = 0; array[4] = 0; array[5] = 0;
                b.Push( 4 );
                b.CopyTo( array.AsSpan( 3, 3 ) );
                array.SequenceEqual( new int[] { -1, 0, 0, 2, 3, 4, -1 } ).Should().BeTrue();

                array[3] = 0; array[4] = 0; array[5] = 0;
                b.CopyTo( array.AsSpan( 2, 4 ) );
                array.SequenceEqual( new int[] { -1, 0, 1, 2, 3, 4, -1 } ).Should().BeTrue();

                array[3] = 0; array[4] = 0; array[5] = 0;
                b.CopyTo( array.AsSpan( 2, 5 ) ).Should().Be( 4 );
                array.SequenceEqual( new int[] { -1, 0, 1, 2, 3, 4, -1 } ).Should().BeTrue( "Sentinel is not changed: there is only 4 items to copy." );

                b.Invoking( sut => sut.CopyTo( array.AsSpan( 2, 6 ) ) ).Should().Throw<ArgumentOutOfRangeException>( "Even if the items fit, there must be an exception." );

                b.Truncate( 1 );
                b.Peek().Should().Be( 4 );
                b.Push( 60 );
                b.Push( 61 );
                b.Push( 62 );
                b.Push( 63 );
                b.Push( 7 ); // oldest
                b.Push( 8 );
                b.Push( 9 );
                b.Push( 10 );
                b.Push( 11 );
                b[0].Should().Be( 7 );

                array[3] = 0; array[4] = 0; array[5] = 0;
                b.CopyTo( array.AsSpan( 1 ) ).Should().Be( 5 );
                array.SequenceEqual( new int[] { -1, 7, 8, 9, 10, 11, -1 } ).Should().BeTrue( "Sentinel is not changed: there is only 5 items to copy." );

                array[5] = 0;
                b.CopyTo( array.AsSpan() ).Should().Be( 5 );
                array.SequenceEqual( new int[] { 7, 8, 9, 10, 11, 0, -1 } ).Should().BeTrue();

                b.CopyTo( array.AsSpan( 5 ) ).Should().Be( 2 );
                array.SequenceEqual( new int[] { 7, 8, 9, 10, 11, 10, 11 } ).Should().BeTrue();
            } );
    }

    [Test]
    public void FIFO_change_capacity_preserves_items()
    {
        FIFOBuffer<int> f = new FIFOBuffer<int>( 0 );
        f.Capacity.Should().Be( 0 );
        AssertEmpty( f );
        f.Push( 5 );
        AssertEmpty( f );
        f.Push( 12 );
        AssertEmpty( f );

        f.Capacity = 1;
        f.Capacity.Should().Be( 1 );
        AssertEmpty( f );
        f.Push( 5 );
        AssertContains( f, 5 );
        f.Push( 6 );
        AssertContains( f, 6 );

        f.Capacity = 2;
        f.Capacity.Should().Be( 2 );
        AssertContains( f, 6 );
        f.Push( 7 );
        AssertContains( f, 6, 7 );
        f.Push( 8 );
        AssertContains( f, 7, 8 );

        f.Capacity = 4;
        f.Capacity.Should().Be( 4 );
        AssertContains( f, 7, 8 );
        f.Push( 9 );
        AssertContains( f, 7, 8, 9 );
        f.Push( 10 );
        AssertContains( f, 7, 8, 9, 10 );
        f.Push( 11 );
        AssertContains( f, 8, 9, 10, 11 );

        f.Capacity = 7;
        f.Capacity.Should().Be( 7 );
        AssertContains( f, 8, 9, 10, 11 );
        f.Push( 12 );
        AssertContains( f, 8, 9, 10, 11, 12 );
        f.Push( 13 );
        AssertContains( f, 8, 9, 10, 11, 12, 13 );
        f.Push( 14 );
        AssertContains( f, 8, 9, 10, 11, 12, 13, 14 );
        f.Push( 15 );
        AssertContains( f, 9, 10, 11, 12, 13, 14, 15 );

        f.Capacity = 2;
        f.Capacity.Should().Be( 2 );
        AssertContains( f, 14, 15 );

        f.Capacity = 3;
        f.Capacity.Should().Be( 3 );
        AssertContains( f, 14, 15 );
        f.Push( 16 );
        AssertContains( f, 14, 15, 16 );

        f.Capacity = 2;
        f.Capacity.Should().Be( 2 );
        AssertContains( f, 15, 16 );

        f.Capacity = 1;
        f.Capacity.Should().Be( 1 );
        AssertContains( f, 16 );

        f.Capacity = 0;
        f.Capacity.Should().Be( 0 );
        AssertEmpty( f );

        f.Capacity = 2;
        f.Capacity = 2;
        f.Capacity.Should().Be( 2 );

        f.ToString().Should().Be( String.Format( "Count = {0} (Capacity = {1})", 0, 2 ) );

        //ExceptionTest
        f.Invoking( sut => sut.Capacity = -1 ).Should().Throw<ArgumentException>();
        f.Invoking( sut => new FIFOBuffer<int>( -1 ) ).Should().Throw<ArgumentException>();
    }

    [Test]
    public void FIFO_supports_removeAt()
    {
        FIFOBuffer<int> f = new FIFOBuffer<int>( 0 );
        f.Invoking( sut => sut.RemoveAt( 0 ) ).Should().Throw<ArgumentOutOfRangeException>();
        f.Invoking( sut => sut.RemoveAt( -1 ) ).Should().Throw<ArgumentOutOfRangeException>();

        f.Capacity = 1;
        f.Push( 1 );
        TestWithInternalOffsetsAndGrowingCapacity( f, b =>
            {
                AssertContains( b, 1 );
                b.RemoveAt( 0 );
                AssertEmpty( b );
            } );

        f.Capacity = 2;
        f.Push( 2 );
        TestWithInternalOffsetsAndGrowingCapacity( f, b =>
            {
                AssertContains( b, 1, 2 );
                b.RemoveAt( 0 );
                AssertContains( b, 2 );
            } );

        f.Capacity = 3;
        f.Push( 3 );
        f.Push( 4 );
        TestWithInternalOffsetsAndGrowingCapacity( f, b =>
            {
                AssertContains( b, 2, 3, 4 );
                b.RemoveAt( 2 );
                AssertContains( b, 2, 3 );
                b.RemoveAt( 1 );
                AssertContains( b, 2 );
                b.RemoveAt( 0 );
                AssertEmpty( b );
            } );

        f.Capacity = 4;
        f.Clear();
        f.Push( 2 );
        f.Push( 3 );
        f.Push( 4 );
        TestWithInternalOffsetsAndGrowingCapacity( f, b =>
        {
            AssertContains( b, 2, 3, 4 );
            b.RemoveAt( 2 );
            AssertContains( b, 2, 3 );
            b.RemoveAt( 1 );
            AssertContains( b, 2 );
        } );

        f.Push( 5 );
        TestWithInternalOffsetsAndGrowingCapacity( f, b =>
        {
            AssertContains( b, 2, 3, 4, 5 );
            b.RemoveAt( 2 );
            AssertContains( b, 2, 3, 5 );
            b.RemoveAt( 1 );
            AssertContains( b, 2, 5 );
            b.RemoveAt( 1 );
            AssertContains( b, 2 );
        } );

        f.Capacity = 5;
        f.Push( 6 );
        TestWithInternalOffsetsAndGrowingCapacity( f, b =>
        {
            AssertContains( b, 2, 3, 4, 5, 6 );
            b.RemoveAt( 2 );
            AssertContains( b, 2, 3, 5, 6 );
            b.RemoveAt( 1 );
            AssertContains( b, 2, 5, 6 );
            b.RemoveAt( 2 );
            AssertContains( b, 2, 5 );
            b.RemoveAt( 1 );
            AssertContains( b, 2 );
        } );

    }

    [Test]
    public void FIFO_supports_Peek_and_PeekLast()
    {
        FIFOBuffer<int> f = new FIFOBuffer<int>( 0 );
        f.Invoking( sut => Console.Write( sut[-1] ) ).Should().Throw<ArgumentOutOfRangeException>();
        f.Invoking( sut => Console.Write( sut[0] ) ).Should().Throw<ArgumentOutOfRangeException>();
        f.Invoking( sut => sut.Peek() ).Should().Throw<InvalidOperationException>();
        f.Invoking( sut => sut.PeekLast() ).Should().Throw<InvalidOperationException>();

        f.Push( 5 );
        f.Invoking( sut => Console.Write( sut[0] ) ).Should().Throw<ArgumentOutOfRangeException>();
        f.Invoking( sut => sut.Peek() ).Should().Throw<InvalidOperationException>();
        f.Invoking( sut => sut.PeekLast() ).Should().Throw<InvalidOperationException>();

        f.Capacity = 1;
        TestWithInternalOffsets( f, b =>
        {
            b.Push( 5 );
            b[0].Should().Be( 5 );
            b.Invoking( sut => Console.Write( sut[1] ) ).Should().Throw<ArgumentOutOfRangeException>();
            b.Peek().Should().Be( 5 );
            b.PeekLast().Should().Be( 5 );
            b.Push( 6 );
            b[0].Should().Be( 6, "Only one item in it." );
            b.Invoking( sut => Console.Write( sut[1] ) ).Should().Throw<ArgumentOutOfRangeException>();
            b.Peek().Should().Be( 6 );
            b.PeekLast().Should().Be( 6 );
        } );

        f.Clear();
        f.Invoking( sut => Console.Write( sut[0] ) ).Should().Throw<ArgumentOutOfRangeException>();
        f.Invoking( sut => Console.Write( sut[1] ) ).Should().Throw<ArgumentOutOfRangeException>();
        f.Invoking( sut => sut.Peek() ).Should().Throw<InvalidOperationException>();
        f.Invoking( sut => sut.PeekLast() ).Should().Throw<InvalidOperationException>();

        f.Capacity = 2;
        TestWithInternalOffsets( f, b =>
        {
            b.Push( 5 );
            b[0].Should().Be( 5 );
            b.Invoking( sut => Console.Write( sut[1] ) ).Should().Throw<ArgumentOutOfRangeException>();
            b.Peek().Should().Be( 5 );
            b.PeekLast().Should().Be( 5 );
            b.Push( 6 );
            b[0].Should().Be( 5 );
            b[1].Should().Be( 6 );
            b.Peek().Should().Be( 5 );
            b.PeekLast().Should().Be( 6 );
            b.Pop();
            b[0].Should().Be( 6 );
            b.Invoking( sut => Console.Write( sut[1] ) ).Should().Throw<ArgumentOutOfRangeException>();
            b.Peek().Should().Be( 6 );
            b.PeekLast().Should().Be( 6 );
            b.Pop();
            b.Invoking( sut => Console.Write( sut[0] ) ).Should().Throw<ArgumentOutOfRangeException>();
            b.Invoking( sut => Console.Write( sut[1] ) ).Should().Throw<ArgumentOutOfRangeException>();
            b.Invoking( sut => sut.Peek() ).Should().Throw<InvalidOperationException>();
            b.Invoking( sut => sut.PeekLast() ).Should().Throw<InvalidOperationException>();

            b.Push( 7 );
            b.Push( 8 );
            b.Push( 9 );
            b[0].Should().Be( 8 );
            b[1].Should().Be( 9 );
            b.ToArray().SequenceEqual( new int[] { 8, 9 } ).Should().BeTrue();
            b.Peek().Should().Be( 8 );
            b.PeekLast().Should().Be( 9 );
            b.Pop().Should().Be( 8 );
            b.Pop().Should().Be( 9 );
            AssertEmpty( b );

            b.Push( 10 );
            b.Push( 11 );
            b.Push( 12 );
            b[0].Should().Be( 11 );
            b[1].Should().Be( 12 );
            b.Peek().Should().Be( 11 );
            b.PeekLast().Should().Be( 12 );
            b.PopLast().Should().Be( 12 );
            b.Peek().Should().Be( 11 );
            b.PeekLast().Should().Be( 11 );
            b.PopLast().Should().Be( 11 );
            AssertEmpty( b );
        } );

        f.Capacity = 3;
        TestWithInternalOffsets( f, b =>
        {
            b.Push( 11 );
            b.Push( 12 );
            b.Push( 13 );
            b[0].Should().Be( 11 );
            b[1].Should().Be( 12 );
            b[2].Should().Be( 13 );
        } );


        f.Capacity = 4;
        f.Push( 11 );
        f.Push( 12 );
        f.Push( 13 );
        TestWithInternalOffsets( f, b =>
        {
            b.Push( 14 );
            b[0].Should().Be( 11 );
            b[1].Should().Be( 12 );
            b[2].Should().Be( 13 );
            b[3].Should().Be( 14 );
            b.Push( 15 );
            b[0].Should().Be( 12 );
            b[1].Should().Be( 13 );
            b[2].Should().Be( 14 );
            b[3].Should().Be( 15 );
            b.Push( 16 );
            b[0].Should().Be( 13 );
            b[1].Should().Be( 14 );
            b[2].Should().Be( 15 );
            b[3].Should().Be( 16 );
        } );

        f.Capacity = 5;
        AssertContains( f, 11, 12, 13 );
        TestWithInternalOffsets( f, b =>
        {
            b.Push( 14 );
            b.Push( 15 );
            b.Push( 16 );
            b.Push( 17 );
            b[0].Should().Be( 13 );
            b[1].Should().Be( 14 );
            b[2].Should().Be( 15 );
            b[3].Should().Be( 16 );
            b[4].Should().Be( 17 );
            f.Invoking( sut => Console.Write( sut[5] ) ).Should().Throw<ArgumentOutOfRangeException>();
        } );
    }
    [Test]
    public void FIFO_with_one_and_only_one_Value_Type()
    {
        FIFOBuffer<int> f = new FIFOBuffer<int>( 1 );
        AssertEmpty( f );

        f.Push( 5 );
        CheckOneValueType( f, 5, 50 );
        f.Pop();
        f.Push( 0 );
        CheckOneValueType( f, 0, 5 );
        f.Push( 1 );
        CheckOneValueType( f, 1, 0 );
        f.Push( 2 );
        CheckOneValueType( f, 2, 0 );

        int iType = f.IndexOf( 2 );
        int iBoxed = f.IndexOf( (object)2 );
        iType.Should().Be( iBoxed );
    }

    [Test]
    public void FIFO_can_be_dynamic_up_to_MaxDynamicCapacity()
    {
        var f = new FIFOBuffer<int>( 0 );
        AssertEmpty( f );
        f.Push( 1 );
        AssertEmpty( f );
        f.MaxDynamicCapacity = 1;
        f.Push( 1 );
        AssertContains( f, 1 );
        f.Capacity.Should().Be( 1, "Buffer has grown to its maximal capacity (and no more)." );
        f.Push( 2 );
        AssertContains( f, 2 );

        f.MaxDynamicCapacity = 5;
        f.Push( 3 );
        f.Capacity.Should().Be( 2, "Buffer size has been doubled." );
        AssertContains( f, 2, 3 );

        f.Push( 4 );
        f.Capacity.Should().Be( 4, "Buffer size has been doubled." );
        AssertContains( f, 2, 3, 4 );

        f.Push( 5 );
        f.Capacity.Should().Be( 4, "Buffer has not been resized." );
        AssertContains( f, 2, 3, 4, 5 );

        f.Push( 6 );
        f.Capacity.Should().Be( 5, "Buffer has grown to its maximal capacity (and no more)." );
        AssertContains( f, 2, 3, 4, 5, 6 );

        f.MaxDynamicCapacity = 2;
        f.Capacity.Should().Be( 2, "Buffer has been shrink." );
        AssertContains( f, 5, 6 );
    }

    [Test]
    public void PopLastRange_test()
    {
        TestWithDynamicCapacity( 14 );
        for( int i = 0; i < 100; i++ )
        {
            TestWithDynamicCapacity( 7 + Random.Shared.Next( 30 ) );
        }

        static void TestWithDynamicCapacity( int maxDynamicCapacity )
        {
            TestWithOffset( 0, maxDynamicCapacity );
            TestWithOffset( 1, maxDynamicCapacity );
            TestWithOffset( 2, maxDynamicCapacity );
            TestWithOffset( 3, maxDynamicCapacity );
            TestWithOffset( 4, maxDynamicCapacity );
            TestWithOffset( 5, maxDynamicCapacity );
            TestWithOffset( 6, maxDynamicCapacity );
            TestWithOffset( 7, maxDynamicCapacity );
            TestWithOffset( 8, maxDynamicCapacity );
            TestWithOffset( 9, maxDynamicCapacity );
            TestWithOffset( 10, maxDynamicCapacity );
            TestWithOffset( 11, maxDynamicCapacity );
            TestWithOffset( 12, maxDynamicCapacity );
            TestWithOffset( 13, maxDynamicCapacity );
            TestWithOffset( 14, maxDynamicCapacity );
        }

        static void TestWithOffset( int offset, int maxDynamicCapacity )
        {
            var f = new FIFOBuffer<int>( 0, maxDynamicCapacity );
            for( int i = 0; i < offset; i++ ) f.Push( -1 );
            for( int i = 0; i < 7; i++ ) f.Push( i );
            f.Count.Should().BeLessThanOrEqualTo( f.MaxDynamicCapacity );
            Span<int> items = stackalloc int[7];
            f.PopLastRange( default ).Should().Be( 0 );
            f.PopLastRange( items.Slice( 0, 1 ) ).Should().Be( 1 );
            items[0].Should().Be( 6 );
            f.PopLastRange( items.Slice( 1, 2 ) ).Should().Be( 2 );
            items[1].Should().Be( 4 );
            items[2].Should().Be( 5 );
            f.PopLastRange( items.Slice( 3, 3 ) ).Should().Be( 3 );
            items[3].Should().Be( 1 );
            items[4].Should().Be( 2 );
            items[5].Should().Be( 3 );
            f.PopLastRange( items.Slice( 6, 1 ) ).Should().Be( 1 );
            items[6].Should().Be( 0 );
            Span<int> padding = stackalloc int[offset];
            var maxOffset = f.MaxDynamicCapacity - 7;
            if( offset > maxOffset ) offset = maxOffset;
            f.PopLastRange( padding ).Should().Be( offset );
            for( int i = 0; i < offset; i++ ) padding[i].Should().Be( -1 );
            f.Count.Should().Be( 0 );
        }
    }

    [Test]
    public void PopRange_test()
    {
        TestWithDynamicCapacity( 14 );
        for( int i = 0; i < 100; i++ )
        {
            TestWithDynamicCapacity( 7 + Random.Shared.Next( 30 ) );
        }

        static void TestWithDynamicCapacity( int maxDynamicCapacity )
        {
            TestWithOffset( 0, maxDynamicCapacity );
            TestWithOffset( 1, maxDynamicCapacity );
            TestWithOffset( 2, maxDynamicCapacity );
            TestWithOffset( 3, maxDynamicCapacity );
            TestWithOffset( 4, maxDynamicCapacity );
            TestWithOffset( 5, maxDynamicCapacity );
            TestWithOffset( 6, maxDynamicCapacity );
            TestWithOffset( 7, maxDynamicCapacity );
            TestWithOffset( 8, maxDynamicCapacity );
            TestWithOffset( 9, maxDynamicCapacity );
            TestWithOffset( 10, maxDynamicCapacity );
            TestWithOffset( 11, maxDynamicCapacity );
            TestWithOffset( 12, maxDynamicCapacity );
            TestWithOffset( 13, maxDynamicCapacity );
            TestWithOffset( 14, maxDynamicCapacity );
        }

        static void TestWithOffset( int offset, int maxDynamicCapacity )
        {
            var f = new FIFOBuffer<int>( 0, 14 );
            for( int i = 0; i < offset; i++ ) f.Push( -1 );
            for( int i = 0; i < 7; i++ ) f.Push( i );
            f.Count.Should().BeLessThanOrEqualTo( f.MaxDynamicCapacity );

            var maxOffset = f.MaxDynamicCapacity - 7;
            if( offset > maxOffset ) offset = maxOffset;
            Span<int> padding = stackalloc int[offset];
            f.PopRange( padding ).Should().Be( offset );
            for( int i = 0; i < offset; i++ ) padding[i].Should().Be( -1 );

            Span<int> items = stackalloc int[7];
            f.PopRange( default ).Should().Be( 0 );
            f.PopRange( items.Slice( 0, 1 ) ).Should().Be( 1 );
            items[0].Should().Be( 0 );
            f.PopRange( items.Slice( 1, 2 ) ).Should().Be( 2 );
            items[1].Should().Be( 1 );
            items[2].Should().Be( 2 );
            f.PopRange( items.Slice( 3, 3 ) ).Should().Be( 3 );
            items[3].Should().Be( 3 );
            items[4].Should().Be( 4 );
            items[5].Should().Be( 5 );
            f.PopRange( items.Slice( 6, 1 ) ).Should().Be( 1 );
            items[6].Should().Be( 6 );
            f.Count.Should().Be( 0 );
        }
    }
}
