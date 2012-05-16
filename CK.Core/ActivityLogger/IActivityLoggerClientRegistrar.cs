using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Offers <see cref="IActivityLoggerClient"/> registering capabilities.
    /// </summary>
    public interface IActivityLoggerClientRegistrar
    {
        /// <summary>
        /// Registers an <see cref="IActivityLoggerClient"/> to the <see cref="RegisteredClients"/> list.
        /// Duplicate IActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        IActivityLoggerClientRegistrar RegisterClient( IActivityLoggerClient client );

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerClient"/> from the <see cref="RegisteredClients"/> list.
        /// Silently ignored unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        IActivityLoggerClientRegistrar UnregisterClient( IActivityLoggerClient client );

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerClient"/>.
        /// </summary>
        IReadOnlyList<IActivityLoggerClient> RegisteredClients { get; }
    }

}
