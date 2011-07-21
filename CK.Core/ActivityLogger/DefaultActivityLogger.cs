using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// Basic implementation of <see cref="IActivityLogger"/>. 
    /// Handles the groups opening/closing stack and multiplexes calls to <see cref="IDefaultActivityLoggerSink"/>.
    /// For more control, the nested <see cref="DefaultActivityLogger.Group"/> class may also be overriden.
    /// </summary>
    public class DefaultActivityLogger : IActivityLogger
    {
        int _curLevel;
        int _depth;
        Group _current;
        List<IDefaultActivityLoggerSink> _loggers;

        /// <summary>
        /// Initializes a new <see cref="DefaultActivityLogger"/>.
        /// </summary>
        public DefaultActivityLogger()
        {
            _curLevel = -1;
            _loggers = new List<IDefaultActivityLoggerSink>();
        }

        /// <summary>
        /// Registers an <see cref="IDefaultActivityLoggerSink"/> to the logger collection.
        /// Duplicate <see cref="IDefaultActivityLoggerSink"/> are silently ignored.
        /// </summary>
        /// <param name="l">An activity logger implementation</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public virtual DefaultActivityLogger Register( IDefaultActivityLoggerSink l )
        {
            if( !_loggers.Contains( l ) ) _loggers.Add( l );
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IDefaultActivityLoggerSink"/> from the collection of loggers.
        /// Silently ignored unregistered logger.
        /// </summary>
        /// <param name="l">An activity logger implementation</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public virtual DefaultActivityLogger Unregister( IDefaultActivityLoggerSink l )
        {
            _loggers.Remove( l );
            return this;
        }

        /// <summary>
        /// Gets an enumeration of registered <see cref="IDefaultActivityLoggerSink"/>.
        /// </summary>
        public virtual IEnumerable<IDefaultActivityLoggerSink> RegisteredLoggers
        {
            get { return _loggers; }
        }

        /// <summary>
        /// Gets the first <see cref="IDefaultActivityLoggerSink"/> that is comaptible with <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Type of the logger that must be returned.</typeparam>
        /// <returns>The first compatible implementation, or null if no compatible logger exists.</returns>
        public T FirstLogger<T>() where T : IDefaultActivityLoggerSink
        {
            return (T)FirstLogger( typeof( T ) );
        }

        /// <summary>
        /// Gets the first <see cref="IDefaultActivityLoggerSink"/> that is comaptible with <paramref name="loggerType"/> type.
        /// </summary>
        /// <param name="loggerType">Type of the logger that must be returned.</param>
        /// <returns>The first compatible implementation, or null if no compatible logger exists.</returns>
        public IDefaultActivityLoggerSink FirstLogger( Type loggerType )
        {
            if( loggerType == null ) throw new ArgumentNullException( "loggerType" );
            return _loggers.FirstOrDefault( t => loggerType.IsAssignableFrom( t.GetType() ) );
        }

        class EmptyLogger : DefaultActivityLogger, IDisposable
        {
            public override IActivityLogger UnfilteredLog( LogLevel level, string text ) { return this; }
            public override IDisposable OpenGroup( LogLevel level, string text, Func<string> getConclusionText ) { return this; }
            public override void CloseGroup( string conclusion ) { }
            public override DefaultActivityLogger Register( IDefaultActivityLoggerSink l ) { return this; }
            public override DefaultActivityLogger Unregister( IDefaultActivityLoggerSink l ) { return this; }
            public override IEnumerable<IDefaultActivityLoggerSink> RegisteredLoggers { get { return ReadOnlyListEmpty<IDefaultActivityLoggerSink>.Empty; } }
            public void Dispose() { }
        }

        /// <summary>
        /// Empty <see cref="IActivityLogger"/>: nothing is done.
        /// </summary>
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The EmptyLogger is indeed IDisposable, but this is just for the coherence of an empty implementation. There is nothing to dispose." )]
        public static readonly DefaultActivityLogger Empty = new EmptyLogger();

        /// <summary>
        /// Groups are linked together from the current one to the very first one.
        /// </summary>
        public class Group : IDisposable
        {
            DefaultActivityLogger _logger;

            /// <summary>
            /// Initializes a new <see cref="Group"/> object.
            /// </summary>
            /// <param name="logger">The logger.</param>
            /// <param name="level">The <see cref="GroupLevel"/>.</param>
            /// <param name="text">The <see cref="GroupText"/>.</param>
            /// <param name="getConclusionText">The delegate to call on close to obtain a conclusion text.</param>
            public Group( DefaultActivityLogger logger, LogLevel level, string text, Func<string> getConclusionText )
            {
                _logger = logger;
                OpeningFilter = _logger.Filter;
                Parent = _logger._current;
                GroupLevel = level;
                GroupText = text;
                Depth = _logger._depth;
            }
            /// <summary>
            /// Gets the associated logger. Null whenever this group is closed.
            /// </summary>
            public IActivityLogger Logger { get { return _logger; } }
            /// <summary>
            /// Gets the <see cref="LogLevelFilter"/> that was active when the 
            /// </summary>
            public LogLevelFilter OpeningFilter { get; private set; }
            /// <summary>
            /// Previous group. Null if this is the first opened group.
            /// </summary>
            public Group Parent { get; private set; }
            /// <summary>
            /// Depth of this group (number of parent groups).
            /// </summary>
            public int Depth { get; private set; }
            /// <summary>
            /// Log level with which this group has been opened.
            /// </summary>
            public LogLevel GroupLevel { get; set; }
            /// <summary>
            /// Text with which this group has been opened.
            /// </summary>
            public string GroupText { get; set; }
            /// <summary>
            /// Optional function that will be called on group closing. 
            /// </summary>
            public Func<string> GetConclusionText { get; set; }

            /// <summary>
            /// Calls <see cref="GetConclusionText"/> and sets it to null.
            /// </summary>
            /// <returns></returns>
            public virtual string ConsumeConclusionText()
            {
                string autoText = null;
                if( GetConclusionText != null )
                {
                    autoText = GetConclusionText();
                    GetConclusionText = null;
                }
                return autoText;
            }

            /// <summary>
            /// Ensures that any opened groups after this one are closed before closing this one.
            /// </summary>
            public void Dispose()
            {
                if( _logger != null )
                {
                    while( _logger._current != this ) _logger._current.Dispose();
                    _logger.CloseGroup( null );
                }
            }

            internal string GroupClose( string externalConclusion )
            {
                string conclusion = OnGroupClose( externalConclusion );
                _logger = null;
                return conclusion;
            }

            /// <summary>
            /// Called whenever a group is closing.
            /// Must return the actual conclusion that will be used for the group: currently combines
            /// the <see cref="GetConclusionText"/> functions and the <paramref name="externalConclusion"/>
            /// in one string.
            /// </summary>
            /// <param name="externalConclusion">Conclusion parameter: comes from <see cref="IActivityLogger.CloseGroup"/>. Can be null.</param>
            /// <returns>The final conclusion to use.</returns>
            public virtual string OnGroupClose( string externalConclusion )
            {
                string autoText = ConsumeConclusionText();
                if( !String.IsNullOrEmpty( autoText ) )
                {
                    if( !String.IsNullOrEmpty( externalConclusion ) ) return autoText + " - " + externalConclusion;
                    return autoText;
                }
                return externalConclusion;
            }

        }

        /// <summary>
        /// Gets or sets the current <see cref="LogLevelFilter"/>.
        /// </summary>
        public LogLevelFilter Filter { get; set; }

        /// <summary>
        /// Do log the text regardless of current <see cref="Filter"/>.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="text">The text to log.</param>
        /// <returns>This <see cref="IActivityLogger"/> to enable fluent syntax.</returns>
        public virtual IActivityLogger UnfilteredLog( LogLevel level, string text )
        {
            if( text == null || text.Length == 0 )
            {
                if( _curLevel != -1 )
                {
                    foreach( var logger in RegisteredLoggers ) logger.OnLeaveLevel( (LogLevel)_curLevel );
                }
                _curLevel = -1;
            }
            else
            {
                if( _curLevel == (int)level )
                {
                    foreach( var logger in RegisteredLoggers ) logger.OnContinueOnSameLevel( level, text );
                }
                else
                {
                    if( _curLevel != -1 )
                    {
                        foreach( var logger in RegisteredLoggers ) logger.OnLeaveLevel( (LogLevel)_curLevel );
                    }
                    foreach( var logger in RegisteredLoggers ) logger.OnEnterLevel( level, text );
                    _curLevel = (int)level;
                }
            }
            return this;
        }

        /// <summary>
        /// Opens a <see cref="Group"/> configured with the given parameters.
        /// </summary>
        /// <param name="level">The log level of the group.</param>
        /// <param name="text">The text associated to the opening of the log.</param>
        /// <param name="getConclusionText">Optional function that will be called on group closing. </param>
        /// <returns>The <see cref="Group"/>.</returns>
        public virtual IDisposable OpenGroup( LogLevel level, string text, Func<string> getConclusionText )
        {
            if( _curLevel != -1 )
            {
                foreach( var logger in RegisteredLoggers ) logger.OnLeaveLevel( (LogLevel)_curLevel );
            }
            Group g = CreateGroup( level, text, getConclusionText );
            ++_depth;
            _current = g;
            _curLevel = -1;

            foreach( var logger in RegisteredLoggers ) logger.OnGroupOpen( g );

            return g;
        }

        /// <summary>
        /// Closes the current <see cref="Group"/>.
        /// </summary>
        /// <param name="conclusion">Text to conclude the group.</param>
        public virtual void CloseGroup( string conclusion )
        {
            if( _current != null )
            {
                if( _curLevel != -1 )
                {
                    foreach( var logger in RegisteredLoggers ) logger.OnLeaveLevel( (LogLevel)_curLevel );
                }

                conclusion = _current.GroupClose( conclusion );

                foreach( var logger in RegisteredLoggers ) logger.OnGroupClose( _current, conclusion );

                _curLevel = -1;
                --_depth;
                Filter = _current.OpeningFilter;
                _current = _current.Parent;
            }
        }

        /// <summary>
        /// Factory method for <see cref="Group"/> (or any specialized class).
        /// This is may be overriden in advanced scenario where groups may support more 
        /// information than the default ones.
        /// </summary>
        /// <param name="level">The <see cref="Group.GroupLevel"/> of the group.</param>
        /// <param name="text">The <see cref="Group.GroupText"/>.</param>
        /// <param name="getConclusionText">An optional delegate to call on close to obtain a conclusion text.</param>
        /// <returns>A new group.</returns>
        protected virtual Group CreateGroup( LogLevel level, string text, Func<string> getConclusionText )
        {
            return new Group( this, level, text, getConclusionText );
        }

    }
}
