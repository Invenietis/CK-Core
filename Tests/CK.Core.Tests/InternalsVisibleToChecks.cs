#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\InternalsVisibleToChecks.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
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
    public class InternalsVisibleToChecks
    {
        [Test]
        public void SuccessfulAccessToCKCoreInternals()
        {
            string msg;
            var d = CK.Core.ActivityMonitor.DependentToken.CreateWithMonitorTopic( TestHelper.ConsoleMonitor, true, out msg );
            Assert.That( msg, Is.StringEnding( "." ), "Just a silly test." );
        }
    }
}
