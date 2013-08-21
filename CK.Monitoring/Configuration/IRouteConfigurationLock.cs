using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig
{
    /// <summary>
    /// Enables routes configuration locking.
    /// </summary>
    public interface IRouteConfigurationLock
    {
        /// <summary>
        /// Locks the configuration.
        /// </summary>
        void Lock();

        /// <summary>
        /// Unlocks the configuration.
        /// </summary>
        void Unlock();
    }
}
