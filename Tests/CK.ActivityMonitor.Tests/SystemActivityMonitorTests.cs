#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\SystemActivityMonitorTests.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    public class SystemActivityMonitorTests
    {
        [SetUp]
        public void CleanLogs()
        {
            var logs = Path.Combine( TestHelper.TestFolder, SystemActivityMonitor.SubDirectoryName );
            if( Directory.Exists( logs ) ) Directory.Delete( logs, true );
            SystemActivityMonitor.RootLogPath = TestHelper.TestFolder;
        }

        [Test]
        public void SimpleTest()
        {
            bool eventHasBeenRaised = false;
            var h = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>(
                    delegate( object sender, SystemActivityMonitor.LowLevelErrorEventArgs e )
                    {
                        Assert.That( e.ErrorWhileWritingLogFile, Is.Null );
                        Assert.That( e.ErrorMessage, Is.StringContaining( "The-Test-Exception-Message" ) );
                        Assert.That( e.ErrorMessage, Is.StringContaining( "Produced by SystemActivityMonitorTests.SimpleTest" ) );
                        Assert.That( File.ReadAllText( e.FullLogFilePath ), Is.EqualTo( e.ErrorMessage ) );
                        eventHasBeenRaised = true;
                    } );
            SystemActivityMonitor.OnError += h;
            try
            {
                ActivityMonitor.CriticalErrorCollector.Add( new CKException( "The-Test-Exception-Message" ), "Produced by SystemActivityMonitorTests.SimpleTest" );
                ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                Assert.That( eventHasBeenRaised );
            }
            finally
            {
                SystemActivityMonitor.OnError -= h;
            }
        }

        [Test]
        public void OnErrorEventIsSecured()
        {
            int eventHandlerCount = 0;
            int buggyEventHandlerCount = 0;

            var hGood = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>( ( sender, e ) => { ++eventHandlerCount; } );
            var hBad = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>( ( sender, e ) => { ++buggyEventHandlerCount; throw new Exception( "From buggy handler." ); } );
            SystemActivityMonitor.OnError += hGood;
            SystemActivityMonitor.OnError += hBad;
            try
            {
                ActivityMonitor.CriticalErrorCollector.Add( new CKException( "The-Test-Exception-Message" ), "First call to SystemActivityMonitorTests.OnErrorEventIsSecured" );
                ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                Assert.That( eventHandlerCount, Is.EqualTo( 2 ), "We also received the error of the buggy handler :-)." );
                Assert.That( buggyEventHandlerCount, Is.EqualTo( 1 ) );

                ActivityMonitor.CriticalErrorCollector.Add( new CKException( "The-Test-Exception-Message" ), "Second call to SystemActivityMonitorTests.OnErrorEventIsSecured" );
                ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                Assert.That( eventHandlerCount, Is.EqualTo( 3 ) );
                Assert.That( buggyEventHandlerCount, Is.EqualTo( 1 ) );
            }
            finally
            {
                SystemActivityMonitor.OnError -= hGood;
                SystemActivityMonitor.OnError -= hBad;
            }
        }

    }
}
