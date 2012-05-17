using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Count fatal, error or warn that occured and automatically sets the conclusion of groups.
    /// </summary>
    public class ActivityLoggerErrorCounter : ActivityLoggerHybridClient
    {
        static readonly string DefaultFatalConclusionFormat = "1 Fatal error";
        static readonly string DefaultFatalsConclusionFormat = "{0} Fatal errors";
        static readonly string DefaultErrorConclusionFormat = "1 Error";
        static readonly string DefaultErrorsConclusionFormat = "{0} Errors";
        static readonly string DefaultWarnConclusionFormat = "1 Warning";
        static readonly string DefaultWarnsConclusionFormat = "{0} Warnings";
        static readonly string DefaultSeparator = ", ";

        /// <summary>
        /// Reuse the ActivityLoggerErrorCounter: since all hooks are empty, nothing happens.
        /// </summary>
        class EmptyErrorCounter : ActivityLoggerErrorCounter
        {
            // Security if OnFilterChanged is implemented one day on ActivityLoggerErrorCounter.
            protected override void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
            {
            }

            protected override void OnUnfilteredLog( LogLevel level, string text )
            {
            }

            protected override void OnOpenGroup( IActivityLogGroup group )
            {
            }

            protected override string OnGroupClosing( IActivityLogGroup group, string conclusion )
            {
                return null;
            }

            // Security if OnGroupClosed is implemented one day on ActivityLoggerErrorCounter.
            protected override void OnGroupClosed( IActivityLogGroup group, string conclusion )
            {
            }
        }

        /// <summary>
        /// Empty <see cref="ActivityLoggerErrorCounter"/> (null object design pattern).
        /// </summary>
        static public new readonly ActivityLoggerErrorCounter Empty = new EmptyErrorCounter();

        /// <summary>
        /// Initializes a new error counter.
        /// </summary>
        public ActivityLoggerErrorCounter()
        {
            MaxLogLevel = LogLevel.Trace;
            ConclusionMode = ConclusionTextMode.SetWhenEmpty;
        }

        /// <summary>
        /// Defines how conclusion text for groups must be updated.
        /// </summary>
        public enum ConclusionTextMode
        {
            /// <summary>
            /// Conclusion is not handled.
            /// </summary>
            None,
            /// <summary>
            /// <see cref="GetCurrentMessage"/> is set as the conclusion 
            /// only if no conclusion exist.
            /// </summary>
            SetWhenEmpty,
            /// <summary>
            /// Appends <see cref="GetCurrentMessage"/> to the conclusion.
            /// </summary>
            AlwaysAppend
        }

        /// <summary>
        /// Gets or sets the <see cref="ConclusionTextMode"/>.
        /// Defaults to <see cref="ConclusionTextMode.SetWhenEmpty"/>.
        /// </summary>
        public ConclusionTextMode ConclusionMode { get; set; }

        /// <summary>
        /// Gets the current number of fatal errors.
        /// </summary>
        public int FatalCount { get; private set; }

        /// <summary>
        /// Gets the current number of errors.
        /// </summary>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// Gets the current number of warnings.
        /// </summary>
        public int WarnCount { get; private set; }

        /// <summary>
        /// Gets the current maximum <see cref="LogLevel"/>.
        /// </summary>
        public LogLevel MaxLogLevel { get; private set; }

        /// <summary>
        /// Gets whether an error or a fatal occurred.
        /// </summary>
        public bool HasError
        {
            get { return MaxLogLevel >= LogLevel.Error; }
        }

        /// <summary>
        /// Gets whether an a fatal, an error or a warn occurred.
        /// </summary>
        public bool HasWarnOrError
        {
            get { return MaxLogLevel >= LogLevel.Warn; }
        }

        /// <summary>
        /// Resets <see cref="FatalCount"/> and <see cref="ErrorCount"/>.
        /// </summary>
        public void ClearError()
        {
            if( MaxLogLevel > LogLevel.Warn )
            {
                FatalCount = ErrorCount = 0;
                MaxLogLevel = WarnCount > 0 ? LogLevel.Warn : LogLevel.Info;
            }
        }

        /// <summary>
        /// Resets current <see cref="WarnCount"/>, and optionnaly <see cref="FatalCount"/> and <see cref="ErrorCount"/>.
        /// </summary>
        public void ClearWarn( bool clearError = false )
        {
            WarnCount = 0;
            if( MaxLogLevel == LogLevel.Warn ) MaxLogLevel = LogLevel.Info;
            else if( clearError ) ClearError();
        }

        /// <summary>
        /// Gets the current message if <see cref="HasWarnOrError"/> is true, otherwise null.
        /// </summary>
        /// <returns>Formatted message or null if no error nor warning occurred.</returns>
        public string GetCurrentMessage()
        {
            if( HasWarnOrError )
            {
                string s = String.Empty;
                if( FatalCount == 1 ) s += DefaultFatalConclusionFormat;
                else if( FatalCount > 1 ) s += String.Format( DefaultFatalsConclusionFormat, FatalCount );
                if( ErrorCount > 0 )
                {
                    if( s.Length > 0 ) s += DefaultSeparator;
                    if( ErrorCount == 1 ) s += DefaultErrorConclusionFormat;
                    else if( ErrorCount > 1 ) s += String.Format( DefaultErrorsConclusionFormat, ErrorCount );
                }
                if( WarnCount > 0 )
                {
                    if( s.Length > 0 ) s += DefaultSeparator;
                    if( WarnCount == 1 ) s += DefaultWarnConclusionFormat;
                    else if( WarnCount > 1 ) s += String.Format( DefaultWarnsConclusionFormat, WarnCount );
                }
                return s;
            }
            return null;
        }

        /// <summary>
        /// Updates error counters.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        protected override void OnUnfilteredLog( LogLevel level, string text )
        {
            CatchLevel( level );
        }

        /// <summary>
        /// Updates error counters.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        protected override void OnOpenGroup( IActivityLogGroup group )
        {
            CatchLevel( group.GroupLevel );
        }

        private void CatchLevel( LogLevel level )
        {           
            switch( level )
            {
                case LogLevel.Fatal: 
                    FatalCount = FatalCount + 1; 
                    MaxLogLevel = LogLevel.Fatal; 
                    break;
                case LogLevel.Error: 
                    ErrorCount = ErrorCount + 1; 
                    if( MaxLogLevel != LogLevel.Fatal ) MaxLogLevel = LogLevel.Error; 
                    break;
                case LogLevel.Warn: 
                    WarnCount = WarnCount + 1; 
                    if( MaxLogLevel < LogLevel.Warn ) MaxLogLevel = LogLevel.Warn; 
                    break;
                default:
                    if( MaxLogLevel < level ) MaxLogLevel = level;
                    break;
            }
        }

        /// <summary>
        /// Handles group conclusion.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusion">Text that concludes the group. Never null but can be empty.</param>
        /// <returns>The potentially overriden conclusion.</returns>
        protected override string OnGroupClosing( IActivityLogGroup group, string conclusion )
        {
            switch( ConclusionMode )
            {
                case ConclusionTextMode.AlwaysAppend:
                    {
                        return conclusion += " - " + GetCurrentMessage();
                    }
                case ConclusionTextMode.SetWhenEmpty:
                    {
                        if( conclusion.Length == 0 ) return GetCurrentMessage();
                        break;
                    }
            }
            return null;
        } 



    }
}
