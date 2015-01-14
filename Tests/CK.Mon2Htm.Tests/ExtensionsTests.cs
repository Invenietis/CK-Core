#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Mon2Htm.Tests\ExtensionsTests.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;

namespace CK.Mon2Htm.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "HtmlGenerator" )]
    public class ExtensionsTests
    {
        [Test]
        public void DateTimeStampToAndFromBytes()
        {
            DateTimeStamp t = new DateTimeStamp( DateTime.UtcNow, GetRandomByte() );

            byte[] bytes = t.ToBytes();

            Assert.That( bytes.Length == 9 );

            DateTimeStamp t2 = MonitoringExtensions.CreateDateTimeStampFromBytes( bytes );

            Assert.That( t == t2 );

            Assert.Throws( typeof( ArgumentException ), () => { MonitoringExtensions.CreateDateTimeStampFromBytes( BitConverter.GetBytes( DateTime.MaxValue.ToBinary() ) ); } );
        }

        [Test]
        public void DateTimeStampToAndFromBase64String()
        {
            DateTimeStamp t = new DateTimeStamp( DateTime.UtcNow, GetRandomByte() );

            string s = t.ToBase64String();

            DateTimeStamp t2 = MonitoringExtensions.CreateDateTimeStampFromBase64( s );

            Assert.That( t == t2 );
        }

        private static byte GetRandomByte()
        {
            Random random = new Random();
            byte[] bytes = new byte[1];
            random.NextBytes( bytes );

            return bytes[0];
        }
    }
}
