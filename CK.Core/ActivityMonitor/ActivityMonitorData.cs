using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Data required by <see cref="IActivityMonitor.UnfilteredLog"/>.
    /// </summary>
    public class ActivityMonitorData
    {
        string _text;
        CKTrait _tags;
        DateTime _logTimeUtc;
        Exception _exception;
        CKExceptionData _exceptionData;

        /// <summary>
        /// Log level. If the log has been successfully filtered, the <see cref="LogLevel.IsFiltered"/> bit flag is set.
        /// </summary>
        public readonly LogLevel Level;

        /// <summary>
        /// The actual level (<see cref="LogLevel.Trace"/> to <see cref="LogLevel.Fatal"/>) associated to this group
        /// without <see cref="LogLevel.IsFiltered"/> bit flag.
        /// </summary>
        public readonly LogLevel MaskedGroupLevel;

        /// <summary>
        /// Name of the source file that emitted the log. Can be null.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Line number in the source filethat emitted the log. Can be null.
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// Gets whether this log data has been successfuly filtered.
        /// </summary>
        public bool IsFilteredLog
        {
            get { return (Level & LogLevel.IsFiltered) != 0; }
        }

        /// <summary>
        /// Tags of the log. Never null.
        /// </summary>
        public CKTrait Tags
        {
            get { return _tags; }
        }

        /// <summary>
        /// Text of the log. Can be null.
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Date and time of the log.
        /// </summary>
        public DateTime LogTimeUtc
        {
            get { return _logTimeUtc; }
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
        /// <returns></returns>
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
        /// Initializes a new <see cref="ActivityMonitorData"/>.
        /// </summary>
        /// <param name="level">Log level. Can not be <see cref="LogLevel.None"/>.</param>
        /// <param name="tags">Tags of the log. Can be null.</param>
        /// <param name="text">Text of the log. Can be null or empty only if <paramref name="exception"/> is not null: the <see cref="Exception.Message"/> is the text.</param>
        /// <param name="logTimeUtc">Date and time of the log. Must be in UTC.</param>
        /// <param name="exception">Exception of the log. Can be null.</param>
        /// <param name="fileName">Name of the source file that emitted the log. Can be null.</param>
        /// <param name="lineNumber">Line number in the source filethat emitted the log. Can be null.</param>
        public ActivityMonitorData( LogLevel level, CKTrait tags, string text, DateTime logTimeUtc, Exception exception, string fileName, int lineNumber )
            : this( level, fileName, lineNumber )
        {
            if( logTimeUtc.Kind != DateTimeKind.Utc ) throw new ArgumentException( R.DateTimeMustBeUtc, "logTimeUtc" );
            if( level == LogLevel.None ) throw new ArgumentException( R.ActivityMonitorLogLevelMustNotBeNone, "level" );
            if( String.IsNullOrEmpty( (_text = text) ) )
            {
                if( exception == null ) throw new ArgumentNullException( "text" );
                _text = exception.Message;
            }
            _tags = tags ?? ActivityMonitor.EmptyTag;
            _logTimeUtc = logTimeUtc;
        }

        internal ActivityMonitorData( LogLevel level, string fileName, int lineNumber )
        {
            Debug.Assert( level != LogLevel.None );
            Level = level;
            MaskedGroupLevel = level & LogLevel.Mask;
            FileName = fileName;
            LineNumber = lineNumber;
        }

        internal void Initialize( string text, Exception exception, CKTrait tags, DateTime logTimeUtc )
        {
            Debug.Assert( logTimeUtc.Kind == DateTimeKind.Utc );
            if( String.IsNullOrEmpty( (_text = text) ) )
            {
                if( exception == null ) throw new ArgumentNullException( "text" );
                _text = exception.Message;
            }
            _exception = exception;
            if( _text == null ) _text = exception.Message;
            _tags = tags ?? ActivityMonitor.EmptyTag;
            _logTimeUtc = logTimeUtc;
        }

        internal void CombineTags( CKTrait tags )
        {
            if( _tags.IsEmpty ) _tags = tags;
            else _tags = _tags.Union( tags );
        }
    }
}
