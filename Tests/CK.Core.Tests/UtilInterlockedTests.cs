#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\UtilInterlockedTests.cs) is part of CiviKey. 
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
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class UtilInterlockedTests
    {
        [Test]
        public void InterlockedAdd_atomically_adds_an_item_to_an_array()
        {
            int[] a = null;
            Util.InterlockedAdd( ref a, 1 );
            Assert.That( a != null && a.Length == 1 && a[0] == 1 );
            Util.InterlockedAdd( ref a, 2 );
            Assert.That( a != null && a.Length == 2 && a[0] == 1 && a[1] == 2 );
            Util.InterlockedAdd( ref a, 3 );
            Assert.That( a != null && a.Length == 3 && a[0] == 1 && a[1] == 2 && a[2] == 3 );
        }
        
        [Test]
        public void InterlockedAdd_can_add_an_item_in_front_of_an_array()
        {
            int[] a = null;
            Util.InterlockedAdd( ref a, 1, true );
            Assert.That( a != null && a.Length == 1 && a[0] == 1 );
            Util.InterlockedAdd( ref a, 2, true  );
            Assert.That( a != null && a.Length == 2 && a[0] == 2 && a[1] == 1 );
            Util.InterlockedAdd( ref a, 3, true  );
            Assert.That( a != null && a.Length == 3 && a[0] == 3 && a[1] == 2 && a[2] == 1 );
        }

        [Test]
        public void InterlockedAddUnique_tests_the_occurrence_of_the_item()
        {
            {
                // Append
                int[] a = null;
                Util.InterlockedAddUnique( ref a, 1 );
                Assert.That( a != null && a.Length == 1 && a[0] == 1 );
                var theA = a;
                Util.InterlockedAddUnique( ref a, 1 );
                Assert.That( a, Is.SameAs( theA ) );
                Util.InterlockedAddUnique( ref a, 2 );
                Assert.That( a != null && a.Length == 2 && a[0] == 1 && a[1] == 2 );
                theA = a;
                Util.InterlockedAddUnique( ref a, 2 );
                Assert.That( a, Is.SameAs( theA ) );
            }
            {
                // Prepend
                int[] a = null;
                Util.InterlockedAddUnique( ref a, 1, true );
                Assert.That( a != null && a.Length == 1 && a[0] == 1 );
                var theA = a;
                Util.InterlockedAddUnique( ref a, 1, true );
                Assert.That( a, Is.SameAs( theA ) );
                Util.InterlockedAddUnique( ref a, 2, true );
                Assert.That( a != null && a.Length == 2 && a[0] == 2 && a[1] == 1 );
                theA = a;
                Util.InterlockedAddUnique( ref a, 2, true );
                Assert.That( a, Is.SameAs( theA ) );
            }
        }

        [Test]
        public void InterlockedRemove_an_item_from_an_array()
        {
            int[] a = new[] { 1, 2, 3, 4, 5, 6, 7 };
            Util.InterlockedRemove( ref a, 1 );
            CollectionAssert.AreEqual( a, new[] { 2, 3, 4, 5, 6, 7 } );
            Util.InterlockedRemove( ref a, 4 );
            CollectionAssert.AreEqual( a, new[] { 2, 3, 5, 6, 7 } );
            Util.InterlockedRemove( ref a, 3712 );
            CollectionAssert.AreEqual( a, new[] { 2, 3, 5, 6, 7 } );
            Util.InterlockedRemove( ref a, 7 );
            CollectionAssert.AreEqual( a, new[] { 2, 3, 5, 6 } );
            Util.InterlockedRemove( ref a, 3 );
            CollectionAssert.AreEqual( a, new[] { 2, 5, 6 } );
            Util.InterlockedRemove( ref a, 5 );
            CollectionAssert.AreEqual( a, new[] { 2, 6 } );
            Util.InterlockedRemove( ref a, 3712 );
            CollectionAssert.AreEqual( a, new[] { 2, 6 } );
            Util.InterlockedRemove( ref a, 6 );
            CollectionAssert.AreEqual( a, new[] { 2 } );
            Util.InterlockedRemove( ref a, 2 );
            CollectionAssert.AreEqual( a, Util.Array.Empty<int>() );

            var aEmpty = a;
            Util.InterlockedRemove( ref a, 2 );
            Assert.That( a, Is.SameAs( aEmpty ) );

            Util.InterlockedRemove( ref a, 3712 );
            Assert.That( a, Is.SameAs( aEmpty ) );

            a = null;
            Util.InterlockedRemove( ref a, 3712 );
            Assert.That( a, Is.Null );

        }

        [Test]
        public void InterlockedRemoveAll_items_that_match_a_condition()
        {
            int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Util.InterlockedRemoveAll( ref a, i => i % 2 == 0 );
            CollectionAssert.AreEqual( a, new[] { 1, 3, 5, 7, 9 } );
            Util.InterlockedRemoveAll( ref a, i => i % 2 != 0 );
            CollectionAssert.AreEqual( a, Util.Array.Empty<int>() );

            a = null;
            Util.InterlockedRemoveAll( ref a, i => i % 2 != 0 );
            Assert.That( a, Is.Null );

        }

        [Test]
        public void InterlockedAdd_item_under_condition()
        {
            int[] a = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var theA = a;
            Util.InterlockedAdd( ref a, i => i == 3, () => 3 );
            Assert.That( a, Is.SameAs( theA ) );

            Util.InterlockedAdd( ref a, i => i == 10, () => 10 );
            CollectionAssert.AreEqual( a, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } );

            Util.InterlockedAdd( ref a, i => i == -1, () => -1, true );
            CollectionAssert.AreEqual( a, new[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } );

            Assert.Throws<InvalidOperationException>( () => Util.InterlockedAdd( ref a, i => i == 11, () => 10 ) );

            a = null;
            Util.InterlockedAdd( ref a, i => i == 3, () => 3 );
            CollectionAssert.AreEqual( a, new[] { 3 } );
            Util.InterlockedAdd( ref a, i => i == 4, () => 4 );
            CollectionAssert.AreEqual( a, new[] { 3, 4 } );

            a = new int[0];
            Util.InterlockedAdd( ref a, i => i == 3, () => 3 );
            CollectionAssert.AreEqual( a, new[] { 3 } );
            Util.InterlockedAdd( ref a, i => i == 4, () => 4 );
            CollectionAssert.AreEqual( a, new[] { 3, 4 } );
        }
    }
}
