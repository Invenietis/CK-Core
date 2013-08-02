#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitorClientBridge.cs) is part of CiviKey. 
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
    /// A <see cref="IActivityMonitorClient"/> that relays what happens in a monitor to another monitor.
    /// Automatically supports logs crossing Application Domains. See <see cref="ActivityMonitorBridgeTarget"/>.
    /// </summary>
    public class ActivityMonitorBridge : IActivityMonitorBoundClient
    {
        readonly ActivityMonitorBridgeTarget _bridge;
        // When the bridge is in the same domain, we relay 
        // directly to the final monitor.
        readonly IActivityMonitor _finalMonitor;
        IActivityMonitor _source;
        // Missing a BitList in the framework...
        readonly List<bool> _openedGroups;

        /// <summary>
        /// Tags group conclusions emitted because of premature (unbalanced) removing of a bridge from a source monitor.
        /// </summary>
        public static readonly CKTrait TagBridgePrematureClose = ActivityMonitor.RegisteredTags.FindOrCreate( "c:ClosedByBridgeRemoved" );

        /// <summary>
        /// Initialize a new <see cref="ActivityMonitorBridge"/> bound to an existing <see cref="ActivityMonitorBridgeTarget"/>
        /// that can live in another AppDomain.
        /// This Client should be registered in the <see cref="IActivityMonitor.Output"/> of a local monitor.
        /// </summary>
        /// <param name="bridge">The bridge to another AppDomain.</param>
        public ActivityMonitorBridge( ActivityMonitorBridgeTarget bridge )
        {
            if( bridge == null ) throw new ArgumentNullException( "bridge" );
            _bridge = bridge;
            if( !System.Runtime.Remoting.RemotingServices.IsTransparentProxy( bridge ) ) _finalMonitor = _bridge.FinalMonitor;
            _openedGroups = new List<bool>();
        }

        /// <summary>
        /// Gets the target monitor if it is in the same Application Domain. 
        /// Null otherwise.
        /// </summary>
        public IActivityMonitor TargetMonitor { get { return _finalMonitor; } }

        /// <summary>
        /// forceBuggyRemove is not used here since this client is not lockable.
        /// </summary>
        LogLevelFilter IActivityMonitorBoundClient.SetMonitor( Impl.IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( source != null && _source != null ) throw ActivityMonitorClient.NewMultipleRegisterOnBoundClientException( this );
            if( _source != null )
            {
                for( int i = 0; i < _openedGroups.Count; ++i )
                {
                    if( _openedGroups[i] )
                    {
                        if( _finalMonitor != null ) _finalMonitor.CloseGroup( new ActivityLogGroupConclusion( R.ClosedByBridgeRemoved, TagBridgePrematureClose ) );
                        else _bridge.CloseGroup( new string[] { TagBridgePrematureClose.ToString(), R.ClosedByBridgeRemoved } );
                    }
                }
                _openedGroups.Clear();
            }
            _source = source;
            return LogLevelFilter.None;
        }

        void IActivityMonitorClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            // Does nothing.
            // We do not change the filter of the receiving monitor: it has its own filter that 
            // must be honored or not (see honorFinalFilter parameter of the bridge).
        }

        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            if( _bridge.TargetFilter <= (int)level )
            {
                if( _finalMonitor != null ) _finalMonitor.UnfilteredLog( tags, level, text, logTimeUtc );
                else _bridge.UnfilteredLog( tags.ToString(), level, text, logTimeUtc );
            }
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            Debug.Assert( group.Depth > 0, "Depth is 1-based." );
            // Make sure the index is available.
            // This handles the case where this ClientBridge has been added to the Monitor.Output
            // after the opening of Groups: we must not trigger a Close on the final monitor for them.
            int idx = group.Depth;
            while( idx > _openedGroups.Count ) _openedGroups.Add( false );
            
            if( _bridge.TargetFilter <= (int)group.GroupLevel )
            {
                if( _finalMonitor != null )
                    _finalMonitor.OpenGroup( group.GroupTags, group.GroupLevel, group.Exception, group.GroupText );
                else _bridge.OpenGroup( group.GroupTags.ToString(), group.GroupLevel, group.Exception, group.GroupText, group.LogTimeUtc );
                _openedGroups[idx - 1] = true;
            }
            else _openedGroups[idx - 1] = false;
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
            // Does nothing.
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _openedGroups[group.Depth - 1] )
            {
                if( _finalMonitor != null ) _finalMonitor.CloseGroup( conclusions );
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
