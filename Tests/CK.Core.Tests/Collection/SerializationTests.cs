#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Collection\SerializationTests.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Collection
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void FIFOBuffer_is_serializable()
        {
            var b = new FIFOBuffer<int>( 50 );
            b.Push( 1 );
            b.Push( 2 );
            b.Push( 3 );
            b.Push( 4 );
            b.Pop();
            b.Pop();
            var b2 = TestHelper.SerializationCopy( b );
            Assert.That( b2.Pop(), Is.EqualTo( 3 ) );
            Assert.That( b2.Pop(), Is.EqualTo( 4 ) );
            Assert.That( b2.Count, Is.EqualTo( 0 ) );
        }


        [Test]
        public void CKReadOnlyListEmpty_is_a_serializable_singleton()
        {
            Assert.That( TestHelper.SerializationCopy( CKReadOnlyListEmpty<int>.Empty ), Is.SameAs( CKReadOnlyListEmpty<int>.Empty ) );
        }

        [Test]
        public void CKReadOnlyListMono_is_serializable()
        {
            Assert.That( TestHelper.SerializationCopy( new CKReadOnlyListMono<int>( 3712 ) ).Single(), Is.EqualTo( 3712 ) );
        }

        [Test]
        public void CKReadOnlyListOnIList_is_serializable()
        {
            var l = new CKReadOnlyListOnIList<int>( new int[] { 1, 5 } );
            var l2 = TestHelper.SerializationCopy( l );
            Assert.That( l2, Is.Not.SameAs( l ) );
            CollectionAssert.AreEqual( l, l2 );
        }

        [Test]
        public void CKReadOnlyCollectionOnICollection_is_serializable()
        {
            var l = new CKReadOnlyCollectionOnICollection<int>( new int[] { 1, 5, 45, 8 } );
            var l2 = TestHelper.SerializationCopy( l );
            Assert.That( l2, Is.Not.SameAs( l ) );
            CollectionAssert.AreEqual( l, l2 );
        }

        [Test]
        public void CKReadOnlyCollectionOnISet_is_serializable()
        {
            var l = new CKReadOnlyCollectionOnISet<int>( new HashSet<int>() { 1, 5, 45, 8 } );
            var l2 = TestHelper.SerializationCopy( l );
            Assert.That( l2, Is.Not.SameAs( l ) );
            CollectionAssert.AreEqual( l, l2 );
        }

    }
}
