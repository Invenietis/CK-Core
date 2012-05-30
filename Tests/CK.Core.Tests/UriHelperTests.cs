#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\UriHelperTests.cs) is part of CiviKey. 
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

namespace CK.Core.Tests
{
    [TestFixture]
    public class UriHelperTests
    {

        void TestAssume( string result, string u, string param, string newValue )
        {
            Assert.AreEqual( result, UriHelper.AssumeUrlParameter( u, param, newValue ) );
            string prefix = "http://";
            Assert.AreEqual( new Uri( prefix + result ), new Uri( prefix + u ).AssumeUrlParameter( param, newValue ) );
        }

        [Test]
        public void AssumeUrlParameter()
        {
            TestAssume( "test.com?kilo=1", "test.com", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?", "kilo", "1" );
            TestAssume( "test.com?a=1&z=2&kilo=1", "test.com?a=1&z=2", "kilo", "1" );
            TestAssume( "test.com?a=kilo&kilo=1", "test.com?a=kilo", "kilo", "1" );
            TestAssume( "test.com?kilo=1&z=2", "test.com?kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?kilo=1&z=2", "test.com?kilo=kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?kilo=1&ki=1", "test.com?kilo=&ki=1", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?kilo=j", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?kilo", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?kilo=", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1&z=2", "test.com?a=z&kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1&z=2", "test.com?a=z&kilo=kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1&ki=1", "test.com?a=z&kilo=&ki=1", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1", "test.com?a=z&kilo=j", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1", "test.com?a=z&kilo", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1", "test.com?a=z&kilo=", "kilo", "1" );

            Assert.That( UriHelper.RemoveUrlParameter( "test.com?a=z&kilo=1", "a" ) == "test.com?kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?a=z&kilo=1", "kilo" ) == "test.com?a=z" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?a=z&toto=6&kilo=1", "toto" ) == "test.com?a=z&kilo=1" );

            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "a" ) == "test.com?&kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "kilo" ) == "test.com?&a=z" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&toto=6&kilo=1", "toto" ) == "test.com?&a=z&kilo=1" );

            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "edhoe" ) == "test.com?&a=z&kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "" ) == "test.com?&a=z&kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", null ) == "test.com?&a=z&kilo=1" );
        }

    }
}
