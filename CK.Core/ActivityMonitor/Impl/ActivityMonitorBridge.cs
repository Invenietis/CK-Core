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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="IActivityMonitorClient"/> that relays what happens in a monitor to another monitor.
    /// Automatically supports logs crossing Application Domains. See <see cref="ActivityMonitorBridgeTarget"/>.
    /// </summary>
    public class ActivityMonitorBridge : IActivityMonitorBoundClient, IActivityMonitorBridgeCallback
    {
        readonly ActivityMonitorBridgeTarget _bridge;
        // When the bridge is in the same domain, we relay 
        // directly to the final monitor.
        readonly IActivityMonitor _finalMonitor;
        IActivityMonitorImpl _source;
        // The callback is this object if we are in the same AppDomain
        // otherwise it is a CrossAppDomainCallback.
        readonly CrossAppDomainCallback _crossADCallback;
        // Missing a BitList in the framework...
        readonly List<bool> _openedGroups;
        LogLevelFilter _targetFilter;
        readonly bool _applyTargetFilterToGroup;

        class CrossAppDomainCallback : MarshalByRefObject, IActivityMonitorBridgeCallback, ISponsor
        {
            readonly ActivityMonitorBridgeTarget _bridge;
            readonly IActivityMonitorBridgeCallback _local;

            internal bool InUse;

            public CrossAppDomainCallback( IActivityMonitorBridgeCallback local, ActivityMonitorBridgeTarget bridge )
            {
                _bridge = bridge;
                _local = local;
            }

            void IActivityMonitorBridgeCallback.OnTargetFilterChanged()
            {
                _local.OnTargetFilterChanged();
            }

            public override object InitializeLifetimeService()
            {
                ILease lease = (ILease)base.InitializeLifetimeService();
                if( lease.CurrentState == LeaseState.Initial )
                {
                    lease.Register( this );
                }
                return lease;
            }

            TimeSpan ISponsor.Renewal( ILease lease )
            {
                return InUse ? TimeSpan.FromMinutes( 2 ) : TimeSpan.Zero;
            }
        }

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
        /// <param name="applyTargetHonorMonitorFilterToOpenGroup">
        /// True to avoid opening group with level below the target <see cref="IActivityMonitor.Filter"/> (when <see cref="ActivityMonitorBridgeTarget.HonorMonitorFilter"/> is true).
        /// This is an optimization that can be used to send less data to the target monitor.
        /// </param>
        public ActivityMonitorBridge( ActivityMonitorBridgeTarget bridge, bool applyTargetHonorMonitorFilterToOpenGroup = false )
        {
            if( bridge == null ) throw new ArgumentNullException( "bridge" );
            _bridge = bridge;
            _applyTargetFilterToGroup = applyTargetHonorMonitorFilterToOpenGroup;
            if( System.Runtime.Remoting.RemotingServices.IsTransparentProxy( bridge ) )
            {
                _crossADCallback = new CrossAppDomainCallback( this, bridge );
            }
            else
            {
                _finalMonitor = _bridge.TargetMonitor;
            }
            _openedGroups = new List<bool>();
        }

        /// <summary>
        /// Gets the target monitor if it is in the same Application Domain. 
        /// Null otherwise: use <see cref="TargetBridge"/> to always have a reference to the target.
        /// </summary>
        public IActivityMonitor TargetMonitor { get { return _finalMonitor; } }

        /// <summary>
        /// Gets whether the target monitor is in the same application domain or not.
        /// </summary>
        public bool IsCrossAppDomain { get { return _finalMonitor != null; } }

        /// <summary>
        /// Gets the target bridge. This is never null, even when this bridge is not in the same application domain as the <see cref="TargetMonitor"/>.
        /// </summary>
        public ActivityMonitorBridgeTarget TargetBridge { get { return _bridge; } }

        void IActivityMonitorBridgeCallback.OnTargetFilterChanged()
        {
            Thread.MemoryBarrier();
            var s = _source;
            _targetFilter = LogLevelFilter.Invalid;
            if( s != null ) s.SetClientMinimalFilterDirty();
        }

        /// <summary>
        /// forceBuggyRemove is not used here since this client is not lockable.
        /// </summary>
        void IActivityMonitorBoundClient.SetMonitor( Impl.IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( source != null && _source != null ) throw ActivityMonitorClient.CreateMultipleRegisterOnBoundClientException( this );
            if( _source != null )
            {
                if( _crossADCallback != null )
                {
                    _bridge.RemoveCallback( _crossADCallback, true );
                    _crossADCallback.InUse = false;
                }
                else _bridge.RemoveCallback( this, false );
                // Unregistering.
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
            else
            {
                if( _crossADCallback != null )
                {
                    _crossADCallback.InUse = true;
                    _bridge.AddCallback( _crossADCallback, true );
                }
                else _bridge.AddCallback( this, false );
                _targetFilter = IsCrossAppDomain ? _bridge.TargetFinalFilterCrossAppDomain : _bridge.TargetFinalFilter;
            }
            _source = source;
            Thread.MemoryBarrier();
        }

        LogLevelFilter IActivityMonitorBoundClient.MinimalFilter { get { return GetMinimalFilter(); } }        

        /// <summary>
        /// This is necessarily called in the context of the activity: we can call the bridge that can call 
        /// the Monitor's ActualFilter that will be resynchronized if needed.
        /// </summary>
        LogLevelFilter GetMinimalFilter() 
        {
            Thread.MemoryBarrier();
            var f = _targetFilter;
            if( f == LogLevelFilter.Invalid )
            {
                do
                {
                    f = IsCrossAppDomain ? _bridge.TargetFinalFilterCrossAppDomain : _bridge.TargetFinalFilter;
                    _targetFilter = f;
                    Thread.MemoryBarrier();
                }
                while( _targetFilter == LogLevelFilter.Invalid );
            }
            return f; 
        }
     
        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            if( _finalMonitor != null ) _finalMonitor.UnfilteredLog( tags, level, text, logTimeUtc );
            else
            {
                _bridge.UnfilteredLog( tags.ToString(), level, text, logTimeUtc );
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

            if( !_applyTargetFilterToGroup || (int)GetMinimalFilter() <= (int)group.GroupLevel )
            {
                if( _finalMonitor != null )
                    _finalMonitor.OpenGroup( group.GroupTags, group.GroupLevel, null, group.GroupText, group.LogTimeUtc, group.Exception );
                else
                {
                    _bridge.OpenGroup( group.GroupTags.ToString(), group.GroupLevel, group.EnsureExceptionData(), group.GroupText, group.LogTimeUtc );
                }
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
