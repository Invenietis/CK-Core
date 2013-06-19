using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class EnumerableExtensionTests
    {
        [Test]
        public void IsSorted()
        {
            List<int> listWithDuplicate = new List<int>();
            listWithDuplicate.AddRangeArray<int>( 1, 2, 2, 3, 3, 5 );

            List<int> listWithoutDuplicate = new List<int>();
            listWithoutDuplicate.AddRangeArray<int>( 1, 2, 3, 5 );

            Assert.That( listWithDuplicate.IsSortedStrict(), Is.False );
            Assert.That( listWithDuplicate.IsSortedLarge(), Is.True );
            Assert.Throws<ArgumentNullException>( () => listWithDuplicate.IsSortedLarge( null ) );
            Assert.Throws<ArgumentNullException>( () => listWithDuplicate.IsSortedStrict( null ) );

            Assert.That( listWithoutDuplicate.IsSortedStrict(), Is.True );
            Assert.That( listWithoutDuplicate.IsSortedLarge(), Is.True );
            Assert.Throws<ArgumentNullException>( () => listWithoutDuplicate.IsSortedLarge( null ) );
            Assert.Throws<ArgumentNullException>( () => listWithoutDuplicate.IsSortedStrict( null ) );

            listWithDuplicate.Reverse(0, listWithDuplicate.Count);
            listWithoutDuplicate.Reverse(0, listWithoutDuplicate.Count);

            Assert.That( listWithDuplicate.IsSortedStrict(), Is.False );
            Assert.That( listWithDuplicate.IsSortedLarge(), Is.False );

            Assert.That( listWithoutDuplicate.IsSortedStrict(), Is.False );
            Assert.That( listWithoutDuplicate.IsSortedLarge(), Is.False );

            //test with 2 items
            listWithoutDuplicate = new List<int>();
            listWithoutDuplicate.AddRangeArray<int>( 1, 5 );

            Assert.That( listWithoutDuplicate.IsSortedStrict(), Is.True );
            Assert.That( listWithoutDuplicate.IsSortedLarge(), Is.True );


            listWithDuplicate = new List<int>();
            listWithDuplicate.AddRangeArray<int>( 5, 5 );

            Assert.That( listWithDuplicate.IsSortedStrict(), Is.False );
            Assert.That( listWithDuplicate.IsSortedLarge(), Is.True );

            listWithDuplicate.Reverse( 0, listWithDuplicate.Count );
            listWithoutDuplicate.Reverse( 0, listWithoutDuplicate.Count );

            Assert.That( listWithDuplicate.IsSortedStrict(), Is.False );
            Assert.That( listWithDuplicate.IsSortedLarge(), Is.True );

            Assert.That( listWithoutDuplicate.IsSortedStrict(), Is.False );
            Assert.That( listWithoutDuplicate.IsSortedLarge(), Is.False );

            //test with 1 items
            listWithoutDuplicate = new List<int>();
            listWithoutDuplicate.Add( 1 );

            Assert.That( listWithoutDuplicate.IsSortedStrict(), Is.True );
            Assert.That( listWithoutDuplicate.IsSortedLarge(), Is.True );

            //Empty test
            listWithoutDuplicate = new List<int>();

            Assert.That( listWithoutDuplicate.IsSortedStrict(), Is.True );
            Assert.That( listWithoutDuplicate.IsSortedLarge(), Is.True );

            listWithDuplicate = new List<int>();

            Assert.That( listWithoutDuplicate.IsSortedStrict(), Is.True );
            Assert.That( listWithoutDuplicate.IsSortedLarge(), Is.True );

            listWithDuplicate = null;
            Assert.Throws<ArgumentNullException>( () => listWithDuplicate.IsSortedLarge() );
            Assert.Throws<ArgumentNullException>( () => listWithDuplicate.IsSortedStrict() );
        }

        [Test]
        public void IndexOf()
        {
            List<int> listToTest = new List<int>();
            listToTest.AddRangeArray<int>( 1, 2 );

            Assert.That( listToTest.IndexOf( a => a == 0 ), Is.EqualTo( -1 ) );
            Assert.That( listToTest.IndexOf( a => a == 1 ), Is.EqualTo( 0 ) );
            Assert.That( listToTest.IndexOf( a => a == 2 ), Is.EqualTo( 1 ) );
            Assert.That( listToTest.IndexOf( ( a, b ) => b == listToTest.IndexOf<int>( c => c == 0 ) && a == 0 ), Is.EqualTo( -1 ) );
            Assert.That( listToTest.IndexOf( ( a, b ) => b == listToTest.IndexOf<int>( c => c == 1 ) && a == 1 ), Is.EqualTo( 0 ) );
            Assert.That( listToTest.IndexOf( ( a, b ) => b == listToTest.IndexOf<int>( c => c == 2 ) && a == 2 ), Is.EqualTo( 1 ) );

            listToTest.Add( 2 );

            Assert.That( listToTest.IndexOf( a => a == 2 ), Is.EqualTo( 1 ) );
            Assert.That( listToTest.IndexOf( ( a, idx ) => idx == 2 && a == 2 ), Is.EqualTo( 2 ) );

            Func<int,bool> nullFunc = null;
            Func<int,int,bool> nullFuncWithIndex = null;
            Assert.Throws<ArgumentNullException>( () => listToTest.IndexOf( nullFunc ) );
            Assert.Throws<ArgumentNullException>( () => listToTest.IndexOf( nullFuncWithIndex ) );
            listToTest = null;
            Assert.Throws<ArgumentNullException>( () => listToTest.IndexOf( a => a == 0 ) );
            Assert.Throws<ArgumentNullException>( () => listToTest.IndexOf( (a,idx) => a == 0 ) );
        }

        [Test]
        public void Append()
        {
            int[] t = new int[0];
            CollectionAssert.AreEqual( t.Append( 5 ), new[] { 5 } );
            CollectionAssert.AreEqual( t.Append( 2 ).Append( 5 ), new[] { 2, 5 } );
            CollectionAssert.AreEqual( t.Append( 2 ).Append( 3 ).Append( 5 ), new[] { 2, 3, 5 } );

            var tX =  t.Append( 2 ).Append( 3 ).Append( 4 ).Append( 5 );
            CollectionAssert.AreEqual( tX, new[] { 2, 3, 4, 5 } );

            var e = tX.GetEnumerator();
            Assert.Throws<InvalidOperationException>( () => Console.Write( e.Current ) );
            Assert.That( e.MoveNext() );
            Assert.That( e.Current, Is.EqualTo( 2 ) );
            e.Reset();
            Assert.That( e.MoveNext() );
            Assert.That( e.Current, Is.EqualTo( 2 ) );
            Assert.That( e.MoveNext() );
            Assert.That( e.Current, Is.EqualTo( 3 ) );
            Assert.That( e.MoveNext() );
            Assert.That( e.Current, Is.EqualTo( 4 ) );
            Assert.That( e.MoveNext() );
            Assert.That( e.Current, Is.EqualTo( 5 ) );
            Assert.That( !e.MoveNext() );
            Assert.Throws<InvalidOperationException>( () => Console.Write( e.Current ) );
            Assert.Throws<InvalidOperationException>( () => e.MoveNext() );

            t = null;
            Assert.Throws<ArgumentNullException>( () => t.Append( 5 ) );

        }

        [Test]
        public void MaxBy()
        {
            Assert.Throws<InvalidOperationException>( () => new int[0].MaxBy( i => -i ) );
            
            int[] t = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            
            Assert.That( t.MaxBy( Util.FuncIdentity ), Is.EqualTo( 12 ) );
            Assert.That( t.MaxBy( i => -i ), Is.EqualTo( 0 ) );
            Assert.That( t.MaxBy( i => (i+1)%6 == 0 ), Is.EqualTo( 5 ) );
            Assert.That( t.MaxBy( i => i.ToString() ), Is.EqualTo( 9 ), "Lexicographical ordering." );

            Assert.That( t.MaxBy( i => i, (x,y) => x - y ), Is.EqualTo( 12 ) );

            Assert.Throws<ArgumentNullException>( () => t.MaxBy<int,int>( null ) );
            Assert.Throws<ArgumentNullException>( () => t.MaxBy( Util.FuncIdentity, null ) );
            t = null;
            Assert.Throws<ArgumentNullException>( () => t.MaxBy( Util.FuncIdentity ) );
        }
    }
}
