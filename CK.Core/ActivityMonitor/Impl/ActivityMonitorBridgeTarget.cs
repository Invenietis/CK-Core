#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitorBridge.cs) is part of CiviKey. 
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
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// This class (a <see cref="MarshalByRefObject"/>), used with <see cref="ActivityMonitorBridge"/>, enables <see cref="IActivityMonitor"/> to be used across Application Domains.
    /// It can also be used to relay logs inside the same application domain.
    /// Each activity monitor exposes such a bridge target on its output thanks to <see cref="IActivityMonitorOutput.BridgeTarget"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This ActivityMonitorBridgeTarget is created in the original <see cref="AppDomain"/> and bound to the final activity monitor (the target) - this is the job of any IActivityMonitorOutput
    /// implementation to offer a BridgeTarget property.
    /// </para>
    /// <para>
    /// The ActivityMonitorBridge (that is a <see cref="IActivityMonitorClient"/>) can be created in remote AppDomain (and registered 
    /// in the <see cref="IActivityMonitor.Output"/> of a monitor in the remote AppDomain) bound to the ActivityMonitorBridgeTarget (one can use <see cref="AppDomain.SetData(string,object)"/> to 
    /// transfer the ActivityMonitorBridgeTarget to the other AppDomain for instance).
    /// </para>
    /// </remarks>
    public class ActivityMonitorBridgeTarget : MarshalByRefObject, ISponsor
    {
        readonly IActivityMonitorImpl _monitor;
        IActivityMonitorBridgeCallback[] _callbacks;
        IActivityMonitorBridgeCallback[] _crossAppDomainBriddges;
        bool _honorTargetFilter;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorBridgeTarget"/> bound to a <see cref="IActivityMonitor"/>.
        /// This object should be transfered to another AppDomain and a <see cref="ActivityMonitorBridge"/> 
        /// should be bound to it.
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
            _callbacks = _crossAppDomainBriddges = Util.EmptyArray<IActivityMonitorBridgeCallback>.Empty;
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
        /// Gest the target monitor directly when used in the same AppDomain.
        /// </summary>
        internal IActivityMonitorImpl TargetMonitor { get { return _monitor; } }

        /// <summary>
        /// Gets the target final filter that must be used: this property takes into account the monitor's filter and the ActivityMonitor.DefaultFilter application domain 
        /// value if HonorMonitorFilter is true (otherwise it is <see cref="LogFilter.Undefined"/>).
        /// </summary>
        internal LogFilter TargetFinalFilterCrossAppDomain
        {
            get
            {
                if( _honorTargetFilter )
                {
                    return _monitor.ActualFilter.CombineNoneOnly( ActivityMonitor.DefaultFilter );
                }
                return LogFilter.Undefined;
            }
        }

        /// <summary>
        /// Gets the target final filter that must be used without taking into account the ActivityMonitor.DefaultFilter application domain value.
        /// </summary>
        internal LogFilter TargetFinalFilter
        {
            get { return _monitor.ActualFilter; }
        }

        /// <summary>
        /// Called by ActivityMonitorBridge.SetMonitor (the reentrant check is acquired).
        /// </summary>
        internal void AddCallback( IActivityMonitorBridgeCallback callback )
        {
            Debug.Assert( Array.IndexOf( _callbacks, callback ) < 0 );
            Util.InterlockedAdd( ref _callbacks, callback );
            if( callback.IsCrossAppDomain )
            {
                var bridges = Util.InterlockedAdd( ref _crossAppDomainBriddges, callback );
                if( bridges.Length == 1 ) ActivityMonitor.DefaultFilterLevelChanged -= DefaultAppDomainFilterChanged;
            }
        }

        /// <summary>
        /// Called by ActivityMonitorBridge.SetMonitor (the reentrant check is acquired).
        /// </summary>
        internal void RemoveCallback( IActivityMonitorBridgeCallback callback )
        {
            Debug.Assert( Array.IndexOf( _callbacks, callback ) >= 0 );
            Util.InterlockedRemove( ref _callbacks, callback );
            if( callback.IsCrossAppDomain )
            {
                var bridges = Util.InterlockedRemove( ref _crossAppDomainBriddges, callback );
                if( bridges.Length == 0 ) ActivityMonitor.DefaultFilterLevelChanged -= DefaultAppDomainFilterChanged;
            }
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
            string cachedTags = null;
            foreach( var b in _callbacks )
            {
                if( b.PullTopicAndAutoTagsFromTarget )
                {
                    if( b.IsCrossAppDomain ) b.OnTargetAutoTagsChanged( cachedTags ?? (cachedTags = _monitor.AutoTags.ToString()) );
                    else b.OnTargetAutoTagsChanged( newTags );
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

        void DefaultAppDomainFilterChanged( object sender, EventArgs e )
        {
            // This occurs on a totally external thread.
            var bridges = _crossAppDomainBriddges;
            foreach( var b in bridges ) b.OnTargetActualFilterChanged();
        }

        #region Cross AppDomain interface.

        internal void UnfilteredLog( string tags, LogLevel level, string text, DateTime logTimeUtc, string fileName, int lineNumber )
        {
            _monitor.UnfilteredLog( new ActivityMonitorData( level, ActivityMonitor.RegisteredTags.FindOrCreate( tags ), text, logTimeUtc, null, fileName, lineNumber ) );
        }

        internal void UnfilteredOpenGroup( string tags, LogLevel level, CKExceptionData exceptionData, string groupText, string fileName, int lineNumber, DateTime logTimeUtc )
        {
            CKException ckEx = exceptionData != null ? new CKException( exceptionData ) : null;
            _monitor.UnfilteredOpenGroup( ActivityMonitor.RegisteredTags.FindOrCreate( tags ), level, null, groupText, logTimeUtc, ckEx, fileName, lineNumber );
        }

        internal void CloseGroup( string[] taggedConclusions )
        {
            Debug.Assert( taggedConclusions == null || (taggedConclusions.Length >= 2 && taggedConclusions.Length % 2 == 0) );
            List<ActivityLogGroupConclusion> c = null;
            if( taggedConclusions != null )
            {
                c = new List<ActivityLogGroupConclusion>();
                int i = 0;
                while( i < taggedConclusions.Length )
                {
                    CKTrait t = ActivityMonitor.RegisteredTags.FindOrCreate( taggedConclusions[i++] );
                    c.Add( new ActivityLogGroupConclusion( t, taggedConclusions[i++] ) );
                }
            }
            _monitor.CloseGroup( c );
        }

        internal void GetTargetAndAutoTags( out string targetTopic, out string marshalledTags )
        {
            targetTopic = _monitor.Topic;
            marshalledTags = _monitor.AutoTags.ToString();
        }

        internal void GetTargetAndAutoTags( out string targetTopic, out CKTrait targetTags )
        {
            targetTopic = _monitor.Topic;
            targetTags = _monitor.AutoTags;
        }

        internal void SetAutoTags( string marshalledTags )
        {
            SetAutoTags( ActivityMonitor.RegisteredTags.FindOrCreate( marshalledTags ) );
        }

        internal void SetTopic( string newTopic, string fileName, int lineNumber )
        {
            _monitor.SetTopic( newTopic, fileName, lineNumber );
        }

        internal void SetAutoTags( CKTrait tags )
        {
            _monitor.AutoTags = tags;
        }

        #endregion

        /// <summary>
        /// Gets the lease for this object.
        /// </summary>
        /// <returns>The lease.</returns>
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
            return _crossAppDomainBriddges.Length == 0 ? TimeSpan.Zero : TimeSpan.FromMinutes( 2 );
        }

    }
}
