using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace CK.Monitoring.Server.Index
{
    public class LogSearcher : IDisposable
    {
        LogIndexer.EnglishAnalyzer _analyzer;
        Lucene.Net.Store.Directory _store;
        IndexStoreFactory _storeFactory;

        public LogSearcher( IndexStoreFactory storeFactory )
        {
            if( storeFactory == null ) throw new ArgumentNullException( "storeFactory" );

            _storeFactory = storeFactory;
            _analyzer = new LogIndexer.EnglishAnalyzer();
            _store = _storeFactory.GetStore( DateTime.UtcNow.Date );
        }

        private Lucene.Net.Search.Query GetQuery( string searchTerm, TimeSpan delta )
        {
            var queryParser = new MultiFieldQueryParser( Lucene.Net.Util.Version.LUCENE_30, new string[] { "Text" }, _analyzer );
            queryParser.AllowLeadingWildcard = true;

            var lowerTerm = DateTime.UtcNow.Add( delta.Negate() ).Ticks;
            var upperTerm = DateTime.UtcNow.Ticks;
            var rangeQuery = Lucene.Net.Search.NumericRangeQuery.NewLongRange( "LogTime", lowerTerm, upperTerm, true, false );

            var termQuery = queryParser.Parse( searchTerm );
            var finalQuery = new BooleanQuery();
            finalQuery.Add( rangeQuery, Occur.MUST );
            finalQuery.Add( termQuery, Occur.MUST );
            return finalQuery;
        }

        public IReadOnlyCollection<IMulticastLogEntry> Search( string term, TimeSpan delta )
        {
            var finalQuery = GetQuery( term, delta );
            if( _store.ListAll().Length > 0 )
            {
                using( var searcher = new IndexSearcher( _store, true ) )
                {
                    var collector = TopScoreDocCollector.Create( 1000, true );
                    searcher.Search( finalQuery, collector );

                    return GetLogs( searcher, collector );
                }
            }
            return CK.Core.CKReadOnlyListEmpty<IMulticastLogEntry>.Empty;
        }

        private static IReadOnlyCollection<IMulticastLogEntry> GetLogs( IndexSearcher searcher, TopScoreDocCollector collector )
        {
            var list = new List<IMulticastLogEntry>();
            var topDocs = collector.TopDocs();
            foreach( ScoreDoc doc in topDocs.ScoreDocs )
            {
                Document hitdocument = searcher.IndexReader.Document( doc.Doc );

                using( MemoryStream ms = new MemoryStream() )
                {
                    using( BinaryWriter writer = new BinaryWriter( ms, Encoding.UTF8, leaveOpen: true ) )
                    {
                        writer.Write( hitdocument.GetBinaryValue( "RawLog" ) );
                    }
                    ms.Seek( 0, SeekOrigin.Begin );
                    using( BinaryReader reader = new BinaryReader( ms, Encoding.UTF8 ) )
                    {
                        bool eof = false;
                        int version = Int32.Parse( hitdocument.Get( "Version" ) );
                        var entry = (IMulticastLogEntry)LogEntry.Read( reader, version, out eof );
                        list.Add( entry );
                    }
                }
            }
            return list;
        }

        public void Dispose()
        {
            _analyzer.Dispose();
            _store.Dispose();
        }
    }
}
