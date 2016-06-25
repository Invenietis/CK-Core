#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Collection\ReadOnlyTests.cs) is part of CiviKey. 
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

using System.Reflection;
using CK.Core;
using NUnit.Framework;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core.Tests.Collection
{
    public class TestCollection<T> : ICKReadOnlyCollection<T>
    {
        public List<T> Content;

        public event EventHandler CountCalled;
        public event EventHandler ContainsCalled;

        public TestCollection()
        {
            Content = new List<T>();
        }

        public bool Contains( object item )
        {
            if( ContainsCalled != null ) ContainsCalled( this, EventArgs.Empty );
            return item is T ? Content.Contains( (T)item ) : false;
        }

        public int Count
        {
            get
            {
                if( CountCalled != null ) CountCalled( this, EventArgs.Empty );
                return Content.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Content.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Content.GetEnumerator();
        }

    }

    public class TestCollectionThatImplementsICollection<T> : ICKReadOnlyCollection<T>, ICollection<T>
    {
        public List<T> Content;

        public event EventHandler CountCalled;
        public event EventHandler ContainsCalled;

        public TestCollectionThatImplementsICollection()
        {
            Content = new List<T>();
        }

        public bool Contains( object item )
        {
            if( ContainsCalled != null ) ContainsCalled( this, EventArgs.Empty );
            return item is T ? Content.Contains( (T)item ) : false;
        }

        public int Count
        {
            get
            {
                if( CountCalled != null ) CountCalled( this, EventArgs.Empty );
                return Content.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Content.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Content.GetEnumerator();
        }


        #region ICollection<T> Members

        void ICollection<T>.Add( T item )
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains( T item )
        {
            return Contains( item );
        }

        void ICollection<T>.CopyTo( T[] array, int arrayIndex )
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove( T item )
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [TestFixture]
    public class ReadOnlyTests
    {
        [Test]
        public void linq_with_mere_IReadOnlyCollection_implementation_is_not_optimal_for_Count()
        {
            TestCollection<int> c = new TestCollection<int>();
            c.Content.Add( 2 );

            bool containsCalled = false, countCalled = false;
            c.ContainsCalled += ( o, e ) => { containsCalled = true; };
            c.CountCalled += ( o, e ) => { countCalled = true; };

            Assert.That( c.Count == 1 );
            Assert.That( countCalled, "Count property on the concrete type logs the calls." ); countCalled = false;

            Assert.That( c.Count() == 1, "Use Linq extension methods (on the concrete type)." );
            Assert.That( !countCalled, "The Linq extension method did NOT call our Count." );

            IEnumerable<int> cLinq = c;

            Assert.That( cLinq.Count() == 1, "Linq can not use our implementation..." );
            Assert.That( !countCalled, "...it did not call our Count property." );

            // Addressing the concrete type: it is our method that is called.
            Assert.That( c.Contains( 2 ) );
            Assert.That( containsCalled, "It is our Contains method that is called (not the Linq one)." ); containsCalled = false;
            Assert.That( !c.Contains( 56 ) );
            Assert.That( containsCalled, "It is our Contains method that is called." ); containsCalled = false;
            Assert.That( !c.Contains( null ), "Contains should accept ANY object without any error." );
            Assert.That( containsCalled, "It is our Contains method that is called." ); containsCalled = false;

            // Unfortunately, addressing the IEnumerable base type, Linq has no way to use our methods...
            Assert.That( cLinq.Contains( 2 ) );
            Assert.That( !containsCalled, "Linq use the enumerator to do the job." );
            Assert.That( !cLinq.Contains( 56 ) );
            Assert.That( !containsCalled );
            // Linq Contains() accept only parameter of the generic type.
            //Assert.That( !cLinq.Contains( null ), "Contains should accept ANY object without any error." );
        }

        [Test]
        public void linq_on_ICollection_implementation_uses_Count_property()
        {
            TestCollectionThatImplementsICollection<int> c = new TestCollectionThatImplementsICollection<int>();
            c.Content.Add( 2 );

            bool containsCalled = false, countCalled = false;
            c.ContainsCalled += ( o, e ) => { containsCalled = true; };
            c.CountCalled += ( o, e ) => { countCalled = true; };

            Assert.That( c.Count == 1 );
            Assert.That( countCalled, "Count property on the concrete type logs the calls." ); countCalled = false;

            IEnumerable<int> cLinq = c;

            Assert.That( cLinq.Count() == 1, "Is it our Count implementation that is called?" );
            Assert.That( countCalled, "Yes!" ); countCalled = false;

            Assert.That( c.Count() == 1, "Linq DOES use our implementation..." );
            Assert.That( countCalled, "...our Count property has been called." ); countCalled = false;

            // What's happening for Contains? 
            // The ICollection<T>.Contains( T ) is more precise than our Contains( object )...

            // Here we target the concrete type.
            Assert.That( c.Contains( 2 ) );
            Assert.That( containsCalled, "It is our Contains method that is called (not the Linq one)." ); containsCalled = false;

            // Here we use the IEnumerable<int>. 
            // It shows that this is not the (slow) enumeration that is used here: it uses a direct call to Contains that can be much more efficient.
            // It works only because TestCollectionThatImplementsICollection relays the call to our Contains.
            Assert.That( cLinq.Contains( 2 ) );
            Assert.That( containsCalled, "It is our Contains method that is called (not the Linq one)." ); containsCalled = false;

            Assert.That( !cLinq.Contains( 56 ) );
            Assert.That( containsCalled, "It is our Contains method that is called." ); containsCalled = false;

        }

        [Test]
        public void covariant_Contains_accepts_any_types()
        {
            TestCollection<Animal> c = new TestCollection<Animal>();
            Animal oneElement = new Animal(null);
            c.Content.Add( oneElement );

            bool containsCalled = false;
            c.ContainsCalled += ( o, e ) => { containsCalled = true; };
            Assert.That( c.Contains( oneElement ) );
            Assert.That( containsCalled, "It is our Contains method that is called." ); containsCalled = false;
            Assert.That( !c.Contains( 56 ), "Contains should accept ANY object without any error." );
            Assert.That( containsCalled, "It is our Contains method that is called." ); containsCalled = false;
            Assert.That( !c.Contains( null ), "Contains should accept ANY object without any error." );
            Assert.That( containsCalled ); containsCalled = false;
        }

        class StringInt : IComparable<int>
        {
            public readonly string Value;
            public StringInt( string value ) { Value = value; }

            public int CompareTo( int other )
            {
                return Int32.Parse( Value ).CompareTo( other );
            }
        }

        [TestCase( "", 5, ~0 )]
        [TestCase( "1", 5, ~1 )]
        [TestCase( "1", -5, ~0 )]
        [TestCase( "1,2,5", 5, 2 )]
        [TestCase( "1,2,5", 4, ~2 )]
        [TestCase( "1,2,5", 2, 1 )]
        [TestCase( "1,2,5", 1, 0 )]
        [TestCase( "1,2,5", 0, ~0 )]
        public void BinarySearch_on_IComparable_TValue_items( string values, int search, int resultIndex )
        {
            var a = values.Split( new[]{','},StringSplitOptions.RemoveEmptyEntries ).Select( v => new StringInt( v ) ).ToArray();
            Assert.That( Util.BinarySearch( a, search ), Is.EqualTo( resultIndex ) );
        }

        [Test]
        public void IndexOf_on_IReadOnlyList()
        {
            IReadOnlyList<int> l = new[] { 3, 7, 9, 1, 3, 8 };
            Assert.That( l.IndexOf( i => i == 3 ), Is.EqualTo( 0 ) );
            Assert.That( l.IndexOf( i => i == 7 ), Is.EqualTo( 1 ) );
            Assert.That( l.IndexOf( i => i == 8 ), Is.EqualTo( 5 ) );
            Assert.That( l.IndexOf( i => i == 0 ), Is.EqualTo( -1 ) );
            Assert.Throws<ArgumentNullException>( () => l.IndexOf( null ) );
        }

    }
}
