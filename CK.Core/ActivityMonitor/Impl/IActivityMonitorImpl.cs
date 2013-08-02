using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Impl
{
    /// <summary>
    /// Defines required aspects that an actual monitor implementation must support.
    /// </summary>
    public interface IActivityMonitorImpl : IActivityMonitor, IUniqueId
    {
        /// <summary>
        /// Gets the currently opened group.
        /// Null when no group is currently opened.
        /// </summary>
        IActivityLogGroup Current { get; }

    }
}
