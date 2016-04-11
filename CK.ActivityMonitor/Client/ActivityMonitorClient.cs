#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Client\ActivityMonitorClient.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Base class that explicitly implements <see cref="IActivityMonitorClient"/> (to hide it from public interface, except its <see cref="MinimalFilter"/>).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ActivityMonitorClient : IActivityMonitorClient
    {
        /// <summary>
        /// Empty <see cref="IActivityMonitorClient"/> (null object design pattern).
        /// </summary>
        public static readonly ActivityMonitorClient Empty = new ActivityMonitorClient();

        /// <summary>
        /// Initialize a new <see cref="ActivityMonitorClient"/> that does nothing.
        /// </summary>
        public ActivityMonitorClient()
        {
        }

        /// <summary>
        /// Gets the minimal log level that this Client expects: defaults to <see cref="LogFilter.Undefined"/>.
        /// </summary>
        public virtual LogFilter MinimalFilter { get { return LogFilter.Undefined; } }

        /// <summary>
        /// Called for each <see cref="IActivityMonitor.UnfilteredLog"/>. Does nothing by default.
        /// The <see cref="ActivityMonitorLogData.Exception"/> is always null since exceptions
        /// are carried by groups.
        /// </summary>
        /// <param name="data">Log data. Never null.</param>
        protected virtual void OnUnfilteredLog( ActivityMonitorLogData data )
        {
        }

        /// <summary>
        /// Called for each <see cref="IActivityMonitor.UnfilteredOpenGroup"/>.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        protected virtual void OnOpenGroup( IActivityLogGroup group )
        {
        }

        /// <summary>
        /// Called once the user conclusions are known at the group level but before 
        /// the group is actually closed: clients can update the conclusions for the group.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">
        /// Mutable conclusions associated to the closing group. 
        /// This can be null if no conclusions have been added yet. 
        /// It is up to the first client that wants to add a conclusion to instantiate a new List object to carry the conclusions.
        /// </param>
        protected virtual void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        /// <summary>
        /// Called when the group is actually closed.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Text that conclude the group. Never null but can be empty.</param>
        protected virtual void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
        }

        /// <summary>
        /// Called when <see cref="IActivityMonitor.Topic"/> changed.
        /// Does nothing by default.
        /// </summary>
        /// <param name="newTopic">The new topic.</param>
        /// <param name="fileName">Source file name where <see cref="IActivityMonitor.SetTopic"/> has been called.</param>
        /// <param name="lineNumber">Source line number where IActivityMonitor.SetTopic has been called.</param>
        protected virtual void OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
        }

        /// <summary>
        /// Called when <see cref="IActivityMonitor.AutoTags"/> changed.
        /// Does nothing by default.
        /// </summary>
        /// <param name="newTags">The new auto tags.</param>
        protected virtual void OnAutoTagsChanged( CKTrait newTags )
        {
        }

        /// <summary>
        /// Creates a standardized exception that can be thrown by <see cref="IActivityMonitorBoundClient.SetMonitor"/>.
        /// </summary>
        /// <param name="boundClient">The bound client.</param>
        /// <returns>An exception with an explicit message.</returns>
        static public InvalidOperationException CreateMultipleRegisterOnBoundClientException( IActivityMonitorBoundClient boundClient )
        {
            return new InvalidOperationException( String.Format( Impl.ActivityMonitorResources.ActivityMonitorBoundClientMultipleRegister, boundClient != null ? boundClient.GetType().FullName : String.Empty ) );
        }

        #region IActivityMonitorClient Members

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            OnUnfilteredLog( data );
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            OnOpenGroup( group );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosing( group, ref conclusions );
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosed( group, conclusions );
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            OnTopicChanged( newTopic, fileName, lineNumber );
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTags )
        {
            OnAutoTagsChanged( newTags );
        }

        #endregion

    }
}
