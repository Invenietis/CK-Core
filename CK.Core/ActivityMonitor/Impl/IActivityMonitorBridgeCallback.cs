using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Internal interface that allows ActivityMonitorBridgeTarget to call back
    /// the ActivityMonitorBridges that are bound to it.
    /// </summary>
    interface IActivityMonitorBridgeCallback
    {
        /// <summary>
        /// Called when the target filter changed or is dirty. This can be called on any thread.
        /// </summary>
        void OnTargetFilterChanged();
    }
}
