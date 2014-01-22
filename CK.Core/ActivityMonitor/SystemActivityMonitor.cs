using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// This <see cref="ActivityMonitor"/> logs errors in a directory (if the static <see cref="RootLogPath"/> property is not null) and 
    /// raises <see cref="OnError"/> events.
    /// Its main goal is to be internally used by the Monitor framework but can be used as a "normal" monitor (if you believe it is a good idea).
    /// The easiest way to configure it is to set an application settings with the key "CK.Core.SystemActivityMonitor.RootLogPath" and the root path 
    /// for logs as the value.
    /// </summary>
    public sealed class SystemActivityMonitor : ActivityMonitor
    {
        /// <summary>
        /// A client that can not be removed and is available as a singleton registered in every new SystemActivityMonitor.
        /// Its MinimalFilter is set to Release ensuring that errors are always monitored but it stores in RootLogPath/SystemActivityMonitor only errors logs.
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

            public void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                var level = data.Level & LogLevel.Mask;
                if( level >= LogLevel.Error )
                {
                    string s = DumpErrorText( data.LogTime, data.Text, data.Level, null, data.Tags );
                    SystemActivityMonitor.HandleError( s );
                }
            }

            public void OnOpenGroup( IActivityLogGroup group )
            {
                if( group.MaskedGroupLevel >= LogLevel.Error )
                {
                    string s = DumpErrorText( group.LogTime, group.GroupText, group.MaskedGroupLevel, group.GroupTags, group.EnsureExceptionData() );
                    SystemActivityMonitor.HandleError( s );
                }
            }

            public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
            {
            }

            public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
            }

            public void OnTopicChanged( string newTopic, string fileName, int lineNumber )
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
            /// Not null if the <see cref="ErrorMessage"/> has been successfully written (if <see cref="SystemActivityMonitor.RootLogPath"/> is set).
            /// Contains the full path of the log file.
            /// </summary>
            public readonly string FullLogFilePath;

            /// <summary>
            /// Exception raised while attempting to create the error file.
            /// This could be used to handle configuration error: an exception here means that something is going really wrong.
            /// </summary>
            public readonly Exception ErrorWhileWritingLogFile;

            internal LowLevelErrorEventArgs( string errorMessage, string fullLogFilePath, Exception writeError )
            {
                ErrorMessage = errorMessage;
                FullLogFilePath = fullLogFilePath;
                ErrorWhileWritingLogFile = writeError;
            }
        }

        static readonly IActivityMonitorClient _client;
        static string _logPath;
        static string _appSettingslogPath;
        static int _activityMonitorErrorTracked;

        static SystemActivityMonitor()
        {
            AppSettingsKey = "CK.Core.SystemActivityMonitor.RootLogPath";
            SubDirectoryName = "SystemActivityMonitor/";
            _client = new SysClient();
            _appSettingslogPath = AppSettings.Default[AppSettingsKey];
            _activityMonitorErrorTracked = 1;
            ActivityMonitor.MonitoringError.OnErrorFromBackgroundThreads += OnTrackActivityMonitorLoggingError;
        }

        /// <summary>
        /// Touches this type to ensure that its static information is initialized.
        /// This does nothing except that, since the Type is solicited, the type constructor is called if needed.
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
                string s = DumpErrorText( DateTimeStamp.UtcNow, error.Comment, LogLevel.Error, error.Exception, null );
                HandleError( s );
            }
        }

        /// <summary>
        /// The key in the application settings used to initialize the <see cref="RootLogPath"/> if it exists in <see cref="AppSettings.Default"/>.
        /// </summary>
        static public readonly string AppSettingsKey;

        /// <summary>
        /// The directory in <see cref="RootLogPath"/> into which errors file will created is "SystemActivityMonitor/".
        /// </summary>
        static public readonly string SubDirectoryName;

        /// <summary>
        /// Event that enables subsequent handling of errors.
        /// Raising this event is protected: a registered handler that raises an exception will be automatically removed and the
        /// exception will be added to the <see cref="ActivityMonitor.MonitoringError"/> collector to give other participants a chance 
        /// to handle it and track the culprit.
        /// </summary>
        static public event EventHandler<LowLevelErrorEventArgs> OnError;

        /// <summary>
        /// Gets or sets whether <see cref="ActivityMonitor.MonitoringError"/> are tracked (this is thread safe).
        /// When true, LoggingError events are tracked, written to a file (if <see cref="RootLogPath"/> is available) and ultimately 
        /// published again as a <see cref="OnError"/> events.
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
                        ActivityMonitor.MonitoringError.OnErrorFromBackgroundThreads += OnTrackActivityMonitorLoggingError;
                    }
                }
                else if( Interlocked.CompareExchange( ref _activityMonitorErrorTracked, 0, 1 ) == 1 )
                {
                    ActivityMonitor.MonitoringError.OnErrorFromBackgroundThreads -= OnTrackActivityMonitorLoggingError;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="RootLogPath"/> that is configured in application settings (null otherwise).
        /// Getting this property ensures that this type's static information is initialized.
        /// </summary>
        static public string AppSettingsRootLogPath
        {
            get { return _appSettingslogPath; }
        }

        /// <summary>
        /// Gets or sets (it can be set only once) the log folder to use (setting multiple time the same path is accepted). 
        /// Once set, the path is <see cref="FileUtil.NormalizePathSeparator">normalized and ends with a path separator</see>.
        /// See remarks.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When setting it, the path must be valid (when it is not an absolute path, it is combined 
        /// with the <see cref="AppDomain.BaseDirectory">AppDomain.CurrentDomain.BaseDirectory</see>).
        /// </para>
        /// <para>
        /// The subordinate directory "SystemActivityMonitor" is created (if not already here) and a test file is created (and deleted) inside it 
        /// to ensure that (at least at configuration time), no security configuration prevents us to create log files: all errors files will be created in this sub directory.
        /// </para>
        /// <para>
        /// It is recommended to use this directory to store all other logs and/or files related to activity tracking.
        /// </para>
        /// <para>
        /// When not null, it necessarily ends with a <see cref="Path.DirectorySeparatorChar"/>.
        /// </para>
        /// <para>
        /// When not sets, the first get initializes it with the value of <see cref="AppSettingsKey"/> in <see cref="AppSettings.Default"/> if it exists.
        /// </para>
        /// </remarks>
        static public string RootLogPath
        {
            get 
            {
                if( _logPath == null && _appSettingslogPath != null )
                {
                    RootLogPath = _appSettingslogPath;
                }
                return _logPath; 
            }
            set 
            {
                if( String.IsNullOrWhiteSpace( value ) ) throw new ArgumentNullException();
                value = NormalizeRootLogPath( value );
                if( _logPath != null && value != _logPath )
                {
                    throw new CKException( R.SystemActivityMonitorRootLogPathSetOnlyOnce );
                }
                try
                {
                    string dirName = value + SubDirectoryName;
                    if( !Directory.Exists( dirName ) ) Directory.CreateDirectory( dirName );
                    string testWriteFile = Path.Combine( dirName, Guid.NewGuid().ToString() );
                    File.AppendAllText( testWriteFile, AppSettingsKey );
                    File.Delete( testWriteFile );
                    _logPath = value;
                }
                catch( Exception ex )
                {
                    throw new CKException( ex, R.InvalidRootLogPath, value, SubDirectoryName, AppSettingsKey );
                }
            }
        }

        static string NormalizeRootLogPath( string value )
        {
            if( !Path.IsPathRooted( value ) ) value = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, value );
            value = FileUtil.NormalizePathSeparator( value, true );
            return value;
        }

        /// <summary>
        /// Checks that <see cref="RootLogPath"/> is correctly configured by throwing a detailed exception if not. 
        /// </summary>
        public static void AssertRootLogPathIsSet()
        {
            if( RootLogPath == null ) throw new CKException( R.RootLogPathMustBeSet, AppSettingsKey );
        }

        /// <summary>
        /// Initializes a new <see cref="SystemActivityMonitor"/>.
        /// </summary>
        public SystemActivityMonitor()
            : this( false, null )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SystemActivityMonitor"/> that can behave as a standard monitor (when
        /// automatic configurations applies).
        /// </summary>
        /// <param name="applyAutoConfigurations">True to apply automatic configurations and, hence, behave like any other <see cref="ActivityMonitor"/>.</param>
        /// <param name="topic">Optional initial topic (can be null).</param>
        public SystemActivityMonitor( bool applyAutoConfigurations, string topic )
            : base( applyAutoConfigurations )
        {
            if( topic != null ) SetTopic( topic );
            Output.RegisterClient( _client );
        }

        static void HandleError( string s )
        {
            string fullLogFilePath = null;
            Exception errorWhileWritingFile = null;
            // Atomically captures the LogPath to use.
            string logPath = _logPath;
            if( logPath != null )
            {
                try
                {
                    fullLogFilePath = FileUtil.WriteUniqueTimedFile( logPath + SubDirectoryName, ".txt", DateTime.UtcNow, Encoding.UTF8.GetBytes( s ), true );
                }
                catch( Exception ex )
                {
                    errorWhileWritingFile = ex;
                }
            }
            var h = OnError;
            if( h != null )
            {
                LowLevelErrorEventArgs e = new LowLevelErrorEventArgs( s, fullLogFilePath, errorWhileWritingFile );
                // h.GetInvocationList() creates an independent copy of Delegate[].
                foreach( EventHandler<LowLevelErrorEventArgs> d in h.GetInvocationList() )
                {
                    try
                    {
                        d( null, e );
                    }
                    catch( Exception ex )
                    {
                        OnError -= (EventHandler<LowLevelErrorEventArgs>)d;
                        ActivityMonitor.MonitoringError.Add( ex, "While raising SystemActivityMonitor.OnError event." );
                    }
                }
            }
        }

        #region Generate text from errors methods.

        static string DumpErrorText( DateTimeStamp logTime, string text, LogLevel level, Exception ex, CKTrait tags )
        {
            StringBuilder buffer = CreateHeader( logTime, text, level, tags );
            if( ex != null )
            {
                ActivityMonitorTextWriterClient.DumpException( buffer, String.Empty, !ReferenceEquals( text, ex.Message ), ex );
            }
            WriteFooter( level, buffer );
            return buffer.ToString();
        }

        static string DumpErrorText( DateTimeStamp logTime, string text, LogLevel level, CKTrait tags, CKExceptionData exData )
        {
            StringBuilder buffer = CreateHeader( logTime, text, level, tags );
            if( exData != null ) exData.ToStringBuilder( buffer, String.Empty );
            WriteFooter( level, buffer );
            return buffer.ToString();
        }

        static StringBuilder CreateHeader( DateTimeStamp logTime, string text, LogLevel level, CKTrait tags )
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append( '<' ).Append( level.ToString() ).Append( '>' ).Append( '@' ).Append( logTime.ToString() );
            if( tags != null && !tags.IsEmpty ) buffer.Append( " - " ).Append( tags.ToString() );
            buffer.AppendLine();
            if( text != null && text.Length > 0 ) buffer.Append( text ).AppendLine();
            return buffer;
        }

        static void WriteFooter( LogLevel level, StringBuilder buffer )
        {
            buffer.Append( "</" ).Append( level.ToString() ).Append( '>' ).AppendLine();
        }
        #endregion

    }
}

