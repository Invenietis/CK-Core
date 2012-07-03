#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerErrorCounter.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
        /// Encapsulates error information.
        /// It is used as the <see cref="ActivityLogGroupConclusion.Conclusion"/> object: the <see cref="ToString"/> method
        /// displays the conclusion in a default text format.
        /// </summary>
        public class State
        {
            internal readonly State Parent;

            internal State( State parent )
            {
                MaxLogLevel = LogLevel.None;
                Parent = parent;
            }

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
            public override string ToString()
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

            internal void CatchLevel( LogLevel level )
            {
                switch( level )
                {
                    case LogLevel.Fatal:
                        {
                            State s = this;
                            do
                            {
                                s.FatalCount = s.FatalCount + 1;
                                s.MaxLogLevel = LogLevel.Fatal;
                            }
                            while( (s = s.Parent) != null );
                            break;
                        }
                    case LogLevel.Error:
                        {
                            State s = this;
                            do
                            {
                                s.ErrorCount = s.ErrorCount + 1;
                                if( s.MaxLogLevel != LogLevel.Fatal ) s.MaxLogLevel = LogLevel.Error;
                            }
                            while( (s = s.Parent) != null );
                            break;
                        }
                    case LogLevel.Warn:
                        {
                            State s = this;
                            do
                            {
                                s.WarnCount = s.WarnCount + 1;
                                if( s.MaxLogLevel < LogLevel.Warn ) s.MaxLogLevel = LogLevel.Warn;
                            }
                            while( (s = s.Parent) != null );
                            break;
                        }
                    default:
                        {
                            State s = this;
                            do
                            {
                                if( s.MaxLogLevel < level ) s.MaxLogLevel = level;
                            }
                            while( (s = s.Parent) != null );
                            break;
                        }
                }
            }
        }

        State _root;
        State _current;

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

            protected override void OnGroupClosing( IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
            {
            }

            // Security if OnGroupClosed is implemented one day on ActivityLoggerErrorCounter.
            protected override void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
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
            _current = _root = new State( null );
            GenerateConclusion = true;
        }

        /// <summary>
        /// Gets the root <see cref="State"/>.
        /// </summary>
        public State Root 
        { 
            get { return _root; } 
        }

        /// <summary>
        /// Gets the current <see cref="State"/>.
        /// </summary>
        public State Current 
        { 
            get { return _current; } 
        }

        /// <summary>
        /// Gets or sets whether the Group conclusion must be generated.
        /// Defaults to true.
        /// </summary>
        public bool GenerateConclusion { get; set; }

        /// <summary>
        /// Updates error counters.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        protected override void OnUnfilteredLog( LogLevel level, string text )
        {
            _current.CatchLevel( level );
        }

        /// <summary>
        /// Updates error counters.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        protected override void OnOpenGroup( IActivityLogGroup group )
        {
            _current = new State( _current );
            _current.CatchLevel( group.GroupLevel );
        }

        /// <summary>
        /// Handles group conclusion.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">Mutable conclusions associated to the closing group.</param>
        protected override void OnGroupClosing( IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
            if( GenerateConclusion && _current != _root && _current.HasWarnOrError )
            {
                conclusions.Add( new ActivityLogGroupConclusion( _current, this ) );
            }
        }

        /// <summary>
        /// Restores current to the previous one (or keep it on the root if no opened group exist).
        /// </summary>
        /// <param name="group">The log group.</param>
        /// <param name="conclusions">Texts that conclude the group.</param>
        protected override void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _current.Parent != null ) _current = _current.Parent;
        }


    }
}
