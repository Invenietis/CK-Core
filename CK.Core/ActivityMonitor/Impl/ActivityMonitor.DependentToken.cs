#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitor.DependentToken.cs) is part of CiviKey. 
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
using CK.Core.Impl;
using System.Threading;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Concrete implementation of <see cref="IActivityMonitor"/>.
    /// </summary>
    public partial class ActivityMonitor
    {
        /// <summary>
        /// Describes the origin of a dependent activity: it is created by <see cref="ActivityMonitorExtension.DependentActivity">IActivityMonitor.DependentActivity</see> 
        /// (extension methods).
        /// </summary>
        [Serializable]
        public class DependentToken
        {
            readonly Guid _originatorId;
            readonly LogTimestamp _creationDate;
            readonly string _topic;

            static readonly string _format = "{0:B} at {1}";

            internal DependentToken( Guid monitorId, LogTimestamp logTime, string topic )
            {
                _originatorId = monitorId;
                _creationDate = logTime;
                _topic = topic;
            }

            /// <summary>
            /// Unique identifier of the activity that created this dependent token.
            /// </summary>
            public Guid OriginatorId
            {
                get { return _originatorId; }
            }

            /// <summary>
            /// Gets the creation date. This is the log time of the unfiltered Info log that has 
            /// been emitted in the originator monitor.
            /// </summary>
            public LogTimestamp CreationDate
            {
                get { return _creationDate; }
            }

            /// <summary>
            /// Gets the topic that must be set on the dependent activity.
            /// When null, the current <see cref="IActivityMonitor.Topic"/> of the dependent monitor is not changed.
            /// </summary>
            public string Topic
            {
                get { return _topic; }
            }

            /// <summary>
            /// Overridden to give a readable description (without Topic) that can be parsed with <see cref="TryParse"/>.
            /// </summary>
            /// <returns>A readable string.</returns>
            public override string ToString()
            {
                return String.Format( _format, _originatorId, _creationDate );
            }

            /// <summary>
            /// Creates a monitor and executes <see cref="ActivityMonitorExtension.DependentSender.StartDependentActivity">StartDependentActivity</see> on it
            /// that opens a root info group with the token information.
            /// </summary>
            /// <param name="configurator">Optionally applies any configuration on the created monitor before opening the root activity group.</param>
            /// <param name="fileName">Source file name of the emitter (automatically injected by C# compiler but can be explicitly set).</param>
            /// <param name="lineNumber">Line number in the source file (automatically injected by C# compiler but can be explicitly set).</param>
            /// <returns>An activity monitor that must be disposed when the activity ends (to close any opened groups).</returns>
            public IDisposableActivityMonitor CreateDependentMonitor( Action<IActivityMonitor> configurator = null, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
            {
                var m = new DisposableActivityMonitor();
                if( configurator != null ) configurator( m );
                Start( this, m, fileName, lineNumber );
                return m;
            }

            /// <summary>
            /// Tries to parse a launch message. 
            /// </summary>
            /// <param name="message">The message to parse.</param>
            /// <param name="launched">True if the activity has been launched or the token has only be created.</param>
            /// <param name="withTopic">True if an explicit topic has been associated to the dependent activity.</param>
            /// <param name="dependentTopic">When <paramref name="withTopic"/> is true, this contains the explicitly set topic.</param>
            /// <returns>True on success.</returns>
            public static bool TryParseLaunchOrCreateMessage( string message, out bool launched, out bool withTopic, out string dependentTopic )
            {
                if( message == null ) throw new ArgumentNullException();
                launched = false;
                withTopic = false;
                dependentTopic = null;

                if( message.Length < 10 ) return false;
                if( message.StartsWith( _prefixLaunchWithTopic ) ) 
                {
                    launched = true;
                    withTopic = true;
                    Debug.Assert( _prefixLaunchWithTopic.Length == 33 );
                    if( !ExtractTopic( message, 33, out dependentTopic ) ) return false;
                }
                else if( message.StartsWith( _prefixCreateWithTopic ) ) 
                {
                    withTopic = true;
                    Debug.Assert( _prefixCreateWithTopic.Length == 37 );
                    if( !ExtractTopic( message, 37, out dependentTopic ) ) return false;
                }
                else if( message.StartsWith( _prefixLaunch ) ) 
                {
                    launched = true;
                }
                else if( !message.StartsWith( _prefixCreate ) ) return false;
                return true;
            }

            private static bool ExtractTopic( string message, int start, out string dependentTopic )
            {
                Debug.Assert( _suffixWithoutTopic.Length == 9 );
                Debug.Assert( _suffixEmptyTopic.Length == 12 );
                Debug.Assert( _suffixWithTopic.Length == 8 );

                dependentTopic = null;

                if( message.Length < start + 8 + 1 ) return false;
                if( String.CompareOrdinal( message, start, _suffixWithTopic, 0, 8 ) == 0 )
                {
                    int idxEndQuote = message.LastIndexOf( '\'' );
                    if( idxEndQuote < start ) return false;
                    start += 8;
                    dependentTopic = message.Substring( start, idxEndQuote - start );
                    return true;
                }
                if( message.Length < start + 9 + 1 ) return false;
                if( String.CompareOrdinal( message, start, _suffixWithoutTopic, 0, 8 ) == 0 )
                {
                    return true;
                }
                if( message.Length < start + 12 + 1 ) return false;
                if( String.CompareOrdinal( message, start, _suffixEmptyTopic, 0, 8 ) == 0 )
                {
                    dependentTopic = String.Empty;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Attempts to parse the start message of a dependent activity (tagged with <see cref="ActivityMonitor.Tags.StartDependentActivity"/>).
            /// </summary>
            /// <param name="startMessage">The start message to parse.</param>
            /// <param name="id">The originator monitor identifier.</param>
            /// <param name="time">The creation time of the dependent activity.</param>
            /// <returns>True on success.</returns>
            static public bool TryParseStartMessage( string startMessage, out Guid id, out LogTimestamp time )
            {
                id = Guid.Empty;
                time = LogTimestamp.MinValue;
                int iIdBracket = -1;
                while( (iIdBracket = startMessage.IndexOf( '{', iIdBracket + 1 )) >= 0 )
                {
                    if( TryParseAt( startMessage, iIdBracket, ref id, ref time ) ) return true; 
                }
                return false; 
            }

            static bool TryParseAt( string s, int iIdBracket, ref Guid id, ref LogTimestamp time )
            {
                Debug.Assert( iIdBracket >= 0 );
                
                Debug.Assert( Guid.Empty.ToString( "B" ).Length == 38 );
                int timeIdx = iIdBracket + 38 + 4;
                if( timeIdx >= s.Length ) return false;
                if( !LogTimestamp.Match( s, ref timeIdx, out time ) ) return false;
                
                int remainder = s.Length - iIdBracket;
                if( String.CompareOrdinal( s, iIdBracket+38, " at ", 0, 4 ) != 0 || !Guid.TryParseExact( s.Substring( iIdBracket, 38 ), "B", out id ) ) return false;
                return true;
            }

            internal string FormatStartMessage()
            {
                return "Starting dependent activity issued by " + ToString() + ".";
            }

            const string _prefixLaunch = "Launching dependent activity";
            const string _prefixCreate = "Activity dependent token created";
            const string _prefixLaunchWithTopic = "Launching dependent activity with";
            const string _prefixCreateWithTopic = "Activity dependent token created with";
            const string _suffixWithoutTopic = "out topic";
            const string _suffixEmptyTopic = " empty topic";
            const string _suffixWithTopic = " topic '";

            internal static DependentToken CreateWithMonitorTopic( IActivityMonitor m, bool launchActivity, out string msg )
            {
                msg = launchActivity ? _prefixLaunch : _prefixCreate;
                DependentToken t = new DependentToken( ((IUniqueId)m).UniqueId, m.NextLogTime(), m.Topic );
                msg += '.';
                return t;
            }

            internal static DependentToken CreateWithDependentTopic( IActivityMonitor m, bool launchActivity, string dependentTopic, out string msg )
            {
                msg = launchActivity ? _prefixLaunchWithTopic : _prefixCreateWithTopic;
                if( dependentTopic == null ) msg += _suffixWithoutTopic;
                else if( String.IsNullOrWhiteSpace( dependentTopic ) )
                {
                    dependentTopic = String.Empty;
                    msg += _suffixEmptyTopic;
                }
                else msg += _suffixWithTopic + dependentTopic + '\'';
                DependentToken t = new DependentToken( ((IUniqueId)m).UniqueId, m.NextLogTime(), dependentTopic );
                msg += '.';
                return t;
            }

            static internal IDisposable Start( ActivityMonitor.DependentToken token, IActivityMonitor monitor, string fileName, int lineNumber )
            {
                string msg = token.FormatStartMessage();
                if( token.Topic != null )
                {
                    string currentTopic = token.Topic;
                    monitor.SetTopic( token.Topic, fileName, lineNumber );
                    var g = monitor.UnfilteredOpenGroup( ActivityMonitor.Tags.StartDependentActivity, LogLevel.Info, null, msg, monitor.NextLogTime(), null, fileName, lineNumber );
                    return Util.CreateDisposableAction( () => { g.Dispose(); monitor.SetTopic( currentTopic, fileName, lineNumber ); } );
                }
                return monitor.UnfilteredOpenGroup( ActivityMonitor.Tags.StartDependentActivity, LogLevel.Info, null, msg, monitor.NextLogTime(), null, fileName, lineNumber );
            }
        }
    }
}
