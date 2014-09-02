using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace CK.Monitoring.Server.Index
{
    public class LogIndexer : IDisposable
    {
        LogEntryDispatcher _dispatcher;
        Lucene.Net.Store.Directory _indexDirectory;
        Thread _indexerThread;

        IndexStoreFactory _storeFactory;
        IActivityMonitor _monitor;
        EnglishAnalyzer _analyzer;

        public LogIndexer( LogEntryDispatcher dispatcher, IndexStoreFactory storeFactory )
        {
            if( dispatcher == null ) throw new ArgumentNullException( "dispatcher" );
            if( storeFactory == null ) throw new ArgumentNullException( "storeFactory" );

            _storeFactory = storeFactory;
            _indexDirectory = _storeFactory.GetStore( DateTime.UtcNow.Date );
            _dispatcher = dispatcher;

            _indexerThread = new Thread( Index );
            _indexerThread.IsBackground = true;
            _indexerThread.Start();
        }

        private void Index()
        {
            _monitor = new ActivityMonitor( "Indexer" );
            _analyzer = new EnglishAnalyzer();
            _dispatcher.LogEntryReceived += ( sender, e ) =>
            {
                var log = e.LogEntry;
                if( log != null )
                {
                    using( _monitor.OpenTrace().Send( "Indexing entry {0}.", e.LogEntry.LogTime ) )
                    {
                        if( !String.IsNullOrEmpty( log.Text ) )
                        {
                            using( IndexWriter writer = new IndexWriter( _indexDirectory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED ) )
                            {
                                var doc = new Lucene.Net.Documents.Document();
                                doc.Add( new Field( "Text", e.LogEntry.Text, Field.Store.YES, Field.Index.ANALYZED ) );
                                _monitor.Trace().Send( "Add Text Field." );

                                doc.Add( new NumericField( "LogTime", Field.Store.YES, true ).SetLongValue( log.LogTime.TimeUtc.Ticks ) );
                                _monitor.Trace().Send( "Add LogTime Field." );

                                doc.Add( new Field( "Version", LogReader.CurrentStreamVersion.ToString(), Field.Store.YES, Field.Index.NO ) );
                                _monitor.Trace().Send( "Add Version Field." );

                                using( MemoryStream ms = new MemoryStream() )
                                using( BinaryWriter bw = new BinaryWriter( ms ) )
                                {
                                    log.WriteLogEntry( bw );
                                    doc.Add( new Field( "RawLog", ms.ToArray(), Field.Store.YES ) );
                                }
                                _monitor.Trace().Send( "Add RawLog Field." );

                                writer.AddDocument( doc );
                                _monitor.Trace().Send( "Document added." );
                            }
                        }
                        else
                        {
                            _monitor.Warn().Send( "Skip log. Text is null or empty." );
                        }
                    }
                }
            };
        }

        internal class EnglishAnalyzer : Lucene.Net.Analysis.Standard.StandardAnalyzer
        {
            public EnglishAnalyzer()
                : base( Lucene.Net.Util.Version.LUCENE_30 )
            {
            }

            public override TokenStream TokenStream( string fieldName, System.IO.TextReader reader )
            {
                var stream =  base.TokenStream( fieldName, reader );
                stream = new Lucene.Net.Analysis.PorterStemFilter( stream );
                //stream = new Lucene.Net.Analysis.En.KStemFilter( stream );
                return stream;
            }
        }

        public void Dispose()
        {
            _indexerThread.Join();

            if( _analyzer != null ) _analyzer.Dispose();
            _storeFactory.ReleaseStore( _indexDirectory );
        }
    }
}
