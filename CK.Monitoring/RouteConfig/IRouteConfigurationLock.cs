using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig
{
    /// <summary>
    /// Enables routes configuration locking.
    /// Route obtained by <see cref="ConfiguredRouteHost{TAction,TRoute}.ObtainRoute"/> are initally locked: they must be unlocked before a new configuration can be applied. 
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
