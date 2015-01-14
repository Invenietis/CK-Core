#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.ActivityMonitorAdapters.Tests\CommonLoggingTests.cs) is part of CiviKey. 
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
using Common.Logging;
using NLog.Targets;
using NUnit.Framework;

namespace CK.Core.ActivityMonitorAdapters.Tests
{
    [TestFixture]
    class CommonLoggingTests
    {
        MemoryTarget _target;
        IActivityMonitor _m;

        [Test]
        public void CommonLoggingViaNLogTest()
        {
            NLogTests.NLogOutputTest( _m, _target );
        }

        [SetUp]
        public void SetUp()
        {
            // Configure Common.Logging
            var properties = new Common.Logging.Configuration.NameValueCollection();
            properties.Add( "configType", "INLINE" );

            ILoggerFactoryAdapter adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter( properties );
            Common.Logging.LogManager.Adapter = adapter;

            // Configure NLog
            _target = new MemoryTarget();
            _target.Name = "mo";
            _target.Layout = "${longdate}\t${uppercase:${level}}\t${message}\t${exception:format=tostring}";

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging( _target, NLog.LogLevel.Trace );

            // Start Common.Logging
            CommonLoggingAdapter.Initialize();

            _m = new ActivityMonitor();
        }

        [TearDown]
        public void TearDown()
        {
            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
        }
    }
}
