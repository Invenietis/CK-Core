using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;

namespace CK.Plugin.Hosting
{
    class ExecutionPlanResult : IExecutionPlanResult
    {
        Exception _error;

        public ExecutionPlanResultStatus Status { get; internal set; }
        public IPluginInfo Culprit { get; internal set; }
        public IPluginSetupInfo SetupInfo { get; internal set; }

        public Exception Error
        {
            get { return _error ?? SetupInfo.Error; }
            set { _error = value; }
        }

    }

}
