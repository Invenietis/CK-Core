using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin
{

    /// <summary>
    /// Simple interface to the actual plugin host.
    /// Concrete implementations may offer a much more complex api.
    /// (This is a typical facade design pattern.)
    /// </summary>
    public interface ISimplePluginRunner
    {
        /// <summary>
        /// Gets or sets whether all plugins should be disabled or not. Defaults to false. 
        /// Changing this property changes <see cref="IsDirty"/> (<see cref="Apply"/> must be called).
        /// </summary>
        bool Disabled { get; set; }

        /// <summary>
        /// Adds a <see cref="RequirementLayer"/> with possible duplicate check.
        /// </summary>
        /// <param name="r">The requirements layer to add.</param>
        /// <param name="allowDuplicate">True to add the requirement even if it already exists.</param>
        /// <returns>Always true if <paramref name="allowDuplicate"/> is true. False if <see paramref="allowDuplicate"/> is false and the layer already exists.</returns>
        bool Add( RequirementLayer r, bool allowDuplicate );

        /// <summary>
        /// Removes <see cref="RequirementLayer"/> (only one or all of them). 
        /// </summary>
        /// <param name="r">The requirements layer to remove.</param>
        /// <param name="removeAll">True to force every occurence of the layer to be removed.</param>
        /// <returns>True if the layer has been found, false otherwise.</returns>
        bool Remove( RequirementLayer r, bool removeAll );

        /// <summary>
        /// Fires when the <see cref="IsDirty"/> changed.
        /// </summary>
        event EventHandler IsDirtyChanged;

        /// <summary>
        /// Gets whether <see cref="Apply"/> should be called because something has changed
        /// in the configuration.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Fires at the end of an <see cref="Apply"/>.
        /// </summary>
        event EventHandler<ApplyDoneEventArgs> ApplyDone;

        /// <summary>
        /// Attempts to start/stop plugins and services according to the current configuration.
        /// Does nothing (and returns true) if <see cref="IsDirty"/> is false.        
        /// </summary>
        /// <returns>True on success, false if an error occured.</returns>
        bool Apply();

        /// <summary>
        /// Attempts to start/stop plugins and services according to the current configuration.
        /// Does nothing (and returns true) if <see cref="IsDirty"/> is false. 
        /// </summary>
        /// <param name="stopLaunchedOptionals">is false by default. If set to true, already running plugins that are optional are stopped (used to switch users for example)</param>
        /// <returns>True on success, false if an error occured.</returns>
        bool Apply( bool stopLaunchedOptionals );

        /// <summary>
        /// Gives access to the <see cref="IPluginDiscoverer"/> object.
        /// </summary>
        IPluginDiscoverer Discoverer { get; }

        /// <summary>
        /// Gives access to the <see cref="IPluginHost"/> object.
        /// </summary>
        IPluginHost PluginHost { get; }

        /// <summary>
        /// Gives access to the <see cref="IServiceHost"/> object.
        /// </summary>
        IServiceHost ServiceHost { get; }

        /// <summary>
        /// Gives access to the <see cref="ILogCenter"/> object.
        /// </summary>
        ILogCenter LogCenter { get; }

    }

}
