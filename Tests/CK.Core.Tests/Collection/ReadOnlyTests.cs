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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core.Tests.Collection
{
    public class TestCollection<T> : IReadOnlyCollection<T>
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

        [ExcludeFromCodeCoverage]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Content.GetEnumerator();
        }

    }

    public class TestCollectionThatImplementsICollection<T> : IReadOnlyCollection<T>, ICollection<T>
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

        [ExcludeFromCodeCoverage]
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
        public void TestImplementationNaked()
        {
            TestCollection<int> c = new TestCollection<int>();
            c.Content.Add( 2 );

            bool containsCalled = false, countCalled = false;
            c.ContainsCalled += ( o, e ) => { containsCalled = true; };
            c.CountCalled += ( o, e ) => { countCalled = true; };

            Assert.That( c.Count == 1 );
            Assert.That( countCalled, "Count property on the concrete type logs the calls." ); countCalled = false;

            Assert.That( c.Count() == 1, "Use Linq extension methods (on the concrete type)." );
            Assert.That( !countCalled, "The Linq extension method dit NOT call our Count." );

            IEnumerable<int> cLinq = c;

            Assert.That( cLinq.Count() == 1, "Linq can not use our implementation..." );
            Assert.That( !countCalled, "...it dit not call our Count property." );

            // Adressing the concrete type: it is our method that is called.
            Assert.That( c.Contains( 2 ) );
            Assert.That( containsCalled, "It is our Contains method that is called (not the Linq one)." ); containsCalled = false;
            Assert.That( !c.Contains( 56 ) );
            Assert.That( containsCalled, "It is our Contains method that is called." ); containsCalled = false;
            Assert.That( !c.Contains( null ), "Contains should accept ANY object without any error." );
            Assert.That( containsCalled, "It is our Contains method that is called." ); containsCalled = false;

            // Unfortunately, adressing the IEnumerable base type, Linq has no way to use our methods...
            Assert.That( cLinq.Contains( 2 ) );
            Assert.That( !containsCalled, "Linq use the enumerator to do the job." );
            Assert.That( !cLinq.Contains( 56 ) );
            Assert.That( !containsCalled );
            // Linq Contains() accept only parameter of the generic type.
            //Assert.That( !cLinq.Contains( null ), "Contains should accept ANY object without any error." );
        }

        [Test]
        public void TestImplementationWithICollection()
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
        public void TestReferenceTypes()
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

        [Test]
        public void TestToReadOnly()
        {
            Func<int,int> nullConvertor = null;
            {
                IList<int> source = new int[0];
                Assert.That( source.ToReadOnlyCollection( Util.FuncIdentity ).SequenceEqual( source ) );
                Assert.That( ((IEnumerable<int>)source).ToReadOnlyCollection().SequenceEqual( source ) );
            }
            {
                IList<int> source = new int[]{ 1 }; 
                Assert.That( source.ToReadOnlyCollection( Util.FuncIdentity ).SequenceEqual( source ) );
            }
            {
                IList<int> source = new int[] { 0, 1, 2, 3, 4, 5 };
                Assert.That( source.ToReadOnlyCollection().SequenceEqual( source ) );
                Assert.That( source.ToReadOnlyCollection( 0 ).SequenceEqual( source ) );
                Assert.That( source.ToReadOnlyCollection( 1 ).SequenceEqual( new int[] { 1, 2, 3, 4, 5 } ) );
                Assert.That( source.ToReadOnlyCollection( 2 ).SequenceEqual( new int[] { 2, 3, 4, 5 } ) );
                Assert.That( source.ToReadOnlyCollection( 3 ).SequenceEqual( new int[] { 3, 4, 5 } ) );
                Assert.That( source.ToReadOnlyCollection( 4 ).SequenceEqual( new int[] { 4, 5 } ) );
                Assert.That( source.ToReadOnlyCollection( 5 ).SequenceEqual( new int[] { 5 } ) );
                Assert.That( source.ToReadOnlyCollection( 6 ).SequenceEqual( new int[] { } ) );
                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( 7 ) );

                Assert.That( source.ToReadOnlyCollection( 0, 5 ).SequenceEqual( new int[] { 0, 1, 2, 3, 4 } ) );
                Assert.That( source.ToReadOnlyCollection( 1, 4 ).SequenceEqual( new int[] { 1, 2, 3, 4 } ) );
                Assert.That( source.ToReadOnlyCollection( 2, 3 ).SequenceEqual( new int[] { 2, 3, 4 } ) );
                Assert.That( source.ToReadOnlyCollection( 3, 2 ).SequenceEqual( new int[] { 3, 4 } ) );
                Assert.That( source.ToReadOnlyCollection( 3, 1 ).SequenceEqual( new int[] { 3 } ) );
                Assert.That( source.ToReadOnlyCollection( 3, 0 ).SequenceEqual( new int[] { } ) );

                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( 3, 98 ) );
                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( 3, -1 ) );
                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( 77, 1 ) );
                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( 1, -1 ) );
                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( -1, 1 ) );
                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( -1, 1, Util.FuncIdentity ) );
                Assert.Throws<ArgumentOutOfRangeException>( () => source.ToReadOnlyCollection( 0, -1, Util.FuncIdentity ) );

                int[] squareSource = new int[] { 0, 1, 2 * 2, 3 * 3, 4 * 4, 5 * 5 };
                Assert.That( source.ToReadOnlyCollection( i => i * i ).SequenceEqual( squareSource ) );
                Assert.That( source.ToReadOnlyCollection( 0, i => i * i ).SequenceEqual( squareSource ) );
                Assert.That( source.ToReadOnlyCollection( 1, i => i * i ).SequenceEqual( squareSource.Skip( 1 ) ) );
                Assert.That( source.ToReadOnlyCollection( 2, i => i * i ).SequenceEqual( squareSource.Skip( 2 ) ) );
                Assert.That( source.ToReadOnlyCollection( 3, i => i * i ).SequenceEqual( squareSource.Skip( 3 ) ) );
                Assert.That( source.ToReadOnlyCollection( 4, i => i * i ).SequenceEqual( squareSource.Skip( 4 ) ) );
                Assert.That( source.ToReadOnlyCollection( 5, i => i * i ).SequenceEqual( squareSource.Skip( 5 ) ) );
                Assert.That( source.ToReadOnlyCollection( 6, i => i * i ).SequenceEqual( squareSource.Skip( 6 ) ) );

                Assert.That( source.ToReadOnlyCollection( 0, 5, i => i * i ).SequenceEqual( squareSource.Take( 5 ) ) );
                Assert.That( source.ToReadOnlyCollection( 1, 4, i => i * i ).SequenceEqual( squareSource.Skip( 1 ).Take( 4 ) ) );
                Assert.That( source.ToReadOnlyCollection( 2, 3, i => i * i ).SequenceEqual( squareSource.Skip( 2 ).Take( 3 ) ) );
                Assert.That( source.ToReadOnlyCollection( 3, 2, i => i * i ).SequenceEqual( squareSource.Skip( 3 ).Take( 2 ) ) );
                Assert.That( source.ToReadOnlyCollection( 3, 1, i => i * i ).SequenceEqual( squareSource.Skip( 3 ).Take( 1 ) ) );
                Assert.That( source.ToReadOnlyCollection( 3, 0, i => i * i ).SequenceEqual( squareSource.Skip( 3 ).Take( 0 ) ) );

                Assert.Throws<ArgumentNullException>( () => source.ToReadOnlyCollection( nullConvertor ) );
                Assert.Throws<ArgumentNullException>( () => source.ToReadOnlyCollection( 1, nullConvertor ) );
                Assert.Throws<ArgumentNullException>( () => source.ToReadOnlyCollection( 1, 2, nullConvertor ) );

                source = null;
                Assert.Throws<NullReferenceException>( () => source.ToReadOnlyCollection() );
                Assert.Throws<NullReferenceException>( () => source.ToReadOnlyCollection( Util.FuncIdentity ) );
                Assert.Throws<NullReferenceException>( () => source.ToReadOnlyCollection( 0, Util.FuncIdentity ) );
                Assert.Throws<NullReferenceException>( () => source.ToReadOnlyCollection( 0, 1, Util.FuncIdentity ) );
            }
            {
                ICollection<int> source = new int[] { 0, 1, 2, 3, 4, 5 };
                Assert.That( source.ToReadOnlyCollection().SequenceEqual( source ) );

                int[] squareSource = new int[] { 0, 1, 2 * 2, 3 * 3, 4 * 4, 5 * 5 };
                Assert.That( source.ToReadOnlyCollection( i => i * i ).SequenceEqual( squareSource ) );

                source = new int[0];
                Assert.That( source.ToReadOnlyCollection().SequenceEqual( source ) );
                Assert.That( source.ToReadOnlyCollection( i => i * i ).SequenceEqual( source ) );

                source = new int[]{ 1 };
                Assert.That( source.ToReadOnlyCollection().SequenceEqual( source ) );
                Assert.That( source.ToReadOnlyCollection( i => i * i ).SequenceEqual( source ) );
                Assert.Throws<ArgumentNullException>( () => source.ToReadOnlyCollection( nullConvertor ) );

                source = null;
                Assert.Throws<NullReferenceException>( () => source.ToReadOnlyCollection() );
                Assert.Throws<NullReferenceException>( () => source.ToReadOnlyCollection( Util.FuncIdentity ) );
            }
            {
                IEnumerable<int> source = new int[] { 0, 1, 2, 3, 4, 5 };
                Assert.That( source.ToReadOnlyCollection().SequenceEqual( source ) );
                Assert.That( source.Skip( 1 ).ToReadOnlyCollection().SequenceEqual( source.Skip( 1 ) ) );
                Assert.That( source.Skip( 2 ).ToReadOnlyCollection().SequenceEqual( source.Skip( 2 ) ) );
                Assert.That( source.Skip( 3 ).ToReadOnlyCollection().SequenceEqual( source.Skip( 3 ) ) );
                Assert.That( source.Skip( 4 ).ToReadOnlyCollection().SequenceEqual( source.Skip( 4 ) ) );
                Assert.That( source.Skip( 5 ).ToReadOnlyCollection().SequenceEqual( source.Skip( 5 ) ) );
                Assert.That( source.Skip( 6 ).ToReadOnlyCollection().SequenceEqual( source.Skip( 6 ) ) );

                source = null;
                Assert.Throws<NullReferenceException>( () => source.ToReadOnlyCollection() );
            }
        }


        [Test]
        public void ToAndAsReadOnlyList()
        {
            List<int> netList = new List<int>();
            CKSortedArrayList<int> ckList = new CKSortedArrayList<int>();
            int[] array = new int[1];

            IReadOnlyList<int> r;
            {
                r = netList.ToReadOnlyList();
                Assert.That( r, Is.Not.SameAs( netList ), "ToReadOnlyList always duplicates the content." );
                r = ckList.ToReadOnlyList();
                Assert.That( r, Is.Not.SameAs( ckList ), "ToReadOnlyList always duplicates the content." );
                r = array.ToReadOnlyList();
                Assert.That( r, Is.Not.SameAs( array ), "ToReadOnlyList always duplicates the content." );
            }
#if net40
            r = netList.AsReadOnlyList();
            Assert.That( r, Is.SameAs( CKReadOnlyListEmpty<int>.Empty ), "In Net40, the List<T> is NOT a IReadOnlyList." );
            r = ckList.AsReadOnlyList();
            Assert.That( r, Is.SameAs( ckList ), "Lists from CK.Core are already IReadOnlyList<T>." );
            
            netList.Add( 1 );
            ckList.Add( 1 );
            r = netList.AsReadOnlyList();
            Assert.That( r, Is.Not.SameAs( netList ).And.Not.Empty );
            r = ckList.AsReadOnlyList();
            Assert.That( r, Is.SameAs( ckList ).And.Not.Empty );
            r = array.AsReadOnlyList();
            Assert.That( r, Is.Not.SameAs( array ).And.Not.Empty, "In 4.0, an array is NOT a IReadOnlyList." );
#else
            r = netList.AsReadOnlyList();
            Assert.That( r, Is.SameAs( netList ), "In Net45, List<T> IS A IReadOnlyList<T>." );
            r = ckList.AsReadOnlyList();
            Assert.That( r, Is.SameAs( ckList ) );

            netList.Add( 1 );
            ckList.Add( 1 );
            r = netList.AsReadOnlyList();
            Assert.That( r, Is.SameAs( netList ).And.Not.Empty );
            r = ckList.AsReadOnlyList();
            Assert.That( r, Is.SameAs( ckList ).And.Not.Empty );
            r = array.AsReadOnlyList();
            Assert.That( r, Is.SameAs( array ).And.Not.Empty, "In 4.5, an array IS a IReadOnlyList :-)." );

            {
                IReadOnlyList<int> roNetList = netList, roCkList = ckList, roArray = array;
                r = roNetList.ToReadOnlyList();
                Assert.That( r, Is.Not.SameAs( roNetList ), "ToReadOnlyList always duplicates the content." );
                r = roCkList.ToReadOnlyList();
                Assert.That( r, Is.Not.SameAs( roCkList ), "ToReadOnlyList always duplicates the content." );
                r = roArray.ToReadOnlyList();
                Assert.That( r, Is.Not.SameAs( roArray ), "ToReadOnlyList always duplicates the content." );
            }

#endif
        }
        
        [Test]
        public void TestToReadOnlyListAdapter()
        {
            var john = new Mammal( "John" );
            List<Mammal> m = new List<Mammal>() { john, new Mammal( "Paul" ) };
            var a = new ReadOnlyListOnIList<Animal, Mammal>( m );
            Assert.That( a.Count, Is.EqualTo( 2 ) );
            Assert.That( a[0].Name, Is.EqualTo( "John" ) );
            Assert.That( a.IndexOf( john ), Is.EqualTo( 0 ) );
            Assert.That( a.IndexOf( this ), Is.EqualTo( Int32.MinValue ) );
            CollectionAssert.AreEqual( a, m );
            Assert.That( a.Inner, Is.SameAs( m ) );

            Assert.That( a.Contains( john ) );
            Assert.That( a.Contains( this ), Is.False );

            Assert.That( ((IList<Animal>)a).IndexOf( john ), Is.EqualTo( 0 ) );
            Assert.That( ((IList<Animal>)a).Contains( john ), Is.True );
            Assert.That( ((IList<Animal>)a)[1].Name, Is.EqualTo( "Paul" ) );
            Assert.That( ((IList<Animal>)a).IsReadOnly );
            var copy = new Animal[2];
            ((IList<Animal>)a).CopyTo( copy, 0 );
            Assert.That( copy[0], Is.SameAs( john ) );
            Assert.That( copy[1].Name, Is.EqualTo( "Paul" ) );
            
            Assert.Throws<ArgumentNullException>( () => a.Inner = null );
            Assert.Throws<NotSupportedException>( () => a.AddRange( m ) );
            Assert.Throws<NotSupportedException>( () => ((IList<Animal>)a).Clear() );
            Assert.Throws<NotSupportedException>( () => ((IList<Animal>)a).Remove( john ) );
            Assert.Throws<NotSupportedException>( () => ((IList<Animal>)a).RemoveAt( 5 ) );
            Assert.Throws<NotSupportedException>( () => ((IList<Animal>)a).Insert( 1, john ) );
            Assert.Throws<NotSupportedException>( () => ((IList<Animal>)a)[0] = john );
        }

        [Test]
        public void TestReadOnlyConverter()
        {
            Dictionary<Guid,IUniqueId> dic = new Dictionary<Guid, IUniqueId>();
            CKReadOnlyCollectionTypeConverter<IUniqueId,Guid> export = new CKReadOnlyCollectionTypeConverter<IUniqueId, Guid>( dic.Keys, g => dic[g], uid => uid.UniqueId );

            dic.Add( SimpleUniqueId.Empty.UniqueId, SimpleUniqueId.Empty );
            Assert.That( export.Count == 1 );
            Assert.That( export.Contains( SimpleUniqueId.Empty ) );
            Assert.That( !export.Contains( Guid.Empty ), "Inner object is hidden." );
            Assert.That( export.First( u => u.UniqueId == Guid.Empty ) == SimpleUniqueId.Empty );

            dic.Add( SimpleUniqueId.InvalidId.UniqueId, SimpleUniqueId.InvalidId );
            Assert.That( export.Count == 2 );

            Assert.That( export.Contains( SimpleUniqueId.Empty ) );
            Assert.That( export.First( u => u.UniqueId == Guid.Empty ) == SimpleUniqueId.Empty );

            Assert.That( export.Contains( SimpleUniqueId.InvalidId ) );
            Assert.That( !export.Contains( SimpleUniqueId.InvalidId.UniqueId ), "Inner object is hidden." );
            Assert.That( export.First( u => u.UniqueId == SimpleUniqueId.InvalidId.UniqueId ) == SimpleUniqueId.InvalidId );



        }
    }
}
