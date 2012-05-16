using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Offers <see cref="IMuxActivityLoggerClient"/> registering capabilities.
    /// </summary>
    public interface IMuxActivityLoggerClientRegistrar
    {
        /// <summary>
        /// Registers an <see cref="IMuxActivityLoggerClient"/> to the <see cref="RegisteredMuxClients"/> list.
        /// Duplicate IMuxActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        IMuxActivityLoggerClientRegistrar RegisterMuxClient( IMuxActivityLoggerClient client );

        /// <summary>
        /// Unregisters the given <see cref="IMuxActivityLoggerClient"/> from the <see cref="RegisteredMuxClients"/> list.
        /// Silently ignored unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        IMuxActivityLoggerClientRegistrar UnregisterMuxClient( IMuxActivityLoggerClient client );

        /// <summary>
        /// Gets the list of registered <see cref="IMuxActivityLoggerClient"/>.
        /// </summary>
        IReadOnlyList<IMuxActivityLoggerClient> RegisteredMuxClients { get; }
    }

}
