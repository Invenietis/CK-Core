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
* Copyright © 2007-2015, 
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
using CK.Text;

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
            readonly DateTimeStamp _creationDate;
            readonly string _topic;
            [NonSerialized]
            string _delayedLaunchMessage;

            internal DependentToken( Guid monitorId, DateTimeStamp logTime, string topic )
            {
                _originatorId = monitorId;
                _creationDate = logTime;
                _topic = topic;
            }

            /// <summary>
            /// Unique identifier of the activity that created this dependent token.
            /// </summary>
            public Guid OriginatorId => _originatorId; 

            /// <summary>
            /// Gets the creation date. This is the log time of the unfiltered Info log that has 
            /// been emitted in the originator monitor.
            /// </summary>
            public DateTimeStamp CreationDate => _creationDate; 

            /// <summary>
            /// Gets the topic that must be set on the dependent activity.
            /// When null, the current <see cref="IActivityMonitor.Topic"/> of the dependent monitor is not changed.
            /// </summary>
            public string Topic => _topic;

            /// <summary>
            /// Overridden to give a readable description of this token that can be <see cref="Parse"/>d (or <see cref="TryParse"/>) back:
            /// The format is "{<see cref="OriginatorId"/>} at <see cref="CreationDate"/> (with topic '...'|without topic).".
            /// </summary>
            /// <returns>A readable string.</returns>
            public override string ToString()
            {
                string s = string.Format( "{0:B} at {1} with", _originatorId, _creationDate );
                return AppendTopic( s, _topic );
            }

            /// <summary>
            /// Tries to parse a <see cref="DependentToken.ToString()"/> string.
            /// </summary>
            /// <param name="s">The string to parse.</param>
            /// <param name="t">The resulting dependent token.</param>
            /// <returns>True on success, false otherwise.</returns>
            static public bool TryParse( string s, out DependentToken t )
            {
                t = null;
                StringMatcher m = new StringMatcher( s );
                Guid id;
                DateTimeStamp time;
                if( MatchOriginatorAndTime( m, out id, out time ) && m.TryMatchText( " with" ) )
                {
                    string topic;
                    if( ExtractTopic( s, m.StartIndex, out topic ) )
                    {
                        t = new DependentToken( id, time, topic );
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Parses a <see cref="DependentToken.ToString()"/> string or throws a <see cref="FormatException"/>
            /// on error.
            /// </summary>
            /// <param name="s">The string to parse.</param>
            /// <returns>The resulting dependent token.</returns>
            static public DependentToken Parse( string s )
            {
                DependentToken t;
                if( !TryParse( s, out t ) ) throw new FormatException( "Invalid Dependent token string." );
                return t;
            }

            /// <summary>
            /// Tries to parse a launch or create message. 
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

            /// <summary>
            /// Captures the log message when created with a delayed launch so that DependentSender.Launch( token ) can log it.
            /// </summary>
            internal string DelayedLaunchMessage
            {
                get { return _delayedLaunchMessage; }
                set { _delayedLaunchMessage = value; }
            }

            private static bool ExtractTopic( string message, int start, out string dependentTopic )
            {
                Debug.Assert( _suffixWithoutTopic.Length == 9 );
                Debug.Assert( _suffixWithTopic.Length == 8 );

                dependentTopic = null;

                if( message.Length < start + 8 + 1 ) return false;
                if( string.CompareOrdinal( message, start, _suffixWithTopic, 0, 8 ) == 0 )
                {
                    int idxEndQuote = message.LastIndexOf( '\'' );
                    if( idxEndQuote < start ) return false;
                    start += 8;
                    dependentTopic = message.Substring( start, idxEndQuote - start );
                    return true;
                }
                if( message.Length < start + 9 + 1 ) return false;
                if( string.CompareOrdinal( message, start, _suffixWithoutTopic, 0, 8 ) == 0 )
                {
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
            static public bool TryParseStartMessage( string startMessage, out Guid id, out DateTimeStamp time )
            {
                int idx = startMessage.IndexOf( '{' );
                if( idx <= 0 )
                {
                    id = Guid.Empty;
                    time = DateTimeStamp.MinValue;
                    return false;
                }
                return MatchOriginatorAndTime( new StringMatcher( startMessage, idx ), out id, out time );
            }

            static bool MatchOriginatorAndTime( StringMatcher m, out Guid id, out DateTimeStamp time )
            {
                time = DateTimeStamp.MinValue;
                if( !m.TryMatchGuid( out id ) ) return false;
                if( !m.TryMatchText( " at " ) ) return false;
                return m.MatchDateTimeStamp( out time );
            }

            internal string FormatStartMessage()
            {
                return string.Format( "Starting dependent activity issued by {0:B} at {1}.", _originatorId, _creationDate );
            }

            const string _prefixLaunch = "Launching dependent activity";
            const string _prefixCreate = "Activity dependent token created";
            const string _prefixLaunchWithTopic = "Launching dependent activity with";
            const string _prefixCreateWithTopic = "Activity dependent token created with";
            const string _suffixWithoutTopic = "out topic";
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
                msg = AppendTopic( launchActivity ? _prefixLaunchWithTopic : _prefixCreateWithTopic, dependentTopic );
                return new DependentToken( ((IUniqueId)m).UniqueId, m.NextLogTime(), dependentTopic );
            }

            static string AppendTopic( string msg, string dependentTopic )
            {
                Debug.Assert( msg.EndsWith( " with" ) );
                if( dependentTopic == null ) msg += _suffixWithoutTopic;
                else msg += _suffixWithTopic + dependentTopic + '\'';
                return msg + '.';
            }

            static internal IDisposable Start( ActivityMonitor.DependentToken token, IActivityMonitor monitor, string fileName, int lineNumber )
            {
                string msg = token.FormatStartMessage();
                if( token.Topic != null )
                {
                    string currentTopic = token.Topic;
                    monitor.SetTopic( token.Topic, fileName, lineNumber );
                    var g = monitor.UnfilteredOpenGroup( Tags.StartDependentActivity, LogLevel.Info, null, msg, monitor.NextLogTime(), null, fileName, lineNumber );
                    return Util.CreateDisposableAction( () => { g.Dispose(); monitor.SetTopic( currentTopic, fileName, lineNumber ); } );
                }
                return monitor.UnfilteredOpenGroup( Tags.StartDependentActivity, LogLevel.Info, null, msg, monitor.NextLogTime(), null, fileName, lineNumber );
            }
        }
    }
}
