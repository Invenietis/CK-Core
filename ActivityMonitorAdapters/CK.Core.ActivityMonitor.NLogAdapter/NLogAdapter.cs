using CK.Core.ActivityMonitorAdapters.NLogImpl;

namespace CK.Core.ActivityMonitorAdapters
{
    public static class NLogAdapter
    {
        /// <summary>
        /// Causes all newly created ActivityMonitors to automatically output to NLog loggers (based on ActivityMonitors' topics).
        /// </summary>
        public static void Initialize()
        {
            ActivityMonitor.AutoConfiguration += ( monitor ) =>
            {
                NLogTopicBasedClient client = new NLogTopicBasedClient( monitor.Topic );
                monitor.Output.RegisterClient( client );
            };
        }
    }
}
