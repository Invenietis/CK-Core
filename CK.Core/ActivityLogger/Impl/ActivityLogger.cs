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
    public partial class ActivityLogger : IActivityLogger
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
        static public readonly CKTraitContext RegisteredTags;
        
        /// <summary>
        /// Shortcut to <see cref="CKTraitContext.EmptyTrait"/> of <see cref="RegisteredTags"/>.
        /// </summary>
        static public readonly CKTrait EmptyTag;

        /// <summary>
        /// Conlusions provided to IActivityLogger.Close(string) are marked with "c:User".
        /// </summary>
        static public readonly CKTrait TagUserConclusion;

        /// <summary>
        /// Conlusions returned by the optional function when a group is opened (see <see cref="IActivityLogger.OpenGroup"/>) are marked with "c:GetText".
        /// </summary>
        static public readonly CKTrait TagGetTextConclusion;

        /// <summary>
        /// The logging error collector. 
        /// Any error that occurs while dispathing logs to <see cref="IActivityLoggerClient"/> or <see cref="IActivityLoggerSink"/> 
        /// are collected and the culprit is removed from <see cref="Output"/> (resp. <see cref="ActivityLoggerTap"/> sinks).
        /// See <see cref="CriticalErrorCollector"/>.
        /// </summary>
        public static readonly CriticalErrorCollector LoggingError;

        static ActivityLogger()
        {
            RegisteredTags = new CKTraitContext( "ActivityLogger" );
            EmptyTag = ActivityLogger.RegisteredTags.EmptyTrait;
            TagUserConclusion = RegisteredTags.FindOrCreate( "c:User" );
            TagGetTextConclusion = RegisteredTags.FindOrCreate( "c:GetText" );
            LoggingError = new CriticalErrorCollector();
        }

        LogLevelFilter _filter;
        Group[] _groups;
        Group _current;
        ActivityLoggerOutput _output;
        CKTrait _currentTag;

        /// <summary>
        /// Initializes a new <see cref="ActivityLogger"/> with a <see cref="ActivityLoggerOutput"/> as its <see cref="Output"/>.
        /// </summary>
        /// <param name="initialTags">Initial <see cref="AutoTags"/>.</param>
        public ActivityLogger( CKTrait initialTags = null )
        {
            Build( new ActivityLoggerOutput( this ), initialTags );
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityLogger"/> with a specific <see cref="Output"/> or null
        /// to postpone the setting of Output by using <see cref="SetOutput"/>.
        /// </summary>
        /// <param name="output">The output to use. Can be null.</param>
        /// <param name="tags">Initial tags.</param>
        protected ActivityLogger( ActivityLoggerOutput output, CKTrait tags = null  )
        {
            Build( output, tags );
        }

        void Build( ActivityLoggerOutput output, CKTrait tags )
        {
            Debug.Assert( RegisteredTags.Separator == '|', "Separator must be the |." );
            _output = output;
            _groups = new Group[16];
            for( int i = 0; i < _groups.Length; ++i ) _groups[i] = CreateGroup( i );
            _currentTag = tags ?? RegisteredTags.EmptyTrait;
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
        /// Gets or sets the tags of this logger: any subsequent logs will be tagged by these tags.
        /// The <see cref="CKTrait"/> must be registered in <see cref="ActivityLogger.RegisteredTags"/>.
        /// Modifications to this property are scoped to the current Group since when a Group is closed, this
        /// property (like <see cref="Filter"/>) is automatically restored to its original value (captured when the Group was opened).
        /// </summary>
        public CKTrait AutoTags 
        {
            get { return _currentTag; }
            set { _currentTag = value ?? RegisteredTags.EmptyTrait; } 
        }

        /// <summary>
        /// Gets or sets a filter based on the log level.
        /// Modifications to this property are scoped to the current Group since when a Group is closed, this
        /// property (like <see cref="AutoTags"/>) is automatically restored to its original value (captured when the Group was opened).
        /// </summary>
        public LogLevelFilter Filter
        {
            get { return _filter; }
            set
            {
                if( _filter != value )
                {

                    List<IActivityLoggerClient> buggyClients = null;
                    foreach( var l in _output.RegisteredClients )
                    {
                        try
                        {
                            l.OnFilterChanged( _filter, value );
                        }
                        catch( Exception exCall )
                        {
                            LoggingError.Add( exCall, l.GetType().FullName );
                            if( buggyClients == null ) buggyClients = new List<IActivityLoggerClient>();
                            buggyClients.Add( l );
                        }
                    }
                    if( buggyClients != null ) foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
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
        /// <param name="tags">Tags (from <see cref="RegisteredTags"/>) to associate to the log, combined with current <see cref="AutoTags"/>.</param>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log. Ignored if null or empty.</param>
        /// <param name="logTimeUtc">Timestamp of the log entry (must be Utc).</param>
        /// <param name="ex">Optional exception associated to the log. When not null, a Group is automatically created.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        /// <remarks>
        /// A null or empty <paramref name="text"/> is not logged.
        /// If needed, the special text <see cref="ActivityLogger.ParkLevel"/> ("PARK-LEVEL") breaks the current <see cref="LogLevel"/>
        /// and resets it: the next log, even with the same LogLevel, will be treated as if
        /// a different LogLevel is used.
        /// </remarks>
        public IActivityLogger UnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc, Exception ex = null )
        {
            if( logTimeUtc.Kind != DateTimeKind.Utc ) throw new ArgumentException( R.DateTimeMustBeUtc, "logTimeUtc" );
            if( level != LogLevel.None )
            {
                if( ex != null )
                {
                    OpenGroup( tags, level, null, text, logTimeUtc, ex );
                    CloseGroup( logTimeUtc );
                }
                else if( !String.IsNullOrEmpty( text ) )
                {
                    if( tags == null || tags.IsEmpty ) tags = _currentTag;
                    else tags = _currentTag.Union( tags );

                    List<IActivityLoggerClient> buggyClients = null;
                    foreach( var l in _output.RegisteredClients )
                    {
                        try
                        {
                            l.OnUnfilteredLog( tags, level, text, logTimeUtc );
                        }
                        catch( Exception exCall )
                        {
                            LoggingError.Add( exCall, l.GetType().FullName );
                            if( buggyClients == null ) buggyClients = new List<IActivityLoggerClient>();
                            buggyClients.Add( l );
                        }
                    }
                    if( buggyClients != null ) foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
                }
            }
            return this;
        }

        /// <summary>
        /// Opens a <see cref="Group"/> configured with the given parameters.
        /// </summary>
        /// <param name="tags">Tags (from <see cref="RegisteredTags"/>) to associate to the log, combined with current <see cref="AutoTags"/>.</param>
        /// <param name="level">The log level of the group.</param>
        /// <param name="getConclusionText">
        /// Optional function that will be called on group closing to obtain a conclusion
        /// if no explicit conclusion is provided through <see cref="CloseGroup"/>.
        /// </param>
        /// <param name="text">Text to log (the title of the group). Null text is valid and considered as <see cref="String.Empty"/> or assigned to the <see cref="Exception.Message"/> if it exists.</param>
        /// <param name="logTimeUtc">Timestamp of the log entry (must be UTC).</param>
        /// <param name="ex">Optional exception associated to the group.</param>
        /// <returns>The <see cref="Group"/> that can be disposed to close it.</returns>
        public virtual IDisposable OpenGroup( CKTrait tags, LogLevel level, Func<string> getConclusionText, string text, DateTime logTimeUtc, Exception ex = null )
        {
            if( logTimeUtc.Kind != DateTimeKind.Utc ) throw new ArgumentException( R.DateTimeMustBeUtc, "logTimeUtc" );
            if( level == LogLevel.None ) return Util.EmptyDisposable;
            int idxNext = _current != null ? _current.Depth : 0;
            if( idxNext == _groups.Length )
            {
                Array.Resize( ref _groups, _groups.Length*2 );
                for( int i = idxNext; i < _groups.Length; ++i ) _groups[i] = CreateGroup( i );
            }
            _current = _groups[idxNext];
            if( tags == null || tags.IsEmpty ) tags = _currentTag;
            else tags = _currentTag.Union( tags );
            _current.Initialize( tags, level, text ?? (ex != null ? ex.Message : String.Empty), getConclusionText, logTimeUtc, ex );
            List<IActivityLoggerClient> buggyClients = null;
            foreach( var l in _output.RegisteredClients )
            {
                try
                {
                    l.OnOpenGroup( _current );
                }
                catch( Exception exCall )
                {
                    LoggingError.Add( exCall, l.GetType().FullName );
                    if( buggyClients == null ) buggyClients = new List<IActivityLoggerClient>();
                    buggyClients.Add( l );
                }
            }
            if( buggyClients != null ) foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
            return _current;
        }

        /// <summary>
        /// Closes the current <see cref="Group"/>. Optional parameter is polymorphic. It can be a string, a <see cref="ActivityLogGroupConclusion"/>, 
        /// a <see cref="List{T}"/> or an <see cref="IEnumerable{T}"/> of ActivityLogGroupConclusion, or any object with an overriden <see cref="Object.ToString"/> method. 
        /// See remarks (especially for List&lt;ActivityLogGroupConclusion&gt;).
        /// </summary>
        /// <param name="userConclusion">Optional string, enumerable of <see cref="ActivityLogGroupConclusion"/>) or object to conclude the group. See remarks.</param>
        /// <param name="logTimeUtc">Timestamp of the group closing.</param>
        /// <remarks>
        /// An untyped object is used here to easily and efficiently accomodate both string and already existing ActivityLogGroupConclusion.
        /// When a List&lt;ActivityLogGroupConclusion&gt; is used, it will be direclty used to collect conclusion objects (new conclusions will be added to it). This is an optimization.
        /// </remarks>
        public virtual void CloseGroup( DateTime logTimeUtc, object userConclusion = null )
        {
            Group g = _current;
            if( g != null )
            {
                g.CloseLogTimeUtc = logTimeUtc;
                var conclusions = userConclusion as List<ActivityLogGroupConclusion>;
                if( conclusions == null && userConclusion != null )
                {
                    conclusions = new List<ActivityLogGroupConclusion>();
                    string s = userConclusion as string;
                    if( s != null ) conclusions.Add( new ActivityLogGroupConclusion( TagUserConclusion, s ) );
                    else
                    {
                        if( userConclusion is ActivityLogGroupConclusion )
                        {
                            conclusions.Add( (ActivityLogGroupConclusion)userConclusion );
                        }
                        else
                        {
                            IEnumerable<ActivityLogGroupConclusion> multi = userConclusion as IEnumerable<ActivityLogGroupConclusion>;
                            if( multi != null ) conclusions.AddRange( multi );
                            else conclusions.Add( new ActivityLogGroupConclusion( TagUserConclusion, userConclusion.ToString() ) );
                        }
                    }
                }
                g.GroupClosing( ref conclusions );

                List<IActivityLoggerClient> buggyClients = null;
                foreach( var l in _output.RegisteredClients )
                {
                    try
                    {
                        l.OnGroupClosing( g, ref conclusions );
                    }
                    catch( Exception exCall )
                    {
                        LoggingError.Add( exCall, l.GetType().FullName );
                        if( buggyClients == null ) buggyClients = new List<IActivityLoggerClient>();
                        buggyClients.Add( l );
                    }
                }
                if( buggyClients != null )
                {
                    foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
                    buggyClients.Clear();
                }
                
                Filter = g.SavedLoggerFilter;
                _currentTag = g.SavedLoggerTags;
                _current = (Group)g.Parent;

                var sentConclusions = conclusions != null ? conclusions.ToReadOnlyList() : CKReadOnlyListEmpty<ActivityLogGroupConclusion>.Empty;
                foreach( var l in _output.RegisteredClients )
                {
                    try
                    {
                        l.OnGroupClosed( g, sentConclusions );
                    }
                    catch( Exception exCall )
                    {
                        LoggingError.Add( exCall, l.GetType().FullName );
                        if( buggyClients == null ) buggyClients = new List<IActivityLoggerClient>();
                        buggyClients.Add( l );
                    }
                }
                if( buggyClients != null ) foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
                g.GroupClosed();
            }
        }

    }
}
