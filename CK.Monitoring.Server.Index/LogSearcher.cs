using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public LogSearcher( IndexStoreFactory storeFactory )
        {
            _analyzer = new LogIndexer.EnglishAnalyzer();
            _store = storeFactory.GetStore( DateTime.UtcNow.Date );
        }

        private Lucene.Net.Search.Query GetQuery( string searchTerm )
        {
            var queryParser = new MultiFieldQueryParser( Lucene.Net.Util.Version.LUCENE_30, new string[] { "Text" }, _analyzer );
            queryParser.AllowLeadingWildcard = true;

            var lowerTerm = DateTools.DateToString( DateTime.UtcNow.AddSeconds( -1 ), DateTools.Resolution.MILLISECOND );
            var upperTerm = DateTools.DateToString( DateTime.UtcNow, DateTools.Resolution.MILLISECOND );
            var rangeQuery = new Lucene.Net.Search.TermRangeQuery( "LogTime", lowerTerm, upperTerm, true, true );

            var termQuery = queryParser.Parse( searchTerm );
            var finalQuery = new BooleanQuery();
            finalQuery.Add( rangeQuery, Occur.MUST );
            finalQuery.Add( termQuery, Occur.MUST );
            return finalQuery;
        }

        public IReadOnlyCollection<IMulticastLogEntry> Search( string term, int start, int count )
        {
            var finalQuery = GetQuery( term );

            using( var searcher = new IndexSearcher( _store ) )
            {
                var collector = TopScoreDocCollector.Create( count, true );
                searcher.Search( finalQuery, collector );

                return GetLogs( start, count, searcher, collector );
            }
        }

        private static IReadOnlyCollection<IMulticastLogEntry> GetLogs( int start, int count, IndexSearcher searcher, TopScoreDocCollector collector )
        {
            var list = new List<IMulticastLogEntry>();
            var topDocs = collector.TopDocs( start, count );
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
