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

namespace CK.Monitoring.Tests.Live
{
    [TestFixture( Category = "ActivityMonitor.Index" )]
    public class LogSearcherTest
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
        public void QueryTheIndexShouldReturnsIndexedLog()
        {
            IndexStoreFactory storeFactory = new IndexStoreFactory( _baseIndexPath );
            using( LogSearcher searcher = new LogSearcher( storeFactory ) )
            {
                IndexOneLog( storeFactory );
                Assert.That( searcher.Search( "*", TimeSpan.FromSeconds( 1 ) ).Count > 0 );
            }
        }

        private void IndexOneLog( IndexStoreFactory storeFactory )
        {
            LogEntryDispatcher d = new LogEntryDispatcher();
            using( LogIndexer indexer = new LogIndexer( d, storeFactory ) )
            {
                Thread.Sleep( 500 ); // Lets the indexer Thread initialized
                d.DispatchLogEntry( CreateLogEntry( "Indexer should receive this log entry!" ) );
                Thread.Sleep( 100 ); // Lets the indexer Thread initialized
            }
        }
    }
}
