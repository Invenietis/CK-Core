#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\CriticalErrorCollectorTests.cs) is part of CiviKey. 
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

namespace CK.Core.Tests
{
    [TestFixture]
    public class CriticalErrorCollectorTests
    {

        [Test]
        public void ValidArguments()
        {
            CriticalErrorCollector c = new CriticalErrorCollector();
            Assert.Throws<ArgumentNullException>( () => c.Add( null, "" ) );
            Assert.DoesNotThrow( () => c.Add( new Exception( "A" ), null ) );
            c.Add( new Exception( "B" ), "Comment" );
            
            var errors = c.ToArray();
            Assert.That( errors[0].ToString(), Is.EqualTo( " - A" ) );
            Assert.That( errors[1].ToString(), Is.EqualTo( "Comment - B" ) );
        }

    }
}
