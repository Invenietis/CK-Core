using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core;
using NUnit.Framework;

namespace CK.Core.Tests.Collection
{
    [TestFixture]
    public class ObservableSortedArrayListTests
    {
        [Test]
        public void ObservableSortedArrayListSimpleTest()
        {
            var a = new ObservableSortedArrayList<int>();
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
        public void ObservableSortedArrayListAllowDuplicatesTest()
        {
            var b = new ObservableSortedArrayList<int>(Comparer<int>.Default.Compare, true );
            b.AddRangeArray( 12, -34, 7, 545, 12 );
            Assert.That( b.AllowDuplicates, Is.True );
            Assert.That( b.Count, Is.EqualTo( 5 ) );
            Assert.That( b, Is.Ordered );
            Assert.That( b.IndexOf( 12 ), Is.EqualTo( 2 ) );
            Assert.That( b.CheckPosition( 2 ), Is.EqualTo( 2 ) );
            Assert.That( b.CheckPosition( 3 ), Is.EqualTo( 3 ) );
        }

        [Test]
        public void ObservableSortedArrayListCovariance()
        {
            var a = new ObservableSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );
            a.Add( new Mammal( "B", 12 ) );
            a.Add( new Canidae( "A", 12, true ) );

            IReadOnlyList<Animal> baseObjects = a;
            for( int i = 0; i < baseObjects.Count; ++i ) Console.Write( baseObjects[i].Name );

            IWritableCollection<Canidae> dogs = a;
            dogs.Add( new Canidae( "C", 8, false ) );
        }

        class TestMammals : ObservableSortedArrayList<Mammal>
        {
            public TestMammals( Comparison<Mammal> m, bool allowDuplicated = false )
                : base( m, allowDuplicated )
            {
            }

            public Mammal[] Tab { get { return Store; } }
        }

        [Test]
        public void ObservableSortedArrayListCheckPos()
        {
            bool collectionChangedPass = false;
            bool propertyChangedPass = false;

            var a = new TestMammals( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ), true );
            a.Add( new Mammal( "B" ) );
            a.Add( new Mammal( "A" ) );
            a.Add( new Mammal( "C" ) );
            Assert.That( String.Join( "", a.Select( m => m.Name ) ), Is.EqualTo( "ABC" ) );

            a.PropertyChanged += ( o, e ) => propertyChangedPass = true;
            a.CollectionChanged += ( o, e ) => collectionChangedPass = true;

            a[0].Name = "Z";
            CheckList( a, "ZBC" );
            Assert.That( a.CheckPosition( 0 ), Is.EqualTo( 2 ) );
            CheckList( a, "BCZ" );

            Assert.That( collectionChangedPass, Is.True );
            Assert.That( propertyChangedPass, Is.True );
            
        }

        [Test]
        public void ObservableSortedArrayListIndexOfWithComparison()
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


        class TestInt : ObservableSortedArrayList<int>
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
                    Assert.That( Tab[i], Is.EqualTo( default( int ) ) );
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
        public void ObservableSortedArrayListAddRemove()
        {
            bool collectionChangedPass = false;
            bool propertyChangedPass = false;

            var a = new TestInt();
            a.PropertyChanged += ( o, e ) => propertyChangedPass = true;
            a.CollectionChanged += ( o, e ) => collectionChangedPass = true;
            a.CheckList();
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( -1 ) );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 0 ) );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 1 ) );

            Assert.That( a.Remove( -1 ), Is.False );
            Assert.That( a.Remove( 0 ), Is.False );
            Assert.That( a.Remove( 1 ), Is.False );

            Assert.That( collectionChangedPass, Is.False );
            Assert.That( propertyChangedPass, Is.False );

            a.Add( 204 );
            a.CheckList();
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( -1 ) );
            Assert.Throws<IndexOutOfRangeException>( () => a.RemoveAt( 1 ) );

            Assert.That( collectionChangedPass, Is.True );
            Assert.That( propertyChangedPass, Is.True );

            collectionChangedPass = false;
            propertyChangedPass = false;

            a.RemoveAt( 0 );
            Assert.That( a.Count, Is.EqualTo( 0 ) );
            a.CheckList();

            Assert.That( collectionChangedPass, Is.True );
            Assert.That( propertyChangedPass, Is.True );

        }
        
        [Test]
        public void ObservableSortedArrayListThrowExceptionTest()
        {
            var a = new ObservableSortedArrayList<Mammal>( ( a1, a2 ) => a1.Name.CompareTo( a2.Name ) );

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

        }

        [Test]
        public void ObservableSortedArrayListDoSetTest()
        {
            var a = new ObservableSortedArrayList<int>();
            a.AddRangeArray( 12, -34, 7, 545, 12 );

            //Cast IList
            IList<int> listToTest = (IList<int>)a;

            bool collectionChangedPass = false;
            bool propertyChangedPass = false;

            a.PropertyChanged += ( o, e ) => propertyChangedPass = true;
            a.CollectionChanged += ( o, e ) => collectionChangedPass = true;

            Assert.That( listToTest[0], Is.EqualTo( -34 ) );
            Assert.That( listToTest[1], Is.EqualTo( 7 ) );
            Assert.That( listToTest[2], Is.EqualTo( 12 ) );
            Assert.That( listToTest[3], Is.EqualTo( 545 ) );

            Assert.That( collectionChangedPass, Is.False );
            Assert.That( propertyChangedPass, Is.False );

            listToTest[0] = -33;
            Assert.That( listToTest[0], Is.EqualTo( -33 ) );
            listToTest[0] = 123456;
            Assert.That( listToTest[0], Is.EqualTo( 123456 ) );

            Assert.That( collectionChangedPass, Is.True );
            Assert.That( propertyChangedPass, Is.True );

            collectionChangedPass = false;
            propertyChangedPass = false;

            //Cast ICollection
            a.Clear();

            Assert.That( collectionChangedPass, Is.True );
            collectionChangedPass = false;

        }
    }
}
