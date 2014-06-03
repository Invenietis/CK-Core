using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using CK.Core;
using CK.Monitoring;
using NUnit.Framework;

namespace CK.Mon2Htm.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "HtmlGenerator" )]
    class ActivityMonitorSerializationTests
    {
        IActivityMonitor _m;

        /// <summary>
        /// Temporary timeout: RemoveAllDuplicates has an infinite loop.
        /// </summary>
        [Test]
        public void SingleCopyDeduplicateTest()
        {
            string directoryName = "DedupTest";

            using( GrandOutput go = PrepareNewGrandOutputFolder( directoryName, 1000 ) )
            {
                go.Register( _m );

                for( int i = 0; i < 3; i++ ) _m.Error().Send( "Entry {0}", i );
            }

            // Make copies in directory
            int j = 0;
            foreach( var file in GetCkmonFilesFromDirectory( directoryName ) )
            {
                string copyFilename = String.Format( "copy-{0}.ckmon", j++ );
                File.Copy( file, Path.Combine( Path.GetDirectoryName( file ), copyFilename ) );
            }

            using( MultiLogReader r = new MultiLogReader() )
            {
                var rawLogFiles = r.Add( GetCkmonFilesFromDirectory( directoryName ) );
                Assert.That( rawLogFiles.All( x => x.Error == null ), "No exceptions were encountered while reading all files" );

                MultiLogReader.ActivityMap m = r.GetActivityMap();

                Assert.That( m.Monitors.Count == 1, "Only one monitor was logged" );
                var monitor = m.Monitors.First();
                List<ILogEntry> readEntries = new List<ILogEntry>();
                var page = monitor.ReadFirstPage( monitor.FirstEntryTime, 1000 );
                do
                {
                    readEntries.AddRange( page.Entries.Select( x => x.Entry ) );
                } while( page.ForwardPage() > 0 );

                Assert.That( readEntries.Count == 3 );
            }
        }

        [Test, Category( "LargeIOTest" )]
        public void MultiChannelDeduplicateTest()
        {
            using( GrandOutput go = new GrandOutput() )
            {
                go.Register( _m );
                GrandOutputConfiguration c = new GrandOutputConfiguration();
                bool result;
                XElement configurationXml = XDocument.Parse( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Channel>
        <Add Type=""BinaryFile"" Name=""Channel1"" Path=""./channel-test/1"" MaxCountPerFile=""1"" />
        <Add Type=""BinaryFile"" Name=""Channel3"" Path=""./channel-test/3"" MaxCountPerFile=""3"" />
        <Add Type=""BinaryFile"" Name=""Channel5"" Path=""./channel-test/5"" MaxCountPerFile=""5"" />
        <Add Type=""BinaryFile"" Name=""Channel7"" Path=""./channel-test/7"" MaxCountPerFile=""7"" />
        <Add Type=""BinaryFile"" Name=""Channel10"" Path=""./channel-test/10"" MaxCountPerFile=""10"" />
    </Channel>
</GrandOutputConfiguration>" ).Root;

                result = c.Load( configurationXml, TestHelper.ConsoleMonitor );
                Assert.That( result, Is.True, "XML could be loaded in GrandOutputConfiguration object." );
                result = go.SetConfiguration( c, TestHelper.ConsoleMonitor );
                Assert.That( result, Is.True, "GrandOutputConfiguration object was accepted by SetConfiguration." );

                // Send log entries
                for( int i = 0; i < 5; i++ ) _m.Warn().Send( "Warn {0}", i ); // Entries 0-4
                using( _m.OpenInfo().Send( "Info group (Depth 0>1)" ) ) // Entry 5
                {
                    for( int i = 0; i < 5; i++ ) _m.Trace().Send( "(Depth 1) Trace {0}", i ); // Entries 6-10
                    using( _m.OpenTrace().Send( "Info group (Depth 1>2)" ) ) // Entry 11
                    {
                        for( int i = 0; i < 10; i++ ) _m.Fatal().Send( "(Depth 2) Fatal {0}", i ); // Entries 12-21
                    } // Entry 22
                    for( int i = 0; i < 5; i++ ) _m.Info().Send( "(Depth 1) Info {0}", i ); // Entries 23-27
                } // Entry 28
            }

            // We logged the same thing in different channels/folder.
            // Testing de-duping on all of them.
            using( MultiLogReader r = new MultiLogReader() )
            {
                var rawLogFiles = r.Add( GetCkmonFilesFromDirectory( "channel-test", true ) );
                Assert.That( rawLogFiles.All( x => x.Error == null ), "No exceptions were encountered while reading all files" );

                MultiLogReader.ActivityMap m = r.GetActivityMap();

                Assert.That( m.Monitors.Count == 1, "Only one monitor was logged regardless of the channel" );
                var monitor = m.Monitors.First();
                List<ILogEntry> readEntries = new List<ILogEntry>();
                var page = monitor.ReadFirstPage( monitor.FirstEntryTime, 4 );
                do
                {
                    readEntries.AddRange( page.Entries.Select( x => x.Entry ) );
                } while( page.ForwardPage() > 0 );

                Assert.That( readEntries.Count < 30 );
            }
        }

        [Test, Category( "LargeIOTest" )]
        public void MaxEntriesPerFile()
        {
            Action<IActivityMonitor> throwEntries = new Action<IActivityMonitor>( ( IActivityMonitor m ) =>
            {
                using( m.OpenError().Send( "OpenGroup Error" ) ) // Entry 0
                {
                    for( int i = 0; i < 10; i++ )
                    {
                        m.Error().Send( "Dummy error" ); // Entries 1-10
                    }
                } // Entry 11

            } );

            List<IMulticastLogEntry> writtenEntries = new List<IMulticastLogEntry>();

            // Here, i is GrandOutput's MaxCountPerFile. Configuration is set inside.
            for( int i = 1; i <= 7; i++ )
            {
                string directoryName = String.Format( "maxEntries-{0}", i );
                writtenEntries.Clear();

                using( GrandOutput go = new GrandOutput() )
                {
                    go.Register( _m );
                    go.RegisterGlobalSink( new LogEntryGeneratorSink( ( e ) => { writtenEntries.Add( e ); } ) );

                    GrandOutputConfiguration c = new GrandOutputConfiguration();

                    bool result;

                    result = c.Load( CreateGrandOutputConfiguration( directoryName, i ), TestHelper.ConsoleMonitor );
                    Assert.That( result, Is.True, "XML could be loaded in GrandOutputConfiguration." );

                    result = go.SetConfiguration( c );
                    Assert.That( result, Is.True, "Configuration could be set in GrandOutput" );

                    throwEntries( _m );

                    result = go.SetConfiguration( c );
                    Assert.That( result, Is.True, "Configuration could be reset/flushed in GrandOutput." );
                }

                Assert.That( writtenEntries.Count == 12 );

                // Read entries
                using( MultiLogReader r = new MultiLogReader() )
                {
                    string directory = Path.Combine( TestHelper.TestFolder, directoryName );
                    Assert.That( Directory.Exists( directory ) );

                    var rawLogFiles = r.Add( GetCkmonFilesFromDirectory( directory ) );

                    rawLogFiles.Sort( ( a, b ) => String.Compare( a.FirstEntryTime.ToString(), b.FirstEntryTime.ToString() ) );

                    Assert.That( rawLogFiles.All( f => f.Error == null ), "No errors while reading files" );

                    // Check TotalEntryCount for all files but last
                    for( int j = 0; j < rawLogFiles.Count - 2; j++ )
                    {
                        Assert.That( rawLogFiles[j].TotalEntryCount == i, "First ckmon's TotalEntryCount matches with EntriesPerFile" );
                    }
                }
                TestHelper.CleanupTestFolder();
            }
        }

        /// <summary>
        /// MultiLogReader testing.
        /// Uses files.
        /// </summary>
        [Test, Category( "LargeIOTest" )]
        public void MonitorPages()
        {
            string dirName = @"ActivityMonitorPagingTest";

            List<IMulticastLogEntry> writtenMulticastLogEntries = new List<IMulticastLogEntry>();
            List<ParentedLogEntry> readLogEntries = new List<ParentedLogEntry>();

            // Prepare GrandOutput
            using( GrandOutput testGrandOutput = PrepareNewGrandOutputFolder( dirName ) )
            {
                IActivityMonitor m = new ActivityMonitor();
                testGrandOutput.Register( m );

                // Store Multicast entries, binding to GrandOutput.
                IGrandOutputSink logSink = new LogEntryGeneratorSink( ( e ) => writtenMulticastLogEntries.Add( e ) );
                testGrandOutput.RegisterGlobalSink( logSink );

                SendDummyMonitorData( m, 10 );
            }

            // GrandOutput is closed, files should be written.
            Assert.That( GetSystemActivityMonitorDumpPaths().Length == 0, "No SystemActivityMonitor dumps were created, logging system did not encounter any error." );
            Assert.That( Directory.Exists( Path.Combine( SystemActivityMonitor.RootLogPath, dirName ) ), "Configured ActivityMonitor folder was created." );

            string[] ckmonFiles = GetCkmonFilesFromDirectory( dirName );

            Assert.That( ckmonFiles.Length > 0, ".ckmon files were created." );


            MultiLogReader.ActivityMap activityMap;
            using( MultiLogReader mlr = new MultiLogReader() )
            {
                var rawLogFiles = mlr.Add( ckmonFiles );

                Assert.That( rawLogFiles.All( f => f.Error == null ), "No log files encountered errors while reading." );

                activityMap = mlr.GetActivityMap();
            }

            Assert.That( activityMap.FirstEntryDate > DateTime.MinValue );
            Assert.That( activityMap.LastEntryDate < DateTime.MaxValue );
            Assert.That( activityMap.LastEntryDate >= activityMap.FirstEntryDate );

            Assert.That( activityMap.ValidFiles.Count == activityMap.AllFiles.Count, "All files are valid files." );
            Assert.That( activityMap.Monitors.Count == 1, "Only one monitor was used." );

            MultiLogReader.Monitor monitor = activityMap.Monitors.First();

            Assert.That( monitor.FirstDepth == 0, "Monitor starts without any group depth." );
            Assert.That( monitor.LastDepth == 0, "Monitor ends without any group depth." );
            Assert.That( monitor.FirstEntryTime > DateTimeStamp.MinValue );
            Assert.That( monitor.LastEntryTime < DateTimeStamp.MaxValue );
            Assert.That( monitor.LastEntryTime > monitor.FirstEntryTime );

            int entriesPerPage = 30;
            var page = monitor.ReadFirstPage( monitor.FirstEntryTime, entriesPerPage );

            // Add all entries in our readLogEntries.
            do
            {
                Assert.That( page.PageLength == entriesPerPage );
                readLogEntries.AddRange( page.Entries );
            } while( page.ForwardPage() > 0 );

            int i = 0;
            ParentedLogEntry previouslyReadEntry = null; // Previous is kept here for debugging
            foreach( var parentedLogEntry in readLogEntries )
            {
                ILogEntry writtenEntry = writtenMulticastLogEntries[i];
                ILogEntry readEntry = parentedLogEntry.Entry;

                Assert.That( parentedLogEntry.IsMissing == false, "No entry should be missing." );

                AssertLogEntryEquivalence( writtenEntry, readEntry );

                previouslyReadEntry = parentedLogEntry;
                i++;
            }
        }

        #region Static utilities

        public static string[] GetSystemActivityMonitorDumpPaths()
        {
            string path = Path.Combine( SystemActivityMonitor.RootLogPath, "SystemActivityMonitor" );

            return Directory.GetFiles( path );
        }

        public static string[] GetCkmonFilesFromDirectory( string directoryName, bool recurse = false )
        {
            SearchOption so = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles( Path.Combine( TestHelper.TestFolder, directoryName ), "*.ckmon", so );

            Array.Sort( files, ( a, b ) => String.Compare( a, b ) );

            return files;
        }

        public static GrandOutput PrepareNewGrandOutputFolder( string grandOutputFolderName = "Default", int entriesPerFile = 1000 )
        {
            GrandOutput go = new GrandOutput();

            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( CreateGrandOutputConfiguration( grandOutputFolderName, entriesPerFile ),
                TestHelper.ConsoleMonitor ) );

            Assert.That( go.SetConfiguration( c ) );

            return go;
        }

        public static XElement CreateGrandOutputConfiguration( string grandOutputDirectoryName, int entriesPerFile )
        {
            string pathEntry = String.Format( @"Path=""./{0}/""", grandOutputDirectoryName );
            return XDocument.Parse(
                    String.Format( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Channel>
        <Add Type=""BinaryFile"" Name=""GlobalCatch"" {0} MaxCountPerFile=""{1}"" />
    </Channel>
</GrandOutputConfiguration>",
                            pathEntry, entriesPerFile ) ).Root;
        }

        public static void SendDummyMonitorData( IActivityMonitor m, int countPerEntry = 10 )
        {
            m.Output.RegisterClient( new CK.Core.ActivityMonitorErrorCounter( true ) );

            CKTrait t1 = ActivityMonitor.Tags.Context.FindOrCreate( "DummyTrait" );
            CKTrait t2 = ActivityMonitor.Tags.Context.FindOrCreate( "DummyTrait2" );
            CKTrait tg = ActivityMonitor.Tags.Context.FindOrCreate( "GroupTrait" );

            using( m.OpenFatal().Send( "Fatal group", t1 ) )
            {
                for( int i = 0; i < countPerEntry; i++ ) m.Fatal().Send( tg, "Fatal Entry {0}", i );
                using( m.OpenError().Send( t1, "Error group" ) )
                {
                    for( int i = 0; i < countPerEntry; i++ ) m.Error().Send( "Error Entry {0}", i );
                    using( m.OpenWarn().Send( t2, "Warn group" ) )
                    {
                        for( int i = 0; i < countPerEntry; i++ ) m.Warn().Send( "Warn Entry {0}", i );
                        using( m.OpenInfo().Send( t1, "Info group" ) )
                        {
                            for( int i = 0; i < countPerEntry; i++ ) m.Info().Send( "Info Entry {0}", i );
                            using( m.OpenTrace().Send( t2, "Trace group" ) )
                            {
                                for( int i = 0; i < countPerEntry; i++ ) m.Trace().Send( tg, "Trace Entry {0}", i );
                            }
                        }
                    }
                }

                for( int i = 0; i < countPerEntry; i++ ) m.Trace().Send( t1, "Trace Entry {0}", i );
                for( int i = 0; i < countPerEntry; i++ ) m.Info().Send( t2, "Info Entry {0}", i );
                for( int i = 0; i < countPerEntry; i++ ) m.Warn().Send( "Warn Entry {0}", i );
                for( int i = 0; i < countPerEntry; i++ ) m.Error().Send( t1, "Error Entry {0}", i );
                for( int i = 0; i < countPerEntry; i++ ) m.Fatal().Send( "Fatal Entry {0}", i );
            }

            List<Exception> dummyExceptions = new List<Exception>();

            Exception e1 = new Exception( "Simple exception" );
            Exception e2 = new IOException( "Simple IO exception with Inner Exception", e1 );
            Exception e3 = new EndOfStreamException( "EOS exception with Inner", e1 );
            Exception e4 = new AggregateException( "Aggregate exception", new[] { e2, e3 } );

            dummyExceptions.Add( e1 );
            dummyExceptions.Add( e2 );
            dummyExceptions.Add( e3 );
            dummyExceptions.Add( e4 );

            foreach( var exception in dummyExceptions )
            {
                try
                {
                    throw exception;
                }
                catch( Exception ex )
                {
                    m.Error().Send( ex, "Exception log message ({0})", ex.Message );
                }
            }
        }

        /// <summary>
        /// Asserts that two log entries are equivalent.
        /// </summary>
        /// <param name="a">Compared log entry.</param>
        /// <param name="b">Compared log entry.</param>
        /// <remarks>
        /// Assertions are made in binary write order.
        /// </remarks>
        private static void AssertLogEntryEquivalence( ILogEntry a, ILogEntry b )
        {
            Assert.That( a.LogType == b.LogType );
            Assert.That( a.LogLevel == b.LogLevel );

            Assert.That( a.LogTime == b.LogTime );

            AssertConclusionsEquivalence( a.Conclusions, b.Conclusions );
            Assert.That( a.Tags.ToString() == b.Tags.ToString() );

            Assert.That( a.FileName == b.FileName );
            Assert.That( a.LineNumber == b.LineNumber );
            AssertExceptionDataEquivalence( a.Exception, b.Exception );
            Assert.That( a.Text == b.Text );
        }

        /// <summary>
        /// Asserts that two CKExceptionData are equivalent.
        /// </summary>
        /// <param name="a">Compared CKExceptionData.</param>
        /// <param name="b">Compared CKExceptionData.</param>
        /// <remarks>Since the string representations contain most of the information, we simply compare those.</remarks>
        private static void AssertExceptionDataEquivalence( CKExceptionData a, CKExceptionData b )
        {
            if( a == null && b == null ) return;
            Assert.That( a.ToString() == b.ToString() );
        }

        /// <summary>
        /// Asserts that two lists of ActivityLogGroupConclusion are data-equivalent and order-equivalent.
        /// </summary>
        /// <param name="a">List to compare.</param>
        /// <param name="b">List to compare.</param>
        private static void AssertConclusionsEquivalence( IReadOnlyList<ActivityLogGroupConclusion> a, IReadOnlyList<ActivityLogGroupConclusion> b )
        {
            if( a == null && b == null ) return;

            Assert.That( a.Count == b.Count );
            for( int i = 0; i < a.Count; i++ )
            {
                var conclusionA = a[i];
                var conclusionB = b[i];

                Assert.That( conclusionA.Text == conclusionB.Text );
                Assert.That( conclusionA.Tag.ToString() == conclusionB.Tag.ToString() );
            }
        }

        #endregion

        #region SetUp & Teardown
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
        #endregion
    }

    #region ActivityMonitor/GrandOutput clients

    /// <summary>
    /// Test class to obtain multicast log entries directly from a GrandOutput, through an action.
    /// </summary>
    /// <remarks>
    /// Action might not be called in a thread-safe way.
    /// </remarks>
    public class LogEntryGeneratorSink : IGrandOutputSink
    {
        Action<IMulticastLogEntry> _newEntry;
        object logEntryLock;

        public LogEntryGeneratorSink( Action<IMulticastLogEntry> onNewLogEntry )
        {
            if( onNewLogEntry == null ) throw new ArgumentNullException( "onNewLogEntry" );

            logEntryLock = new Object();

            _newEntry = onNewLogEntry;
        }

        private void RaiseNewLogEntry( IMulticastLogEntry entry )
        {
            lock( logEntryLock )
            {
                _newEntry( entry );
            }
        }

        #region IGrandOutputSink Members

        public void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            RaiseNewLogEntry( logEvent.Entry );
        }

        #endregion
    }

    /// <summary>
    /// Test class to create and obtain monocast log entries directly from a single monitor, through an action.
    /// </summary>
    public class MonocastLogEntryGeneratorClient : IActivityMonitorClient
    {
        readonly Action<ILogEntry> _newEntry;

        public MonocastLogEntryGeneratorClient( Action<ILogEntry> onNewEntry )
        {
            if( onNewEntry == null ) throw new ArgumentNullException( "onNewEntry" );

            _newEntry = onNewEntry;
        }

        public LogFilter MinimalFilter { get { return LogFilter.Debug; } }

        public void OnAutoTagsChanged( CKTrait newTrait )
        {
        }

        public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            ILogEntry e = LogEntry.CreateCloseGroup( group.CloseLogTime, group.GroupLevel, conclusions );

            _newEntry( e );
        }

        public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            ILogEntry e = LogEntry.CreateOpenGroup( group.GroupText, group.LogTime, group.GroupLevel, group.FileName, group.LineNumber, group.GroupTags, group.ExceptionData );

            _newEntry( e );
        }

        public void OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
        }

        public void OnUnfilteredLog( ActivityMonitorLogData data )
        {
            ILogEntry e = LogEntry.CreateLog( data.Text, data.LogTime, data.Level, data.FileName, data.LineNumber, data.Tags, CKExceptionData.CreateFrom( data.Exception ) );

            _newEntry( e );
        }
    }
    #endregion
}
