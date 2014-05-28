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
