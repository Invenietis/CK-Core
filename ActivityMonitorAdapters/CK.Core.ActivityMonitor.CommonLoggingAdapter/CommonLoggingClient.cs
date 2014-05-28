using System;
using System.Collections.Generic;
using Common.Logging;

namespace CK.Core.ActivityMonitorAdapters.CommonLoggingImpl
{
    class CommonLoggingClient : IActivityMonitorClient
    {
        protected ILog Logger;

        /// <summary>
        /// Creates a single NLog client which outputs to a single given NLog Logger.
        /// </summary>
        /// <param name="logger">NLog logger to log to.</param>
        public CommonLoggingClient( ILog logger )
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
