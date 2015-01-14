#region LGPL License
/*----------------------------------------------------------------------------
* This file (ActivityMonitorAdapters\CK.Core.ActivityMonitor.NLogAdapter\NLogTopicBasedClient.cs) is part of CiviKey. 
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

using CK.Core.Impl;
using NLog;

namespace CK.Core.ActivityMonitorAdapters.NLogImpl
{
    /// <summary>
    /// ActivityMonitor client that outputs to NLog, which auto-gets the NLog logger based in the ActivityMonitor's topic.
    /// </summary>
    class NLogTopicBasedClient : NLogClient, IActivityMonitorBoundClient
    {
        IActivityMonitorImpl _source;

        public NLogTopicBasedClient( string initialTopic = "" )
            : base( LogManager.GetLogger( initialTopic ) )
        {
        }

        #region IActivityMonitorBoundClient Members

        LogFilter IActivityMonitorBoundClient.MinimalFilter
        {
            get { return LogFilter.Undefined; }
        }

        void IActivityMonitorBoundClient.SetMonitor( CK.Core.Impl.IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( !forceBuggyRemove )
            {
                if( source != null && _source != null ) throw ActivityMonitorClient.CreateMultipleRegisterOnBoundClientException( this );
            }
            _source = source;
            Logger = LogManager.GetLogger( _source.Topic );
        }

        #endregion

        #region IActivityMonitorClient Members

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            Logger = LogManager.GetLogger( newTopic );
            Logger.LogActivityMonitorTopicChanged( newTopic, fileName, lineNumber );
        }

        #endregion
    }
}
