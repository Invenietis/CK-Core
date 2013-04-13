#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLogger.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Concrete implementation of <see cref="IActivityLogger"/>.
    /// </summary>
    public class ActivityLogger : IActivityLogger
    {
        /// <summary>
        /// String to use to break the current <see cref="LogLevel"/> (as if a different <see cref="LogLevel"/> was used).
        /// </summary>
        static public readonly string ParkLevel = "PARK-LEVEL";

        /// <summary>
        /// Thread-safe contexts for traits used to categorize log entries and group conclusions.
        /// All traits used in logging must be registered here.
        /// </summary>
        /// <remarks>
        /// Tags used for conclusions should start with "c:".
        /// </remarks>
        static public readonly CKTraitContext Tags;
        
        /// <summary>
        /// Conlusions provided to IActivityLogger.Close(string) are marked with "c:User".
        /// </summary>
        static public readonly CKTrait TagUserConclusion;

        /// <summary>
        /// Conlusions returned by the optional function when a group is opened (see <see cref="IActivityLogger.OpenGroup"/>) are marked with "c:GetText".
        /// </summary>
        static public readonly CKTrait TagGetTextConclusion;

        static ActivityLogger()
        {
            Tags = new CKTraitContext();
            TagUserConclusion = Tags.FindOrCreate( "c:User" );
            TagGetTextConclusion = Tags.FindOrCreate( "c:GetText" );
        }

        LogLevelFilter _filter;
        Group _current;
        int _depth;
        ActivityLoggerOutput _output;

        /// <summary>
        /// Initializes a new <see cref="ActivityLogger"/> with a <see cref="ActivityLoggerOutput"/> as its <see cref="Output"/>.
        /// </summary>
        public ActivityLogger()
        {
            Debug.Assert( Tags.Separator == '|', "Separator must be the |." );
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
                    ((IActivityLoggerClient)_output).OnFilterChanged( _filter, value );
                    _filter = value;
                }
            }
        }

        /// <summary>
        /// Logs a text regardless of <see cref="Filter"/> level. 
        /// Each call to log is considered as a unit of text: depending on the rendering engine, a line or a 
        /// paragraph separator (or any appropriate separator) should be appended between each text if 
        /// the <paramref name="level"/> is the same as the previous one.
        /// See remarks.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log. Ignored if null or empty.</param>
        /// <param name="ex">Optional exception associated to the log. When not null, a Group is automatically created.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        /// <remarks>
        /// A null or empty <paramref name="text"/> is not logged.
        /// The special text "PARK-LEVEL" breaks the current <see cref="LogLevel"/>
        /// and resets it: the next log, even with the same LogLevel, will be treated as if
        /// a different LogLevel is used.
        /// </remarks>
        public IActivityLogger UnfilteredLog( LogLevel level, string text, Exception ex )
        {
            if( level != LogLevel.None )
            {
                if( ex != null )
                {
                    OpenGroup( level, null, text, ex );
                    CloseGroup();
                }
                else if( !String.IsNullOrEmpty( text ) )
                {
                    ((IActivityLoggerClient)_output).OnUnfilteredLog( level, text );
                }
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
            /// explicit conclusion is provided through <see cref="IActivityLogger.CloseGroup"/>.
            /// </param>
            /// <param name="ex">Optional exception associated to the group.</param>
            internal protected Group( ActivityLogger logger, LogLevel level, string text, Func<string> defaultConclusionText, Exception ex )
            {
                _logger = logger;
                Parent = logger._current;
                Depth = logger._depth;
                Filter = logger.Filter;
                // Logs everything when a Group is an error: we then have full details without
                // logging all with Error or Fatal.
                if( level >= LogLevel.Error ) logger.Filter = LogLevelFilter.Trace;
                GroupLevel = level;
                GroupText = text;
                GetConclusionText = defaultConclusionText;
                Exception = ex;
            }

            /// <summary>
            /// Gets the origin <see cref="IActivityLogger"/> for the log group.
            /// </summary>
            public IActivityLogger OriginLogger { get { return _logger; } }

            /// <summary>
            /// Get the previous group in its <see cref="OriginLogger"/>. Null if this is a top level group.
            /// </summary>
            public IActivityLogGroup Parent { get; private set; }
            
            /// <summary>
            /// Gets or sets the <see cref="LogLevelFilter"/> for this group.
            /// Initialized with the <see cref="IActivityLogger.Filter"/> when the group has been opened.
            /// </summary>
            public LogLevelFilter Filter { get; protected set; }

            /// <summary>
            /// Gets the depth of this group in its <see cref="OriginLogger"/> (1 for top level groups).
            /// </summary>
            public int Depth { get; private set; }

            /// <summary>
            /// Gets the level of this group.
            /// </summary>
            public LogLevel GroupLevel { get; private set; }
            
            /// <summary>
            /// Gets the text with which this group has been opened.
            /// </summary>
            public string GroupText { get; private set; }

            /// <summary>
            /// Gets the associated <see cref="Exception"/> if it exists.
            /// </summary>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Gets whether the <see cref="GroupText"/> is actually the <see cref="Exception"/> message.
            /// </summary>
            public bool IsGroupTextTheExceptionMessage 
            {
                get { return Exception != null && ReferenceEquals( Exception.Message, GroupText ); } 
            }

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

            internal void GroupClose( List<ActivityLogGroupConclusion> conclusions )
            {
                string auto = ConsumeConclusionText();
                if( auto != null ) conclusions.Add( new ActivityLogGroupConclusion( TagGetTextConclusion, auto ) );
                _logger = null;
            }

            /// <summary>
            /// Calls <see cref="GetConclusionText"/> and sets it to null.
            /// </summary>
            string ConsumeConclusionText()
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
        /// <param name="ex">Optional exception associated to the group.</param>
        /// <returns>The <see cref="Group"/>.</returns>
        public virtual IDisposable OpenGroup( LogLevel level, Func<string> defaultConclusionText, string text, Exception ex )
        {
            if( level == LogLevel.None ) return Util.EmptyDisposable;
            ++_depth;
            Group g = CreateGroup( level, text ?? (ex != null ? ex.Message : String.Empty), defaultConclusionText, ex );
            _current = g;
            ((IActivityLoggerClient)_output).OnOpenGroup( g );
            return g;
        }

        /// <summary>
        /// Closes the current <see cref="Group"/>. Optionl parameter is ploymorphic. It can be a string, an enumerable of <see cref="ActivityLogGroupConclusion"/>, 
        /// or any object with an overriden <see cref="Object.ToString"/> method.
        /// </summary>
        /// <param name="userConclusion">Optional string, enumerable of <see cref="ActivityLogGroupConclusion"/>) or object to conclude the group. See remarks.</param>
        /// <remarks>
        /// An untyped object is used here to easily and efficiently accomodate both string and already existing IEnumerable&lt;ActivityLogGroupConclusion&gt; conclusions.
        /// </remarks>
        public virtual void CloseGroup( object userConclusion = null )
        {
            Group g = _current;
            if( g != null )
            {
                var conclusions = new List<ActivityLogGroupConclusion>();
                if( userConclusion != null )
                {
                    string s = userConclusion as string;
                    if( s != null ) conclusions.Add( new ActivityLogGroupConclusion( TagUserConclusion, s ) );
                    else
                    {
                        IEnumerable<ActivityLogGroupConclusion> multi = userConclusion as IEnumerable<ActivityLogGroupConclusion>;
                        if( multi != null ) conclusions.AddRange( multi );
                        else conclusions.Add( new ActivityLogGroupConclusion( TagUserConclusion, userConclusion.ToString() ) );
                    }
                }
                g.GroupClose( conclusions );
                ((IActivityLoggerClient)_output).OnGroupClosing( g, conclusions );
                --_depth;
                Filter = g.Filter;
                _current = (Group)g.Parent;
                ((IActivityLoggerClient)_output).OnGroupClosed( g, conclusions.ToReadOnlyList() );
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
        /// <param name="ex">Optional exception associated to the group.</param>
        /// <returns>A new group.</returns>
        protected virtual Group CreateGroup( LogLevel level, string text, Func<string> defaultConclusionText, Exception ex )
        {
            return new Group( this, level, text, defaultConclusionText, ex );
        }

    }
}
