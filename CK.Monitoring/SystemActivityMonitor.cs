using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SystemActivityMonitor : ActivityMonitor
    {
        /// <summary>
        /// A client that can not be removed and is available as a singleton registered in every new SystemActivityMonitor.
        /// </summary>
        class SysClient : IActivityMonitorBoundClient
        {
            public LogFilter MinimalFilter
            {
                get { return LogFilter.Release; }
            }

            public void SetMonitor( IActivityMonitorImpl source, bool forceBuggyRemove )
            {
                if( !forceBuggyRemove && source == null ) throw new InvalidOperationException();
            }

            public void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                level &= LogLevel.Mask;
                if( level >= LogLevel.Error )
                {
                    string s = DumpErrorText( logTimeUtc, text, level, null, tags );
                    SystemActivityMonitor.HandleError( s );
                }
            }

            public void OnOpenGroup( IActivityLogGroup group )
            {
                if( group.MaskedGroupLevel >= LogLevel.Error )
                {
                    string s = DumpErrorText( group.LogTimeUtc, group.GroupText, group.MaskedGroupLevel, group.GroupTags, group.EnsureExceptionData() );
                    SystemActivityMonitor.HandleError( s );
                }
            }

            public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
            {
            }

            public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
            }

            public void OnTopicChanged( string newTopic )
            {
            }

            public void OnAutoTagsChanged( CKTrait newTrait )
            {
            }
        }

        /// <summary>
        /// Defines the event argument of <see cref="SystemActivityMonitor.OnError"/>.
        /// </summary>
        public sealed class LowLevelErrorEventArgs : EventArgs
        {
            /// <summary>
            /// The error message. Never null nor empty.
            /// </summary>
            public readonly string ErrorMessage;

            /// <summary>
            /// True if the <see cref="ErrorMessage"/> has been successfully written (if <see cref="SystemActivityMonitor.LogPath"/> is set).
            /// </summary>
            public readonly bool SuccessfullyWritten;

            /// <summary>
            /// Exception raised while attempting to write the error file.
            /// This could be used to handle configuration error: an exception here means that something is going really wrong.
            /// </summary>
            public readonly Exception ErrorWhileWritingLogFile;

            internal LowLevelErrorEventArgs( string errorMessage, bool successfulWrite, Exception writeError )
            {
                ErrorMessage = errorMessage;
                SuccessfullyWritten = successfulWrite;
                ErrorWhileWritingLogFile = writeError;
            }
        }

        static readonly IActivityMonitorClient _client;
        static string _logPath;
        static int _activityMonitorErrorTracked;

        static SystemActivityMonitor()
        {
            AppSettingsKey = "CK.Core.SystemActivityMonitor.LogPath";
            SubDirectoryName = "SystemActivityMonitor/";
            _client = new SysClient();
            LogPath = ConfigurationManager.AppSettings[AppSettingsKey];
            _activityMonitorErrorTracked = 1;
            ActivityMonitor.LoggingError.OnErrorFromBackgroundThreads += OnTrackActivityMonitorLoggingError;
        }

        /// <summary>
        /// Touches this type to ensure that its static information is initalized.
        /// This does nothing except that, since the Type is sollicited, the type constructor is called if needed.
        /// </summary>
        /// <returns>Always true.</returns>
        static public bool EnsureStaticInitialization()
        {
            return _client != null;
        }

        static void OnTrackActivityMonitorLoggingError( object sender, CriticalErrorCollector.ErrorEventArgs e )
        {
            foreach( var error in e.LoggingErrors )
            {
                string s = DumpErrorText( DateTime.UtcNow, error.Comment, LogLevel.Error, error.Exception, null );
                HandleError( s );
            }
        }

        /// <summary>
        /// The key in the application settings used to initialize the <see cref="LogPath"/> if it exists in <see cref="ConfigurationManager.AppSettings"/> section.
        /// </summary>
        static readonly string AppSettingsKey;

        /// <summary>
        /// The directory in <see cref="LogPath"/> into which errors file will created is "SystemActivityMonitor/".
        /// </summary>
        static readonly string SubDirectoryName;

        /// <summary>
        /// Event that enables subsequent handling of errors.
        /// Raising this event is protected: a registered handler that raises an exception will be automatically removed and the
        /// exception will be added to the <see cref="ActivityMonitor.LoggingError"/> collector to give other participants a chance 
        /// to handle it and track the culprit.
        /// </summary>
        static event EventHandler<LowLevelErrorEventArgs> OnError;

        /// <summary>
        /// Gets or sets whether <see cref="ActivityMonitor.LoggingError"/> are tracked (this is thread safe).
        /// When true, LoggingError events are tracked, written to a file (if <see cref="LogPath"/> is available) and ultimately 
        /// republished throug as <see cref="OnError"/> events.
        /// Defaults to true.
        /// </summary>
        static public bool TrackActivityMonitorLoggingError
        {
            get { return _activityMonitorErrorTracked == 1; }
            set
            {
                if( value )
                {
                    if( Interlocked.CompareExchange( ref _activityMonitorErrorTracked, 1, 0 ) == 0 )
                    {
                        ActivityMonitor.LoggingError.OnErrorFromBackgroundThreads += OnTrackActivityMonitorLoggingError;
                    }
                }
                else if( Interlocked.CompareExchange( ref _activityMonitorErrorTracked, 0, 1 ) == 1 )
                {
                    ActivityMonitor.LoggingError.OnErrorFromBackgroundThreads -= OnTrackActivityMonitorLoggingError;
                }
            }
        }

        /// <summary>
        /// Gets or sets the log folder to use. When setting it, the path must be valid: the directory "SystemActivityMonitor" is created (if not already here) and 
        /// a test file is created (and deleted) inside it to ensure that (at least at configuration time), no security configuration prevents us to create log files:
        /// all errors files will be created in this sub directory.
        /// When not null, it necessarily ends with a <see cref="Path.DirectorySeparatorChar"/>.
        /// Defaults to the value of <see cref="AppSettingsKey"/> in <see cref="ConfigurationManager.AppSettings"/> or null.
        /// </summary>
        static public string LogPath
        {
            get { return _logPath; }
            set 
            {
                if( String.IsNullOrWhiteSpace( value ) ) value = null;
                if( _logPath != value )
                {
                    if( value != null )
                    {
                        try
                        {
                            value = FileUtil.NormalizePathSeparator( value, true );
                            string dirName = value + SubDirectoryName;
                            if( !Directory.Exists( dirName ) ) Directory.CreateDirectory( dirName );
                            string testWriteFile = Path.Combine( dirName, Guid.NewGuid().ToString() );
                            File.AppendAllText( testWriteFile, AppSettingsKey );
                            File.Delete( testWriteFile );
                        }
                        catch( Exception ex )
                        {
                            throw new CKException( ex, "CK.Core.SystemActivityMonitor.LogPath = '{0}' is invalid: unable to create a test file in '{1}'.", value, SubDirectoryName );
                        }
                    }
                    _logPath = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="SystemActivityMonitor"/>.
        /// </summary>
        public SystemActivityMonitor()
            : base( false )
        {
            Output.RegisterClient( _client );
        }

        static void HandleError( string s )
        {
            // Atomically captures the LogPath to use.
            bool fileHasBeenLogged = false;
            Exception errorWhileWritingFile = null;
            string logPath = _logPath;
            if( logPath != null )
            {
                string p = LogPath + Guid.NewGuid().ToString( "N" ) + ".txt";
                try
                {
                    File.AppendAllText( p, s );
                    fileHasBeenLogged = true;
                }
                catch( Exception ex )
                {
                    errorWhileWritingFile = ex;
                }
            }
            var h = OnError;
            if( h != null )
            {
                LowLevelErrorEventArgs e = new LowLevelErrorEventArgs( s, fileHasBeenLogged, errorWhileWritingFile );
                // h.GetInvocationList() creates an independant copy of Delegate[].
                foreach( EventHandler<LowLevelErrorEventArgs> d in h.GetInvocationList() )
                {
                    try
                    {
                        d( null, e );
                    }
                    catch( Exception ex )
                    {
                        OnError -= (EventHandler<LowLevelErrorEventArgs>)d;
                        ActivityMonitor.LoggingError.Add( ex, "While raising SystemActivityMonitor.Errors event." );
                    }
                }
            }
        }

        #region Generate text from errors methods.

        static string DumpErrorText( DateTime logTimeUtc, string text, LogLevel level, Exception ex, CKTrait tags )
        {
            StringBuilder buffer = CreateHeader( logTimeUtc, text, level, tags );
            if( ex != null )
            {
                ActivityMonitorTextWriterClient.DumpException( new StringWriter( buffer ), String.Empty, !ReferenceEquals( text, ex.Message ), ex );
            }
            WriteFooter( level, buffer );
            return buffer.ToString();
        }

        static string DumpErrorText( DateTime logTimeUtc, string text, LogLevel level, CKTrait tags, CKExceptionData exData )
        {
            StringBuilder buffer = CreateHeader( logTimeUtc, text, level, tags );
            if( exData != null ) exData.ToStringBuilder( buffer, String.Empty );
            WriteFooter( level, buffer );
            return buffer.ToString();
        }

        static StringBuilder CreateHeader( DateTime logTimeUtc, string text, LogLevel level, CKTrait tags )
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append( '>' ).Append( level.ToString() ).Append( '-', 10 ).AppendLine();
            buffer.Append( " - " ).Append( logTimeUtc );
            if( tags != null && !tags.IsEmpty ) buffer.Append( " - " ).Append( tags.ToString() );
            buffer.AppendLine();
            if( text != null && text.Length > 0 ) buffer.Append( text ).AppendLine();
            return buffer;
        }

        static void WriteFooter( LogLevel level, StringBuilder buffer )
        {
            buffer.Append( '<' ).Append( level.ToString() ).Append( '-', 10 ).AppendLine();
        }
        #endregion

    }
}

