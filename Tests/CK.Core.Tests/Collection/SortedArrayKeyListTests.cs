#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Collection\SortedArrayKeyListTests.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;

namespace Core.Collection
{
    [TestFixture]
    public class SortedArrayKeyListTests
    {
        [Test]
        public void LexicographicIntegers()
        {
            var a = new SortedArrayKeyList<int,string>( i => i.ToString() );
            a.AddRangeArray( 1, 2, 3 );
            CheckList( a, 1, 2, 3 );

            a.AddRangeArray( 10, 20, 30 );
            CheckList( a, 1, 10, 2, 20, 3, 30 );

            a.AddRangeArray( 10, 20, 30 );
            CheckList( a, 1, 10, 2, 20, 3, 30 );

            a.AddRangeArray( 10000, 1000, 100, 10, 1, 56 );
            CheckList( a, 1, 10, 100, 1000, 10000, 2, 20, 3, 30, 56 );

            a.AddRangeArray( 10000, 1000, 100, 10, 1, 46 );
            CheckList( a, 1, 10, 100, 1000, 10000, 2, 20, 3, 30, 46, 56 );
        }

        [Test]
        public void NoDuplicateKeyedCollection()
        {
            var a = new SortedArrayKeyList<int, string>( i => i.ToString() );
            a.AddRangeArray( 3, 2, 1 );

            bool exists;
            Assert.That( a.GetByKey( "1", out exists ) == 1 && exists );
            Assert.That( a.GetByKey( "10", out exists ) == 0 && !exists );
            Assert.That( a.GetByKey( "2", out exists ) == 2 && exists );

            Assert.That( a.Contains( "2" ) );
            Assert.That( a.Contains( "1" ) );
            Assert.That( !a.Contains( "21" ) );

            object o;
            o = "2";
            Assert.That( a.Contains( o ), "Using the key." );
            o = 2;
            Assert.That( a.Contains( o ), "Using the value itself." );

            Assert.That( !a.Add( 3 ) );
            Assert.That( !a.Add( 2 ) );
            Assert.That( !a.Add( 1 ) );

            CheckList( a.GetAllByKey( "2" ), 2 );
        }


        [Test]
        public void DuplicateKeyedCollection()
        {
            var a = new SortedArrayKeyList<int, string>( i => (i%100).ToString(), true );
            a.AddRangeArray( 2, 1 );

            bool exists;
            Assert.That( a.GetByKey( "1", out exists ) == 1 && exists );
            Assert.That( a.GetByKey( "2", out exists ) == 2 && exists );

            Assert.That( a.Add( 102 ) );
            Assert.That( a.Add( 101 ) );
            
            int v1 = a.GetByKey( "1", out exists );
            Assert.That( v1, Is.EqualTo( 1 ).Or.EqualTo( 101 ), "It is one or the other that is returned." );
            int v2 = a.GetByKey( "2", out exists );
            Assert.That( v2, Is.EqualTo( 2 ).Or.EqualTo( 102 ), "It is one or the other that is returned." );

            Assert.That( a.KeyCount( "2" ) == 2 );
            CheckList( a.GetAllByKey( "2" ).OrderBy( Util.FuncIdentity ), 2, 102 );

            Assert.That( a.Add( 102 ) );
            Assert.That( a.Add( 102 ) );
            Assert.That( a.Add( 102 ) );
            Assert.That( a.Add( 202 ) );
            Assert.That( a.Add( 302 ) );

            Assert.That( a.KeyCount( "2" ) == 7 );
            CheckList( a.GetAllByKey( "2" ).OrderBy( Util.FuncIdentity ), 2, 102, 102, 102, 102, 202, 302 );

            Assert.That( a.KeyCount( "5454" ) == 0 );
            Assert.That( a.GetAllByKey( "5454" ), Is.Empty );

        }



        private static void CheckList( IEnumerable<int> a, params int[] p )
        {
            Assert.That( a.SequenceEqual( p ) );
        }

    }
}
