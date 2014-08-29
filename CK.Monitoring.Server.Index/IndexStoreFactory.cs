using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server.Index
{
    public class IndexStoreFactory
    {
        string _basePath;

        public IndexStoreFactory( string basePath )
        {
            _basePath = basePath;
        }

        public Lucene.Net.Store.Directory GetStore( DateTime date )
        {
            return GetStore( Path.Combine( _basePath, date.ToString( "yyyy-MM-dd" ) ) );
        }

        public void ReleaseStore( Lucene.Net.Store.Directory store )
        {
            store.Dispose();
        }

        protected virtual Lucene.Net.Store.Directory GetStore( string path )
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( path );
            if( !directoryInfo.Exists ) directoryInfo.Create();

            return Lucene.Net.Store.FSDirectory.Open( directoryInfo );
        }
    }
}
