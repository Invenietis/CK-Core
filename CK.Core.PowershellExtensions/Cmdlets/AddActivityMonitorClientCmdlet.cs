using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.PowershellExtensions.Cmdlets
{
    [Cmdlet( VerbsCommon.Add, "ActivityMonitorClient" )]
    public class AddActivityMonitorClientCmdlet : Cmdlet
    {
        [Parameter( Position = 0, Mandatory = true )]
        public IActivityMonitor ActivityMonitor { get; set; }

        [Parameter( Position = 1, Mandatory = true )]
        public FileInfo LogFile { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject( ActivityMonitor.Output.RegisterClient( new ActivityMonitorTextWriterClient( ( s ) =>
            {
                File.AppendAllText( LogFile.FullName, s );
            } ) ) );
        }
    }
}
