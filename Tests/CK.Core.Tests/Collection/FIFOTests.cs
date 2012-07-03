#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Collection\FIFOTests.cs) is part of CiviKey. 
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
using System.Globalization;

namespace Core.Collection
{
    [TestFixture]
    public class FIFOTests
    {
        [Test]
        public void FIFOChangeCapacity()
        {
            FIFOBuffer<int> f = new FIFOBuffer<int>( 0 );
            Assert.That( f.Capacity, Is.EqualTo( 0 ) );
            AssertEmpty( f );
            f.Push( 5 );
            AssertEmpty( f );
            f.Push( 12 );
            AssertEmpty( f );
            
            f.Capacity = 1;
            Assert.That( f.Capacity, Is.EqualTo( 1 ) );
            AssertEmpty( f );
            f.Push( 5 );
            AssertContains( f, 5 );
            f.Push( 6 );
            AssertContains( f, 6 );

            f.Capacity = 2;
            Assert.That( f.Capacity, Is.EqualTo( 2 ) );
            AssertContains( f, 6 );
            f.Push( 7 );
            AssertContains( f, 6, 7 );
            f.Push( 8 );
            AssertContains( f, 7, 8 );

            f.Capacity = 4;
            Assert.That( f.Capacity, Is.EqualTo( 4 ) );
            AssertContains( f, 7, 8 );
            f.Push( 9 );
            AssertContains( f, 7, 8, 9 );
            f.Push( 10 );
            AssertContains( f, 7, 8, 9, 10 );
            f.Push( 11 );
            AssertContains( f, 8, 9, 10, 11 );

            f.Capacity = 7;
            Assert.That( f.Capacity, Is.EqualTo( 7 ) );
            AssertContains( f, 8, 9, 10, 11 );
            f.Push( 12 );
            AssertContains( f, 8, 9, 10, 11, 12 );
            f.Push( 13 );
            AssertContains( f, 8, 9, 10, 11, 12, 13 );
            f.Push( 14 );
            AssertContains( f, 8, 9, 10, 11, 12, 13, 14 );
            f.Push( 15 );
            AssertContains( f, 9, 10, 11, 12, 13, 14, 15 );

            f.Capacity = 2;
            Assert.That( f.Capacity, Is.EqualTo( 2 ) );
            AssertContains( f, 14, 15 );

            f.Capacity = 3;
            Assert.That( f.Capacity, Is.EqualTo( 3 ) );
            AssertContains( f, 14, 15 );
            f.Push( 16 );
            AssertContains( f, 14, 15, 16 );

            f.Capacity = 2;
            Assert.That( f.Capacity, Is.EqualTo( 2 ) );
            AssertContains( f, 15, 16 );

            f.Capacity = 1;
            Assert.That( f.Capacity, Is.EqualTo( 1 ) );
            AssertContains( f, 16 );

            f.Capacity = 0;
            Assert.That( f.Capacity, Is.EqualTo( 0 ) );
            AssertEmpty( f );
        }

        [Test]
        public void FIFOSupportNull()
        {
            CultureInfo c0 = CultureInfo.InvariantCulture;
            CultureInfo c1 = CultureInfo.GetCultureInfo( "fr" );

            FIFOBuffer<CultureInfo> f = new FIFOBuffer<CultureInfo>( 2 );
            AssertEmpty( f );
            
            // When calling with null, it is the IndexOf( T ) that is called
            // since T is a reference type.
            int iNull = f.IndexOf( null );
            Assert.That( iNull, Is.LessThan( 0 ) );

            f.Push( c0 );
            Assert.That( f.Contains( null ), Is.False );
            Assert.That( f.IndexOf( null ), Is.LessThan( 0 ) );
            AssertContains( f, c0 );
            
            f.Push( null );
            Assert.That( f.Count, Is.EqualTo( 2 ) );
            Assert.That( f.IndexOf( null ), Is.EqualTo( 1 ) );
            Assert.That( f.IndexOf( c0 ), Is.EqualTo( 0 ) );
            AssertContains( f, c0, null );

            f.Push( c1 );
            Assert.That( f.IndexOf( null ), Is.EqualTo( 0 ) );
            Assert.That( f.IndexOf( c1 ), Is.EqualTo( 1 ) );
            Assert.That( f.Contains( c0 ), Is.False );
            Assert.That( f.IndexOf( c0 ), Is.LessThan( 0 ) );
            AssertContains( f, null, c1 );

            f.Push( null );
            AssertContains( f, c1, null );
            f.Push( null );
            AssertContains( f, null, null );
        }

        [Test]
        public void FIFOOneValueType()
        {
            FIFOBuffer<int> f = new FIFOBuffer<int>( 1 );
            AssertEmpty( f );
            
            f.Push( 5 );
            CheckOneValueType( f, 5, 50 );
            f.Pop();
            f.Push( 0 );
            CheckOneValueType( f, 0, 5 );
            f.Push( 1 );
            CheckOneValueType( f, 1, 0 );
            f.Push( 2 );
            CheckOneValueType( f, 2, 0 );

            int iType = f.IndexOf( 2 );
            int iBoxed = f.IndexOf( (object)2 );
            Assert.That( iType == iBoxed );
        }

        private static void AssertContains<T>( FIFOBuffer<T> f, params T[] values )
        {
            Assert.That( f.Count, Is.EqualTo( values.Length ) );
            Assert.That( f.SequenceEqual( values ), Is.True );
            Assert.That( f.ToArray().SequenceEqual( values ), Is.True );
        }

        private static void AssertEmpty<T>( FIFOBuffer<T> f )
        {
            Assert.Throws<IndexOutOfRangeException>( () => Console.WriteLine( f[-1] ) );
            Assert.Throws<IndexOutOfRangeException>( () => Console.WriteLine( f[0] ) );
            Assert.Throws<IndexOutOfRangeException>( () => Console.WriteLine( f[1] ) );

            Assert.Throws<InvalidOperationException>( () => f.Pop() );
            Assert.That( f.Count, Is.EqualTo( 0 ) );
            Assert.That( f, Is.Empty );

            AssertContains( f );
        }

        private static void CheckOneValueType<T>( FIFOBuffer<T> f, T value, T otherValue ) where T : struct
        {
            CheckOnlyOneValueType<T>( f, value, otherValue );
            Assert.That( f.Pop(), Is.EqualTo( value ) );
            AssertEmpty( f );
            f.Push( value );
            CheckOnlyOneValueType<T>( f, value, otherValue );
        }

        private static void CheckOnlyOneValueType<T>( FIFOBuffer<T> f, T value, T otherValue ) where T : struct
        {
            Assert.That( f[0], Is.EqualTo( value ) );
            Assert.That( f.Contains( value ), Is.True );
            Assert.That( f.Contains( otherValue ), Is.False );
            Assert.That( f.Contains( null ), Is.False );
            Assert.That( f.SequenceEqual( new[] { value } ), Is.True );
        }
    }
}
