using System.Collections.Generic;
using System.Linq;
using CK.Core;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{
    /// <summary>
    /// Describes the final state that must be reached to satisfy 
    /// a new plugins and service requirements description.
    /// </summary>
    /// <remarks>
    /// The current running status of the plugins can be used to compute the best execution plan but does not change the content 
    /// of the <see cref="PluginsToStart"/>, <see cref="PluginsToStop"/> and <see cref="PluginsToDisable"/>.
    /// </remarks>
    public class ExecutionPlan
    {
        /// <summary>
        /// Gets the collection of plugins that must be started.
        /// Null when <see cref="Impossible"/> is true.
        /// </summary>
        public IReadOnlyCollection<IPluginInfo> PluginsToStart { get; private set; }

        /// <summary>
        /// Gets the collection of plugins that must be stopped.
        /// Null when <see cref="Impossible"/> is true.
        /// </summary>
        public IReadOnlyCollection<IPluginInfo> PluginsToStop { get; private set; }

        /// <summary>
        /// Gets the collection of plugins that must be disabled. 
        /// Null when <see cref="Impossible"/> is true.
        /// </summary>
        public IReadOnlyCollection<IPluginInfo> PluginsToDisable { get; private set; }

        /// <summary>
        /// Gets whether the execution is not possible (no running configuration that satisfy
        /// the requirements can be found).
        /// </summary>
        public bool Impossible { get { return PluginsToStart == null; } }

        internal ExecutionPlan() 
        { 
            // Let properties be null: This is the Impossible one.
        }

        internal ExecutionPlan( IEnumerable<IPluginInfo> pluginsToStart, IEnumerable<IPluginInfo> pluginsToStop, IReadOnlyCollection<IPluginInfo> pluginsToDisable )
        {
            Debug.Assert( pluginsToStart != null && pluginsToStop != null && pluginsToDisable != null );
            PluginsToStart = pluginsToStart.ToReadOnlyCollection();
            PluginsToStop = pluginsToStop.ToReadOnlyCollection();
            PluginsToDisable = pluginsToDisable;
        }

    }
}
