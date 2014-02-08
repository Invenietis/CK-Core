using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Binary file handler.
    /// </summary>
    public sealed class BinaryFile : HandlerBase
    {
        readonly MonitorBinaryFileOutput _file;

        public BinaryFile( BinaryFileConfiguration config )
            : base( config )
        {
            _file = new MonitorBinaryFileOutput( config.Path, config.MaxCountPerFile );
            _file.FileWriteThrough = config.FileWriteThrough;
            _file.FileBufferSize = config.FileBufferSize;
        }

        public override void Initialize( IActivityMonitor m )
        {
            using( m.OpenTrace().Send( "Initializing binary output file for configuration '{0}' (MaxCountPerFile = {1}).", Name, _file.MaxCountPerFile ) )
            {
                _file.Initialize( m );
            }
        }

        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            _file.Write( logEvent.Entry );
        }

        public override void Close( IActivityMonitor m )
        {
            _file.Close();
        }

    }

}
