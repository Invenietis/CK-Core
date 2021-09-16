using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CK.Core.Tests.Collection
{
    public class SortedArrayListTests
    {
        [Test]
        public void SortedArrayListSimpleTest()
        {
            var a = new CKSortedArrayList<int>();
            a.AddRangeArray( 12, -34, 7, 545, 12 );
            a.AllowDuplicates.Should().BeFalse();
            a.Count.Should().Be( 4 );
            a.Should().BeInAscendingOrder();

            a.Contains( 14 ).Should().BeFalse();
            a.IndexOf( 12 ).Should().Be( 2 );

            object? o = 21;
            a.Contains( o ).Should().BeFalse();
            a.IndexOf( o ).Should().BeLessThan( 0 );

            o = 12;
            a.Contains( o ).Should().BeTrue();
            a.IndexOf( o ).Should().Be( 2 );

            o = null;
            a.Contains( o! ).Should().BeFalse();
            a.IndexOf( o! ).Should().Be( int.MinValue );

            int[] arrayToTest = new int[5];
            a.CopyTo( arrayToTest, 1 );
            arrayToTest[0].Should().Be( 0 );
            arrayToTest[1].Should().Be( -34 );
            arrayToTest[4].Should().Be( 545 );
        }

        [Test]
        public void SortedArrayListAllowDuplicatesTest()
        {
            var b = new CKSortedArrayList<int>( true );
            b.AddRangeArray( 12, -34, 7, 545, 12 );
            b.AllowDuplicates.Should().BeTrue();
            b.Count.Should().Be( 5 );
            b.Should().BeInAscendingOrder();
            b.IndexOf( 12 ).Should().Be( 2 );
            b.CheckPosition( 2 ).Should().Be( 2 );
            b.CheckPosition( 3 ).Should().Be( 3 );
        }

        [Test]
        public void Covariance_support_via_ICKReadOnlyList_and_ICKWritableCollection()
        {
            var a = new CKSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) )
            {
                new Mammal( "B", 12 ),
                new Canidae( "A", 12, true )
            };

            IReadOnlyList<Animal> baseObjects = a;
            for( int i = 0; i < baseObjects.Count; ++i )
            {
                baseObjects[i].Should().BeAssignableTo<Animal>( "This does not test anything. It's just to be read." );
            }
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
            var a = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) )
            {
                new Mammal( "B" ),
                new Mammal( "A" ),
                new Mammal( "D" ),
                new Mammal( "F" ),
                new Mammal( "C" ),
                new Mammal( "E" )
            };
            String.Join( "", a.Select( m => m.Name ) ).Should().Be( "ABCDEF" );

            for( int i = 0; i < a.Count; ++i )
            {
                a.CheckPosition( i ).Should().Be( i, "Nothing changed." );
            }
            CheckList( a, "ABCDEF" );

            a[0].Name = "Z";
            CheckList( a, "ZBCDEF" );
            a.CheckPosition( 0 ).Should().Be( 5 );
            CheckList( a, "BCDEFZ" );
            a[5].Name = "Z+";
            CheckList( a, "BCDEFZ+" );
            a.CheckPosition( 5 ).Should().Be( 5 );
            CheckList( a, "BCDEFZ+" );
            a[5].Name = "A";
            a.CheckPosition( 5 ).Should().Be( 0 );
            CheckList( a, "ABCDEF" );

            a[1].Name = "A";
            a.CheckPosition( 1 ).Should().BeLessThan( 0 );
            CheckList( a, "AACDEF" );

            a[1].Name = "B";
            a.CheckPosition( 1 ).Should().Be( 1 );
            CheckList( a, "ABCDEF" );

            a[1].Name = "C";
            a.CheckPosition( 1 ).Should().BeLessThan( 0 );
            CheckList( a, "ACCDEF" );

            a[1].Name = "Z";
            a.CheckPosition( 1 ).Should().Be( 5 );
            CheckList( a, "ACDEFZ" );

            a[5].Name = "D+";
            a.CheckPosition( 5 ).Should().Be( 3 );
            CheckList( a, "ACDD+EF" );

            a[3].Name = "D";
            a.CheckPosition( 3 ).Should().BeLessThan( 0 );
            CheckList( a, "ACDDEF" );

            a[3].Name = "B";
            a.CheckPosition( 3 ).Should().Be( 1 );
            CheckList( a, "ABCDEF" );

            var b = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) )
            {
                new Mammal( "B" ),
                new Mammal( "A" )
            };
            String.Join( "", b.Select( m => m.Name ) ).Should().Be( "AB" );

            b[0].Name = "Z";
            CheckList( b, "ZB" );
            b.CheckPosition( 0 ).Should().Be( 1 );
            CheckList( b, "BZ" );

            var c = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ), true )
            {
                new Mammal( "B" ),
                new Mammal( "A" )
            };
            String.Join( "", c.Select( m => m.Name ) ).Should().Be( "AB" );

            c[0].Name = "Z";
            CheckList( c, "ZB" );
            c.CheckPosition( 0 ).Should().Be( 1 );
            CheckList( c, "BZ" );

            var d = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) )
            {
                new Mammal( "B" ),
                new Mammal( "C" )
            };
            String.Join( "", d.Select( m => m.Name ) ).Should().Be( "BC" );

            d[1].Name = "A";
            CheckList( d, "BA" );
            d.CheckPosition( 1 ).Should().Be( 0 );
            CheckList( d, "AB" );
        }

        [Test]
        public void using_binary_search_algorithms_on_SortedArrayList()
        {
            var a = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) )
            {
                new Mammal( "B" ),
                new Mammal( "A" ),
                new Mammal( "D" ),
                new Mammal( "F" ),
                new Mammal( "C" ),
                new Mammal( "E" )
            };

            int idx;

            // External use of Util.BinarySearch on the exposed Store of the SortedArrayList.
            {
                idx = Util.BinarySearch<Mammal, string>( a.Tab, 0, a.Count, "E", ( m, name ) => m.Name.CompareTo( name ) );
                idx.Should().Be( 4 );

                idx = Util.BinarySearch<Mammal, string>( a.Tab, 0, a.Count, "A", ( m, name ) => m.Name.CompareTo( name ) );
                idx.Should().Be( 0 );

                idx = Util.BinarySearch<Mammal, string>( a.Tab, 0, a.Count, "Z", ( m, name ) => m.Name.CompareTo( name ) );
                idx.Should().Be( ~6 );
            }
            // Use of the extended SortedArrayList.IndexOf().
            {
                idx = a.IndexOf( "E", ( m, name ) => m.Name.CompareTo( name ) );
                idx.Should().Be( 4 );

                idx = a.IndexOf( "A", ( m, name ) => m.Name.CompareTo( name ) );
                idx.Should().Be( 0 );

                idx = a.IndexOf( "Z", ( m, name ) => m.Name.CompareTo( name ) );
                idx.Should().Be( ~6 );
            }
        }

        private static void CheckList( TestMammals a, string p )
        {
            HashSet<Mammal> dup = new HashSet<Mammal>();
            int i = 0;
            while( i < a.Count )
            {
                a[i].Should().NotBeNull();
                dup.Add( a[i] ).Should().BeTrue();
                ++i;
            }
            while( i < a.Tab.Length )
            {
                a.Tab[i].Should().BeNull();
                ++i;
            }
            string.Join( "", a.Select( m => m.Name ) ).Should().Be( p );
        }


        class TestInt : CKSortedArrayList<int>
        {
            public TestInt()
            {
            }

            public int[] Tab => Store;

            public void CheckList()
            {
                this.IsSortedStrict();
            }
        }

        private static void CheckList( TestInt a, params int[] p )
        {
            a.CheckList();
            a.SequenceEqual( p ).Should().BeTrue();
        }

        [Test]
        public void testing_add_and_remove_items()
        {
            var a = new TestInt();
            a.CheckList();
            a.Invoking( sut => sut.RemoveAt( -1 ) ).Should().Throw<IndexOutOfRangeException>();
            a.Invoking( sut => sut.RemoveAt( 0 ) ).Should().Throw<IndexOutOfRangeException>();
            a.Invoking( sut => sut.RemoveAt( 1 ) ).Should().Throw<IndexOutOfRangeException>();

            a.Remove( -1 ).Should().BeFalse();
            a.Remove( 0 ).Should().BeFalse();
            a.Remove( 1 ).Should().BeFalse();

            a.Add( 204 );
            a.CheckList();
            a.Invoking( sut => sut.RemoveAt( -1 ) ).Should().Throw<IndexOutOfRangeException>();
            a.Invoking( sut => sut.RemoveAt( 1 ) ).Should().Throw<IndexOutOfRangeException>();

            a.RemoveAt( 0 );
            a.Count.Should().Be( 0 );
            a.CheckList();

            a.Add( 206 );
            a.Add( 205 );
            a.Add( 204 );
            CheckList( a, 204, 205, 206 );

            a.RemoveAt( 1 );
            CheckList( a, 204, 206 );
            a.Invoking( sut => sut.RemoveAt( 2 ) ).Should().Throw<IndexOutOfRangeException>();
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
            a.Invoking( sut => sut.RemoveAt( 5 ) ).Should().Throw<IndexOutOfRangeException>();
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
            a.Remove( 203 ).Should().BeFalse();
            CheckList( a, 204, 205, 206, 207, 208 );
            a.Remove( 204 ).Should().BeTrue();
            CheckList( a, 205, 206, 207, 208 );
            a.Remove( 208 ).Should().BeTrue();
            CheckList( a, 205, 206, 207 );
            a.Remove( 208 ).Should().BeFalse();
            CheckList( a, 205, 206, 207 );
            a.Remove( 206 ).Should().BeTrue();
            CheckList( a, 205, 207 );
            a.Remove( 207 ).Should().BeTrue();
            CheckList( a, 205 );
            a.Remove( 205 ).Should().BeTrue();
            CheckList( a );

        }

        [Test]
        public void testing_capacity_changes()
        {
            var a = new CKSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );

            a.Capacity.Should().Be( 0 );
            a.Capacity = 3;
            a.Capacity.Should().Be( 4 );
            a.Capacity = 0;
            a.Capacity.Should().Be( 0 );

            a.Add( new Mammal( "1" ) );

            a.Invoking( sut => sut.Capacity = 0 ).Should().Throw<ArgumentException>();

            a.Add( new Mammal( "2" ) );
            a.Add( new Mammal( "3" ) );
            a.Add( new Mammal( "4" ) );
            a.Add( new Mammal( "5" ) );

            a.Capacity.Should().Be( 8 );
            a.Capacity = 5;
            a.Capacity.Should().Be( 5 );

            a.Add( new Mammal( "6" ) );
            a.Add( new Mammal( "7" ) );
            a.Add( new Mammal( "8" ) );
            a.Add( new Mammal( "9" ) );
            a.Add( new Mammal( "10" ) );

            a.Capacity.Should().Be( 10 );

            a.Clear();

            a.Capacity.Should().Be( 10 );

        }

        [Test]
        public void testing_expected_Argument_InvalidOperation_and_IndexOutOfRangeException()
        {
            var a = new CKSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );

            a.Invoking( sut => sut.IndexOf( null! ) ).Should().Throw<ArgumentNullException>();
            a.Invoking( sut => sut.IndexOf( null! ) ).Should().Throw<ArgumentNullException>();
            a.Invoking( sut => sut.IndexOf<Mammal>( new Mammal( "Nothing" ), null! ) ).Should().Throw<ArgumentNullException>();
            a.Invoking( sut => sut.Add( null! ) ).Should().Throw<ArgumentNullException>();

            a.Add( new Mammal( "A" ) );
            a.Add( new Mammal( "B" ) );

            a.Invoking( sut => { Mammal test = sut[2]; } ).Should().Throw<IndexOutOfRangeException>();
            a.Invoking( sut => sut.CheckPosition( 2 ) ).Should().Throw<IndexOutOfRangeException>();
            a.Invoking( sut => { Mammal test = a[-1]; } ).Should().Throw<IndexOutOfRangeException>();

            //Enumerator Exception (considering the non generic version since generics have weaken the invariants).
            var enumerator = ((System.Collections.IEnumerable)a).GetEnumerator();
            enumerator.Invoking( sut => { object? temp = sut.Current; } ).Should().Throw<InvalidOperationException>();
            enumerator.MoveNext();
            enumerator.Current.Should().Be( a[0] );
            enumerator.Reset();
            enumerator.Invoking( sut => { object? temp = sut.Current; } ).Should().Throw<InvalidOperationException>();
            a.Clear(); //change _version
            enumerator.Invoking( sut => sut.Reset() ).Should().Throw<InvalidOperationException>();
            enumerator.Invoking( sut => sut.MoveNext() ).Should().Throw<InvalidOperationException>();

            //Exception
            IList<Mammal> testException = new CKSortedArrayList<Mammal>
            {
                new Mammal( "Nothing" )
            };
            testException.Invoking( sut => sut[-1] = new Mammal( "A" ) ).Should().Throw<IndexOutOfRangeException>();
            testException.Invoking( sut => sut[1] = new Mammal( "A" ) ).Should().Throw<IndexOutOfRangeException>();
            testException.Invoking( sut => sut[0] = null! ).Should().Throw<ArgumentNullException>();
            testException.Invoking( sut => sut.Insert( -1, new Mammal( "A" ) ) ).Should().Throw<IndexOutOfRangeException>();
            testException.Invoking( sut => sut.Insert( 2, new Mammal( "A" ) ) ).Should().Throw<IndexOutOfRangeException>();

            testException.Invoking( sut => sut.Insert( 0, null! ) ).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void SortedArrayList_can_be_cast_into_IList_or_ICollection()
        {
            var a = new CKSortedArrayList<int>();
            a.AddRangeArray( 12, -34, 7, 545, 12 );

            //Cast IList
            IList<int> listToTest = (IList<int>)a;

            listToTest[0].Should().Be( -34 );
            listToTest[1].Should().Be( 7 );
            listToTest[2].Should().Be( 12 );
            listToTest[3].Should().Be( 545 );

            listToTest.Add( 12345 );
            listToTest.Add( 1234 );
            listToTest[4].Should().Be( 1234 );
            listToTest[5].Should().Be( 12345 );

            listToTest[0] = -33;
            listToTest[0].Should().Be( -33 );
            listToTest[0] = 123456;
            listToTest[0].Should().Be( 123456 );

            listToTest.Insert( 0, -33 );
            listToTest[0].Should().Be( -33 );
            listToTest.Insert( 0, 123456 );
            listToTest[0].Should().Be( 123456 );

            //Cast ICollection
            a.Clear();
            a.AddRangeArray( 12, -34, 7, 545, 12 );
            ICollection<int> collectionToTest = (ICollection<int>)a;

            collectionToTest.IsReadOnly.Should().BeFalse();

            collectionToTest.Add( 123 );
            collectionToTest.Contains( 123 ).Should().BeTrue();
            collectionToTest.Contains( -34 ).Should().BeTrue();
            collectionToTest.Contains( 7 ).Should().BeTrue();
        }

    }
}
