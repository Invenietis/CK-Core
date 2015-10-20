#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Collection\SortedArrayListTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;

namespace CK.Core.Tests.Collection
{
    [TestFixture]
    [Category("SortedArrayList")]
    public class SortedArrayListTests
    {
        [Test]
        public void SortedArrayListSimpleTest()
        {
            var a = new CKSortedArrayList<int>();
            a.AddRangeArray( 12, -34, 7, 545, 12 );
            Assert.That( a.AllowDuplicates, Is.False );
            Assert.That( a.Count, Is.EqualTo( 4 ) );
            Assert.That( a, Is.Ordered );

            Assert.That( a.Contains( 14 ), Is.False );
            Assert.That( a.IndexOf( 12 ), Is.EqualTo( 2 ) );

            object o = 21;
            Assert.That( a.Contains( o ), Is.False );
            Assert.That( a.IndexOf( o ), Is.LessThan( 0 ) );

            o = 12;
            Assert.That( a.Contains( o ), Is.True );
            Assert.That( a.IndexOf( o ), Is.EqualTo( 2 ) );

            o = null;
            Assert.That( a.Contains( o ), Is.False );
            Assert.That( a.IndexOf( o ), Is.EqualTo( Int32.MinValue ) );

            int[] arrayToTest = new int[5];
            a.CopyTo( arrayToTest, 1 );
            Assert.That( arrayToTest[0], Is.EqualTo( 0 ) );
            Assert.That( arrayToTest[1], Is.EqualTo( -34 ) );
            Assert.That( arrayToTest[4], Is.EqualTo( 545 ) );
        }

        [Test]
        public void SortedArrayListAllowDuplicatesTest()
        {
            var b = new CKSortedArrayList<int>(true);
            b.AddRangeArray( 12, -34, 7, 545, 12 );
            Assert.That( b.AllowDuplicates, Is.True );
            Assert.That( b.Count, Is.EqualTo( 5 ) );
            Assert.That( b, Is.Ordered );
            Assert.That( b.IndexOf( 12 ), Is.EqualTo( 2 ) );
            Assert.That( b.CheckPosition( 2 ), Is.EqualTo( 2 ) );
            Assert.That( b.CheckPosition( 3 ), Is.EqualTo( 3 ) );
        }

        [Test]
        public void Covariance_support_via_ICKReadOnlyList_and_ICKWritableCollection()
        {
            var a = new CKSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );
            a.Add( new Mammal( "B", 12 ) );
            a.Add( new Canidae( "A", 12, true ) );

            IReadOnlyList<Animal> baseObjects = a;
            for( int i = 0; i < baseObjects.Count; ++i ) Console.Write( baseObjects[i].Name );

            ICKWritableCollection<Canidae> dogs = a;
            dogs.Add( new Canidae( "C", 8, false ) );
        }

        class TestMammals : CKSortedArrayList<Mammal>
        {
            public TestMammals( Comparison<Mammal> m, bool allowDuplicated = false )
                : base( m, allowDuplicated )
            {
            }

            public Mammal[] Tab { get { return Store; } }
        }

        [Test]
        public void CheckPosition_locally_reorders_the_items()
        {
            var a = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );
            a.Add( new Mammal( "B" ) );
            a.Add( new Mammal( "A" ) );
            a.Add( new Mammal( "D" ) );
            a.Add( new Mammal( "F" ) );
            a.Add( new Mammal( "C" ) );
            a.Add( new Mammal( "E" ) );
            Assert.That( String.Join( "", a.Select( m => m.Name ) ), Is.EqualTo( "ABCDEF" ) );

            for( int i = 0; i < a.Count; ++i )
            {
                Assert.That( a.CheckPosition( i ), Is.EqualTo( i ), "Nothing changed." );
            }
            CheckList( a, "ABCDEF" );

            a[0].Name = "Z";
            CheckList( a, "ZBCDEF" );
            Assert.That( a.CheckPosition( 0 ), Is.EqualTo( 5 ) );
            CheckList( a, "BCDEFZ" );
            a[5].Name = "Z+";
            CheckList( a, "BCDEFZ+" );
            Assert.That( a.CheckPosition( 5 ), Is.EqualTo( 5 ) );
            CheckList( a, "BCDEFZ+" );
            a[5].Name = "A";
            Assert.That( a.CheckPosition( 5 ), Is.EqualTo( 0 ) );
            CheckList( a, "ABCDEF" );

            a[1].Name = "A";
            Assert.That( a.CheckPosition( 1 ), Is.LessThan( 0 ) );
            CheckList( a, "AACDEF" );

            a[1].Name = "B";
            Assert.That( a.CheckPosition( 1 ), Is.EqualTo( 1 ) );
            CheckList( a, "ABCDEF" );

            a[1].Name = "C";
            Assert.That( a.CheckPosition( 1 ), Is.LessThan( 0 ) );
            CheckList( a, "ACCDEF" );

            a[1].Name = "Z";
            Assert.That( a.CheckPosition( 1 ), Is.EqualTo( 5 ) );
            CheckList( a, "ACDEFZ" );

            a[5].Name = "D+";
            Assert.That( a.CheckPosition( 5 ), Is.EqualTo( 3 ) );
            CheckList( a, "ACDD+EF" );

            a[3].Name = "D";
            Assert.That( a.CheckPosition( 3 ), Is.LessThan( 0 ) );
            CheckList( a, "ACDDEF" );

            a[3].Name = "B";
            Assert.That( a.CheckPosition( 3 ), Is.EqualTo( 1 ) );
            CheckList( a, "ABCDEF" );

            var b = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );
            b.Add( new Mammal( "B" ) );
            b.Add( new Mammal( "A" ) );
            Assert.That( String.Join( "", b.Select( m => m.Name ) ), Is.EqualTo( "AB" ) );

            b[0].Name = "Z";
            CheckList( b, "ZB" );
            Assert.That( b.CheckPosition( 0 ), Is.EqualTo( 1 ) );
            CheckList( b, "BZ" );

            var c = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ), true );
            c.Add( new Mammal( "B" ) );
            c.Add( new Mammal( "A" ) );
            Assert.That( String.Join( "", c.Select( m => m.Name ) ), Is.EqualTo( "AB" ) );

            c[0].Name = "Z";
            CheckList( c, "ZB" );
            Assert.That( c.CheckPosition( 0 ), Is.EqualTo( 1 ) );
            CheckList( c, "BZ" );

            var d = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );
            d.Add( new Mammal( "B" ) );
            d.Add( new Mammal( "C" ) );
            Assert.That( String.Join( "", d.Select( m => m.Name ) ), Is.EqualTo( "BC" ) );

            d[1].Name = "A";
            CheckList( d, "BA" );
            Assert.That( d.CheckPosition( 1 ), Is.EqualTo( 0 ) );
            CheckList( d, "AB" );
        }

        [Test]
        public void using_binary_search_algorithms_on_SortedArrayList()
        {
            var a = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );
            a.Add( new Mammal( "B" ) );
            a.Add( new Mammal( "A" ) );
            a.Add( new Mammal( "D" ) );
            a.Add( new Mammal( "F" ) );
            a.Add( new Mammal( "C" ) );
            a.Add( new Mammal( "E" ) );

            int idx;

            // External use of Util.BinarySearch on the exposed Store of the SortedArrayList.
            {
                idx = Util.BinarySearch<Mammal, string>( a.Tab, 0, a.Count, "E", ( m, name ) => m.Name.CompareTo( name ) );
                Assert.That( idx, Is.EqualTo( 4 ) );

                idx = Util.BinarySearch<Mammal, string>( a.Tab, 0, a.Count, "A", ( m, name ) => m.Name.CompareTo( name ) );
                Assert.That( idx, Is.EqualTo( 0 ) );

                idx = Util.BinarySearch<Mammal, string>( a.Tab, 0, a.Count, "Z", ( m, name ) => m.Name.CompareTo( name ) );
                Assert.That( idx, Is.EqualTo( ~6 ) );
            }
            // Use of the extended SortedArrayList.IndexOf().
            {
                idx = a.IndexOf( "E", ( m, name ) => m.Name.CompareTo( name ) );
                Assert.That( idx, Is.EqualTo( 4 ) );

                idx = a.IndexOf( "A", ( m, name ) => m.Name.CompareTo( name ) );
                Assert.That( idx, Is.EqualTo( 0 ) );

                idx = a.IndexOf( "Z", ( m, name ) => m.Name.CompareTo( name ) );
                Assert.That( idx, Is.EqualTo( ~6 ) );
            }
        }

        private static void CheckList( TestMammals a, string p )
        {
            HashSet<Mammal> dup = new HashSet<Mammal>();
            int i = 0;
            while( i < a.Count )
            {
                Assert.That( a[i], Is.Not.Null );
                Assert.That( dup.Add( a[i] ), Is.True );
                ++i;
            }
            while( i < a.Tab.Length )
            {
                Assert.That( a.Tab[i], Is.Null );
                ++i;
            }
            Assert.That( String.Join( "", a.Select( m => m.Name ) ), Is.EqualTo( p ) );
        }


        class TestInt : CKSortedArrayList<int>
        {
            public TestInt()
            {
            }

            public int[] Tab { get { return Store; } }

            public void CheckList()
            {
                Assert.That( this.IsSortedStrict() );
                int i = Count;
                while( i < Tab.Length )
                {
                    Assert.That( Tab[i], Is.EqualTo( default(int) ) );
                    ++i;
                }
            }
        }

        private static void CheckList( TestInt a, params int[] p )
        {
            a.CheckList();
            Assert.That( a.SequenceEqual( p ) );
        }

        [Test]
        public void testing_add_and_remove_items()
        {
            var a = new TestInt();
            a.CheckList();
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( -1 ) );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 0 ) );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 1 ) );

            Assert.That( a.Remove( -1 ), Is.False ); 
            Assert.That( a.Remove( 0 ), Is.False );
            Assert.That( a.Remove( 1 ), Is.False );

            a.Add( 204 );
            a.CheckList();
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( -1 ) );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 1 ) );

            a.RemoveAt( 0 );
            Assert.That( a.Count, Is.EqualTo( 0 ) );
            a.CheckList();

            a.Add( 206 );
            a.Add( 205 );
            a.Add( 204 );
            CheckList( a, 204, 205, 206 );

            a.RemoveAt( 1 );
            CheckList( a, 204, 206 );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 2 ) );
            a.RemoveAt( 1 );
            CheckList( a, 204 );
            a.RemoveAt( 0 );
            CheckList( a );

            a.Add( 206 );
            a.Add( 205 );
            a.Add( 204 );
            a.Add( 207 );
            a.Add( 208 );
            CheckList( a, 204, 205, 206, 207, 208 );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 5 ) );
            a.RemoveAt( 0 );
            CheckList( a, 205, 206, 207, 208 );
            a.RemoveAt( 3 );
            CheckList( a, 205, 206, 207 );
            a.RemoveAt( 1 );
            CheckList( a, 205, 207 );
            a.RemoveAt( 1 );
            CheckList( a, 205 );
            a.RemoveAt( 0 );
            CheckList( a );

            a.Add( 206 );
            a.Add( 205 );
            a.Add( 204 );
            a.Add( 207 );
            a.Add( 208 );
            CheckList( a, 204, 205, 206, 207, 208 );
            Assert.That( a.Remove( 203 ), Is.False );
            CheckList( a, 204, 205, 206, 207, 208 );
            Assert.That( a.Remove( 204 ), Is.True );
            CheckList( a, 205, 206, 207, 208 );
            Assert.That( a.Remove( 208 ), Is.True );
            CheckList( a, 205, 206, 207 );
            Assert.That( a.Remove( 208 ), Is.False );
            CheckList( a, 205, 206, 207 );
            Assert.That( a.Remove( 206 ), Is.True );
            CheckList( a, 205, 207 );
            Assert.That( a.Remove( 207 ), Is.True );
            CheckList( a, 205 );
            Assert.That( a.Remove( 205 ), Is.True );
            CheckList( a );

        }

        [Test]
        public void testing_capacity_changes()
        {
            var a = new CKSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );

            Assert.That( a.Capacity, Is.EqualTo( 0 ) );
            a.Capacity = 3;
            Assert.That( a.Capacity, Is.EqualTo( 4 ) );
            a.Capacity = 0;
            Assert.That( a.Capacity, Is.EqualTo( 0 ) );

            a.Add( new Mammal( "1" ) );

            Assert.Throws<ArgumentException>( () => a.Capacity = 0 );

            a.Add( new Mammal( "2" ) );
            a.Add( new Mammal( "3" ) );
            a.Add( new Mammal( "4" ) );
            a.Add( new Mammal( "5" ) );

            Assert.That( a.Capacity, Is.EqualTo( 8 ) );
            a.Capacity = 5;
            Assert.That( a.Capacity, Is.EqualTo( 5 ) );

            a.Add( new Mammal( "6" ) );
            a.Add( new Mammal( "7" ) );
            a.Add( new Mammal( "8" ) );
            a.Add( new Mammal( "9" ) );
            a.Add( new Mammal( "10" ) );

            Assert.That( a.Capacity, Is.EqualTo( 10 ) );

            a.Clear();

            Assert.That( a.Capacity, Is.EqualTo( 10 ) );

        }

        [Test]
        public void testing_expected_Argument_InvalidOperation_and_IndexOutOfRangeException()
        {
            var a = new CKSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );

            Assert.Throws<ArgumentNullException>( () => a.IndexOf( null ) );
            Assert.Throws<ArgumentNullException>( () => a.IndexOf<Mammal>( new Mammal( "Nothing" ), null ) );
            Assert.Throws<ArgumentNullException>( () => a.Add( null ) );

            a.Add( new Mammal( "A" ) );
            a.Add( new Mammal( "B" ) );

            Assert.Throws<IndexOutOfRangeException>( () => { Mammal test = a[2]; } );
            Assert.Throws<IndexOutOfRangeException>( () => a.CheckPosition( 2 ) );
            Assert.Throws<IndexOutOfRangeException>( () => { Mammal test = a[-1]; } );
            Assert.Throws<IndexOutOfRangeException>( () => a.CheckPosition( -1 ) );

            //Enumerator Exception
            var enumerator = a.GetEnumerator();
            Assert.Throws<InvalidOperationException>( () => { Mammal temp = enumerator.Current; } );
            enumerator.MoveNext();
            Assert.That( enumerator.Current, Is.EqualTo( a[0] ) );
            enumerator.Reset();
            Assert.Throws<InvalidOperationException>( () => { Mammal temp = enumerator.Current; } );
            a.Clear(); //change _version
            Assert.Throws<InvalidOperationException>( () => enumerator.Reset() );
            Assert.Throws<InvalidOperationException>( () => enumerator.MoveNext() );

            //Exception
            IList<Mammal> testException = new CKSortedArrayList<Mammal>();
            testException.Add( new Mammal( "Nothing" ) );
            Assert.Throws<IndexOutOfRangeException>( () => testException[-1] = new Mammal( "A" ) );
            Assert.Throws<IndexOutOfRangeException>( () => testException[1] = new Mammal( "A" ) );
            Assert.Throws<ArgumentNullException>( () => testException[0] = null );
            Assert.Throws<IndexOutOfRangeException>( () => testException.Insert( -1, new Mammal( "A" ) ) );
            Assert.Throws<IndexOutOfRangeException>( () => testException.Insert( 2, new Mammal( "A" ) ) );
            Assert.Throws<ArgumentNullException>( () => testException.Insert( 0, null ) );
        }

        [Test]
        public void SortedArrayList_can_be_cast_into_IList_or_ICollection()
        {
            var a = new CKSortedArrayList<int>();
            a.AddRangeArray( 12, -34, 7, 545, 12 );

            //Cast IList
            IList<int> listToTest = (IList<int>)a;

            Assert.That( listToTest[0], Is.EqualTo( -34 ) );
            Assert.That( listToTest[1], Is.EqualTo( 7 ) );
            Assert.That( listToTest[2], Is.EqualTo( 12 ) );
            Assert.That( listToTest[3], Is.EqualTo( 545 ) );

            listToTest.Add( 12345 );
            listToTest.Add( 1234 );
            Assert.That( listToTest[4], Is.EqualTo( 1234 ) );
            Assert.That( listToTest[5], Is.EqualTo( 12345 ) );

            listToTest[0] = -33;
            Assert.That( listToTest[0], Is.EqualTo( -33 ) );
            listToTest[0] = 123456;
            Assert.That( listToTest[0], Is.EqualTo( 123456 ) );

            listToTest.Insert( 0, -33 );
            Assert.That( listToTest[0], Is.EqualTo( -33 ) );
            listToTest.Insert( 0, 123456 );
            Assert.That( listToTest[0], Is.EqualTo( 123456 ) );

            //Cast ICollection
            a.Clear();
            a.AddRangeArray( 12, -34, 7, 545, 12 );
            ICollection<int> collectionToTest = (ICollection<int>)a;

            Assert.That( collectionToTest.IsReadOnly, Is.False );
            
            collectionToTest.Add( 123 );
            Assert.That( collectionToTest.Contains( 123 ), Is.True );
            Assert.That( collectionToTest.Contains( -34 ), Is.True );
            Assert.That( collectionToTest.Contains( 7 ), Is.True );
        }

    }
}
