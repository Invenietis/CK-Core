using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class EnumerableExtensionTests
    {
        [Test]
        public void EnumerableExtensionTestsIsSorted()
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
        }

        [Test]
        public void EnumerableExtensionTestsIndexOf()
        {
            List<int> listToTest = new List<int>();
            listToTest.AddRangeArray<int>( 1, 2 );

            Assert.That( listToTest.IndexOf<int>( a => a == 0 ), Is.EqualTo( -1 ) );
            Assert.That( listToTest.IndexOf<int>( a => a == 1 ), Is.EqualTo( 0 ) );
            Assert.That( listToTest.IndexOf<int>( a => a == 2 ), Is.EqualTo( 1 ) );
            Assert.That( listToTest.IndexOf<int>( ( a, b ) => b == listToTest.IndexOf<int>( c => c == 0 ) && a == 0 ), Is.EqualTo( -1 ) );
            Assert.That( listToTest.IndexOf<int>( ( a, b ) => b == listToTest.IndexOf<int>( c => c == 1 ) && a == 1 ), Is.EqualTo( 0 ) );
            Assert.That( listToTest.IndexOf<int>( ( a, b ) => b == listToTest.IndexOf<int>( c => c == 2 ) && a == 2 ), Is.EqualTo( 1 ) );

            listToTest.Add( 2 );

            Assert.That( listToTest.IndexOf<int>( a => a == 2 ), Is.EqualTo( 1 ) );
            Assert.That( listToTest.IndexOf<int>( ( a, b ) => b == 2 && a == 2 ), Is.EqualTo( 2 ) );
        }
    }
}
