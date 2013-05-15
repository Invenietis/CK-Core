#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerClientBridge.cs) is part of CiviKey. 
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
* Copyright © 2007-2013, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="IActivityLoggerClient"/> that relays what happens in a logger to another logger.
    /// Automatically supports logs crossing Application Domains. See <see cref="ActivityLoggerBridgeTarget"/>.
    /// </summary>
    public class ActivityLoggerBridge : IActivityLoggerBoundClient
    {
        readonly ActivityLoggerBridgeTarget _bridge;
        // When the bridge is in the same domain, we relay 
        // directly to the final logger.
        readonly IActivityLogger _finalLogger;
        IActivityLogger _source;
        // Missing a BitList in the framework...
        readonly List<bool> _openedGroups;

        /// <summary>
        /// Tags group conclusions emitted because of premature (unbalanced) removing of a bridge from a source logger.
        /// </summary>
        public static readonly CKTrait TagBridgePrematureClose = ActivityLogger.RegisteredTags.FindOrCreate( "c:ClosedByBridgeRemoved" );

        /// <summary>
        /// Initialize a new <see cref="ActivityLoggerBridge"/> bound to an existing <see cref="ActivityLoggerBridgeTarget"/>
        /// that can live in another AppDomain.
        /// This Client should be registered in the <see cref="IActivityLogger.Output"/> of a local logger.
        /// </summary>
        /// <param name="bridge">The bridge to another AppDomain.</param>
        public ActivityLoggerBridge( ActivityLoggerBridgeTarget bridge )
        {
            if( bridge == null ) throw new ArgumentNullException( "bridge" );
            _bridge = bridge;
            if( !System.Runtime.Remoting.RemotingServices.IsTransparentProxy( bridge ) ) _finalLogger = _bridge.FinalLogger;
            _openedGroups = new List<bool>();
        }

        /// <summary>
        /// Gets the target logger if it is in the same Application Domain. 
        /// Null otherwise.
        /// </summary>
        public IActivityLogger TargetLogger { get { return _finalLogger; } }

        /// <summary>
        /// forceBuggyRemove is not used here since this client is not lockable.
        /// </summary>
        void IActivityLoggerBoundClient.SetLogger( IActivityLogger source, bool forceBuggyRemove )
        {
            if( source != null && _source != null ) throw new InvalidOperationException( String.Format( R.ActivityLoggerBoundClientMultipleRegister, GetType().FullName ) );
            if( _source != null )
            {
                for( int i = 0; i < _openedGroups.Count; ++i )
                {
                    if( _openedGroups[i] )
                    {
                        if( _finalLogger != null ) _finalLogger.CloseGroup( new ActivityLogGroupConclusion( R.ClosedByBridgeRemoved, TagBridgePrematureClose ) );
                        else _bridge.CloseGroup( new string[] { TagBridgePrematureClose.ToString(), R.ClosedByBridgeRemoved } );
                    }
                }
                _openedGroups.Clear();
            }
            _source = source;
        }

        void IActivityLoggerClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            // Does nothing.
            // We do not change the filter of the receiving logger: it has its own filter that 
            // must be honored or not (see honorFinalFilter parameter of the bridge).
        }

        void IActivityLoggerClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            if( _bridge.TargetFilter <= (int)level )
            {
                if( _finalLogger != null ) _finalLogger.UnfilteredLog( tags, level, text, logTimeUtc );
                else _bridge.UnfilteredLog( tags.ToString(), level, text, logTimeUtc );
            }
        }

        void IActivityLoggerClient.OnOpenGroup( IActivityLogGroup group )
        {
            Debug.Assert( group.Depth > 0, "Depth is 1-based." );
            // Make sure the index is available.
            // This handles the case where this ClientBridge has been added to the Logger.Output
            // after the opening of Groups: we must not trigger a Close on the final logger for them.
            int idx = group.Depth;
            while( idx > _openedGroups.Count ) _openedGroups.Add( false );
            
            if( _bridge.TargetFilter <= (int)group.GroupLevel )
            {
                if( _finalLogger != null )
                    _finalLogger.OpenGroup( group.GroupTags, group.GroupLevel, group.Exception, group.GroupText );
                else _bridge.OpenGroup( group.GroupTags.ToString(), group.GroupLevel, group.Exception, group.GroupText, group.LogTimeUtc );
                _openedGroups[idx - 1] = true;
            }
            else _openedGroups[idx - 1] = false;
        }

        void IActivityLoggerClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
            // Does nothing.
        }

        void IActivityLoggerClient.OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _openedGroups[group.Depth - 1] )
            {
                if( _finalLogger != null ) _finalLogger.CloseGroup( conclusions );
                else
                {
                    string[] taggedConclusions = null;
                    if( conclusions.Count > 0 )
                    {
                        taggedConclusions = new string[conclusions.Count * 2];
                        int i = 0;
                        foreach( var c in conclusions )
                        {
                            taggedConclusions[i++] = c.Tag.ToString();
                            taggedConclusions[i++] = c.Text;
                        }
                    }
                    _bridge.CloseGroup( taggedConclusions );
                }
            }
        }

    }
}
