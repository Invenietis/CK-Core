#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\CollectionExtensionTests.cs) is part of CiviKey. 
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
