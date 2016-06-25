using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Helper class that encapsulates temporary stream and final renaming for log entries streams.
    /// This currently handles only the maximum count of entries per file but this may be extended with options like "SubFolderMode" that can be based 
    /// on current time (to group logs inside timed intermediate folders like one per day: 2014/01/12 or 2014-01/12, etc.). 
    /// </summary>
    public class MonitorFileOutputBase : IDisposable
    {
        readonly string _configPath;
        readonly int _maxCountPerFile;
        readonly string _fileNameSuffix;
        readonly bool _useGzipCompression;

        string _basePath;
        FileStream _output;
        DateTime _openedTimeUtc;
        int _countRemainder;
        int _fileBufferSize;
        bool _fileWriteThrough;

        /// <summary>
        /// Initializes a new file for <see cref="IMulticastLogEntry"/>: the final file name is based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> with a ".ckmon" extension.
        /// You must call <see cref="Initialize"/> before actually using this object.
        /// </summary>
        /// <param name="configuredPath">The path: it can be absolute and when relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> (that must be set).</param>
        /// <param name="fileNameSuffix">Suffix of the file including its extension. Must not be null nor empty.</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        /// <param name="useGzipCompression">True to gzip the file.</param>
        protected MonitorFileOutputBase( string configuredPath, string fileNameSuffix, int maxCountPerFile, bool useGzipCompression )
        {
            if( string.IsNullOrEmpty( fileNameSuffix ) ) throw new ArgumentException( nameof( fileNameSuffix ) );
            if( maxCountPerFile < 1 ) throw new ArgumentException( "Must be greater than 0.", "maxCountPerFile" );
            _configPath = configuredPath;
            _maxCountPerFile = maxCountPerFile;
            _fileNameSuffix = fileNameSuffix;
            _fileBufferSize = 4096;
            _useGzipCompression = useGzipCompression;
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
        /// <param name="useGzipCompression">True to gzip the file.</param>
        protected MonitorFileOutputBase( string configuredPath, Guid monitorId, int maxCountPerFile, bool useGzipCompression )
            : this( configuredPath, '-' + monitorId.ToString( "B" ), maxCountPerFile, useGzipCompression )
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
            if( String.IsNullOrWhiteSpace( _configPath ) ) m.SendLine( LogLevel.Error, "The configured path is empty.", null );
            else if( FileUtil.IndexOfInvalidPathChars( _configPath ) >= 0 ) m.SendLine( LogLevel.Error, string.Format( "The configured path '{0}' is invalid.", _configPath ), null );
            else
            {
                rootPath = _configPath;
                if( !Path.IsPathRooted( rootPath ) )
                {
                    string rootLogPath = SystemActivityMonitor.RootLogPath;
                    if( String.IsNullOrWhiteSpace( rootLogPath ) ) m.SendLine( LogLevel.Error, string.Format( "The relative path '{0}' requires that {1} be specified (typically in the AppSettings).", _configPath, SystemActivityMonitor.AppSettingsKey ), null );
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
        /// Checks whether this <see cref="MonitorFileOutputBase"/> is valid: its base path is successfully created.
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
                    monitor.SendLine( LogLevel.Error, null, ex );
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether this file is currently opened.
        /// </summary>
        public bool IsOpened
        {
            get { return _output != null; }
        }

        /// <summary>
        /// Closes the file if it is currently opened.
        /// Does nothing otherwise.
        /// </summary>
        public void Close()
        {
            if( _output != null ) CloseCurrentFile();
        }

        /// <summary>
        /// This method must be called before any write: it calls <see cref="OpenNewFile"/> if needed.
        /// </summary>
        protected void BeforeWrite()
        {
            if( _output == null ) OpenNewFile();
        }

        /// <summary>
        /// This method must be called after write: it closes and produces the final file
        /// if the current file is full.
        /// </summary>
        protected void AfterWrite()
        {
            if( --_countRemainder == 0 )
            {
                CloseCurrentFile();
            }
        }

        /// <summary>
        /// Simply calls <see cref="Close"/>.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Opens a new file suffixed by ".tmp".
        /// </summary>
        /// <returns>The opened stream to write to.</returns>
        protected virtual Stream OpenNewFile()
        {
            FileOptions opt = FileOptions.SequentialScan;
            if( _fileWriteThrough ) opt |= FileOptions.WriteThrough;
            _openedTimeUtc = DateTime.UtcNow;
            _output = new FileStream( _basePath + Guid.NewGuid().ToString() + _fileNameSuffix + ".tmp", FileMode.CreateNew, FileAccess.Write, FileShare.Read, _fileBufferSize, opt );
            _countRemainder = _maxCountPerFile;
            return _output;
        }

        /// <summary>
        /// Closes the currently opended file.
        /// </summary>
        protected virtual void CloseCurrentFile()
        {
            string fName = _output.Name;
            _output.Close();
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
                if( _useGzipCompression )
                {
                    string newPath = _basePath + FileUtil.FormatTimedUniqueFilePart( _openedTimeUtc ) + _fileNameSuffix;
                    FileUtil.CompressFileToGzipFile( fName, newPath, true );
                }
                else
                {
                    FileUtil.MoveToUniqueTimedFile( fName, _basePath, _fileNameSuffix, _openedTimeUtc );
                }
            }
            _output = null;
        }
    }
}
