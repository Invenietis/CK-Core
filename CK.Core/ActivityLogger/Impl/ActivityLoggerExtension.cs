#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerExtension.cs) is part of CiviKey. 
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IActivityLogger"/> and other types from the Activity logger framework.
    /// </summary>
    public static partial class ActivityLoggerExtension
    {
        /// <summary>
        /// Gets this Group conclusions as a readeable string.
        /// </summary>
        /// <param name="this">This group conclusion. Can be null.</param>
        /// <param name="conclusionSeparator">Conclusion separator.</param>
        /// <returns>A lovely concatened string of conclusions.</returns>
        public static string ToStringGroupConclusion( this IEnumerable<ActivityLogGroupConclusion> @this, string conclusionSeparator = " - " )
        {
            if( @this == null ) return String.Empty;
            StringBuilder b = new StringBuilder();
            foreach( var e in @this )
            {
                if( b.Length > 0 ) b.Append( conclusionSeparator );
                b.Append( e.Text );
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets the path as a readable string.
        /// </summary>
        /// <param name="this">This path. Can be null.</param>
        /// <param name="elementSeparator">Between elements.</param>
        /// <param name="withoutConclusionFormat">There must be 3 placeholders {0} for the level, {1} for the text and {2} for the conclusion.</param>
        /// <param name="withConclusionFormat">There must be 2 placeholders {0} for the level and {1} for the text.</param>
        /// <param name="conclusionSeparator">Conclusion separator.</param>
        /// <param name="fatal">For Fatal errors.</param>
        /// <param name="error">For Errors.</param>
        /// <param name="warn">For Warnings.</param>
        /// <param name="info">For Infos.</param>
        /// <param name="trace">For Traces.</param>
        /// <returns>A lovely path.</returns>
        public static string ToStringPath( this IEnumerable<ActivityLoggerPathCatcher.PathElement> @this,
            string elementSeparator = "> ",
            string withoutConclusionFormat = "{0}{1} ",
            string withConclusionFormat = "{0}{1} -{{ {2} }}",
            string conclusionSeparator = " - ",
            string fatal = "[Fatal]- ",
            string error = "[Error]- ",
            string warn = "[Warning]- ",
            string info = "[Info]- ",
            string trace = "" )
        {
            if( @this == null ) return String.Empty;
            StringBuilder b = new StringBuilder();
            foreach( var e in @this )
            {
                if( b.Length > 0 ) b.Append( elementSeparator );
                string prefix = trace;
                switch( e.Level )
                {
                    case LogLevel.Fatal: prefix = fatal; break;
                    case LogLevel.Error: prefix = error; break;
                    case LogLevel.Warn: prefix = warn; break;
                    case LogLevel.Info: prefix = info; break;
                }
                if( e.GroupConclusion != null ) b.AppendFormat( withConclusionFormat, prefix, e.Text, e.GroupConclusion.ToStringGroupConclusion( conclusionSeparator ) );
                else b.AppendFormat( withoutConclusionFormat, prefix, e.Text );
            }
            return b.ToString();
        }

        /// <summary>
        /// Finds or creates a bridge to another logger.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLoggerOutput"/>.</param>
        /// <param name="logger">The logger that will receive our logs.</param>
        /// <returns>The <see cref="ActivityLoggerBridge"/> that has been created and registered or the one that already exists.</returns>
        public static ActivityLoggerBridge BridgeTo( this IActivityLoggerOutput @this, IActivityLogger logger )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            if( logger == null ) throw new ArgumentNullException( "logger" );
            var bridge = @this.RegisteredClients.OfType<ActivityLoggerBridge>().Where( b => b.TargetLogger == logger ).FirstOrDefault();
            if( bridge == null )
            {
                bridge = new ActivityLoggerBridge( logger.Output.ExternalInput );
                @this.RegisterClient( bridge );
            }
            return bridge;
        }

        /// <summary>
        /// Removes an existing <see cref="ActivityLoggerBridge"/> to another logger.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLoggerOutput"/>.</param>
        /// <param name="logger">The logger that will no more receive our logs.</param>
        /// <returns>The unregistered <see cref="ActivityLoggerBridge"/> if found, null otherwise.</returns>
        public static ActivityLoggerBridge UnbridgeTo( this IActivityLoggerOutput @this, IActivityLogger logger )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            if( logger == null ) throw new ArgumentNullException( "logger" );
            var bridge = @this.RegisteredClients.OfType<ActivityLoggerBridge>().Where( b => b.TargetLogger == logger ).FirstOrDefault();
            if( bridge != null ) @this.UnregisterClient( bridge );
            return bridge;
        }

        /// <summary>
        /// Closes the current Group. Optional parameter is polymorphic. It can be a string, a <see cref="ActivityLogGroupConclusion"/>, 
        /// a <see cref="List{T}"/> or an <see cref="IEnumerable{T}"/> of ActivityLogGroupConclusion, or any object with an overriden <see cref="Object.ToString"/> method. 
        /// See remarks (especially for List&lt;ActivityLogGroupConclusion&gt;).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/>.</param>
        /// <param name="userConclusion">Optional string, ActivityLogGroupConclusion object, enumerable of ActivityLogGroupConclusion or object to conclude the group. See remarks.</param>
        /// <remarks>
        /// An untyped object is used here to easily and efficiently accomodate both string and already existing ActivityLogGroupConclusion.
        /// When a List&lt;ActivityLogGroupConclusion&gt; is used, it will be direclty used to collect conclusion objects (new conclusions will be added to it). This is an optimization.
        /// </remarks>
        public static void CloseGroup( this IActivityLogger @this, object userConclusion = null )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            @this.CloseGroup( DateTime.UtcNow, userConclusion );
        }

        #region Catch & CatchCounter

        /// <summary>
        /// Enables simple "using" syntax to easily catch any <see cref="LogLevel"/> (or above) entries (defaults to <see cref="LogLevel.Error"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/>.</param>
        /// <param name="errorHandler">An action that accepts a list of fatal or error <see cref="ActivityLoggerSimpleCollector.Entry">entries</see>.</param>
        /// <param name="level">Defines the level of the entries caught (by default fatal or error entries).</param>
        /// <returns>A <see cref="IDisposable"/> object used to manage the scope of this handler.</returns>
        public static IDisposable Catch( this IActivityLogger @this, Action<IReadOnlyList<ActivityLoggerSimpleCollector.Entry>> errorHandler, LogLevelFilter level = LogLevelFilter.Error )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            if( errorHandler == null ) throw new ArgumentNullException( "errorHandler" );
            ActivityLoggerSimpleCollector errorTracker = new ActivityLoggerSimpleCollector() { LevelFilter = level };
            @this.Output.RegisterClient( errorTracker );
            return Util.CreateDisposableAction( () =>
            {
                @this.Output.UnregisterClient( errorTracker );
                if( errorTracker.Entries.Count > 0 ) errorHandler( errorTracker.Entries );
            } );
        }

        /// <summary>
        /// Enables simple "using" syntax to easily detect <see cref="LogLevel.Fatal"/>, <see cref="LogLevel.Error"/> or <see cref="LogLevel.Warn"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/>.</param>
        /// <param name="fatalErrorWarnCount">An action that accepts three counts for fatals, errors and warnings.</param>
        /// <returns>A <see cref="IDisposable"/> object used to manage the scope of this handler.</returns>
        public static IDisposable CatchCounter( this IActivityLogger @this, Action<int, int, int> fatalErrorWarnCount )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            if( fatalErrorWarnCount == null ) throw new ArgumentNullException( "fatalErrorWarnCount" );
            ActivityLoggerErrorCounter errorCounter = new ActivityLoggerErrorCounter();
            Debug.Assert( errorCounter.GenerateConclusion == false, "It is false by default." );
            @this.Output.RegisterClient( errorCounter );
            return Util.CreateDisposableAction( () =>
            {
                @this.Output.UnregisterClient( errorCounter );
                if( errorCounter.Current.HasWarnOrError ) fatalErrorWarnCount( errorCounter.Current.FatalCount, errorCounter.Current.ErrorCount, errorCounter.Current.WarnCount );
            } );
        }

        /// <summary>
        /// Enables simple "using" syntax to easily detect <see cref="LogLevel.Fatal"/> and <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/>.</param>
        /// <param name="fatalErrorCount">An action that accepts two counts for fatals and errors.</param>
        /// <returns>A <see cref="IDisposable"/> object used to manage the scope of this handler.</returns>
        public static IDisposable CatchCounter( this IActivityLogger @this, Action<int, int> fatalErrorCount )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            if( fatalErrorCount == null ) throw new ArgumentNullException( "fatalErrorCount" );
            ActivityLoggerErrorCounter errorCounter = new ActivityLoggerErrorCounter() { GenerateConclusion = false };
            @this.Output.RegisterClient( errorCounter );
            return Util.CreateDisposableAction( () =>
            {
                @this.Output.UnregisterClient( errorCounter );
                if( errorCounter.Current.HasError ) fatalErrorCount( errorCounter.Current.FatalCount, errorCounter.Current.ErrorCount );
            } );
        }

        /// <summary>
        /// Enables simple "using" syntax to easily detect <see cref="LogLevel.Fatal"/> or <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/>.</param>
        /// <param name="fatalOrErrorCount">An action that accepts one count that sums fatals and errors.</param>
        /// <returns>A <see cref="IDisposable"/> object used to manage the scope of this handler.</returns>
        public static IDisposable CatchCounter( this IActivityLogger @this, Action<int> fatalOrErrorCount )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            if( fatalOrErrorCount == null ) throw new ArgumentNullException( "fatalErrorCount" );
            ActivityLoggerErrorCounter errorCounter = new ActivityLoggerErrorCounter() { GenerateConclusion = false };
            @this.Output.RegisterClient( errorCounter );
            return Util.CreateDisposableAction( () =>
            {
                @this.Output.UnregisterClient( errorCounter );
                if( errorCounter.Current.HasError ) fatalOrErrorCount( errorCounter.Current.FatalCount + errorCounter.Current.ErrorCount );
            } );
        }
        
        #endregion


        #region IActivityLogger.Filter( level )

        class LogFilterSentinel : IDisposable
        {
            IActivityLogger _logger;
            LogLevelFilter _prevLevel;

            public LogFilterSentinel( IActivityLogger l, LogLevelFilter filterLevel )
            {
                _prevLevel = l.Filter;
                _logger = l;
                l.Filter = filterLevel;
            }

            public void Dispose()
            {
                _logger.Filter = _prevLevel;
            }

        }

        /// <summary>
        /// Sets a filter level on this <see cref="IActivityLogger"/>. The current <see cref="IActivityLogger.Filter"/> will be automatically 
        /// restored when the returned <see cref="IDisposable"/> will be disposed.
        /// This may not be useful since when a Group is closed, the IActivityLogger.Filter is automatically restored to its original value 
        /// (captured when the Group was opened).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="filterLevel">The new filter level.</param>
        /// <returns>A <see cref="IDisposable"/> object that will restore the current level.</returns>
        public static IDisposable Filter( this IActivityLogger @this, LogLevelFilter filterLevel )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            return new LogFilterSentinel( @this, filterLevel );
        }

        #endregion IActivityLogger.Filter( level )


        #region IActivityLogger.Tags( Tags, SetOperation )
        class TagsSentinel : IDisposable
        {
            readonly IActivityLogger _logger;
            readonly CKTrait _previous;

            public TagsSentinel( IActivityLogger l, CKTrait t )
            {
                _previous = l.AutoTags;
                _logger = l;
                l.AutoTags = t;
            }

            public void Dispose()
            {
                _logger.AutoTags = _previous;
            }

        }

        /// <summary>
        /// Alter tags of this <see cref="IActivityLogger"/>. Current <see cref="IActivityLogger.AutoTags"/> will be automatically 
        /// restored when the returned <see cref="IDisposable"/> will be disposed.
        /// This may not be useful since when a Group is closed, the IActivityLogger.Tags is automatically restored to its original value 
        /// (captured when the Group was opened).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to combine with the current one.</param>
        /// <param name="operation">Defines the way the new <paramref name="tags"/> must be combined with current ones.</param>
        /// <returns>A <see cref="IDisposable"/> object that will restore the current tag when disposed.</returns>
        public static IDisposable AutoTags( this IActivityLogger @this, CKTrait tags, SetOperation operation = SetOperation.Union )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            return new TagsSentinel( @this, @this.AutoTags.Apply( tags, operation ) );
        }
        
        #endregion


        #region Registrar

        /// <summary>
        /// Registers multiple <see cref="IActivityLoggerClient"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLoggerOutput"/> object.</param>
        /// <param name="clients">Multiple clients to register.</param>
        /// <returns>This registrar to enable fluent syntax.</returns>
        public static IActivityLoggerOutput Register( this IActivityLoggerOutput @this, IEnumerable<IActivityLoggerClient> clients )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            foreach( var c in clients ) @this.RegisterClient( c );
            return @this;
        }

        /// <summary>
        /// Registers multiple <see cref="IActivityLoggerClient"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLoggerOutput"/> object.</param>
        /// <param name="clients">Multiple clients to register.</param>
        /// <returns>This registrar to enable fluent syntax.</returns>
        public static IActivityLoggerOutput Register( this IActivityLoggerOutput @this, params IActivityLoggerClient[] clients )
        {
            if( @this == null ) throw new NullReferenceException( "@this" );
            return Register( @this, (IEnumerable<IActivityLoggerClient>)clients );
        }

        #endregion

    }
}
