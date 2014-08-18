#region LGPL License
/*----------------------------------------------------------------------------
* This file (ActivityMonitorAdapters\CK.Core.ActivityMonitor.NLogAdapter\NLogClient.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using NLog;

namespace CK.Core.ActivityMonitorAdapters.NLogImpl
{
    /// <summary>
    /// ActivityMonitor client which outputs to a single given NLog Logger.
    /// </summary>
    public class NLogClient : IActivityMonitorClient
    {
        /// <summary>
        /// The assigned logger.
        /// <see cref="NLogTopicBasedClient"/> changed this whenever the monitor's topic changes.
        /// </summary>
        protected Logger Logger;

        /// <summary>
        /// Creates a single NLog client which outputs to a single given NLog Logger.
        /// </summary>
        /// <param name="logger">NLog logger to log to.</param>
        public NLogClient( Logger logger )
        {
            if( logger == null ) throw new ArgumentNullException( "logger" );

            Logger = logger;
        }

        #region IActivityMonitorClient Members

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
            Logger.LogActivityMonitorAutoTagsChanged( newTrait );
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            Logger.LogActivityMonitorGroupClosed( group, conclusions );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            Logger.LogActivityMonitorOpenGroup( group );
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            Logger.LogActivityMonitorTopicChanged( newTopic, fileName, lineNumber );
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            Logger.LogActivityMonitorEntry( data );
        }

        #endregion
    }
}
