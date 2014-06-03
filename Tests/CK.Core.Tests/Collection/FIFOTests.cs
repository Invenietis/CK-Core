#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Collection\FIFOTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Reflection;
using CK.Core;
using NUnit.Framework;
using System.Linq;
using System;
using System.Globalization;

namespace CK.Core.Tests.Collection
{
    [TestFixture]
    [Category("FIFOBuffer")]
    public class FIFOTests
    {
        [Test]
        public void FIFOToArray()
        {
            int[] initialArray = new int[7];
            initialArray[0] = initialArray[6] = -1;
            FIFOBuffer<int> f = new FIFOBuffer<int>( 5 );
            CollectionAssert.AreEqual( f.ToArray(), new int[0] );

            Assert.Throws<ArgumentNullException>( () => f.CopyTo( null ) );
            Assert.Throws<ArgumentNullException>( () => f.CopyTo( null, 0 ) );
            Assert.Throws<ArgumentNullException>( () => f.CopyTo( null, 0, 0 ) );

            TestWithInternalOffsets( f, b =>
                {
                    var array = (int[])initialArray.Clone();
                    b.Push( 1 );
                    b.CopyTo( array, 3, 2 );
                    CollectionAssert.AreEqual( array, new int[] { -1, 0, 0, 1, 0, 0, -1 } );

                    array[3] = 0;
                    b.Push( 2 );
                    b.CopyTo( array, 3, 2 );
                    CollectionAssert.AreEqual( array, new int[] { -1, 0, 0, 1, 2, 0, -1 } );

                    array[3] = 0; array[4] = 0;
                    b.Push( 3 );
                    b.CopyTo( array, 3, 3 );
                    CollectionAssert.AreEqual( array, new int[] { -1, 0, 0, 1, 2, 3, -1 } );

                    array[3] = 0; array[4] = 0; array[5] = 0;
                    b.Push( 4 );
                    b.CopyTo( array, 3, 3 );
                    CollectionAssert.AreEqual( array, new int[] { -1, 0, 0, 2, 3, 4, -1 } );

                    array[3] = 0; array[4] = 0; array[5] = 0;
                    b.CopyTo( array, 2, 4 );
                    CollectionAssert.AreEqual( array, new int[] { -1, 0, 1, 2, 3, 4, -1 } );

                    array[3] = 0; array[4] = 0; array[5] = 0;
                    Assert.That( b.CopyTo( array, 2, 5 ), Is.EqualTo( 4 ) );
                    CollectionAssert.AreEqual( array, new int[] { -1, 0, 1, 2, 3, 4, -1 }, "Sentinel is not changed: there is only 4 items to copy." );

                    Assert.Throws<IndexOutOfRangeException>( () => b.CopyTo( array, 2, 6 ), "Even if the items fit, there must be an exception." );

                    b.Truncate( 1 );
                    Assert.That( b.Peek(), Is.EqualTo( 4 ) );
                    b.Push( 60 );
                    b.Push( 61 );
                    b.Push( 62 );
                    b.Push( 63 );
                    b.Push( 7 ); // oldest
                    b.Push( 8 );
                    b.Push( 9 );
                    b.Push( 10 );
                    b.Push( 11 );
                    Assert.That( b[0], Is.EqualTo( 7 ) );

                    array[3] = 0; array[4] = 0; array[5] = 0;
                    Assert.That( b.CopyTo( array, 1 ), Is.EqualTo( 5 ) );
                    CollectionAssert.AreEqual( array, new int[] { -1, 7, 8, 9, 10, 11, -1 }, "Sentinel is not changed: there is only 5 items to copy." );

                    array[5] = 0;
                    Assert.That( b.CopyTo( array, 0 ), Is.EqualTo( 5 ) );
                    CollectionAssert.AreEqual( array, new int[] { 7, 8, 9, 10, 11, 0, -1 } );

                    Assert.That( b.CopyTo( array, 5 ), Is.EqualTo( 2 ) );
                    CollectionAssert.AreEqual( array, new int[] { 7, 8, 9, 10, 11, 10, 11 } );
                } );
        }

        [Test]
        public void FIFOChangeCapacity()
        {
            FIFOBuffer<int> f = new FIFOBuffer<int>( 0 );
            Assert.That( f.Capacity, Is.EqualTo( 0 ) );
            AssertEmpty( f );
            f.Push( 5 );
            AssertEmpty( f );
            f.Push( 12 );
            AssertEmpty( f );
            
            f.Capacity = 1;
            Assert.That( f.Capacity, Is.EqualTo( 1 ) );
            AssertEmpty( f );
            f.Push( 5 );
            AssertContains( f, 5 );
            f.Push( 6 );
            AssertContains( f, 6 );

            f.Capacity = 2;
            Assert.That( f.Capacity, Is.EqualTo( 2 ) );
            AssertContains( f, 6 );
            f.Push( 7 );
            AssertContains( f, 6, 7 );
            f.Push( 8 );
            AssertContains( f, 7, 8 );

            f.Capacity = 4;
            Assert.That( f.Capacity, Is.EqualTo( 4 ) );
            AssertContains( f, 7, 8 );
            f.Push( 9 );
            AssertContains( f, 7, 8, 9 );
            f.Push( 10 );
            AssertContains( f, 7, 8, 9, 10 );
            f.Push( 11 );
            AssertContains( f, 8, 9, 10, 11 );

            f.Capacity = 7;
            Assert.That( f.Capacity, Is.EqualTo( 7 ) );
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
            Assert.That( f.Capacity, Is.EqualTo( 2 ) );
            AssertContains( f, 14, 15 );

            f.Capacity = 3;
            Assert.That( f.Capacity, Is.EqualTo( 3 ) );
            AssertContains( f, 14, 15 );
            f.Push( 16 );
            AssertContains( f, 14, 15, 16 );

            f.Capacity = 2;
            Assert.That( f.Capacity, Is.EqualTo( 2 ) );
            AssertContains( f, 15, 16 );

            f.Capacity = 1;
            Assert.That( f.Capacity, Is.EqualTo( 1 ) );
            AssertContains( f, 16 );

            f.Capacity = 0;
            Assert.That( f.Capacity, Is.EqualTo( 0 ) );
            AssertEmpty( f );

            f.Capacity = 2;
            f.Capacity = 2;
            Assert.That( f.Capacity, Is.EqualTo( 2 ) );

            Assert.That( f.ToString(), Is.EqualTo( String.Format( "Count = {0} (Capacity = {1})", 0, 2 ) ) );

            //ExceptionTest
            Assert.Throws<ArgumentException>( () => f.Capacity = -1 );
            Assert.Throws<ArgumentException>( () => new FIFOBuffer<int>( -1 ) );
            Assert.Throws<IndexOutOfRangeException>( () => f.CopyTo(new int[2], 0, -1) );
        }

        [Test]
        public void FIFORemoveAt()
        {
            FIFOBuffer<int> f = new FIFOBuffer<int>( 0 );
            Assert.Throws<IndexOutOfRangeException>( () => f.RemoveAt( 0 ) );
            Assert.Throws<IndexOutOfRangeException>( () => f.RemoveAt( -1 ) );

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
        public void FIFOPeekAndIndex()
        {
            FIFOBuffer<int> f = new FIFOBuffer<int>( 0 );
            Assert.Throws<IndexOutOfRangeException>( () => Console.Write( f[-1] ) );
            Assert.Throws<IndexOutOfRangeException>( () => Console.Write( f[0] ) );
            Assert.Throws<InvalidOperationException>( () => f.Peek() );
            Assert.Throws<InvalidOperationException>( () => f.PeekLast() );

            f.Push( 5 );
            Assert.Throws<IndexOutOfRangeException>( () => Console.Write( f[0] ) );
            Assert.Throws<InvalidOperationException>( () => f.Peek() );
            Assert.Throws<InvalidOperationException>( () => f.PeekLast() );

            f.Capacity = 1;
            TestWithInternalOffsets( f, b =>
            {
                b.Push( 5 );
                Assert.That( b[0], Is.EqualTo( 5 ) );
                Assert.Throws<IndexOutOfRangeException>( () => Console.Write( b[1] ) );
                Assert.That( b.Peek(), Is.EqualTo( 5 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 5 ) );
                b.Push( 6 );
                Assert.That( b[0], Is.EqualTo( 6 ), "Only one item in it." );
                Assert.Throws<IndexOutOfRangeException>( () => Console.Write( b[1] ) );
                Assert.That( b.Peek(), Is.EqualTo( 6 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 6 ) );
            } );

            f.Clear();
            Assert.Throws<IndexOutOfRangeException>( () => Console.Write( f[0] ) );
            Assert.Throws<IndexOutOfRangeException>( () => Console.Write( f[1] ) );
            Assert.Throws<InvalidOperationException>( () => f.Peek() );
            Assert.Throws<InvalidOperationException>( () => f.PeekLast() );

            f.Capacity = 2;
            TestWithInternalOffsets( f, b =>
            {
                b.Push( 5 );
                Assert.That( b[0], Is.EqualTo( 5 ) );
                Assert.Throws<IndexOutOfRangeException>( () => Console.Write( b[1] ) );
                Assert.That( b.Peek(), Is.EqualTo( 5 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 5 ) );
                b.Push( 6 );
                Assert.That( b[0], Is.EqualTo( 5 ) );
                Assert.That( b[1], Is.EqualTo( 6 ) );
                Assert.That( b.Peek(), Is.EqualTo( 5 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 6 ) );
                b.Pop();
                Assert.That( b[0], Is.EqualTo( 6 ) );
                Assert.Throws<IndexOutOfRangeException>( () => Console.Write( b[1] ) );
                Assert.That( b.Peek(), Is.EqualTo( 6 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 6 ) );
                b.Pop();
                Assert.Throws<IndexOutOfRangeException>( () => Console.Write( b[0] ) );
                Assert.Throws<IndexOutOfRangeException>( () => Console.Write( b[1] ) );
                Assert.Throws<InvalidOperationException>( () => b.Peek() );
                Assert.Throws<InvalidOperationException>( () => b.PeekLast() );

                b.Push( 7 );
                b.Push( 8 );
                b.Push( 9 );
                Assert.That( b[0], Is.EqualTo( 8 ) );
                Assert.That( b[1], Is.EqualTo( 9 ) );
                CollectionAssert.AreEqual( b.ToArray(), new int[] { 8, 9 } );
                Assert.That( b.Peek(), Is.EqualTo( 8 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 9 ) );
                Assert.That( b.Pop(), Is.EqualTo( 8 ) );
                Assert.That( b.Pop(), Is.EqualTo( 9 ) );
                AssertEmpty( b );

                b.Push( 10 );
                b.Push( 11 );
                b.Push( 12 );
                Assert.That( b[0], Is.EqualTo( 11 ) );
                Assert.That( b[1], Is.EqualTo( 12 ) );
                Assert.That( b.Peek(), Is.EqualTo( 11 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 12 ) );
                Assert.That( b.PopLast(), Is.EqualTo( 12 ) );
                Assert.That( b.Peek(), Is.EqualTo( 11 ) );
                Assert.That( b.PeekLast(), Is.EqualTo( 11 ) );
                Assert.That( b.PopLast(), Is.EqualTo( 11 ) );
                AssertEmpty( b );
            } );

            f.Capacity = 3;
            TestWithInternalOffsets( f, b =>
            {
                b.Push( 11 );
                b.Push( 12 );
                b.Push( 13 );
                Assert.That( b[0], Is.EqualTo( 11 ) );
                Assert.That( b[1], Is.EqualTo( 12 ) );
                Assert.That( b[2], Is.EqualTo( 13 ) );
            } );


            f.Capacity = 4;
            f.Push( 11 );
            f.Push( 12 );
            f.Push( 13 );
            TestWithInternalOffsets( f, b =>
            {
                b.Push( 14 );
                Assert.That( b[0], Is.EqualTo( 11 ) );
                Assert.That( b[1], Is.EqualTo( 12 ) );
                Assert.That( b[2], Is.EqualTo( 13 ) );
                Assert.That( b[3], Is.EqualTo( 14 ) );
                b.Push( 15 );
                Assert.That( b[0], Is.EqualTo( 12 ) );
                Assert.That( b[1], Is.EqualTo( 13 ) );
                Assert.That( b[2], Is.EqualTo( 14 ) );
                Assert.That( b[3], Is.EqualTo( 15 ) );
                b.Push( 16 );
                Assert.That( b[0], Is.EqualTo( 13 ) );
                Assert.That( b[1], Is.EqualTo( 14 ) );
                Assert.That( b[2], Is.EqualTo( 15 ) );
                Assert.That( b[3], Is.EqualTo( 16 ) );
            } );

            f.Capacity = 5;
            AssertContains( f, 11, 12, 13 );
            TestWithInternalOffsets( f, b =>
            {
                b.Push( 14 );
                b.Push( 15 );
                b.Push( 16 );
                b.Push( 17 );
                Assert.That( b[0], Is.EqualTo( 13 ) );
                Assert.That( b[1], Is.EqualTo( 14 ) );
                Assert.That( b[2], Is.EqualTo( 15 ) );
                Assert.That( b[3], Is.EqualTo( 16 ) );
                Assert.That( b[4], Is.EqualTo( 17 ) );
                Assert.Throws<IndexOutOfRangeException>( () => Console.Write( f[5] ) );
            } );
        }

        [Test]
        public void FIFOSupportNull()
        {
            CultureInfo c0 = CultureInfo.InvariantCulture;
            CultureInfo c1 = CultureInfo.GetCultureInfo( "fr" );

            FIFOBuffer<CultureInfo> f = new FIFOBuffer<CultureInfo>( 2 );
            AssertEmpty( f );
            
            // When calling with null, it is the IndexOf( T ) that is called
            // since T is a reference type.
            int iNull = f.IndexOf( null );
            Assert.That( iNull, Is.LessThan( 0 ) );

            f.Push( c0 );
            Assert.That( f.Contains( null ), Is.False );
            Assert.That( f.IndexOf( null ), Is.LessThan( 0 ) );
            Assert.That( f.PeekLast(), Is.SameAs( c0 ) );
            AssertContains( f, c0 );
            
            f.Push( null );
            Assert.That( f.Count, Is.EqualTo( 2 ) );
            Assert.That( f.IndexOf( null ), Is.EqualTo( 1 ) );
            Assert.That( f.IndexOf( c0 ), Is.EqualTo( 0 ) );
            Assert.That( f.PeekLast(), Is.Null );
            AssertContains( f, c0, null );

            f.Push( c1 );
            Assert.That( f.IndexOf( null ), Is.EqualTo( 0 ) );
            Assert.That( f.IndexOf( c1 ), Is.EqualTo( 1 ) );
            Assert.That( f.Contains( c0 ), Is.False );
            Assert.That( f.IndexOf( c0 ), Is.LessThan( 0 ) );
            Assert.That( f.PeekLast(), Is.SameAs( c1 ) );
            AssertContains( f, null, c1 );

            f.Push( null );
            AssertContains( f, c1, null );
            f.Push( null );
            AssertContains( f, null, null );
            Assert.That( f.PopLast(), Is.Null );
            Assert.That( f.PopLast(), Is.Null );
            Assert.Throws<InvalidOperationException>( () => f.PopLast() );
        }

        [Test]
        public void FIFOOneValueType()
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
            Assert.That( iType == iBoxed );
        }

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
                for( int i = 0; i < iTry; ++i ) f.Push( default( T ) );
                foreach( var i in saved ) f.Push( i );
                while( f.Count > saved.Length ) f.Pop();
                CollectionAssert.AreEqual( f, saved );
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
                f.Capacity = f.Capacity + 1;
            }
            f.Capacity = c;
        }

        static void AssertContains<T>( FIFOBuffer<T> f, params T[] values )
        {
            Assert.That( f.Count, Is.EqualTo( values.Length ) );
            CollectionAssert.AreEqual( f, values );
            CollectionAssert.AreEqual( f.ToArray(), values );
        }

        static void AssertEmpty<T>( FIFOBuffer<T> f )
        {
            Assert.That( f, Is.Empty );
            AssertContains( f );
        }

        static void CheckOneValueType<T>( FIFOBuffer<T> f, T value, T otherValue ) where T : struct
        {
            CheckOnlyOneValueType<T>( f, value, otherValue );
            Assert.That( f.Pop(), Is.EqualTo( value ) );
            AssertEmpty( f );
            f.Push( value );
            CheckOnlyOneValueType<T>( f, value, otherValue );
        }

        static void CheckOnlyOneValueType<T>( FIFOBuffer<T> f, T value, T otherValue ) where T : struct
        {
            Assert.That( f[0], Is.EqualTo( value ) );
            Assert.That( f.Contains( value ), Is.True );
            Assert.That( f.Contains( otherValue ), Is.False );
            Assert.That( f.Contains( null ), Is.False );
            Assert.That( f.SequenceEqual( new[] { value } ), Is.True );
        }
    }
}
