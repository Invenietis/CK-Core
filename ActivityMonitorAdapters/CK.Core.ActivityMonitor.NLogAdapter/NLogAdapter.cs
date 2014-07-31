using CK.Core.ActivityMonitorAdapters.NLogImpl;

namespace CK.Core.ActivityMonitorAdapters
{
    /// <summary>
    /// Startup class: <see cref="Initialize"/> makes all new <see cref="ActivityMonitor"/> routes their output to NLog loggers
    /// named with the monitor's topic.
    /// </summary>
    public static class NLogAdapter
    {
        static bool _isInitialized;
        static object _lock = new object();

        /// <summary>
        /// Causes all newly created ActivityMonitors to automatically output to NLog loggers (based on ActivityMonitors' topics).
        /// </summary>
        public static void Initialize()
        {
            lock( _lock )
            {
                if( !_isInitialized )
                {
                    ActivityMonitor.AutoConfiguration += ( monitor ) =>
                    {
                        NLogTopicBasedClient client = new NLogTopicBasedClient( monitor.Topic );
                        monitor.Output.RegisterClient( client );
                    };
                    _isInitialized = true;
                }
            }
        }
    }
}
