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
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// This class (a <see cref="MarshalByRefObject"/>), used with <see cref="ActivityMonitorBridge"/>, enables <see cref="IActivityMonitor"/> to be used across Application Domains.
    /// It can also be used to relay logs inside the same application domain.
    /// Each activity monitor exposes a bridge on its output thanks to <see cref="IActivityMonitorOutput.ExternalInput"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This ActivityMonitorBridgeTarget is created in the original <see cref="AppDomain"/> and bound to the final activity monitor (the target) - this is the job of any IActivityMonitorOutput
    /// implementation to offer an ExternalInput property.
    /// </para>
    /// <para>
    /// The ActivityMonitorBridge (that is a <see cref="IActivityMonitorClient"/>) can be created in remote AppDomain (and registered 
    /// in the <see cref="IActivityMonitor.Output"/> of a monitor in the remote AppDomain) bound to the ActivityMonitorBridgeTarget (one can use <see cref="AppDomain.SetData(string,object)"/> to 
    /// transfer the ActivityMonitorBridgeTarget to the other AppDomain for instance).
    /// </para>
    /// </remarks>
    public class ActivityMonitorBridgeTarget : MarshalByRefObject
    {
        readonly IActivityMonitorImpl _monitor;
        bool _honorTargetFilter;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorBridgeTarget"/> bound to a <see cref="IActivityMonitor"/>.
        /// This object should be transfered to another AppDomain and a <see cref="ActivityMonitorBridge"/> 
        /// should be bound to it.
        /// </summary>
        /// <param name="targetMonitor">Monitor that will receive the logs.</param>
        /// <param name="honorMonitorFilter">
        /// False to ignore the final filter <see cref="IActivityMonitor.Filter"/> value: logs from the remote Application Domain
        /// will always be added to the final monitor.
        /// </param>
        public ActivityMonitorBridgeTarget( IActivityMonitorImpl targetMonitor, bool honorMonitorFilter = true )
        {
            if( targetMonitor == null ) throw new ArgumentNullException( "targetMonitor" );
            _monitor = targetMonitor;
            _honorTargetFilter = honorMonitorFilter;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IActivityMonitor.Filter"/> of the target monitor should be honored or not.
        /// Defaults to true.
        /// </summary>
        public bool HonorMonitorFilter
        {
            get { return _honorTargetFilter; }
            set { _honorTargetFilter = value; }
        }

        /// <summary>
        /// Gest the final monitor directly when used in the same AppDomain.
        /// </summary>
        internal IActivityMonitorImpl FinalMonitor { get { return _monitor; } }

        internal int TargetFilter
        {
            get { return _honorTargetFilter ? (int)_monitor.Filter : (int)LogLevelFilter.None; }
        }

        #region Cross AppDomain interface.
        internal void UnfilteredLog( string tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            Debug.Assert( (int)_monitor.Filter <= (int)level );
            _monitor.UnfilteredLog( ActivityMonitor.RegisteredTags.FindOrCreate( tags ), level, text, logTimeUtc );
        }

        internal void OpenGroup( string tags, LogLevel level, Exception exception, string groupText, DateTime logTimeUtc )
        {
            Debug.Assert( (int)_monitor.Filter <= (int)level );
            _monitor.OpenGroup( ActivityMonitor.RegisteredTags.FindOrCreate( tags ), level, exception, groupText, logTimeUtc );
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
        #endregion
    }
}
