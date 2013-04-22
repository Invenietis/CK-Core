using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core.Tests;
using NUnit.Framework;

namespace CK.Core.Tests.Collection
{
    [TestFixture]
    [Category("SortedArrayList")]
    public class ObservableSortedArrayListTests
    {
        [Test]
        public void ObservableSortedArrayListDoMove()
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
        public void ObservableSortedArrayListAddRemove()
        {
            bool collectionChangedPass = false;
            bool propertyChangedPass = false;

            var a = new TestInt();
            a.PropertyChanged += ( o, e ) => propertyChangedPass = true;
            a.CollectionChanged += ( o, e ) => collectionChangedPass = true;
            a.CheckList();

            Assert.That( collectionChangedPass, Is.False );
            Assert.That( propertyChangedPass, Is.False );

            a.Add( 204 );
            a.CheckList();

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
        public void ObservableSortedArrayListDoSetTest()
        {
            var a = new CKObservableSortedArrayList<int>();
            a.AddRangeArray( 12, -34, 7, 545, 12 );

            //Cast IList
            IList<int> listToTest = (IList<int>)a;

            bool collectionChangedPass = false;
            bool propertyChangedPass = false;

            a.PropertyChanged += ( o, e ) => propertyChangedPass = true;
            a.CollectionChanged += ( o, e ) => collectionChangedPass = true;

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
            Assert.That( propertyChangedPass, Is.False );
            collectionChangedPass = false;

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

        class TestInt : CKObservableSortedArrayList<int>
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

        class TestMammals : CKObservableSortedArrayList<Mammal>
        {
            public TestMammals( Comparison<Mammal> m, bool allowDuplicated = false )
                : base( m, allowDuplicated )
            {
            }

            public Mammal[] Tab { get { return Store; } }
        }
    }
}
