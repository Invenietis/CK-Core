using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.Server;
using CK.Monitoring.Server.Index;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
    [TestFixture( Category = "ActivityMonitor.Index" )]
    public class LogIndexerTest
    {
        string _baseIndexPath;
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
            Directory.CreateDirectory( SystemActivityMonitor.RootLogPath );
            Directory.CreateDirectory( _baseIndexPath = Path.Combine( TestHelper.TestFolder, "Index" ) );
        }

        IMulticastLogEntry CreateLogEntry( string text, CKExceptionData exception = null )
        {
            return LogEntry.CreateMulticastLog( Guid.NewGuid(), LogEntryType.Line, DateTimeStamp.UtcNow, 0, text, DateTimeStamp.UtcNow, LogLevel.Info, "", 0, null, exception );
        }

        [Test]
        public void DispatcherShouldSendLogToTheLogIndexer_AndIndexerShouldIndexIt()
        {
            LogEntryDispatcher d = new LogEntryDispatcher();

            IndexStoreFactory storeFactory = new IndexStoreFactory( _baseIndexPath );
            using( LogIndexer indexer = new LogIndexer( d, storeFactory ) )
            {
                Thread.Sleep( 500 ); // Lets the indexer Thread initialized
                d.LogEntryReceived += OnLogEntryReceived;
                d.DispatchLogEntry( CreateLogEntry( "Indexer should receive this log entry!" ) );

                Assert.That( _received, Is.True );
                d.LogEntryReceived -= OnLogEntryReceived;
            }

            var store = storeFactory.GetStore( DateTime.UtcNow.Date );
            Assert.That( store.ListAll().Length > 0 );
            storeFactory.ReleaseStore( store );

        }

        bool _received = false;

        void OnLogEntryReceived( object sender, LogEntryEventArgs e )
        {
            _received = true;
            Assert.That( e.LogEntry.Text == "Indexer should receive this log entry!" );
        }
    }
}
