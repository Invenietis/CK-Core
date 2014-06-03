using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// The interface that carries Send extension methods.
    /// </summary>
    public interface IActivityMonitorLineSender
    {
        /// <summary>
        /// Gets whether the log has been rejected.
        /// </summary>
        bool IsRejected { get; }
    }
}
