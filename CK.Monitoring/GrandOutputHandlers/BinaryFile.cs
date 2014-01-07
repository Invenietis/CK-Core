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
    /// 
    /// </summary>
    public sealed class BinaryFile : HandlerBase, IDisposable
    {
        readonly string _configPath;
        readonly int _maxCountPerFile;
        string _path;
        DateTime _initializedTime;
        FileStream _output;
        BinaryWriter _writer;
        int _countRemainder;


        public BinaryFile( BinaryFileConfiguration config )
            : base( config )
        {
            _configPath = config.Path;
            _maxCountPerFile = config.MaxCountPerFile;
        }

        public override void Initialize( IActivityMonitor m )
        {
            using( m.OpenTrace().Send( "Initializing binary output file for configuration '{0}'.", Name ) )
            {
                if( (_path = ComputePath( m )) == null ) return;
                _initializedTime = DateTime.UtcNow;
                Directory.CreateDirectory( _path );
                OpenFile();
                _countRemainder = _maxCountPerFile;
                m.Trace().Send( "File with MaxCountPerFile = {0} is '{1}'.", _maxCountPerFile, _output.Name );
            }
        }

        private void OpenFile()
        {
            _output = FileUtil.CreateAndOpenUniqueTimedFile( _path, ".ckmon.tmp", _initializedTime, FileAccess.Write, FileShare.Read, 8, FileOptions.SequentialScan|FileOptions.WriteThrough );
            _writer = new BinaryWriter( _output );
            _writer.Write( LogReader.CurrentStreamVersion );
        }

        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            if( --_countRemainder == 0 )
            {
                _countRemainder = _maxCountPerFile;
                CloseCurrentFile();
                OpenFile();
            }
            logEvent.Entry.WriteMultiCastLogEntry( _writer );
        }

        public override void Close( IActivityMonitor m )
        {
            if( _writer != null )
            {
                using( m.OpenTrace().Send( "Closing binary output file for configuration '{0}'.", Name ) )
                {
                    CloseCurrentFile();
                }
            }
        }

        void CloseCurrentFile()
        {
            _writer.Write( (byte)0 );
            string fName = _output.Name;
            _writer.Dispose();
            File.Move( fName, fName.Substring( 0, fName.Length - 4 ) );
            _writer = null;
        }

        void IDisposable.Dispose()
        {
            Close( new SystemActivityMonitor() );
        }

        private string ComputePath( IActivityMonitor m )
        {
            string path = null;
            if( String.IsNullOrWhiteSpace( _configPath ) ) m.Error().Send( "The configured path is empty." );
            else if( FileUtil.IndexOfInvalidPathChars( _configPath ) >= 0 ) m.Error().Send( "The configured path '{0}' is invalid.", _configPath );
            else
            {
                path = _configPath;
                if( !Path.IsPathRooted( path ) )
                {
                    string logPath = SystemActivityMonitor.RootLogPath;
                    if( String.IsNullOrWhiteSpace( logPath ) ) m.Error().Send( "The relative path '{0}' requires that {1} be specified (typically in the appsettings).", _configPath, SystemActivityMonitor.AppSettingsKey );
                    else path = Path.Combine( logPath, _configPath );
                }
            }
            return FileUtil.NormalizePathSeparator( path, true ); ;
        }

    }
}
