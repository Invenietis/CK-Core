#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\CoreExtensionTests.cs) is part of CiviKey. 
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

namespace Core
{

    [TestFixture]
    public class CoreExtensionTest
    {
        [Test]
        public void TestLog2()
        {
            // Stupid raw test to avoid a buggy test ;-)
            Assert.That( Util.Math.Log2( 1 ) == 0 );
            Assert.That( Util.Math.Log2( 2 ) == 1 );
            Assert.That( Util.Math.Log2( 4 ) == 2 );
            Assert.That( Util.Math.Log2( 8 ) == 3 );
            Assert.That( Util.Math.Log2( 16 ) == 4 );
            Assert.That( Util.Math.Log2( 32 ) == 5 );
            Assert.That( Util.Math.Log2( 64 ) == 6 );
            Assert.That( Util.Math.Log2( 128 ) == 7 );
            Assert.That( Util.Math.Log2( 256 ) == 8 );
            Assert.That( Util.Math.Log2( 512 ) == 9 );
            Assert.That( Util.Math.Log2( 1024 ) == 10 );
            Assert.That( Util.Math.Log2( 2048 ) == 11 );
            Assert.That( Util.Math.Log2( 4096 ) == 12 );
            Assert.That( Util.Math.Log2( 8192 ) == 13 );
            Assert.That( Util.Math.Log2( 16384 ) == 14 );
            Assert.That( Util.Math.Log2( 32768 ) == 15 );
            Assert.That( Util.Math.Log2( 65536 ) == 16 );
            Assert.That( Util.Math.Log2( 131072 ) == 17 );
            Assert.That( Util.Math.Log2( 262144 ) == 18 );
            Assert.That( Util.Math.Log2( 524288 ) == 19 );
            Assert.That( Util.Math.Log2( 1048576 ) == 20 );
            Assert.That( Util.Math.Log2( 2097152 ) == 21 );
            Assert.That( Util.Math.Log2( 4194304 ) == 22 );
            Assert.That( Util.Math.Log2( 8388608 ) == 23 );
            Assert.That( Util.Math.Log2( 16777216 ) == 24 );
            Assert.That( Util.Math.Log2( 33554432 ) == 25 );
            Assert.That( Util.Math.Log2( 67108864 ) == 26 );
            Assert.That( Util.Math.Log2( 134217728 ) == 27 );
            Assert.That( Util.Math.Log2( 268435456 ) == 28 );
            Assert.That( Util.Math.Log2( 536870912 ) == 29 );
            Assert.That( Util.Math.Log2( 1073741824 ) == 30 );
            Assert.That( Util.Math.Log2( 2147483648 ) == 31 );

            Assert.That( Util.Math.Log2( 4 + 2 + 1 ) == 2 );
            Assert.That( Util.Math.Log2( 64 + 25 ) == 6 );
            Assert.That( Util.Math.Log2( 512 + 255 ) == 9 );
            Assert.That( Util.Math.Log2( 65536 + 8723 ) == 16 );
            Assert.That( Util.Math.Log2( 67108864 + 72868 ) == 26 );
            Assert.That( Util.Math.Log2( 536870912 + 897397 ) == 29 );
            Assert.That( Util.Math.Log2( 2147483648 + 76575 ) == 31 );
                             
            Assert.That( Util.Math.Log2ForPower2( 4 ) == 2 );
            Assert.That( Util.Math.Log2ForPower2( 64 ) == 6 );
            Assert.That( Util.Math.Log2ForPower2( 512 ) == 9 );
            Assert.That( Util.Math.Log2ForPower2( 65536 ) == 16 );
            Assert.That( Util.Math.Log2ForPower2( 67108864 ) == 26 );
            Assert.That( Util.Math.Log2ForPower2( 536870912 ) == 29 );
            Assert.That( Util.Math.Log2ForPower2( 2147483648 ) == 31 );
                             
            Assert.That( Util.Math.Log2ForPower2( 4 + 2 + 1 ) != 2 );
            Assert.That( Util.Math.Log2ForPower2( 64 + 25 ) != 6 );
            Assert.That( Util.Math.Log2ForPower2( 512 + 255 ) != 9 );
            Assert.That( Util.Math.Log2ForPower2( 65536 + 8723 ) != 16 );
            Assert.That( Util.Math.Log2ForPower2( 67108864 + 72868 ) != 26 );
            Assert.That( Util.Math.Log2ForPower2( 536870912 + 897397 ) != 29 );
            Assert.That( Util.Math.Log2ForPower2( 2147483648 + 9879633 ) != 31 );

        }

        [Test]
        public void TestCountBits()
        {
            Assert.That( Util.Math.BitCount( 0 ) == 0 );
            Assert.That( Util.Math.BitCount( 4 ) == 1 );
            Assert.That( Util.Math.BitCount( 32 ) == 1 );
            Assert.That( Util.Math.BitCount( 128 ) == 1 );
            Assert.That( Util.Math.BitCount( 1 ) == 1 );
            Assert.That( Util.Math.BitCount( 1 | 2 ) == 2 );
            Assert.That( Util.Math.BitCount( 1 | 2 | 4 ) == 3 );
            Assert.That( Util.Math.BitCount( 1 | 2 | 64 ) == 3 );
            Assert.That( Util.Math.BitCount( 1 | 2 | 128 ) == 3 );
            Assert.That( Util.Math.BitCount( 1 | 2 | 32 | 128 ) == 4 );
            Assert.That( Util.Math.BitCount( 1 | 2 | 32 | 64 | 128 ) == 5 );
            Assert.That( Util.Math.BitCount( 1 | 2 | 4 | 8 | 16 | 32 | 64 | 128 ) == 8 );
        }
    }
}
