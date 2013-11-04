using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Defines a sink that can be registered onto a <see cref="GrandOutput"/>
    /// to intercept any log event. It is also supported by <see cref="CK.Monitoring.GrandOutputHandlers.HandlerBase"/>.
    /// </summary>
    public interface IGrandOutputSink
    {
        /// <summary>
        /// This is initially called non concurrently from a dispatcher background thread:
        /// implementations do not need any synchronization mechanism by default except when <paramref name="parrallelCall"/> is true.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="parrallelCall">True when this method is called in parallel with other sinks.</param>
        void Handle( GrandOutputEventInfo logEvent, bool parrallelCall );
    }

}
