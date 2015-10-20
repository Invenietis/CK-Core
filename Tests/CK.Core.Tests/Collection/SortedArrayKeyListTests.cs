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
using NUnit.Framework;
using CK.Core;

namespace CK.Core.Tests.Collection
{
    [TestFixture]
    [Category( "SortedArrayList" )]
    public class SortedArrayKeyListTests
    {
        [Test]
        public void sorting_Lexicographic_integers()
        {
            var a = new CKSortedArrayKeyList<int,string>( i => i.ToString() );
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
        public void SortedArrayKeyList_can_allow_duplicates()
        {
            var a = new CKSortedArrayKeyList<int, string>( i => i.ToString() );

            a.AddRangeArray( 1, 10, 100, 100, 1000, 10000, 2, 20, 3, 30, 100, 46, 56 );
            CheckList( a, 1, 10, 100, 1000, 10000, 2, 20, 3, 30, 46, 56 );

            Assert.That( a.IndexOf( 1 ), Is.EqualTo( 0 ) );
            Assert.That( a.IndexOf( 2 ), Is.EqualTo( 5 ) );
            Assert.That( a.IndexOf( 3 ), Is.EqualTo( 7 ) );

            Assert.That( a.KeyCount( "100" ), Is.EqualTo( 1 ) );

            object o;
            o = "2";
            Assert.That( a.IndexOf( o ), Is.EqualTo( 5 ) );
            o = 2;
            Assert.That( a.IndexOf( o ), Is.EqualTo( 5 ) );
            o = null;
            Assert.That( a.IndexOf( o ), Is.EqualTo( Int32.MinValue ) );
            o = new ClassToTest( "A" );
            Assert.That( a.IndexOf( o ), Is.EqualTo( Int32.MinValue ) );
            o = "42";
            Assert.That( a.Contains( o ), Is.False );

            a.Remove( "10" );
            Assert.That( a.KeyCount( "10" ), Is.EqualTo( 0 ) );
            CheckList( a, 1, 100, 1000, 10000, 2, 20, 3, 30, 46, 56 );
            a.Remove( "20" );
            CheckList( a, 1, 100, 1000, 10000, 2, 3, 30, 46, 56 );
            a.Remove( "100" );
            Assert.That( a.KeyCount( "100" ), Is.EqualTo( 0 ) );
            CheckList( a, 1, 1000, 10000, 2, 3, 30, 46, 56 );
            Assert.That( a.Remove( "Nothing" ), Is.False );
        }

        [Test]
        public void SortedArrayKeyList_does_not_accept_null_entries()
        {
            var b = new CKSortedArrayKeyList<ClassToTest, string>( i => i.ToString(), false );
            ClassToTest classToTest = new ClassToTest( "A" );

            b.Add( classToTest );
            b.Add( new ClassToTest( "B" ) );

            Assert.That( b.Contains( classToTest ), Is.True );
            Assert.That( b.IndexOf( classToTest ), Is.EqualTo( 0 ) );
            Assert.Throws<ArgumentNullException>( () => b.IndexOf( (ClassToTest)null ) );
        }

        [Test]
        public void SortedArrayKeyList_without_duplicates()
        {
            var a = new CKSortedArrayKeyList<int, string>( i => i.ToString() );
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
            o = null;
            Assert.That( a.Contains( o ), Is.False );
            o = 42;
            Assert.That( a.Contains( o ), Is.False );
            o = "42";
            Assert.That( a.Contains( o ), Is.False );

            Assert.That( !a.Add( 3 ) );
            Assert.That( !a.Add( 2 ) );
            Assert.That( !a.Add( 1 ) );

            CheckList( a.GetAllByKey( "2" ), 2 );
        }


        [Test]
        public void another_test_with_duplicates_in_SortedArrayKeyList()
        {
            var a = new CKSortedArrayKeyList<int, string>( i => (i%100).ToString(), true );
            a.AddRangeArray( 2, 1 );

            bool exists;
            Assert.That( a.GetByKey( "1", out exists ) == 1 && exists );
            Assert.That( a.GetByKey( "2", out exists ) == 2 && exists );

            Assert.That( a.Add( 102 ) );
            Assert.That( a.Add( 101 ) );
            
            int v1 = a.GetByKey( "1" );
            Assert.That( v1, Is.EqualTo( 1 ).Or.EqualTo( 101 ), "It is one or the other that is returned." );
            int v2 = a.GetByKey( "2" );
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

        class ClassToTest
        {
            public ClassToTest( string name )
            {
                Name = name;
            }

            public string Name { get; set; }

            public override string ToString()
            {
                return Name; 
            }
        }

    }
}
