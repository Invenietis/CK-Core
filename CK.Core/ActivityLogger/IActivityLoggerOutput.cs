using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Combines the two registrars (<see cref="IActivityLoggerClientRegistrar"/> and <see cref="IMuxActivityLoggerClientRegistrar"/>)
    /// and exposes an <see cref="ExternalInput"/> (an <see cref="IMuxActivityLoggerClient"/>) that can be registered as a
    /// client far any number of other loggers.
    /// </summary>
    public interface IActivityLoggerOutput : IActivityLoggerClientRegistrar, IMuxActivityLoggerClientRegistrar
    {
        /// <summary>
        /// Gets an entry point for other loggers: by registering this <see cref="IMuxActivityLoggerClient"/> in other <see cref="IActivityLogger.Output"/>,
        /// log streams can easily be merged.
        /// </summary>
        IMuxActivityLoggerClient ExternalInput { get; }
    }

}
