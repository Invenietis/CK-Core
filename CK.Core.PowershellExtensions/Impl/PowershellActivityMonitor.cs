using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.PowershellExtensions.Impl
{
    internal class PowershellActivityMonitor : IPowershellActivityMonitor
    {
        ActivityMonitor _monitor;
        TemporaryFile _underlyingFile;

        public PowershellActivityMonitor( bool createConsoleClient = false )
        {
            _monitor = new ActivityMonitor();
            _underlyingFile = new TemporaryFile();

            if( createConsoleClient )
                _monitor.Output.RegisterUniqueClient( ( c ) => true, () => new ActivityMonitorConsoleClient() );

            _monitor.Output.RegisterClient( new ActivityMonitorTextWriterClient( ( s ) =>
            {
                File.AppendAllText( _underlyingFile.Path, s );
            } ) );
        }

        #region IPowershellActivityMonitor members

        public void Clear()
        {
            _underlyingFile.Dispose();
            _underlyingFile = new TemporaryFile();
        }

        public IEnumerable<string> ReadAllLines()
        {
            return File.ReadLines( _underlyingFile.Path );
        }

        public void WriteError( string log )
        {
            _monitor.Error().Send( log );
        }

        public void WriteFatal( string log )
        {
            _monitor.Fatal().Send( log );
        }

        public void WriteInfo( string log )
        {
            _monitor.Info().Send( log );
        }

        public void WriteTrace( string log )
        {
            _monitor.Trace().Send( log );
        }

        public void WriteWarn( string log )
        {
            _monitor.Warn().Send( log );
        }

        #endregion

        #region IActivityMonitor members

        public LogFilter ActualFilter
        {
            get { return _monitor.ActualFilter; }
        }

        public CKTrait AutoTags
        {
            get { return _monitor.AutoTags; }
            set { _monitor.AutoTags = value; }
        }

        public void CloseGroup( DateTime logTimeUtc, object userConclusion = null )
        {
            _monitor.CloseGroup( logTimeUtc, userConclusion );
        }

        public LogFilter MinimalFilter
        {
            get { return _monitor.MinimalFilter; }
            set { _monitor.MinimalFilter = value; }
        }

        public IActivityMonitorOutput Output
        {
            get { return _monitor.Output; }
        }

        public void SetTopic( string newTopic, string fileName = null, int lineNumber = 0 )
        {
            _monitor.SetTopic( newTopic, fileName, lineNumber );
        }

        public string Topic
        {
            get { return _monitor.Topic; }
        }

        public void UnfilteredLog( ActivityMonitorLogData data )
        {
            _monitor.UnfilteredLog( data );
        }

        public IDisposableGroup UnfilteredOpenGroup( ActivityMonitorGroupData data )
        {
            return _monitor.UnfilteredOpenGroup( data );
        }

        #endregion
    }
}
