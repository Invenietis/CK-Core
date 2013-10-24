#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitor.cs) is part of CiviKey. 
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
using System.Threading;
using CK.Core.Impl;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Concrete implementation of <see cref="IActivityMonitor"/>.
    /// </summary>
    public partial class ActivityMonitor : IActivityMonitorImpl
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
        /// Conlusions provided to IActivityMonitor.Close(string) are marked with "c:User".
        /// </summary>
        static public readonly CKTrait TagUserConclusion;

        /// <summary>
        /// Conlusions returned by the optional function when a group is opened (see <see cref="IActivityMonitor.UnfilteredOpenGroup"/>) are marked with "c:GetText".
        /// </summary>
        static public readonly CKTrait TagGetTextConclusion;

        /// <summary>
        /// Whenever <see cref="Topic"/> changed, a <see cref="LogLevel.Info"/> is emitted marked with "MonitorTopicChanged".
        /// </summary>
        static public readonly CKTrait TagMonitorTopicChanged;

        /// <summary>
        /// The monitoring error collector. 
        /// Any error that occurs while dispathing logs to <see cref="IActivityMonitorClient"/>
        /// are collected and the culprit is removed from <see cref="Output"/>.
        /// See <see cref="CriticalErrorCollector"/>.
        /// </summary>
        public static readonly CriticalErrorCollector MonitoringError;

        static LogFilter _defaultFilterLevel;
        
        /// <summary>
        /// Internal event used by ActivityMonitorBridgeTarget that have at least one ActivityMonitorBridge in another application domain.
        /// </summary>
        static internal event EventHandler DefaultFilterLevelChanged;
        static object _lockDefaultFilterLevel;

        /// <summary>
        /// Gets or sets the default filter that should be used when the <see cref="IActivityMonitor.ActualFilter"/> is <see cref="LogLevelFilter.None"/>.
        /// This configuration is per application domain (the backing field is static).
        /// It defaults to <see cref="LogLevelFilter.None"/>: it has the same effect as setting it to <see cref="LogLevelFilter.Trace"/> (ie. logging everything) when
        /// no other configuration exists.
        /// </summary>
        public static LogFilter DefaultFilter
        {
            get { return _defaultFilterLevel; }
            set 
            {
                lock( _lockDefaultFilterLevel )
                {
                    if( _defaultFilterLevel != value )
                    {
                        _defaultFilterLevel = value;
                        var h = DefaultFilterLevelChanged;
                        if( h != null )
                        {
                            try
                            {
                                h( null, EventArgs.Empty );
                            }
                            catch( Exception ex )
                            {
                                MonitoringError.Add( ex, "DefaultFilter changed." );
                            }
                        }
                    }
                }
            } 
        }

        /// <summary>
        /// The automatic configuration actions.
        /// Registers actions via += (or <see cref="Delegate.Combine"/> if you like pain), unregister with -= operator (or <see cref="Delegate.Remove"/>).
        /// Simply sets it to null to clear all currently registered actions (this, of course, only from tests and not in real code).
        /// </summary>
        static public Action<IActivityMonitor> AutoConfiguration;

        static ActivityMonitor()
        {
            RegisteredTags = new CKTraitContext( "ActivityMonitor" );
            EmptyTag = ActivityMonitor.RegisteredTags.EmptyTrait;
            TagUserConclusion = RegisteredTags.FindOrCreate( "c:User" );
            TagGetTextConclusion = RegisteredTags.FindOrCreate( "c:GetText" );
            TagMonitorTopicChanged = RegisteredTags.FindOrCreate( "MonitorTopicChanged" );
            MonitoringError = new CriticalErrorCollector();
            AutoConfiguration = null;
            _defaultFilterLevel = LogFilter.Undefined;
            _lockDefaultFilterLevel = new object();
        }

        LogFilter _actualFilter;
        LogFilter _configuredFilter;
        LogFilter _clientFilter;
        Group[] _groups;
        Group _current;
        Group _currentUnfiltered;
        ActivityMonitorOutput _output;
        CKTrait _currentTag;
        int _enteredThreadId;
        Guid _uniqueId;
        string _topic;
        bool _actualFilterIsDirty;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitor"/>.
        /// </summary>
        /// <param name="applyAutoConfigurations">Whether <see cref="AutoConfiguration"/> should be applied.</param>
        public ActivityMonitor( bool applyAutoConfigurations = true )
        {
            Build( new ActivityMonitorOutput( this ), EmptyTag, applyAutoConfigurations );
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitor"/> with a specific <see cref="Output"/> or null
        /// to postpone the setting of Output by using <see cref="SetOutput"/>.
        /// </summary>
        /// <param name="output">The output to use. Can be null.</param>
        /// <param name="tags">Initial tags.</param>
        /// <param name="applyAutoConfigurations">Whether <see cref="AutoConfiguration"/> should be applied.</param>
        protected ActivityMonitor( ActivityMonitorOutput output, CKTrait tags = null, bool applyAutoConfigurations = true  )
        {
            Build( output, tags, applyAutoConfigurations );
        }

        void Build( ActivityMonitorOutput output, CKTrait tags, bool applyAutoConfigurations )
        {
            Debug.Assert( RegisteredTags.Separator == '|', "Separator must be the |." );
            _output = output;
            _groups = new Group[8];
            for( int i = 0; i < _groups.Length; ++i ) _groups[i] = CreateGroup( i );
            _currentTag = tags ?? EmptyTag;
            _uniqueId = Guid.NewGuid();
            _topic = String.Empty;
            var autoConf = AutoConfiguration;
            if( autoConf != null && applyAutoConfigurations ) autoConf( this );
        }

        Guid IUniqueId.UniqueId
        {
            get { return _uniqueId; }
        }

        /// <summary>
        /// Sets the unique identifier of this activity monitor.
        /// This must be used only during monitor construction or initialization, before the 
        /// monitor is actually used.
        /// </summary>
        /// <param name="uniqueId">New unique identifier.</param>
        protected void SetUniqueId( Guid uniqueId )
        {
            _uniqueId = uniqueId;
        }

        /// <summary>
        /// Gets the <see cref="IActivityMonitorOutput"/> for this monitor.
        /// </summary>
        public IActivityMonitorOutput Output
        {
            get { return _output; }
        }

        /// <summary>
        /// Sets the <see cref="Output"/>.
        /// </summary>
        /// <param name="output">Can not be null.</param>
        protected void SetOutput( ActivityMonitorOutput output )
        {
            if( output == null ) throw new ArgumentNullException( "output" );
            _output = output;
        }

        /// <summary>
        /// Gets the current topic for this monitor. This can be any non null string (null topic is mapped to the empty string) that describes
        /// the current activity. It must be set with <see cref="SetTopic"/> and unlike <see cref="MinimalFilter"/> and <see cref="AutoTags"/>, 
        /// the topic is not reseted when groups are closed.
        /// </summary>
        public string Topic 
        {
            get { return _topic; }
        }

        /// <summary>
        /// Sets the current topic for this monitor. This can be any non null string (null topic is mapped to the empty string) that describes
        /// the current activity.
        /// </summary>
        public void SetTopic( string newTopic, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            if( newTopic == null ) newTopic = String.Empty;
            if( _topic != newTopic )
            {
                ReentrantAndConcurrentCheck();
                try
                {
                    DoSetTopic( newTopic, fileName, lineNumber );
                }
                finally
                {
                    ReentrantAndConcurrentRelease();
                }
            }
        }

        void DoSetTopic( string newTopic, string fileName, int lineNumber )
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            Debug.Assert( newTopic != null && _topic != newTopic );
            _topic = newTopic;
            _output.BridgeTarget.TargetTopicChanged( newTopic, fileName, lineNumber );
            MonoParameterSafeCall( ( client, topic ) => client.OnTopicChanged( topic, fileName, lineNumber ), newTopic );
            DoUnfilteredLog( new ActivityMonitorData( LogLevel.Info, TagMonitorTopicChanged, "Topic:" + newTopic, DateTime.UtcNow, null, fileName, lineNumber ) );
        }

        /// <summary>
        /// Gets or sets the tags of this monitor: any subsequent logs will be tagged by these tags.
        /// The <see cref="CKTrait"/> must be registered in <see cref="ActivityMonitor.RegisteredTags"/>.
        /// Modifications to this property are scoped to the current Group since when a Group is closed, this
        /// property (like <see cref="MinimalFilter"/>) is automatically restored to its original value (captured when the Group was opened).
        /// </summary>
        public CKTrait AutoTags 
        {
            get { return _currentTag; }
            set
            {
                if( value == null ) value = ActivityMonitor.EmptyTag;
                else if( value.Context != ActivityMonitor.RegisteredTags ) throw new ArgumentException( R.ActivityMonitorTagMustBeRegistered, "value" );
                if( _currentTag != value )
                {
                    ReentrantAndConcurrentCheck();
                    try
                    {
                        DoSetAutoTags( value );
                    }
                    finally
                    {
                        ReentrantAndConcurrentRelease();
                    }
                }
            }
        }

        void DoSetAutoTags( CKTrait newTags )
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            Debug.Assert( newTags != null && _currentTag != newTags && newTags.Context == RegisteredTags );
            _currentTag = newTags;
            _output.BridgeTarget.TargetAutoTagsChanged( newTags );
            MonoParameterSafeCall( ( client, tags ) => client.OnAutoTagsChanged( tags ), newTags );
        }

        /// <summary>
        /// Called by IActivityMonitorBoundClient clients to initialize Topic and AutoTag from 
        /// inside their SetMonitor or any other methods provided that a reentrancy and concurrent lock 
        /// has been obtained (otherwise an InvalidOperationException is thrown).
        /// </summary>
        void IActivityMonitorImpl.InitializeTopicAndAutoTags( string newTopic, CKTrait newTags, string fileName, int lineNumber )
        {
            RentrantOnlyCheck();
            if( newTopic != null && _topic != newTopic ) DoSetTopic( newTopic, fileName, lineNumber );
            if( newTags != null && _currentTag != newTags ) DoSetAutoTags( newTags );
        }

        /// <summary>
        /// Gets or sets a filter based for the log level.
        /// Modifications to this property are scoped to the current Group since when a Group is closed, this
        /// property (like <see cref="AutoTags"/>) is automatically restored to its original value (captured when the Group was opened).
        /// </summary>
        public LogFilter MinimalFilter
        {
            get { return _configuredFilter; }
            set
            {
                if( _configuredFilter != value )
                {
                    ReentrantAndConcurrentCheck();
                    try
                    {
                        DoSetConfiguredFilter( value );
                    }
                    finally
                    {
                        ReentrantAndConcurrentRelease();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the actual filter level for logs: this combines the configured <see cref="MinimalFilter"/> and the minimal requirements
        /// of any <see cref="IActivityMonitorBoundClient"/> that specifies such a minimal filter level.
        /// </summary>
        /// <remarks>
        /// This does NOT take into account the static (application-domain) <see cref="ActivityMonitor.DefaultFilter"/>.
        /// This global default must be used if this ActualFilter is <see cref="LogLevelFilter.None"/> for <see cref="LogFilter.Line"/> or <see cref="LogFilter.Group"/>: 
        /// the <see cref="ActivityMonitorExtension.ShouldLogLine">ShouldLog</see> extension method takes it into account.
        /// </remarks>
        public LogFilter ActualFilter 
        {
            get 
            {
                if( _actualFilterIsDirty ) ResyncActualFilter();
                return _actualFilter; 
            } 
        }

        void ResyncActualFilter()
        {
            ReentrantAndConcurrentCheck();
            try
            {
                do
                {
                    Thread.MemoryBarrier();
                    _actualFilterIsDirty = false;
                    _clientFilter = DoGetBoundClientMinimalFilter();
                    Thread.MemoryBarrier();
                }
                while( _actualFilterIsDirty );
                UpdateActualFilter();
            }
            finally
            {
                ReentrantAndConcurrentRelease();
            }
        }

        internal void DoSetConfiguredFilter( LogFilter value )
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            Debug.Assert( _configuredFilter != value );
            _configuredFilter = value;
            UpdateActualFilter();
        }

        void UpdateActualFilter()
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            LogFilter newLevel = _configuredFilter.Combine( _clientFilter );
            if( newLevel != _actualFilter )
            {
                _actualFilter = newLevel;
                _output.BridgeTarget.TargetActualFilterChanged();
            }
        }

        LogFilter DoGetBoundClientMinimalFilter()
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            
            LogFilter minimal = LogFilter.Undefined;
            List<IActivityMonitorClient> buggyClients = null;
            foreach( var l in _output.Clients )
            {
                IActivityMonitorBoundClient bound = l as IActivityMonitorBoundClient;
                if( bound != null )
                {
                    try
                    {
                        minimal = minimal.Combine( bound.MinimalFilter );
                        if( minimal == LogFilter.Debug ) break;
                    }
                    catch( Exception exCall )
                    {
                        MonitoringError.Add( exCall, l.GetType().FullName );
                        if( buggyClients == null ) buggyClients = new List<IActivityMonitorClient>();
                        buggyClients.Add( l );
                    }
                }
            }
            if( buggyClients != null )
            {
                foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
            }
            return minimal;
        }

        void IActivityMonitorImpl.SetClientMinimalFilterDirty()
        {
            _actualFilterIsDirty = true;
            Thread.MemoryBarrier();
            // By signaling here the change to the bridge, we handle the case where the current
            // active thread works on a bridged monitor: the bridged monitor's _actualFilterIsDirty
            // is set to true and any interaction with its ActualFilter will trigger a resynchronization
            // of this _actualFilter.
            _output.BridgeTarget.TargetActualFilterChanged();
        }

        void IActivityMonitorImpl.OnClientMinimalFilterChanged( LogFilter oldLevel, LogFilter newLevel )
        {
            // Silently ignores stupid calls.
            if( oldLevel == newLevel ) return;
            bool reentrantCall = ConcurrentOnlyCheck();
            try
            {
                Thread.MemoryBarrier();
                bool dirty = _actualFilterIsDirty;
                do
                {
                    _actualFilterIsDirty = false;
                    // Optimization for some cases: if we can be sure that the oldLevel has no impact on the current 
                    // client filter, we can conclude without getting the all the minimal filters.
                    if( !dirty && ((oldLevel.Line == LogLevelFilter.None || oldLevel.Line > _clientFilter.Line) && (oldLevel.Group == LogLevelFilter.None || oldLevel.Group > _clientFilter.Group)) )
                    {
                        // This Client had no impact on the current final client filter: if its new level has 
                        // no impact on the current client filter, there is nothing to do.
                        var f = _clientFilter.Combine( newLevel );
                        if( f == _clientFilter ) return;
                        _clientFilter = f;
                    }
                    else
                    {
                        // Whatever the new level is we have to update our client final filter.
                        _clientFilter = DoGetBoundClientMinimalFilter();
                    }
                    Thread.MemoryBarrier();
                }
                while( (dirty = _actualFilterIsDirty) );
                UpdateActualFilter();
            }
            finally
            {
                if( reentrantCall ) ReentrantAndConcurrentCheck();
            }
        }


        /// <summary>
        /// Logs a text regardless of <see cref="MinimalFilter"/> level. 
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
        /// <param name="fileName">The source code file name from which the log is emitted.</param>
        /// <param name="lineNumber">The line number in the source from which the log is emitted.</param>
        /// <remarks>
        /// A null or empty <paramref name="text"/> is not logged.
        /// If needed, the special text <see cref="ActivityMonitor.ParkLevel"/> ("PARK-LEVEL") breaks the current <see cref="LogLevel"/>
        /// and resets it: the next log, even with the same LogLevel, will be treated as if
        /// a different LogLevel is used.
        /// </remarks>
        public void UnfilteredLog( ActivityMonitorData logLine )
        {
            if( logLine == null ) throw new ArgumentNullException( "logLine" );
            ReentrantAndConcurrentCheck();
            try
            {
                DoUnfilteredLog( logLine );
            }
            finally
            {
                ReentrantAndConcurrentRelease();
            }
        }

        void DoUnfilteredLog( ActivityMonitorData data )
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            Debug.Assert( data.Level != LogLevel.None );
            Debug.Assert( !String.IsNullOrEmpty( data.Text ) );

            if( data.Exception != null )
            {
                DoOpenGroup( data.Tags, data.Level, null, data.Text, data.LogTimeUtc, data.FileName, data.LineNumber, data.Exception );
                DoCloseGroup( data.LogTimeUtc );
            }
            else
            {
                data.CombineTags( _currentTag );
                List<IActivityMonitorClient> buggyClients = null;
                foreach( var l in _output.Clients )
                {
                    try
                    {
                        l.OnUnfilteredLog( data );
                    }
                    catch( Exception exCall )
                    {
                        MonitoringError.Add( exCall, l.GetType().FullName );
                        if( buggyClients == null ) buggyClients = new List<IActivityMonitorClient>();
                        buggyClients.Add( l );
                    }
                }
                if( buggyClients != null )
                {
                    foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
                    _clientFilter = DoGetBoundClientMinimalFilter();
                    UpdateActualFilter();
                }
            }
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
        /// <param name="fileName">The source code file name from which the group is opened.</param>
        /// <param name="lineNumber">The line number in the source from which the group is opened.</param>
        /// <param name="ex">Optional exception associated to the group.</param>
        /// <returns>The <see cref="Group"/> that can be disposed to close it.</returns>
        public virtual IDisposable UnfilteredOpenGroup( CKTrait tags, LogLevel level, Func<string> getConclusionText, string text, DateTime logTimeUtc, Exception ex = null, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            if( level == LogLevel.IsFiltered ) throw new ArgumentException( "level" );
            if( logTimeUtc.Kind != DateTimeKind.Utc && level != LogLevel.None ) throw new ArgumentException( R.DateTimeMustBeUtc, "logTimeUtc" );

            ReentrantAndConcurrentCheck();
            try
            {
                return DoOpenGroup( tags, level, getConclusionText, text, logTimeUtc, fileName, lineNumber, ex );
            }
            finally
            {
                ReentrantAndConcurrentRelease();
            }
        }

        IDisposable DoOpenGroup( CKTrait tags, LogLevel level, Func<string> getConclusionText, string text, DateTime logTimeUtc, string fileName, int lineNumber, Exception ex = null )
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            Debug.Assert( logTimeUtc.Kind == DateTimeKind.Utc || level == LogLevel.None );

            int idxNext = _current != null ? _current.Index + 1 : 0;
            if( idxNext == _groups.Length )
            {
                Array.Resize( ref _groups, _groups.Length * 2 );
                for( int i = idxNext; i < _groups.Length; ++i ) _groups[i] = CreateGroup( i );
            }
            _current = _groups[idxNext];
            if( level == LogLevel.None )
            {
                _current.InitializeFilteredGroup();
            }
            else
            {
                if( tags == null || tags.IsEmpty ) tags = _currentTag;
                else tags = _currentTag.Union( tags );
                _current.Initialize( tags, level, text ?? (ex != null ? ex.Message : String.Empty), getConclusionText, logTimeUtc, fileName, lineNumber, ex );
                _currentUnfiltered = _current;
                MonoParameterSafeCall( ( client, group ) => client.OnOpenGroup( group ), _current ); 
            }
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
            if( logTimeUtc.Kind != DateTimeKind.Utc ) throw new ArgumentException( R.DateTimeMustBeUtc, "logTimeUtc" );
            ReentrantAndConcurrentCheck();
            try
            {
                DoCloseGroup( logTimeUtc, userConclusion );
            }
            finally
            {
                ReentrantAndConcurrentRelease();
            }
        }

        void DoCloseGroup( DateTime logTimeUtc, object userConclusion = null )
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            Debug.Assert( logTimeUtc.Kind == DateTimeKind.Utc );
            Group g = _current;
            if( g != null )
            {
                // Handles th filtered case first (easiest).
                if( g.GroupLevel == LogLevel.None )
                {
                    if( g.SavedMonitorFilter != _configuredFilter ) DoSetConfiguredFilter( g.SavedMonitorFilter );
                    _currentTag = g.SavedMonitorTags;
                    _current = g.Index > 0 ? _groups[g.Index - 1] : null;
                    g.GroupClosed();
                }
                else
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

                    bool hasBuggyClients = false;
                    List<IActivityMonitorClient> buggyClients = null;
                    foreach( var l in _output.Clients )
                    {
                        try
                        {
                            l.OnGroupClosing( g, ref conclusions );
                        }
                        catch( Exception exCall )
                        {
                            MonitoringError.Add( exCall, l.GetType().FullName );
                            if( buggyClients == null ) buggyClients = new List<IActivityMonitorClient>();
                            buggyClients.Add( l );
                        }
                    }
                    if( buggyClients != null )
                    {
                        foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
                        buggyClients.Clear();
                        hasBuggyClients = true;
                    }
                    if( g.SavedMonitorFilter != _configuredFilter ) DoSetConfiguredFilter( g.SavedMonitorFilter );
                    _currentTag = g.SavedMonitorTags;
                    _current = g.Index > 0 ? _groups[g.Index - 1] : null;
                    _currentUnfiltered = (Group)g.Parent;

                    var sentConclusions = conclusions != null ? conclusions.ToReadOnlyList() : CKReadOnlyListEmpty<ActivityLogGroupConclusion>.Empty;
                    foreach( var l in _output.Clients )
                    {
                        try
                        {
                            l.OnGroupClosed( g, sentConclusions );
                        }
                        catch( Exception exCall )
                        {
                            MonitoringError.Add( exCall, l.GetType().FullName );
                            if( buggyClients == null ) buggyClients = new List<IActivityMonitorClient>();
                            buggyClients.Add( l );
                        }
                    }
                    if( buggyClients != null )
                    {
                        foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
                        hasBuggyClients = true;
                    }
                    if( hasBuggyClients )
                    {
                        _clientFilter = DoGetBoundClientMinimalFilter();
                        UpdateActualFilter();
                    }
                    g.GroupClosed();
                }
            }
        }

        /// <summary>
        /// Generalizes calls to IActivityMonitorClient methods that have only one parameter.
        /// </summary>
        void MonoParameterSafeCall<T>( Action<IActivityMonitorClient, T> call, T arg )
        {
            Debug.Assert( _enteredThreadId == Thread.CurrentThread.ManagedThreadId );
            List<IActivityMonitorClient> buggyClients = null;
            foreach( var l in _output.Clients )
            {
                try
                {
                    call( l, arg );
                }
                catch( Exception exCall )
                {
                    MonitoringError.Add( exCall, l.GetType().FullName );
                    if( buggyClients == null ) buggyClients = new List<IActivityMonitorClient>();
                    buggyClients.Add( l );
                }
            }
            if( buggyClients != null )
            {
                foreach( var l in buggyClients ) _output.ForceRemoveBuggyClient( l );
                _clientFilter = DoGetBoundClientMinimalFilter();
                UpdateActualFilter();
            }
        }


        class RAndCChecker : IDisposable
        {
            readonly ActivityMonitor _m;

            public RAndCChecker( ActivityMonitor m )
            {
                _m = m;
                _m.ReentrantAndConcurrentCheck();
            }

            public void Dispose()
            {
                _m.ReentrantAndConcurrentRelease();
            }
        }

        IDisposable IActivityMonitorImpl.ReentrancyAndConcurrencyLock()
        {
            return new RAndCChecker( this );
        }

        void RentrantOnlyCheck()
        {
            if( _enteredThreadId != Thread.CurrentThread.ManagedThreadId ) throw new InvalidOperationException( R.ActivityMonitorReentrancyCallOnly );
        }

        void ReentrantAndConcurrentCheck()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int alreadyEnteredId;
            if( (alreadyEnteredId = Interlocked.CompareExchange( ref _enteredThreadId, currentThreadId, 0 )) != 0 )
            {
                if( alreadyEnteredId == currentThreadId )
                {
                    throw new InvalidOperationException( R.ActivityMonitorReentrancyError );
                }
                else
                {
                    throw new InvalidOperationException( R.ActivityMonitorConcurrentThreadAccess );
                }
            }
        }

        /// <summary>
        /// Checks only for concurrency issues. 
        /// False if a call already exists (reentrant call): when true is returned, ReentrantAndConcurrentRelease must be called.
        /// </summary>
        /// <returns>False for a reentrant call, true otherwise.</returns>
        bool ConcurrentOnlyCheck()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int alreadyEnteredId;
            if( (alreadyEnteredId = Interlocked.CompareExchange( ref _enteredThreadId, currentThreadId, 0 )) != 0 )
            {
                if( alreadyEnteredId == currentThreadId )
                {
                    return false;
                }
                else
                {
                    throw new InvalidOperationException( R.ActivityMonitorConcurrentThreadAccess );
                }
            }
            return true;
        }

        void ReentrantAndConcurrentRelease()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if( Interlocked.CompareExchange( ref _enteredThreadId, 0, currentThreadId ) != currentThreadId )
            {
                throw new CKException( R.ActivityMonitorReentrancyReleaseError, _enteredThreadId, Thread.CurrentThread.Name, currentThreadId );
            }
        }
    }
}
