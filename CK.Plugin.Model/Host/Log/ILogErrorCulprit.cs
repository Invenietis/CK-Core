using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CK.Plugin
{
    /// <summary>
    /// Base interface that defines a log event that holds an <see cref="Exception"/>.
    /// </summary>
    public interface ILogErrorCulprit : ILogEntry
    {
        /// <summary>
        /// The culprit is actually required to define an error. 
        /// The specialized <see cref="ILogErrorCaught"/> holds an exception but there exist errors 
        /// that do not have any associated exception to expose.
        /// This is the case of <see cref="ILogEventNotRunningError"/>: when a plugin raises an event 
        /// while beeing stopped, it is an error (silently ignored by the kernel), but there is
        /// no exception to associate with.
        /// </summary>
        MemberInfo Culprit { get; }

    }
}
