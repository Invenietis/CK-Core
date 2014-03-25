using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core.ActivityMonitorAdapters.CommonLoggingImpl;

namespace CK.Core.ActivityMonitorAdapters
{
    public class CommonLoggingAdapter
    {
        /// <summary>
        /// Causes all newly created ActivityMonitors to automatically output to Common.Logging loggers (based on ActivityMonitors' topics).
        /// </summary>
        public static void Initialize()
        {
            ActivityMonitor.AutoConfiguration += ( monitor ) =>
            {
                CommonLoggingTopicBasedClient client = new CommonLoggingTopicBasedClient( monitor.Topic );
                monitor.Output.RegisterClient( client );
            };
        }
    }
}
