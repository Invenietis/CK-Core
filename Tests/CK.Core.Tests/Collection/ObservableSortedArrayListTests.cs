#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Collection\ObservableSortedArrayListTests.cs) is part of CiviKey. 
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
using System.Collections.Specialized;
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

            List<NotifyCollectionChangedAction> collectionChangedActions = new List<NotifyCollectionChangedAction>();
            List<string> propertyChangedNames = new List<string>();

            a.PropertyChanged += ( o, e ) => propertyChangedNames.Add( e.PropertyName );
            a.CollectionChanged += ( o, e ) => collectionChangedActions.Add( e.Action );

            a.AddRangeArray( 12, -34, 7, 545, 12 );
            Assert.That( collectionChangedActions.Count, Is.EqualTo( 4 ) );
            Assert.That( collectionChangedActions.All( action => action == NotifyCollectionChangedAction.Add ) );
            Assert.That( propertyChangedNames.Count, Is.EqualTo( 4*2 ) );
            Assert.That( propertyChangedNames.Where( n => n == "Count" ).Count(), Is.EqualTo( 4 ) );
            Assert.That( propertyChangedNames.Where( n => n == "Item[]" ).Count(), Is.EqualTo( 4 ) );

            collectionChangedActions.Clear();
            propertyChangedNames.Clear();

            IList<int> listToTest = (IList<int>)a;
            listToTest[0] = -33;
            Assert.That( listToTest[0], Is.EqualTo( -33 ) );
            listToTest[0] = 123456;
            Assert.That( listToTest[0], Is.EqualTo( 123456 ) );

            Assert.That( collectionChangedActions.Count, Is.EqualTo( 2 ) );
            Assert.That( collectionChangedActions.All( action => action == NotifyCollectionChangedAction.Replace ) );
            Assert.That( propertyChangedNames.Count, Is.EqualTo( 2 ) );
            Assert.That( propertyChangedNames.All( n => n == "Item[]" ) );

            collectionChangedActions.Clear();
            propertyChangedNames.Clear();

            a.Clear();

            Assert.That( collectionChangedActions.Count, Is.EqualTo( 1 ) );
            Assert.That( collectionChangedActions.All( action => action == NotifyCollectionChangedAction.Reset ) );
            Assert.That( propertyChangedNames.Count, Is.EqualTo( 2 ) );
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
