using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// The "Path Catcher" captures the current path of the log (<see cref="DynamicPath"/>),
    /// and two specific paths, the <see cref="LastErrorPath"/> and the <see cref="LastWarnOrErrorPath"/>.
    /// It is both a <see cref="IMuxActivityLoggerClient"/> and a <see cref="IActivityLoggerClient"/>.
    /// </summary>
    public class ActivityLoggerPathCatcher : ActivityLoggerHybridClient
    {
        /// <summary>
        /// Element of the <see cref="ActivityLoggerPathCatcher.DynamicPath">DynamicPath</see>, <see cref="ActivityLoggerPathCatcher.LastErrorPath">LastErrorPath</see>,
        /// or <see cref="ActivityLoggerPathCatcher.LastWarnOrErrorPath">LastWarnOrErrorPath</see>.
        /// </summary>
        public class PathElement
        {
            /// <summary>
            /// Gets the log level of the log entry.
            /// </summary>
            public LogLevel Level { get; internal set; }
            /// <summary>
            /// Gets the text of the log entry.
            /// </summary>
            public string Text { get; internal set; }
            /// <summary>
            /// Gets the conclusion associated to a group. Null if ther is no conclusion
            /// or if this element does not correspond to a group.
            /// </summary>
            public string GroupConclusion { get; internal set; }
        }

        /// <summary>
        /// Reuse the ActivityLoggerPathCatcher: since all hooks are empty, no paths exist.
        /// </summary>
        class EmptyPathCatcher : ActivityLoggerPathCatcher
        {
            // Security if OnFilterChanged is implemented one day on ActivityLoggerPathCatcher.
            protected override void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
            {
            }

            protected override void OnUnfilteredLog( LogLevel level, string text )
            {
            }

            protected override void OnOpenGroup( IActivityLogGroup group )
            {
            }

            // Security if OnGroupClosing is implemented one day on ActivityLoggerPathCatcher.
            protected override string OnGroupClosing( IActivityLogGroup group, string conclusion )
            {
                return null;
            }

            protected override void OnGroupClosed( IActivityLogGroup group, string conclusion )
            {
            }
        }

        /// <summary>
        /// Empty <see cref="ActivityLoggerPathCatcher"/> (null object design pattern).
        /// </summary>
        static public new readonly ActivityLoggerPathCatcher Empty = new EmptyPathCatcher();

        IReadOnlyList<PathElement> _errorSnaphot;
        IReadOnlyList<PathElement> _warnSnaphot;

        List<PathElement> _path;
        IReadOnlyList<PathElement> _pathEx;
        PathElement _current;
        bool _currentIsGroup;

        /// <summary>
        /// Initializes a new <see cref="ActivityLoggerPathCatcher"/>.
        /// </summary>
        public ActivityLoggerPathCatcher()
        {
            _path = new List<PathElement>();
            _pathEx = new ReadOnlyListOnIList<PathElement>( _path );
        }

        /// <summary>
        /// Gets the current (mutable) path. You should use <see cref="ReadOnlyExtension.ToReadOnlyList{T}(IList{T})"/> or other ToArray 
        /// or ToList methods to take a snapshot of this list.
        /// </summary>
        public IReadOnlyList<PathElement> DynamicPath
        {
            get { return _pathEx; }
        }

        /// <summary>
        /// Gets the last <see cref="DynamicPath"/> where an <see cref="LogLevel.Error"/> or a <see cref="LogLevel.Fatal"/> occured.
        /// Null if no error nor fatal occured.
        /// </summary>
        public IReadOnlyList<PathElement> LastErrorPath
        {
            get { return _errorSnaphot; }
        }

        /// <summary>
        /// Clears current <see cref="LastErrorPath"/> (sets it to null).
        /// </summary>
        public void ClearLastErrorPath()
        {
            _errorSnaphot = null;
        }

        /// <summary>
        /// Gets the last path with a <see cref="LogLevel.Fatal"/>, <see cref="LogLevel.Error"/> or a <see cref="LogLevel.Warn"/>.
        /// Null if no error, fatal nor warn occured.
        /// </summary>
        public IReadOnlyList<PathElement> LastWarnOrErrorPath
        {
            get { return _warnSnaphot; }
        }

        /// <summary>
        /// Clears current <see cref="LastWarnOrErrorPath"/> (sets it to null), and
        /// optionnaly clears <see cref="LastErrorPath"/>.
        /// </summary>
        public void ClearLastWarnPath( bool clearLastErrorPath = false )
        {
            _warnSnaphot = null;
            if( clearLastErrorPath ) _errorSnaphot = null;
        }

        /// <summary>
        /// Appends or updates the last <see cref="PathElement"/> of <see cref="DynamicPath"/>
        /// and handles errors or warning.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        protected override void OnUnfilteredLog( LogLevel level, string text )
        {
            if( text != ActivityLogger.ParkLevel )
            {
                if( _currentIsGroup || _current == null )
                {
                    _current = new PathElement();
                    _path.Add( _current );
                    _currentIsGroup = false;
                }
                _current.Level = level;
                _current.Text = text;
                CheckSnapshot();
            }
        }

        /// <summary>
        /// Appends or updates the last <see cref="PathElement"/> of <see cref="DynamicPath"/>
        /// and handles errors or warning.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        protected override void OnOpenGroup( IActivityLogGroup group )
        {
            if( _currentIsGroup || _current == null )
            {
                _current = new PathElement();
                _path.Add( _current );
            }
            _currentIsGroup = true;
            _current.Level = group.GroupLevel;
            _current.Text = group.GroupText;
            CheckSnapshot();
        }

        /// <summary>
        /// Removes one or two last <see cref="PathElement"/> of <see cref="DynamicPath"/>.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusion">Text that concludes the group. Never null but can be empty.</param>
        protected override void OnGroupClosed( IActivityLogGroup group, string conclusion )
        {
            if( _path.Count > 0 )
            {
                if( !_currentIsGroup ) _path.RemoveAt( _path.Count - 1 );
                _current = null;
                if( _path.Count > 0 )
                {
                    _current = _path[_path.Count - 1];
                    _current.GroupConclusion = conclusion;
                    _path.RemoveAt( _path.Count - 1 );
                    if( _path.Count > 0 ) _current = _path[_path.Count - 1];
                }
                _currentIsGroup = _current != null;
            }
        }

        void CheckSnapshot()
        {
            Debug.Assert( _current != null );
            if( _current.Level >= LogLevel.Warn )
            {
                // Clone the last element if it is not a group: since it is updated
                // with levels, it has to be snapshoted.
                _warnSnaphot = _path.Select( ( e, idx ) => _currentIsGroup || idx < _path.Count-1 ? e : new PathElement() { Level = e.Level, Text = e.Text } ).ToReadOnlyList();
                if( _current.Level >= LogLevel.Error )
                {
                    _errorSnaphot = _warnSnaphot;
                }
            }
        }

    }
}
