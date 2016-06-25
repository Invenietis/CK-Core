using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="IActivityMonitorClient"/> that relays what happens in a monitor to another monitor.
    /// In Net55, automatically supports logs crossing Application Domains. See <see cref="ActivityMonitorBridgeTarget"/>.
    /// </summary>
    public sealed class ActivityMonitorBridge : IActivityMonitorBoundClient, IActivityMonitorBridgeCallback
    {
        readonly ActivityMonitorBridgeTarget _bridgeTarget;
        readonly IActivityMonitor _targetMonitor;
        IActivityMonitorImpl _source;
        // I'm missing a BitList in the framework...
        readonly List<bool> _openedGroups;
        LogFilter _targetActualFilter;
        readonly bool _pullTargetTopicAndAutoTagsFromTarget;
        readonly bool _pushTopicAndAutoTagsToTarget;
        readonly bool _applyTargetFilterToUnfilteredLogs;

        /// <summary>
        /// Tags group conclusions emitted because of premature (unbalanced) removing of a bridge from a source monitor.
        /// </summary>
        public static readonly CKTrait TagBridgePrematureClose = ActivityMonitor.Tags.Register( "c:ClosedByBridgeRemoved" );

        /// <summary>
        /// Initialize a new <see cref="ActivityMonitorBridge"/> bound to an existing <see cref="ActivityMonitorBridgeTarget"/>
        /// This Client should be registered in the <see cref="IActivityMonitor.Output"/> of a local monitor.
        /// </summary>
        /// <param name="bridge">The target bridge.</param>
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
            _targetMonitor = _bridgeTarget.TargetMonitor;
            _openedGroups = new List<bool>();
        }

        /// <summary>
        /// Gets the target monitor. 
        /// </summary>
        public IActivityMonitor TargetMonitor => _targetMonitor;

        /// <summary>
        /// Gets the target bridge of the <see cref="TargetMonitor"/>. 
        /// </summary>
        public ActivityMonitorBridgeTarget BridgeTarget => _bridgeTarget;

        /// <summary>
        /// Gets whether this bridge updates the Topic and AutoTags of its monitor whenever 
        /// they change on the target monitor.
        /// </summary>
        public bool PullTopicAndAutoTagsFromTarget => _pullTargetTopicAndAutoTagsFromTarget; 

        void IActivityMonitorBridgeCallback.OnTargetActualFilterChanged()
        {
            Interlocked.MemoryBarrier();
            var s = _source;
            _targetActualFilter = LogFilter.Invalid;
            Interlocked.MemoryBarrier();
            if( s != null ) s.SetClientMinimalFilterDirty();
        }

        void IActivityMonitorBridgeCallback.OnTargetTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            _source.SetTopic( newTopic, fileName, lineNumber );
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
                _bridgeTarget.RemoveCallback( this );
                // Unregistering.
                for( int i = 0; i < _openedGroups.Count; ++i )
                {
                    if( _openedGroups[i] )
                    {
                        _targetMonitor.CloseGroup( new ActivityLogGroupConclusion( ActivityMonitorResources.ClosedByBridgeRemoved, TagBridgePrematureClose ) );
                    }
                }
                _openedGroups.Clear();
            }
            else
            {
                _bridgeTarget.AddCallback( this );
                _targetActualFilter = _bridgeTarget.TargetFinalFilter;
                if( _pullTargetTopicAndAutoTagsFromTarget )
                {
                    source.InitializeTopicAndAutoTags( this._targetMonitor.Topic, _targetMonitor.AutoTags );
                }

            }
            _source = source;
            Interlocked.MemoryBarrier();
        }

        LogFilter IActivityMonitorBoundClient.MinimalFilter => GetActualTargetFilter();

        /// <summary>
        /// This is necessarily called in the context of the activity: we can call the bridge that can call 
        /// the Monitor's ActualFilter that will be resynchronized if needed.
        /// </summary>
        LogFilter GetActualTargetFilter()
        {
            Interlocked.MemoryBarrier();
            var f = _targetActualFilter;
            if( f == LogFilter.Invalid )
            {
                do
                {
                    f = _bridgeTarget.TargetFinalFilter;
                    _targetActualFilter = f;
                    Interlocked.MemoryBarrier();
                }
                while( _targetActualFilter == LogFilter.Invalid );
            }
            return f;
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            // If the level is above the actual target filter, we always send the message.
            // If the level is lower: if the log has not been filtered (UnfilteredLog has been called and not an extension method) we must
            // send it to honor the "Unfiltered" contract, but if _applyTargetFilterToUnfilteredLogs is true, we avoid sending it.
            var level = data.Level;
            if( ((level & LogLevel.IsFiltered) == 0 && !_applyTargetFilterToUnfilteredLogs) || (int)GetActualTargetFilter().Line <= (int)(level & LogLevel.Mask) )
            {
                _targetMonitor.UnfilteredLog( data );
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
            // solicitation (and marshaling when crossing application domains).
            // Note: If the group has already been filtered out by extension methods (group.GroupLevel == LogLevel.None),
            // we do not see it here. Checking the LogLevelFilter is ok.
            if( ((group.GroupLevel & LogLevel.IsFiltered) == 0 && !_applyTargetFilterToUnfilteredLogs) || (int)GetActualTargetFilter().Group <= (int)group.MaskedGroupLevel )
            {
                _targetMonitor.UnfilteredOpenGroup( group.GroupTags, group.GroupLevel, null, group.GroupText, group.LogTime, group.Exception, group.FileName, group.LineNumber );
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
                _targetMonitor.CloseGroup( conclusions );
                _openedGroups[group.Depth - 1] = false;
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
                _bridgeTarget.SetAutoTags( newTags );
            }
        }

    }
}
