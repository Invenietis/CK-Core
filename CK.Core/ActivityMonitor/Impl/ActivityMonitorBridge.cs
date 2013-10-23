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
        readonly ActivityMonitorBridgeTarget _bridgeTarget;
        // When the bridge is in the same domain, we relay 
        // directly to the target monitor.
        readonly IActivityMonitor _targetMonitor;
        IActivityMonitorImpl _source;
        // The callback is this object if we are in the same AppDomain
        // otherwise it is a CrossAppDomainCallback.
        readonly CrossAppDomainCallback _crossADCallback;
        // I'm missing a BitList in the framework...
        readonly List<bool> _openedGroups;
        LogFilter _targetActualFilter;
        readonly bool _pullTargetTopicAndAutoTagsFromTarget;
        readonly bool _pushTopicAndAutoTagsToTarget;
        readonly bool _applyTargetFilterToUnfilteredLogs;

        class CrossAppDomainCallback : MarshalByRefObject, IActivityMonitorBridgeCallback, ISponsor
        {
            readonly ActivityMonitorBridgeTarget _bridge;
            readonly IActivityMonitorBridgeCallback _local;

            internal bool InUse;

            public CrossAppDomainCallback( ActivityMonitorBridge local, ActivityMonitorBridgeTarget bridge )
            {
                _bridge = bridge;
                _local = local;
            }

            bool IActivityMonitorBridgeCallback.IsCrossAppDomain 
            { 
                get { return true; } 
            }

            bool IActivityMonitorBridgeCallback.PullTopicAndAutoTagsFromTarget 
            {
                get { return _local.PullTopicAndAutoTagsFromTarget; } 
            }

            void IActivityMonitorBridgeCallback.OnTargetActualFilterChanged()
            {
                _local.OnTargetActualFilterChanged();
            }

            void IActivityMonitorBridgeCallback.OnTargetAutoTagsChanged( string marshalledNewTags )
            {
                _local.OnTargetAutoTagsChanged( ActivityMonitor.RegisteredTags.FindOrCreate( marshalledNewTags ) );
            }

            void IActivityMonitorBridgeCallback.OnTargetAutoTagsChanged( CKTrait newTags )
            {
                Debug.Fail( "Never called." );
            }

            void IActivityMonitorBridgeCallback.OnTargetTopicChanged( string newTopic, string fileName, int lineNumber )
            {
                _local.OnTargetTopicChanged( newTopic, fileName, lineNumber );
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
        /// <param name="pullTargetTopicAndAutoTagsFromTarget">
        /// When true, the <see cref="IActivityMonitor.Topic"/> and <see cref="IActivityMonitor.AutoTags"/> are automaticaly updated whenever they change on the target monitor.
        /// </param>
        /// <param name="pushTopicAndAutoTagsToTarget">
        /// When true, any change to <see cref="IActivityMonitor.Topic"/> or <see cref="IActivityMonitor.AutoTags"/> are applied to the target monitor.
        /// </param>
        /// <param name="applyTargetFilterToUnfilteredLogs">
        /// True to avoid sending logs with level below the target <see cref="IActivityMonitor.MinimalFilter"/> (when <see cref="ActivityMonitorBridgeTarget.HonorMonitorFilter"/> is true
        /// and it is an unfiltered line or group log).
        /// This is an optimization that can be used to send less data to the target monitor but breaks the UnfilteredLog/UnfilteredOpenGroup contract.
        /// </param>
        public ActivityMonitorBridge( ActivityMonitorBridgeTarget bridge, bool pullTargetTopicAndAutoTagsFromTarget, bool pushTopicAndAutoTagsToTarget, bool applyTargetFilterToUnfilteredLogs = false )
        {
            if( bridge == null ) throw new ArgumentNullException( "bridge" );
            _bridgeTarget = bridge;
            _pullTargetTopicAndAutoTagsFromTarget = pullTargetTopicAndAutoTagsFromTarget;
            _pushTopicAndAutoTagsToTarget = pushTopicAndAutoTagsToTarget;
            _applyTargetFilterToUnfilteredLogs = applyTargetFilterToUnfilteredLogs;
            if( System.Runtime.Remoting.RemotingServices.IsTransparentProxy( bridge ) )
            {
                _crossADCallback = new CrossAppDomainCallback( this, bridge );
            }
            else
            {
                _targetMonitor = _bridgeTarget.TargetMonitor;
            }
            _openedGroups = new List<bool>();
        }

        /// <summary>
        /// Gets the target monitor if it is in the same Application Domain. 
        /// Null otherwise: use <see cref="BridgeTarget"/> to always have a reference to the target.
        /// </summary>
        public IActivityMonitor TargetMonitor { get { return _targetMonitor; } }

        /// <summary>
        /// Gets whether the target monitor is in the same application domain or not.
        /// </summary>
        public bool IsCrossAppDomain { get { return _targetMonitor == null; } }

        /// <summary>
        /// Gets the target bridge. This is never null, even when this bridge is not in the same application domain as the <see cref="TargetMonitor"/>.
        /// </summary>
        public ActivityMonitorBridgeTarget BridgeTarget { get { return _bridgeTarget; } }

        /// <summary>
        /// Gets whether this bridge updates the Topic and AutoTags of its monitor whenever they change on the target monitor.
        /// </summary>
        public bool PullTopicAndAutoTagsFromTarget
        {
            get { return _pullTargetTopicAndAutoTagsFromTarget; }
        }

        void IActivityMonitorBridgeCallback.OnTargetActualFilterChanged()
        {
            Thread.MemoryBarrier();
            var s = _source;
            _targetActualFilter = LogFilter.Invalid;
            Thread.MemoryBarrier();
            if( s != null ) s.SetClientMinimalFilterDirty();
        }

        void IActivityMonitorBridgeCallback.OnTargetTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            _source.SetTopic( newTopic, fileName, lineNumber );
        }

        void IActivityMonitorBridgeCallback.OnTargetAutoTagsChanged( string marshalledNewTags )
        {
            Debug.Fail( "Never called." );
        }

        void IActivityMonitorBridgeCallback.OnTargetAutoTagsChanged( CKTrait newTags )
        {
            _source.AutoTags = newTags;
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
                    _bridgeTarget.RemoveCallback( _crossADCallback );
                    _crossADCallback.InUse = false;
                }
                else _bridgeTarget.RemoveCallback( this );
                // Unregistering.
                for( int i = 0; i < _openedGroups.Count; ++i )
                {
                    if( _openedGroups[i] )
                    {
                        if( _targetMonitor != null ) _targetMonitor.CloseGroup( new ActivityLogGroupConclusion( R.ClosedByBridgeRemoved, TagBridgePrematureClose ) );
                        else _bridgeTarget.CloseGroup( new string[] { TagBridgePrematureClose.ToString(), R.ClosedByBridgeRemoved } );
                    }
                }
                _openedGroups.Clear();
            }
            else
            {
                if( _crossADCallback != null )
                {
                    _crossADCallback.InUse = true;
                    _bridgeTarget.AddCallback( _crossADCallback );
                }
                else _bridgeTarget.AddCallback( this );
                _targetActualFilter = IsCrossAppDomain ? _bridgeTarget.TargetFinalFilterCrossAppDomain : _bridgeTarget.TargetFinalFilter;
                if( _pullTargetTopicAndAutoTagsFromTarget )
                {
                    string targetTopic;
                    CKTrait targetTags;
                    if( IsCrossAppDomain ) 
                    {
                        string marshalledTags;
                        _bridgeTarget.GetTargetAndAutoTags( out targetTopic, out marshalledTags );
                        targetTags = ActivityMonitor.RegisteredTags.FindOrCreate( marshalledTags );
                    }
                    else _bridgeTarget.GetTargetAndAutoTags( out targetTopic, out targetTags );
                    source.InitializeTopicAndAutoTags( targetTopic, targetTags );
                }

            }
            _source = source;
            Thread.MemoryBarrier();
        }

        LogFilter IActivityMonitorBoundClient.MinimalFilter { get { return GetActualTargetFilter(); } }        

        /// <summary>
        /// This is necessarily called in the context of the activity: we can call the bridge that can call 
        /// the Monitor's ActualFilter that will be resynchronized if needed.
        /// </summary>
        LogFilter GetActualTargetFilter() 
        {
            Thread.MemoryBarrier();
            var f = _targetActualFilter;
            if( f == LogFilter.Invalid )
            {
                do
                {
                    f = IsCrossAppDomain ? _bridgeTarget.TargetFinalFilterCrossAppDomain : _bridgeTarget.TargetFinalFilter;
                    _targetActualFilter = f;
                    Thread.MemoryBarrier();
                }
                while( _targetActualFilter == LogFilter.Invalid );
            }
            return f; 
        }

        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc, string fileName, int lineNumber )
        {
            // If the level is above the actual target filter, we always send the message.
            // If the level is lower: if the log has not been filtered (UnfilteredLog has been called and not an extension method) we must
            // send it to honor the "Unfiltered" contract, but if _applyTargetFilterToUnfilteredLogs is true, we avoid sending it.
            if( ((level&LogLevel.IsFiltered) == 0 && !_applyTargetFilterToUnfilteredLogs) || (int)GetActualTargetFilter().Line <= (int)(level & LogLevel.Mask) )
            {
                if( _targetMonitor != null ) _targetMonitor.UnfilteredLog( tags, level, text, logTimeUtc, null, fileName, lineNumber );
                else
                {
                    _bridgeTarget.UnfilteredLog( tags.ToString(), level, text, logTimeUtc, fileName, lineNumber );
                }
            }
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            Debug.Assert( group.GroupLevel != LogLevel.None, "A client never sees a filtered group." );
            Debug.Assert( group.Depth > 0, "Depth is 1-based." );
            // Make sure the index is available.
            // This handles the case where this ClientBridge has been added to the Monitor.Output
            // after the opening of Groups: we must not trigger a Close on the final monitor for them.
            int idx = group.Depth;
            while( idx > _openedGroups.Count ) _openedGroups.Add( false );

            // By using here our array of boolean to track filtered opened groups against the target, we avoid useless 
            // sollicitation (and marshalling when crossing application domains).
            // Note: If the group has already been filtered out by extension methods (group.GroupLevel == LogLevel.None),
            // we do not see it here. Checking the LogLevelFilter is ok.
            if( ( (group.GroupLevel&LogLevel.IsFiltered) == 0 && !_applyTargetFilterToUnfilteredLogs) || (int)GetActualTargetFilter().Group <= (int)group.MaskedGroupLevel )
            {
                if( _targetMonitor != null )
                    _targetMonitor.UnfilteredOpenGroup( group.GroupTags, group.GroupLevel, null, group.GroupText, group.LogTimeUtc, group.Exception, group.FileName, group.LineNumber );
                else
                {
                    _bridgeTarget.UnfilteredOpenGroup( group.GroupTags.ToString(), group.GroupLevel, group.EnsureExceptionData(), group.GroupText, group.FileName, group.LineNumber, group.LogTimeUtc );
                }
                _openedGroups[idx - 1] = true;
            }
            else _openedGroups[idx - 1] = false;
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
            // Does nothing.
            // The Clients of the target do not see the "Closing" of a Group here: it will receive it as part of the CloseGroup issued by 
            // OnGroupClosed method below.
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _openedGroups[group.Depth - 1] )
            {
                if( _targetMonitor != null ) _targetMonitor.CloseGroup( conclusions );
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
                    _bridgeTarget.CloseGroup( taggedConclusions );
                }
            }
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            if( _pushTopicAndAutoTagsToTarget ) _bridgeTarget.SetTopic( newTopic, fileName, lineNumber );
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTags )
        {
            if( _pushTopicAndAutoTagsToTarget )
            {
                if( IsCrossAppDomain ) _bridgeTarget.SetAutoTags( newTags.ToString() );
                else _bridgeTarget.SetAutoTags( newTags );
            }
        }

    }
}
