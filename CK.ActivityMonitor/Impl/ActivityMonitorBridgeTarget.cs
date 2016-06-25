using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

#if NET451 || NET46
using System.Runtime.Remoting.Lifetime;
#endif
using System.Threading;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// This class used with <see cref="ActivityMonitorBridge"/>, enables <see cref="IActivityMonitor"/> to relay logs.
    /// Each activity monitor exposes such a bridge target on its output thanks to <see cref="IActivityMonitorOutput.BridgeTarget"/>.
    /// </summary>
    public sealed class ActivityMonitorBridgeTarget
    {
        readonly IActivityMonitorImpl _monitor;
        IActivityMonitorBridgeCallback[] _callbacks;
        bool _honorTargetFilter;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorBridgeTarget"/> bound to a <see cref="IActivityMonitor"/>.
        /// </summary>
        /// <param name="targetMonitor">Monitor that will receive the logs.</param>
        /// <param name="honorMonitorFilter">
        /// False to ignore the actual filter <see cref="IActivityMonitor.ActualFilter"/> value: logs coming from the bridge (ie. the remote Application Domain)
        /// will always be added to this target monitor.
        /// </param>
        public ActivityMonitorBridgeTarget( IActivityMonitorImpl targetMonitor, bool honorMonitorFilter = true )
        {
            if( targetMonitor == null ) throw new ArgumentNullException( "targetMonitor" );
            _monitor = targetMonitor;
            _honorTargetFilter = honorMonitorFilter;
            _callbacks = Util.Array.Empty<IActivityMonitorBridgeCallback>();
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IActivityMonitor.ActualFilter"/> of the target monitor should be honored or not.
        /// Defaults to true.
        /// </summary>
        public bool HonorMonitorFilter
        {
            get { return _honorTargetFilter; }
            set
            {
                if( _honorTargetFilter != value )
                {
                    using( _monitor.ReentrancyAndConcurrencyLock() )
                    {
                        _honorTargetFilter = value;
                        TargetActualFilterChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the target monitor.
        /// </summary>
        internal IActivityMonitorImpl TargetMonitor => _monitor;

        /// <summary>
        /// Gets the target final filter that must be used without taking into account the ActivityMonitor.DefaultFilter application domain value.
        /// </summary>
        internal LogFilter TargetFinalFilter => _monitor.ActualFilter; 

        /// <summary>
        /// Called by ActivityMonitorBridge.SetMonitor (the reentrant check is acquired).
        /// </summary>
        internal void AddCallback( IActivityMonitorBridgeCallback callback )
        {
            Debug.Assert( Array.IndexOf( _callbacks, callback ) < 0 );
            Util.InterlockedAdd( ref _callbacks, callback );
        }

        /// <summary>
        /// Called by ActivityMonitorBridge.SetMonitor (the reentrant check is acquired).
        /// </summary>
        internal void RemoveCallback( IActivityMonitorBridgeCallback callback )
        {
            Debug.Assert( Array.IndexOf( _callbacks, callback ) >= 0 );
            Util.InterlockedRemove( ref _callbacks, callback );
        }

        /// <summary>
        /// This is called when HonorMonitorFilter changes or by ActivityMonitor.UpdateActualFilter 
        /// whenever the monitors's ActualFilter changed (in such cases, we are bound to the activity: the Reentrancy and concurrency 
        /// lock has been obtained), or by our monitor's SetClientMinimalFilterDirty() method (in this case, we are called on 
        /// any thread).
        /// </summary>
        internal void TargetActualFilterChanged()
        {
            var callbacks = _callbacks;
            foreach( var b in callbacks ) b.OnTargetActualFilterChanged();
        }

        internal void TargetAutoTagsChanged( CKTrait newTags )
        {
            foreach( var b in _callbacks )
            {
                if( b.PullTopicAndAutoTagsFromTarget )
                {
                    b.OnTargetAutoTagsChanged( newTags );
                }
            }
        }

        internal void TargetTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            foreach( var b in _callbacks )
            {
                if( b.PullTopicAndAutoTagsFromTarget ) b.OnTargetTopicChanged( newTopic, fileName, lineNumber );
            }
        }

        internal void GetTargetAndAutoTags( out string targetTopic, out CKTrait targetTags )
        {
            targetTopic = _monitor.Topic;
            targetTags = _monitor.AutoTags;
        }

        internal void SetTopic( string newTopic, string fileName, int lineNumber )
        {
            _monitor.SetTopic( newTopic, fileName, lineNumber );
        }

        internal void SetAutoTags( CKTrait tags )
        {
            _monitor.AutoTags = tags;
        }
    }
}
