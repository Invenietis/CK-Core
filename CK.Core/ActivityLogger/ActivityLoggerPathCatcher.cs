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
    public class ActivityLoggerPathCatcher : IActivityLoggerClient, IMuxActivityLoggerClient
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
        /// Gets the last where an <see cref="LogLevel.Error"/> or a <see cref="LogLevel.Fatal"/> occured.
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
        public void ClearLastWarnOrErrorPath( bool clearLastErrorPath = false )
        {
            _warnSnaphot = null;
            if( clearLastErrorPath ) _errorSnaphot = null;
        }

        void IActivityLoggerClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
        }

        void IActivityLoggerClient.OnUnfilteredLog( LogLevel level, string text )
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

        void IActivityLoggerClient.OnOpenGroup( IActivityLogGroup group )
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

        string IActivityLoggerClient.OnGroupClosing( IActivityLogGroup group, string conclusion )
        {
            return null;
        }

        void IActivityLoggerClient.OnGroupClosed( IActivityLogGroup group, string conclusion )
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
                _warnSnaphot = _path.ToReadOnlyList();
                if( _current.Level >= LogLevel.Error )
                {
                    _errorSnaphot = _warnSnaphot;
                }
            }
        }

        #region IMuxActivityLoggerClient relayed to IActivityLoggerClient

        void IMuxActivityLoggerClient.OnFilterChanged( IActivityLogger sender, LogLevelFilter current, LogLevelFilter newValue )
        {
            ((IActivityLoggerClient)this).OnFilterChanged( current, newValue );
        }

        void IMuxActivityLoggerClient.OnUnfilteredLog( IActivityLogger sender, LogLevel level, string text )
        {
            ((IActivityLoggerClient)this).OnUnfilteredLog( level, text );
        }

        void IMuxActivityLoggerClient.OnOpenGroup( IActivityLogger sender, IActivityLogGroup group )
        {
            ((IActivityLoggerClient)this).OnOpenGroup( group );
        }

        string IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            return ((IActivityLoggerClient)this).OnGroupClosing( group, conclusion );
        }

        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            ((IActivityLoggerClient)this).OnGroupClosed( group, conclusion );
        }

        #endregion
    }
}
