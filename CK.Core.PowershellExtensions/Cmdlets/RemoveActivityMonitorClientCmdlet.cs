using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.PowershellExtensions.Cmdlets
{
    [Cmdlet( VerbsCommon.Remove, "ActivityMonitorClient" )]
    public class RemoveActivityMonitorClientCmdlet : Cmdlet
    {
        [Parameter( Position = 0, Mandatory = true )]
        public IActivityMonitor ActivityMonitor { get; set; }

        [Parameter( Position = 1, Mandatory = true )]
        public IActivityMonitorClient Client { get; set; }

        protected override void ProcessRecord()
        {
            ActivityMonitor.Output.UnregisterClient( Client );
        }
    }
}
