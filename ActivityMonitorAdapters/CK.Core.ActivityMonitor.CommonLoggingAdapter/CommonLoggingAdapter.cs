using CK.Core.ActivityMonitorAdapters.CommonLoggingImpl;

namespace CK.Core.ActivityMonitorAdapters
{
    /// <summary>
    /// Startup class: <see cref="Initialize"/> makes all new <see cref="ActivityMonitor"/> routes their output to Common.Logging loggers
    /// named with the monitor's topic.
    /// </summary>
    public class CommonLoggingAdapter
    {
        static bool _isInitialized;
        static object _lock = new object();

        /// <summary>
        /// Causes all newly created ActivityMonitors to automatically output to Common.Logging loggers (based on ActivityMonitors' topics).
        /// </summary>
        public static void Initialize()
        {
            lock( _lock )
            {
                if( !_isInitialized )
                {
                    ActivityMonitor.AutoConfiguration += ( monitor ) =>
                    {
                        CommonLoggingTopicBasedClient client = new CommonLoggingTopicBasedClient( monitor.Topic );
                        monitor.Output.RegisterClient( client );
                    };
                    _isInitialized = true;
                }
            }
        }
    }
}
