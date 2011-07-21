using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Plugin;
using CK.SharedDic;
using CK.Plugin.Hosting;
using CK.Core;
using CK.Storage;

namespace CK.Context
{
    public partial class Context : IContext
    {
        ISharedDictionary _dic;
        IConfigManagerExtended _configManager;

        class ContextServiceContainer : SimpleServiceContainer
        {
            Context _c;

            public ContextServiceContainer( Context c )
            {
                _c = c;
            }

            protected override object GetDirectService( Type serviceType )
            {
                object result = base.GetDirectService( serviceType );
                if( result == null )
                {
                    if( serviceType == typeof( IConfigManager ) || serviceType == typeof( IConfigManagerExtended ) ) return _c._configManager;
                    if( serviceType == typeof( ISimplePluginRunner ) || serviceType == typeof( PluginRunner ) ) return _c._pluginRunner;
                    if( serviceType == typeof( IConfigContainer ) ) return _c._dic;
                    if( serviceType == typeof( IStructuredSerializer<RequirementLayer> ) ) return RequirementLayerSerializer.Instance;
                    result = _c._pluginRunner.ServiceHost.GetProxy( serviceType );
                }
                return result;
            }
        }

        ContextServiceContainer _serviceContainer;

        RequirementLayer _reqLayer;
        PluginRunner _pluginRunner;
        IContext _proxifiedContext;
        
        bool _isContextConfigDirty;

        /// <summary>
        /// Initializes a new context that is proxified by default.
        /// </summary>
        public static IContext CreateInstance()
        {
            return CreateInstance( true );
        }

        /// <summary>
        /// Initializes a new context.
        /// </summary>
        public static IContext CreateInstance( bool proxified )
        {
            return new Context( proxified ).ProxifiedContext;
        }

        private Context( bool proxified )
        {
            _serviceContainer = new ContextServiceContainer( this );
            _dic = SharedDictionary.Create( _serviceContainer );
            _configManager = ConfigurationManager.Create( _dic );
            _dic.Changed += ( o, e ) => _isContextConfigDirty = true;

            _reqLayer = new RequirementLayer( "Context" );

            _pluginRunner = new PluginRunner( _serviceContainer, _configManager.ConfigManager );
            _serviceContainer.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.Default );

            if( proxified )
            {
                _proxifiedContext = (IContext)_pluginRunner.ServiceHost.InjectExternalService( typeof( IContext ), this );
            }
            else
            {
                _proxifiedContext = this;
                _serviceContainer.Add<IContext>( this );
            }
            _pluginRunner.Initialize( _proxifiedContext );
       }

        public IContext ProxifiedContext
        {
            get { return _proxifiedContext; }
        }

        public bool IsProxified
        {
            get { return !ReferenceEquals( _proxifiedContext, this ); }
        }

        public IServiceProvider BaseServiceProvider
        {
            get { return _serviceContainer.BaseProvider; }
            set { _serviceContainer.BaseProvider = value; }
        }
        
        public ISimpleServiceContainer ServiceContainer { get { return _serviceContainer; } }

        public object GetService( Type serviceType )
        {
            return _serviceContainer.GetService( serviceType );
        }

        public event EventHandler<ApplicationExitingEventArgs> BeforeExitApplication;
        public event EventHandler<ApplicationExitEventArgs> OnExitApplication;

        public event EventHandler SaveContextRequired;

        public event EventHandler Loading;
        public event EventHandler Loaded;

        /// <summary>
        /// Raises the <see cref="OnExitApplication"/> event after having stoppped all the plugins and 
        /// automatically saved configurations if needed.
        /// </summary>
        public bool RaiseExitApplication( bool hostShouldExit )
        {
            var exiting = new ApplicationExitingEventArgs( this, hostShouldExit );
            if( BeforeExitApplication != null ) BeforeExitApplication( this, exiting );
            if( exiting.Cancel ) return false;

            _pluginRunner.Disabled = true;
            _pluginRunner.Apply();

            if( OnExitApplication != null ) OnExitApplication( this, new ApplicationExitEventArgs( this, hostShouldExit ) );
            return true;
        }

        public ILogCenter LogCenter
        {
            get { return _pluginRunner.LogCenter; }
        }

        IConfigManager IContext.ConfigManager
        {
            get { return _configManager.ConfigManager; }
        }

        internal IConfigManagerExtended ConfigManager
        {
            get { return _configManager; }
        }

        ISimplePluginRunner IContext.PluginRunner
        {
            get { return _pluginRunner; }
        }

        internal PluginRunner PluginRunner
        {
            get { return _pluginRunner; }
        }

        public RequirementLayer RequirementLayer
        {
            get { return _reqLayer; }
        }
    }
}
