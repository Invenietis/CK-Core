using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Helper class that encapsulates temporary stream and final renaming for log entries streams.
    /// This currently handles only the maximum count of entries per file but this may be extended with options like "SubFolderMode" that can be based 
    /// on current time (to group logs inside timed intermediate folders like one per day: 2014/01/12 or 2014-01/12, etc.). 
    /// </summary>
    public class MonitorBinaryFileOutput : IDisposable
    {
        readonly string _configPath;
        readonly int _maxCountPerFile;
        readonly string _fileNameSuffix;

        string _basePath;
        FileStream _output;
        BinaryWriter _writer;
        DateTime _openedTimeUtc;
        int _countRemainder;
        int _fileBufferSize;
        bool _fileWriteThrough;

        MonitorBinaryFileOutput( string configuredPath, string fileNameSuffix, int maxCountPerFile )
        {
            if( maxCountPerFile < 1 ) throw new ArgumentException( "Must be greater than 0.", "maxCountPerFile" );
            _configPath = configuredPath;
            _maxCountPerFile = maxCountPerFile;
            _fileNameSuffix = fileNameSuffix + ".ckmon";
            _fileBufferSize = 4096;
        }

        /// <summary>
        /// Initializes a new file for <see cref="IMulticastLogEntry"/>: the final file name is based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> with a ".ckmon" extension.
        /// You must call <see cref="Initialize"/> before actually using this object.
        /// </summary>
        /// <param name="configuredPath">The path: it can be absolute and when relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> (that must be set).</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        public MonitorBinaryFileOutput( string configuredPath, int maxCountPerFile )
            : this( configuredPath, String.Empty, maxCountPerFile )
        {
        }

        /// <summary>
        /// Initializes a new file for <see cref="ILogEntry"/> issued from a specific monitor: the final file name is 
        /// based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> with a "-{XXX...XXX}.ckmon" suffix where {XXX...XXX} is the unique identifier (Guid with the B format - 32 digits separated by 
        /// hyphens, enclosed in braces) of the monitor.
        /// You must call <see cref="Initialize"/> before actually using this object.
        /// </summary>
        /// <param name="configuredPath">The path. Can be absolute. When relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> that must be set.</param>
        /// <param name="monitorId">Monitor identifier.</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        public MonitorBinaryFileOutput( string configuredPath, Guid monitorId, int maxCountPerFile )
            : this( configuredPath, '-' + monitorId.ToString( "B" ), maxCountPerFile )
        {
        }

        /// <summary>
        /// Computes the root path.
        /// </summary>
        /// <param name="m">A monitor (must not be null).</param>
        /// <returns>The final path to use (ends with '\'). Null if unable to compute the path.</returns>
        string ComputeBasePath( IActivityMonitor m )
        {
            string rootPath = null;
            if( String.IsNullOrWhiteSpace( _configPath ) ) m.Error().Send( "The configured path is empty." );
            else if( FileUtil.IndexOfInvalidPathChars( _configPath ) >= 0 ) m.Error().Send( "The configured path '{0}' is invalid.", _configPath );
            else
            {
                rootPath = _configPath;
                if( !Path.IsPathRooted( rootPath ) )
                {
                    string rootLogPath = SystemActivityMonitor.RootLogPath;
                    if( String.IsNullOrWhiteSpace( rootLogPath ) ) m.Error().Send( "The relative path '{0}' requires that {1} be specified (typically in the AppSettings).", _configPath, SystemActivityMonitor.AppSettingsKey );
                    else rootPath = Path.Combine( rootLogPath, _configPath );
                }
            }
            return rootPath != null ? FileUtil.NormalizePathSeparator( rootPath, true ) : null;
        }

        /// <summary>
        /// Gets the maximum number of entries per file.
        /// </summary>
        public int MaxCountPerFile
        {
            get { return _maxCountPerFile; }
        }

        /// <summary>
        /// Gets or sets whether files will be opened with <see cref="FileOptions.WriteThrough"/>.
        /// Defaults to false.
        /// </summary>
        public bool FileWriteThrough
        {
            get { return _fileWriteThrough; }
            set { _fileWriteThrough = value; }
        }

        /// <summary>
        /// Gets or sets the buffer size used to write files.
        /// Defaults to 4096.
        /// </summary>
        public int FileBufferSize
        {
            get { return _fileBufferSize; }
            set 
            {
                if( value < 0 ) throw new ArgumentException();
                _fileBufferSize = value; 
            }
        }

        /// <summary>
        /// Checks whether this <see cref="MonitorBinaryFileOutput"/> is valid: its base path is successfully created.
        /// Can be called multiple times.
        /// </summary>
        /// <param name="monitor">Required monitor.</param>
        public bool Initialize( IActivityMonitor monitor )
        {
            if( _basePath != null ) return true;
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            string b = ComputeBasePath( monitor );
            if( b != null )
            {
                try
                {
                    Directory.CreateDirectory( b );
                    _basePath = b;
                    return true;
                }
                catch( Exception ex )
                {
                    monitor.Error().Send( ex );
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether this file is currently opened.
        /// </summary>
        public bool IsOpened
        {
            get { return _writer != null; }
        }

        /// <summary>
        /// Closes the file if it is currently opened.
        /// Does nothing otherwise.
        /// </summary>
        public void Close()
        {
            if( _writer != null ) CloseCurrentFile();
        }

        #region Write methods

        /// <summary>
        /// Writes a log entry (that can actually be a <see cref="IMulticastLogEntry"/>).
        /// </summary>
        /// <param name="e">The log entry.</param>
        public void Write( ILogEntry e )
        {
            BeforeWrite();
            e.WriteLogEntry( _writer );
            AfterWrite();
        }

        /// <summary>
        /// Writes a line entry as a uni-cast compact entry or as a multi-cast one if needed.
        /// </summary>
        /// <param name="data">The log line.</param>
        /// <param name="adapter">Multi-cast information to be able to write multi-cast entry when needed.</param>
        public void UnicastWrite( ActivityMonitorLogData data, IMulticastLogInfo adapter )
        {
            BeforeWrite();
            LogEntry.WriteLog( _writer, adapter.MonitorId, adapter.PreviousEntryType, adapter.PreviousLogTime, adapter.GroupDepth, false, data.Level, data.LogTime, data.Text, data.Tags, data.ExceptionData, data.FileName, data.LineNumber );
            AfterWrite();
        }

        /// <summary>
        /// Writes a group opening entry as a uni-cast compact entry or as a multi-cast one if needed.
        /// </summary>
        /// <param name="g">The group line.</param>
        /// <param name="adapter">Multi-cast information to be able to write multi-cast entry when needed.</param>
        public void UnicastWriteOpenGroup( IActivityLogGroup g, IMulticastLogInfo adapter )
        {
            BeforeWrite();
            LogEntry.WriteLog( _writer, adapter.MonitorId, adapter.PreviousEntryType, adapter.PreviousLogTime, adapter.GroupDepth, true, g.GroupLevel, g.LogTime, g.GroupText, g.GroupTags, g.ExceptionData, g.FileName, g.LineNumber );
            AfterWrite();
        }

        /// <summary>
        /// Writes a group closing entry as a uni-cast compact entry or as a multi-cast one if needed.
        /// </summary>
        /// <param name="g">The group.</param>
        /// <param name="adapter">Multi-cast information to be able to write multi-cast entry when needed.</param>
        public void UnicastWriteCloseGroup( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions, IMulticastLogInfo adapter )
        {
            BeforeWrite();
            LogEntry.WriteCloseGroup( _writer, adapter.MonitorId, adapter.PreviousEntryType, adapter.PreviousLogTime, adapter.GroupDepth, g.GroupLevel, g.CloseLogTime, conclusions );
            AfterWrite();
        }

        void BeforeWrite()
        {
            if( _writer == null ) OpenTemporaryFile();
        }

        void AfterWrite()
        {
            if( --_countRemainder == 0 )
            {
                CloseCurrentFile();
            }
        }

        #endregion

        /// <summary>
        /// Simply calls <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        void OpenTemporaryFile()
        {
            FileOptions opt = FileOptions.SequentialScan;
            if( _fileWriteThrough ) opt |= FileOptions.WriteThrough;
            _openedTimeUtc = DateTime.UtcNow;
            _output = new FileStream( _basePath + Guid.NewGuid().ToString() + _fileNameSuffix + ".tmp", FileMode.CreateNew, FileAccess.Write, FileShare.Read, _fileBufferSize, opt );
            _writer = new BinaryWriter( _output );
            _writer.Write( LogReader.CurrentStreamVersion );
            _countRemainder = _maxCountPerFile;
        }

        void CloseCurrentFile()
        {
            _writer.Write( (byte)0 );
            string fName = _output.Name;
            _writer.Close();
            if( _countRemainder == _maxCountPerFile )
            {
                // No entries were written: we try to delete file.
                // If this fails, this is not an issue.
                try
                {
                    File.Delete( fName );
                }
                catch( IOException )
                {
                    // Forget it.
                }
            }
            else
            {
                FileUtil.MoveToUniqueTimedFile( fName, _basePath, _fileNameSuffix, _openedTimeUtc );
            }
            _writer = null;
        }
    }
}
