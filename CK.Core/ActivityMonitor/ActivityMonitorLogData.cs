using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Data required by <see cref="IActivityMonitor.UnfilteredLog"/>.
    /// This is also the base class for <see cref="ActivityMonitorGroupData"/>.
    /// </summary>
    public class ActivityMonitorLogData
    {
        string _text;
        CKTrait _tags;
        LogTimestamp _logTime;
        Exception _exception;
        CKExceptionData _exceptionData;

        /// <summary>
        /// Log level. Can not be <see cref="LogLevel.None"/>.
        /// If the log has been successfully filtered, the <see cref="LogLevel.IsFiltered"/> bit flag is set.
        /// </summary>
        public readonly LogLevel Level;

        /// <summary>
        /// The actual level (<see cref="LogLevel.Trace"/> to <see cref="LogLevel.Fatal"/>) associated to this group
        /// without <see cref="LogLevel.IsFiltered"/> bit flag.
        /// </summary>
        public readonly LogLevel MaskedLevel;

        /// <summary>
        /// Name of the source file that emitted the log. Can be null.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Line number in the source file that emitted the log. Can be null.
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// Gets whether this log data has been successfully filtered (otherwise it is an unfiltered log).
        /// </summary>
        public bool IsFilteredLog
        {
            get { return (Level & LogLevel.IsFiltered) != 0; }
        }

        /// <summary>
        /// Tags (from <see cref="ActivityMonitor.Tags"/>) associated to the log. 
        /// It will be union-ed with the current <see cref="IActivityMonitor.AutoTags"/>.
        /// </summary>
        public CKTrait Tags
        {
            get { return _tags; }
        }

        /// <summary>
        /// Text of the log. Can not be null.
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Gets the time of the log.
        /// </summary>
        public LogTimestamp LogTime
        {
            get { return _logTime; }
        }

        /// <summary>
        /// Exception of the log. Can be null.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        /// Gets the <see cref="CKExceptionData"/> that captures exception information 
        /// if it exists. Returns null if no <see cref="P:Exception"/> exists.
        /// </summary>
        public CKExceptionData ExceptionData
        {
            get
            {
                if( _exceptionData == null && _exception != null )
                {
                    CKException ckEx = _exception as CKException;
                    if( ckEx != null )
                    {
                        _exceptionData = ckEx.ExceptionData;
                    }
                }
                return _exceptionData;
            }
        }

        /// <summary>
        /// Gets or creates the <see cref="CKExceptionData"/> that captures exception information.
        /// If <see cref="P:Exception"/> is null, this returns null.
        /// </summary>
        /// <returns>A data representation of the exception or null.</returns>
        public CKExceptionData EnsureExceptionData()
        {
            return _exceptionData ?? (_exceptionData = CKExceptionData.CreateFrom( _exception ));
        }

        /// <summary>
        /// Gets whether the <see cref="Text"/> is actually the <see cref="P:Exception"/> message.
        /// </summary>
        public bool IsTextTheExceptionMessage
        {
            get { return _exception != null && ReferenceEquals( _exception.Message, _text ); }
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorLogData"/>.
        /// </summary>
        /// <param name="level">Log level. Can not be <see cref="LogLevel.None"/>.</param>
        /// <param name="exception">Exception of the log. Can be null.</param>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.Tags"/>) to associate to the log. It will be union-ed with the current <see cref="IActivityMonitor.AutoTags"/>.</param>
        /// <param name="text">Text of the log. Can be null or empty only if <paramref name="exception"/> is not null: the <see cref="T:Exception.Message"/> is the text.</param>
        /// <param name="logTime">
        /// Time of the log. 
        /// You can use <see cref="LogTime.UtcNow"/> or <see cref="ActivityMonitorExtension.NextLogTime">IActivityMonitor.NextLogTime()</see> extension method.</param>
        /// </param>
        /// <param name="fileName">Name of the source file that emitted the log. Can be null.</param>
        /// <param name="lineNumber">Line number in the source file that emitted the log. Can be null.</param>
        public ActivityMonitorLogData( LogLevel level, Exception exception, CKTrait tags, string text, LogTimestamp logTime, string fileName, int lineNumber )
            : this( level, fileName, lineNumber )
        {
            if( MaskedLevel == LogLevel.None || MaskedLevel == LogLevel.Mask ) throw new ArgumentException( R.ActivityMonitorInvalidLogLevel, "level" );
            Initialize( text, exception, tags, logTime );
        }

        internal ActivityMonitorLogData( LogLevel level, string fileName, int lineNumber )
        {
            // level == LogLevel.None is for fake senders (when log filtering is rejected).
            Level = level;
            MaskedLevel = level & LogLevel.Mask;
            FileName = fileName;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Used only to initialize a ActivityMonitorGroupSender for rejected opened group.
        /// </summary>
        internal ActivityMonitorLogData()
        {
            Debug.Assert( Level == LogLevel.None );
        }

        internal void Initialize( string text, Exception exception, CKTrait tags, LogTimestamp logTime )
        {
            if( String.IsNullOrEmpty( (_text = text) ) )
            {
                if( exception == null ) throw new ArgumentNullException( "text" );
                _text = exception.Message;
            }
            _exception = exception;
            _tags = tags ?? ActivityMonitor.Tags.Empty;
            _logTime = logTime;
        }

        internal LogTimestamp CombineTagsAndAdjustLogTime( CKTrait tags, LogTimestamp lastLogTime )
        {
            if( _tags.IsEmpty ) _tags = tags;
            else _tags = _tags.Union( tags );
            return _logTime = new LogTimestamp( lastLogTime, _logTime.IsDefined ? _logTime : LogTimestamp.UtcNow );
        }
    }
}
