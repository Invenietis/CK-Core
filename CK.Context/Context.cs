#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
using System.Xml;

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
                    if( serviceType == typeof( ISimplePluginRunner ) || serviceType == typeof( PluginRunner ) ) return _c._pluginRunner;
                    if( serviceType == typeof( IConfigContainer ) ) return _c._dic;
                    result = _c._pluginRunner.ServiceHost.GetProxy( serviceType );
                }
                return result;
            }
        }

        ContextServiceContainer _serviceContainer;
        RequirementLayer _reqLayer;
        PluginRunner _pluginRunner;
        IContext _proxifiedContext;
        
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
            _reqLayer = new RequirementLayer( "Context" );

            _pluginRunner = new PluginRunner( _serviceContainer, _configManager.ConfigManager );
            _serviceContainer.Add( RequirementLayerSerializer.Instance );
            _serviceContainer.Add( SimpleTypeFinder.Default );

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

        public event EventHandler<ApplicationExitingEventArgs> ApplicationExiting;
        public event EventHandler<ApplicationExitedEventArgs> ApplicationExited;

        public bool RaiseExitApplication( bool hostShouldExit )
        {
            var e = new ApplicationExitingEventArgs( this, hostShouldExit );
            var before = ApplicationExiting;
            if( before != null ) before( this, e );
            if( e.Cancel ) return false;

            _pluginRunner.Disabled = true;
            _pluginRunner.Apply();

            var after = ApplicationExited;
            if( after != null ) after( this, e );
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

        public void SaveContext( IStructuredWriter writer )
        {
            if( writer == null ) throw new ArgumentNullException( "writer" );

            using( ISharedDictionaryWriter dw = _dic.RegisterWriter( writer ) )
            {
                XmlWriter w = writer.Xml;
                w.WriteStartElement( "CKContext" );
                writer.WriteInlineObjectStructuredElement( "RequirementLayer", _reqLayer );
                dw.WritePluginsDataElement( "PluginData", _proxifiedContext );
                w.WriteEndElement();
            }
        }

        public IReadOnlyList<ISimpleErrorMessage> LoadContext( IStructuredReader reader )
        {
            if( reader == null ) throw new ArgumentNullException( "reader" );
            IList<ReadElementObjectInfo> errors;
            using( ISharedDictionaryReader dr = _dic.RegisterReader( reader, MergeMode.None ) )
            {
                XmlReader r = reader.Xml;
                r.ReadStartElement( "CKContext" );
                reader.ReadInlineObjectStructuredElement( "RequirementLayer", _reqLayer );
                dr.ReadPluginsDataElement( "PluginData", _proxifiedContext );
                r.ReadEndElement();
                errors = dr.ErrorCollector;
            }
            return errors.ToReadOnlyList();
        }

    }
}
