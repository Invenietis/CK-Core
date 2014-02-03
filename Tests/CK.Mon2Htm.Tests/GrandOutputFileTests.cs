using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CK.Core;
using CK.Monitoring;
using NUnit.Framework;

namespace CK.Mon2Htm.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "HtmlGenerator" )]
    public class GrandOutputFileTests
    {
        IActivityMonitor _m;

        [SetUp]
        public void SetUp()
        {
            TestHelper.CleanupTestFolder();
            TestHelper.LogsToConsole = true;

            SystemActivityMonitor.RootLogPath = TestHelper.TestFolder;

            _m = new ActivityMonitor();
            _m.SetFilter( LogFilter.Debug );
        }

        [TearDown]
        public void TearDown()
        {
            TestHelper.CleanupTestFolder();
        }

        [Test]
        public void GrandOutputFileWriteRead()
        {
            string dirName = "GrandOutputFileWriteRead";
            using( GrandOutput go = ActivityMonitorSerializationTests.PrepareNewGrandOutputFolder( dirName ) )
            {
                go.Register( _m );
                // Post entries
                _m.Trace().Send( "TraceText" );
                _m.Info().Send( "InfoText" );
                _m.Warn().Send( "WarnText" );
                _m.Error().Send( "ErrorText" );
                _m.Fatal().Send( "FatalText" );

                using( _m.OpenFatal().Send( "FatalOpen" ) )
                {
                    _m.Error().Send( "ErrorGroupLine" );
                    Exception e = new Exception( "ExceptionMessage" );
                    _m.Info().Send( e, "LogMessage" );
                }

            }

            // Read monitor
            MultiLogReader r = new MultiLogReader();
            r.Add( ActivityMonitorSerializationTests.GetCkmonFilesFromDirectory( dirName ) );
            MultiLogReader.ActivityMap activityMap = r.GetActivityMap();

            Assert.That( activityMap.AllFiles.All( file => file.Error == null ), "No files encountered errors during reading." );
            Assert.That( activityMap.AllFiles.Count == activityMap.ValidFiles.Count, "All files are valid." );
            Assert.That( activityMap.Monitors.Count == 1, "We only have one monitor here" );
            var monitor = activityMap.Monitors.First();

            List<ILogEntry> logEntryList = new List<ILogEntry>();

            var page = monitor.ReadFirstPage( monitor.FirstEntryTime, 9 );
            do
            {
                logEntryList.AddRange( page.Entries.Select( x => x.Entry ) );
            } while( page.ForwardPage() > 0 );

            // Check log entries
            Assert.That( logEntryList[0].LogLevel.HasFlag( LogLevel.Trace ) && logEntryList[0].Text == "TraceText" && logEntryList[0].LogType == LogEntryType.Line );
            Assert.That( logEntryList[1].LogLevel.HasFlag( LogLevel.Info ) && logEntryList[1].Text == "InfoText" && logEntryList[1].LogType == LogEntryType.Line );
            Assert.That( logEntryList[2].LogLevel.HasFlag( LogLevel.Warn ) && logEntryList[2].Text == "WarnText" && logEntryList[2].LogType == LogEntryType.Line );
            Assert.That( logEntryList[3].LogLevel.HasFlag( LogLevel.Error ) && logEntryList[3].Text == "ErrorText" && logEntryList[3].LogType == LogEntryType.Line );
            Assert.That( logEntryList[4].LogLevel.HasFlag( LogLevel.Fatal ) && logEntryList[4].Text == "FatalText" && logEntryList[4].LogType == LogEntryType.Line );

            Assert.That( logEntryList[5].LogLevel.HasFlag( LogLevel.Fatal ) && logEntryList[5].Text == "FatalOpen" && logEntryList[5].LogType == LogEntryType.OpenGroup );

            Assert.That( logEntryList[6].LogLevel.HasFlag( LogLevel.Error ) && logEntryList[6].Text == "ErrorGroupLine" && logEntryList[6].LogType == LogEntryType.Line );

            Assert.That( logEntryList[7].LogLevel.HasFlag( LogLevel.Info ) && logEntryList[7].Text == "LogMessage" && logEntryList[7].LogType == LogEntryType.Line );
            Assert.That( logEntryList[7].Exception.Message == "ExceptionMessage" );

            Assert.That( logEntryList[8].LogLevel.HasFlag( LogLevel.Fatal ) && logEntryList[8].LogType == LogEntryType.CloseGroup );
        }

        [Test]
        public static void GrandOutputFileWrite()
        {
            IActivityMonitor _m = new ActivityMonitor();

            using( GrandOutput go = ActivityMonitorSerializationTests.PrepareNewGrandOutputFolder( @"GrandOutputFileWrite", 1000 ) )
            {
                go.Register( _m );

                _m.Trace().Send( "Trace" );
                _m.Info().Send( "Info" );
                _m.Warn().Send( "Warn" );
                _m.Error().Send( "Error" );
                _m.Fatal().Send( "Fatal" );
            }

            var ckmonFiles = ActivityMonitorSerializationTests.GetCkmonFilesFromDirectory( "GrandOutputFileWrite" );

            Assert.That( ckmonFiles.Length == 1, "A single .ckmon file exists." );
        }

    }
}
