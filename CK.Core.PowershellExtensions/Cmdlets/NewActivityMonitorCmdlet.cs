using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using CK.Core.PowershellExtensions.Impl;

namespace CK.Core.PowershellExtensions.Cmdlets
{
    [Cmdlet( VerbsCommon.New, "ActivityMonitor" )]
    public class NewActivityMonitorCmdlet : Cmdlet
    {
        [Parameter( Position = 0 )]
        public SwitchParameter ConsoleOutput
        {
            get { return _console; }
            set { _console = value; }
        }
        bool _console;

        protected override void ProcessRecord()
        {
            WriteObject( new PowershellActivityMonitor( _console ) );
        }
    }
}
