using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class CollectionExtensionTests
    {
        [Test]
        public void testing_RemoveWhereAndReturnsRemoved_extension_method()
        {
            {
                List<int> l = new List<int>();
                l.AddRangeArray( 12, 15, 12, 13, 14 );
                var r = l.RemoveWhereAndReturnsRemoved( x => x == 12 );
                Assert.That( l.Count, Is.EqualTo( 5 ) );
                Assert.That( r.Count(), Is.EqualTo( 2 ) );
                Assert.That( l.Count, Is.EqualTo( 3 ) );
            }
            {
                // Removes from and add in the same list!
                List<int> l = new List<int>();
                l.AddRangeArray( 12, 15, 12, 13, 14, 12 );
                Assert.Throws<ArgumentOutOfRangeException>( () => l.AddRange( l.RemoveWhereAndReturnsRemoved( x => x == 12 ) ) );
            }
        }

    }
}
