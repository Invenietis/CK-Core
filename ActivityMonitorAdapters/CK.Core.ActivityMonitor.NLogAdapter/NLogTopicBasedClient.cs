using CK.Core.Impl;
using NLog;

namespace CK.Core.ActivityMonitorAdapters.NLogImpl
{
    /// <summary>
    /// ActivityMonitor client, outputting to NLog, which auto-gets the NLog logger based in the ActivityMonitor's topic.
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
