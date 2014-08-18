#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Mon2Htm.Tests\IndexingTests.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Monitoring;
using NUnit.Framework;

namespace CK.Mon2Htm.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "HtmlGenerator" )]
    class MonitorIndexingTests
    {
        IActivityMonitor _m;
        public static readonly string SYSTEM_ACTIVITY_MONITOR_PATH = Path.Combine( TestHelper.TestFolder, "SystemActivityMonitor" );

        [SetUp]
        public void SetUp()
        {
            TestHelper.CleanupTestFolder();
            TestHelper.LogsToConsole = true;

            SystemActivityMonitor.RootLogPath = TestHelper.TestFolder;

            _m = new ActivityMonitor();
            _m.SetMinimalFilter( LogFilter.Debug );
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.CleanupTestFolder();
        }

        [Test]
        public void SinglePageMonitorIndexing()
        {
            string dirName = "GrandOutputFileWrite";

            using( GrandOutput go = ActivityMonitorSerializationTests.PrepareNewGrandOutputFolder( dirName ) )
            {
                go.Register( _m );
                // Post entries
                _m.Trace().Send( "TraceText" );
                _m.Info().Send( "InfoText" );
                _m.Warn().Send( "WarnText" );
                _m.Error().Send( "ErrorText" );
                _m.Fatal().Send( "FatalText" );
            }

            MultiLogReader r = new MultiLogReader();
            r.Add( ActivityMonitorSerializationTests.GetCkmonFilesFromDirectory( dirName ) );
            MultiLogReader.ActivityMap activityMap = r.GetActivityMap();

            Assert.That( activityMap.Monitors.Count == 1, "We only have one monitor here" );
            var monitor = activityMap.Monitors.First();

            MonitorIndexInfo info = MonitorIndexInfo.IndexMonitor( monitor, 100 );

            Assert.That( info.MonitorGuid == monitor.MonitorId );
            Assert.That( info.TotalTraceCount == 1 );
            Assert.That( info.TotalInfoCount == 1 );
            Assert.That( info.TotalErrorCount == 1 );
            Assert.That( info.TotalWarnCount == 1 );
            Assert.That( info.TotalFatalCount == 1 );

            Assert.That( info.PageLength == 100 );
            Assert.That( info.GetPageIndexOf( DateTimeStamp.MinValue ) == -1, "Unknown timestamps return -1" );
            Assert.That( info.GuessTimestampPage( DateTimeStamp.MinValue ) == -1, "Timestamps out of bounds return -1" );
        }

        [Test, Category( "LargeIOTest" )]
        public void RoundMultiPageMonitorIndexing()
        {
            string dirName = "RoundMultiPageMonitorIndexing";
            int pageLength = 30;

            using( GrandOutput go = ActivityMonitorSerializationTests.PrepareNewGrandOutputFolder( dirName ) )
            {
                go.Register( _m );

                // Post entries
                for( int i = 0; i < pageLength; i++ ) _m.Trace().Send( "TraceText" );
                for( int i = 0; i < pageLength; i++ ) _m.Info().Send( "InfoText" );
                for( int i = 0; i < pageLength; i++ ) _m.Warn().Send( "WarnText" );
                for( int i = 0; i < pageLength; i++ ) _m.Error().Send( "ErrorText" );
                for( int i = 0; i < pageLength; i++ ) _m.Fatal().Send( "FatalText" );

                using( _m.OpenWarn().Send( "WarningTest" ) )
                {
                    for( int i = 0; i < pageLength * 2 - 2; i++ ) _m.Error().Send( "ErrorText" );
                }
            }

            MultiLogReader r = new MultiLogReader();
            r.Add( ActivityMonitorSerializationTests.GetCkmonFilesFromDirectory( dirName ) );
            MultiLogReader.ActivityMap activityMap = r.GetActivityMap();

            Assert.That( activityMap.Monitors.Count == 1, "We only have one monitor here" );
            var monitor = activityMap.Monitors.First();

            MonitorIndexInfo info = MonitorIndexInfo.IndexMonitor( monitor, pageLength );

            Assert.That( info.MonitorGuid == monitor.MonitorId );

            Assert.That( info.PageCount == 7 );

            foreach( MonitorPageReference page in info.Pages )
            {
                Assert.That( page.PageLength == pageLength );
                Assert.That( page.EntryCount == pageLength );
            }

            Assert.That( info.Groups.Count == 1 );
            var group = info.Groups.First();

            Assert.That( group.OpenGroupEntry.Text == "WarningTest" );
            Assert.That( info.GetPageIndexOf( group.OpenGroupTimestamp ) == 5 );
            Assert.That( info.GetPageIndexOf( group.CloseGroupTimestamp ) == 6 );

            Assert.That( info.TotalTraceCount == pageLength );
            Assert.That( info.TotalInfoCount == pageLength );
            Assert.That( info.TotalErrorCount == pageLength * 3 - 2 );
            Assert.That( info.TotalWarnCount == pageLength + 2 );
            Assert.That( info.TotalFatalCount == pageLength );

            Assert.That( info.TotalEntryCount == pageLength * 7 );

            var t1 = info.Pages[2].FirstEntryTimestamp;
            var t2 = info.Pages[4].LastEntryTimestamp;
            Assert.That( info.GuessTimestampPage( t1 ) == 2 );
            Assert.That( info.GuessTimestampPage( t2 ) == 4 );
            Assert.That( info.GetPageIndexOf( t1 ) == 2 );
            Assert.That( info.GetPageIndexOf( t2 ) == 4 );
        }
    }
}
