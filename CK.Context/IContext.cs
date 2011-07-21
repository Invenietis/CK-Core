using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Plugin;
using CK.Storage;
using CK.Core;
using CK.Plugin.Hosting;
using Common.Logging;

namespace CK.Context
{
    public interface IContext : IServiceProvider
    {
        /// <summary>
        /// Gets the <see cref="IConfigManager"/> that can be used to read and save the configuration.
        /// </summary>
        IConfigManager ConfigManager { get; }

        /// <summary>
        /// Gets or sets a fallback <see cref="IServiceProvider"/> that will be queried for a service
        /// if it is not found at this level.
        /// </summary>
        IServiceProvider BaseServiceProvider { get; set; }

        /// <summary>
        /// Gets the service container for this context.
        /// </summary>
        ISimpleServiceContainer ServiceContainer { get; }

        /// <summary>
        /// Gets the <see cref="RequirementLayer"/> of this context.
        /// </summary>
        RequirementLayer RequirementLayer { get; }

        /// <summary>
        /// Gets the <see cref="ISimplePluginRunner"/>
        /// </summary>
        ISimplePluginRunner PluginRunner { get; }

        /// <summary>
        /// Gets the <see cref="ILogCenter"/>
        /// </summary>
        ILogCenter LogCenter { get; }

        /// <summary>
        /// Fires whenever a new context is about to be loaded: the content of this context will be replaced.
        /// </summary>
        event EventHandler Loading;

        /// <summary>
        /// Fires when a new context has been loaded: the content of this context has been replaced.
        /// </summary>
        event EventHandler Loaded;

        /// <summary>
        /// Fires when the context has to be saved.
        /// Host should call <see cref="SaveContext"/>.
        /// </summary>
        event EventHandler SaveContextRequired;

        /// <summary>
        /// Writes this <see cref="IContext">context</see> as in a stream.
        /// </summary>
        /// <param name="stream">Stream to the saved document.</param>
        void SaveContext( IStructuredWriter writer );

        /// <summary>
        /// Loads this <see cref="IContext">context</see> from a file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>
        /// True if the context has been succesfully loaded. 
        /// False if an <see cref="DisplayError"/> has been raised and no context can be loaded from the path.
        /// </returns>
        bool LoadContext( IStructuredReader reader );

        /// <summary>
        /// Fired by <see cref="RaiseExitApplication"/> to signal the end of the application.
        /// After this event, configuration is saved if needed and then <see cref="OnExitApplication"/>
        /// event is fired.
        /// </summary>
        event EventHandler<ApplicationExitingEventArgs> BeforeExitApplication;

        /// <summary>
        /// Fired by <see cref="RaiseExitApplication"/> to signal the very end of the application. 
        /// Once this event has fired, this <see cref="IContext"/> is no more functionnal.
        /// </summary>
        event EventHandler<ApplicationExitEventArgs> OnExitApplication;

        /// <summary>
        /// Raises the <see cref="BeforeExitApplication"/> (any persistence of information/configuration should be done), 
        /// and <see cref="OnExitApplication"/> event.
        /// </summary>
        /// <param name="hostShouldExit">When true, the application host should exit: this is typically used by a plugin to
        /// trigger the end of the current application (ie. the "Exit" button). 
        /// A host would better use false to warn any services and plugins to do what they have to do before leaving and manage to exit the way it wants.</param>
        /// <returns>False if the <see cref="BeforeExitApplication"/> event has been canceled, true otherwise.</returns>
        bool RaiseExitApplication( bool hostShouldExit );
    }
}
