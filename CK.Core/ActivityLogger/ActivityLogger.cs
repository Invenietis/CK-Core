using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Core
{
    public class ActivityLogger : IActivityLogger
    {
        /// <summary>
        /// String to use to break the current <see cref="LogLevel"/> (as if a different <see cref="LogLevel"/> was used).
        /// </summary>
        static public readonly string ParkLevel = "PARK-LEVEL";
        
        LogLevelFilter _filter;
        Group _current;
        int _depth;
        ActivityLoggerOutput _output;


        /// <summary>
        /// Initializes a new <see cref="ActivityLogger"/> with a <see cref="ActivityLoggerOutput"/> as its <see cref="Output"/>.
        /// </summary>
        public ActivityLogger()
        {
            _output = new ActivityLoggerOutput( this );
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityLogger"/> with a specific <see cref="Output"/> or null
        /// to postpone the setting of Output by using <see cref="SetOutput"/>.
        /// </summary>
        /// <param name="output">The output to use. Can be null.</param>
        protected ActivityLogger( ActivityLoggerOutput output )
        {
            _output = output;
        }

        /// <summary>
        /// Gets the <see cref="IActivityLoggerOutput"/> for this logger.
        /// </summary>
        public IActivityLoggerOutput Output
        {
            get { return _output; }
        }

        /// <summary>
        /// Sets the <see cref="Output"/>.
        /// </summary>
        /// <param name="output">Can not be null.</param>
        protected void SetOutput( ActivityLoggerOutput output )
        {
            if( output == null ) throw new ArgumentNullException( "output" );
            _output = output;
        }

        /// <summary>
        /// Gets or sets a filter based on the log level.
        /// This filter applies to the currently opened group (it is automatically restored when <see cref="CloseGroup"/> is called).
        /// </summary>
        public LogLevelFilter Filter
        {
            get { return _filter; }
            set
            {
                if( _filter != value )
                {
                    _output.OnFilterChanged( _filter, value );
                    _filter = value;
                }
            }
        }

        /// <summary>
        /// Logs a text regardless of <see cref="Filter"/> level. 
        /// Each call to log is considered as a line: a paragraph (or line separator) is appended
        /// between each text if the <paramref name="level"/> is the same as the previous one.
        /// See remarks.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log. Ignored if null or empty.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        /// <remarks>
        /// A null or empty <paramref name="text"/> is not logged.
        /// The special text "PARK-LEVEL" breaks the current <see cref="LogLevel"/>
        /// and resets it: the next log, even with the same LogLevel, will be treated as if
        /// a different LogLevel is used.
        /// </remarks>
        public IActivityLogger UnfilteredLog( LogLevel level, string text )
        {
            if( !String.IsNullOrEmpty( text ) )
            {
                _output.OnUnfilteredLog( level, text );
            }
            return this;
        }

        /// <summary>
        /// Groups are linked together from the current one to the very first one (stack).
        /// </summary>
        public class Group : IActivityLogGroup, IDisposable
        {
            ActivityLogger _logger;

            /// <summary>
            /// Initializes a new <see cref="Group"/> object.
            /// </summary>
            /// <param name="logger">The logger.</param>
            /// <param name="level">The <see cref="GroupLevel"/>.</param>
            /// <param name="text">The <see cref="GroupText"/>.</param>
            /// <param name="defaultConclusionText">
            /// Optional delegate to call on close to obtain a conclusion text if no 
            /// explicit conclusion is provided through <see cref="DefaultActivityLogger.CloseGroup"/>.
            /// </param>
            internal protected Group( ActivityLogger logger, LogLevel level, string text, Func<string> defaultConclusionText )
            {
                _logger = logger;
                Parent = logger._current;
                Depth = logger._depth;
                Filter = logger.Filter;
                GroupLevel = level;
                GroupText = text;
                GetConclusionText = defaultConclusionText;
            }

            /// <summary>
            /// Get the previous group. Null if this is a top level group.
            /// </summary>
            public IActivityLogGroup Parent { get; private set; }
            
            /// <summary>
            /// Gets or sets the <see cref="LogLevelFilter"/> for this group.
            /// Initialized with the <see cref="IActivityLogger.Filter"/> when the group has been opened.
            /// </summary>
            public LogLevelFilter Filter { get; protected set; }

            /// <summary>
            /// Gets the depth of this group (1 for top level groups).
            /// </summary>
            public int Depth { get; private set; }

            /// <summary>
            /// Gets the level of this group.
            /// </summary>
            public LogLevel GroupLevel { get; private set; }
            
            /// <summary>
            /// Getst the text with which this group has been opened.
            /// </summary>
            public string GroupText { get; private set; }

            /// <summary>
            /// Optional function that will be called on group closing. 
            /// </summary>
            protected Func<string> GetConclusionText { get; set; }
            
            /// <summary>
            /// Ensures that any groups opened after this one are closed before closing this one.
            /// </summary>
            void IDisposable.Dispose()
            {
                if( _logger != null )
                {
                    while( _logger._current != this ) ((IDisposable)_logger._current).Dispose();
                    _logger.CloseGroup( null );
                }
            }           

            internal string GroupClose( string externalConclusion )
            {
                string conclusion = OnGroupClose( externalConclusion );
                _logger = null;
                return conclusion ?? String.Empty;
            }

            /// <summary>
            /// Called whenever the group is closing.
            /// Must return the actual conclusion that will be used for the group: if the <paramref name="externalConclusion"/> is 
            /// not null nor empty, it takes precedence on the (optional) <see cref="GetConclusionText"/> functions.
            /// </summary>
            /// <param name="externalConclusion">Conclusion parameter: comes from <see cref="IActivityLogger.CloseGroup"/>. Can be null.</param>
            /// <returns>The final conclusion to use.</returns>
            protected virtual string OnGroupClose( string externalConclusion )
            {
                if( String.IsNullOrEmpty( externalConclusion ) )
                {
                    externalConclusion = ConsumeConclusionText();
                }
                return externalConclusion;
            }

            /// <summary>
            /// Calls <see cref="GetConclusionText"/> and sets it to null.
            /// </summary>
            /// <returns></returns>
            protected virtual string ConsumeConclusionText()
            {
                string autoText = null;
                if( GetConclusionText != null )
                {
                    try
                    {
                        autoText = GetConclusionText();
                    }
                    catch( Exception ex )
                    {
                        autoText = "Unexpected Error while getting conclusion text: " + ex.Message;
                    }
                    GetConclusionText = null;
                }
                return autoText;
            }
        }

        /// <summary>
        /// Opens a <see cref="Group"/> configured with the given parameters.
        /// </summary>
        /// <param name="level">The log level of the group.</param>
        /// <param name="defaultConclusionText">
        /// Optional function that will be called on group closing to obtain a conclusion
        /// if no explicit conclusion is provided through <see cref="CloseGroup"/>.
        /// </param>
        /// <param name="text">Text to log (the title of the group). Null text is valid and considered as <see cref="String.Empty"/>.</param>
        /// <returns>The <see cref="Group"/>.</returns>
        public virtual IDisposable OpenGroup( LogLevel level, Func<string> defaultConclusionText, string text )
        {
            ++_depth;
            Group g = CreateGroup( level, text ?? String.Empty, defaultConclusionText );
            _current = g;
            _output.OnOpenGroup( g );
            return g;
        }

        /// <summary>
        /// Closes the current <see cref="Group"/>.
        /// </summary>
        /// <param name="conclusion">Optional text to conclude the group.</param>
        public virtual void CloseGroup( string conclusion = null )
        {
            Group g = _current;
            if( g != null )
            {
                conclusion = g.GroupClose( conclusion );
                Debug.Assert( conclusion != null );
                conclusion = _output.OnGroupClosing( g, conclusion );
                --_depth;
                Filter = g.Filter;
                _current = (Group)g.Parent;
                _output.OnGroupClosed( g, conclusion );
            }
        }

        /// <summary>
        /// Factory method for <see cref="Group"/> (or any specialized class).
        /// This is may be overriden in advanced scenario where groups may support more 
        /// information than the default ones.
        /// </summary>
        /// <param name="level">The <see cref="Group.GroupLevel"/> of the group.</param>
        /// <param name="text">The <see cref="Group.GroupText"/>.</param>
        /// <param name="defaultConclusionText">
        /// An optional delegate to call on close to obtain a conclusion text
        /// if no explicit conclusion is provided through <see cref="CloseGroup"/>.
        /// </param>
        /// <returns>A new group.</returns>
        protected virtual Group CreateGroup( LogLevel level, string text, Func<string> defaultConclusionText )
        {
            return new Group( this, level, text, defaultConclusionText );
        }

    }
}
