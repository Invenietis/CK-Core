#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\ActivityMonitorFilterPropagation.cs) is part of CiviKey. 
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

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    public class ActivityMonitorFilterPropagation
    {
        [Test]
        public void ThreadSafeOnClientMinimalFilterChanged()
        {
            var monitor = new ActivityMonitor( false );
            var c = monitor.Output.RegisterClient( new ActivityMonitorClientTester() );
            Parallel.For( 0, 20, i => c.AsyncSetMinimalFilterBlock( new LogFilter( LogLevelFilter.Info, (LogLevelFilter)(i % 5 + 1) ), 1 ) );

        }

    }
}
