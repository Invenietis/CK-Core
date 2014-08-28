using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring.Server
{
    public class ActivityMonitorServerHostConfiguration
    {
        public int Port { get; set; }

        public int CrititcalErrorPort { get; set; }
    }
}
