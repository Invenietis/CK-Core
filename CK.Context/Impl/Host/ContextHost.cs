using System;
using CK.Plugin.Config;
using CK.Core;
using System.Diagnostics;
using CK.Plugin.Hosting;
using CK.Plugin;

namespace CK.Context
{
    /// <summary>
    /// Base class for all hosts. It is a thin wrapper around one (and only one) <see cref="IContext"/>.
    /// The <see cref="ContextHost"/> is in charge of providing User and System configuration to the context.
    /// </summary>
    public abstract class ContextHost
    {
        IContext _ctx;
        
        /// <summary>
        /// Initializes a new ContextHost instance. 
        /// Absolutely nothing is done by this constructor: <see cref="CreateContext"/> must be called
        /// in order to obtain a new <see cref="IContext"/>.
        /// </summary>
        public ContextHost()
        {           
        }

        /// <summary>
        /// Gets the <see cref="IContext"/> wrapped by the host.
        /// Null if <see cref="CreateContext"/> has not been called yet.
        /// </summary>
        public IContext Context { get { return _ctx; } }

        /// <summary>
        /// Gets the user configuration.
        /// </summary>
        /// <remarks>
        /// This is a shortcut to the current <see cref="IUserConfiguration.HostConfig"/>.
        /// </remarks>
        public IObjectPluginConfig UserConfig
        {
            get { return _ctx.ConfigManager.Extended.HostUserConfig; }
        }

        /// <summary>
        /// Gets the system configuration.
        /// </summary>
        /// <remarks>
        /// This is a shortcut to the <see cref="ISystemConfiguration.HostConfig"/>.
        /// </remarks>
        public IObjectPluginConfig SystemConfig
        {
            get { return _ctx.ConfigManager.Extended.HostSystemConfig; }
        }

        /// <summary>
        /// Initializes a new <see cref="IContext"/>: one and only one context can be created by a host.
        /// Context events are associated to the abstract methods <see cref="LoadSystemConfig"/>/<see cref="SaveSystemConfig"/>, 
        /// <see cref="LoadUserConfig"/>/<see cref="SaveUserConfig"/> and <see cref="SaveContext"/>: it is up to this host to provide
        /// actual System and User configuration.
        /// </summary>
        /// <returns>A new context.</returns>
        public virtual IContext CreateContext()
        {
            if( Context != null ) throw new InvalidOperationException( R.HostAlreadyOwnsContext );

            _ctx = CK.Context.Context.CreateInstance();

            IConfigManagerExtended cfg = _ctx.ConfigManager.Extended;

            cfg.SaveSystemConfigRequired += ( o, e ) => SaveSystemConfig();
            cfg.SaveUserConfigRequired += ( o, e ) => SaveUserConfig();

            cfg.LoadSystemConfigRequired += ( o, e ) => LoadSystemConfig();
            cfg.LoadUserConfigRequired += ( o, e ) => LoadUserConfig();

            _ctx.SaveContextRequired += ( o, e ) => SaveContext();

            PluginRunner runner = (PluginRunner)_ctx.PluginRunner;
            runner.ServiceHost.DefaultConfiguration.SetAllEventsConfiguration( typeof( IContext ), ServiceLogEventOptions.LogErrors | ServiceLogEventOptions.SilentEventError );
            runner.ServiceHost.ApplyConfiguration();

            return _ctx;
        }

        protected abstract bool LoadSystemConfig();

        public abstract void SaveSystemConfig();

        protected abstract bool LoadUserConfig();

        public abstract void SaveUserConfig();

        public abstract void SaveContext();

    }
}
