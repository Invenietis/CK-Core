using CK.Core.Impl;
using Common.Logging;

namespace CK.Core.ActivityMonitorAdapters.CommonLoggingImpl
{
    class CommonLoggingTopicBasedClient : CommonLoggingClient, IActivityMonitorBoundClient
    {
        IActivityMonitorImpl _source;

        public CommonLoggingTopicBasedClient( string initialTopic = "" )
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
