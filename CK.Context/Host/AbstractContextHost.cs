#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Host\AbstractContextHost.cs) is part of CiviKey. 
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
using CK.Plugin.Config;
using CK.Core;
using System.Diagnostics;
using CK.Plugin.Hosting;
using CK.Plugin;
using System.IO;
using System.Linq;
using CK.Storage;
using System.Collections.Generic;
using System.Reflection;

namespace CK.Context
{

    public class LoadResult
    {
        ILogCenter _log;
        MethodInfo _loader;

        internal LoadResult( ILogCenter log, MethodInfo loader, bool fileNotFound, IReadOnlyList<ISimpleErrorMessage> warnings )
        {
            Debug.Assert( log != null && loader != null );
            _log = log;
            _loader = loader;
            FileNotFound = fileNotFound;
            Warnings = warnings ?? ReadOnlyListEmpty<ISimpleErrorMessage>.Empty;
        }

        public bool FileNotFound { get; private set; }

        public IReadOnlyList<ISimpleErrorMessage> Warnings { get; private set; }

        public bool Logged { get { return _log == null; } }

        public void Log()
        {
            if( _log == null ) return;
            
            _log = null;
        }

    }

    /// <summary>
    /// Base class for all hosts. It is a thin wrapper around one (and only one) <see cref="IContext"/>.
    /// The <see cref="AbstractContextHost"/> is in charge of providing User/System configuration to the context
    /// through <see cref="GetSystemConfigAddress"/>, <see cref="GetDefaultUserConfigAddress"/> and <see cref="GetDefaultContextProfile"/>
    /// abstract methods.
    /// </summary>
    public abstract class AbstractContextHost
    {
        IContext _ctx;
        
        /// <summary>
        /// Initializes a new ContextHost instance. 
        /// Absolutely nothing is done by this constructor: <see cref="CreateContext"/> must be called
        /// in order to obtain a new <see cref="IContext"/>.
        /// </summary>
        public AbstractContextHost()
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
            if( Context != null ) throw new InvalidOperationException( Res.R.HostAlreadyOwnsContext );

            _ctx = CK.Context.Context.CreateInstance();

            IConfigManagerExtended cfg = _ctx.ConfigManager.Extended;
            cfg.LoadSystemConfigRequired += ( o, e ) => LoadSystemConfig();
            cfg.LoadUserConfigRequired += ( o, e ) => LoadUserConfig();

            ISimplePluginRunner runner = _ctx.PluginRunner;
            runner.ServiceHost.DefaultConfiguration.SetAllEventsConfiguration( typeof( IContext ), ServiceLogEventOptions.LogErrors | ServiceLogEventOptions.SilentEventError );
            runner.ServiceHost.ApplyConfiguration();

            return _ctx;
        }

        public abstract Uri GetSystemConfigAddress();

        /// <summary>
        /// This method is called by <see cref="EnsureCurrentUserProfile"/> whenever the host needs an address to store the user configuration
        /// and there is no already registered user profile in the system configuration.
        /// </summary>
        /// <param name="saving">True if we are saving the configuration. False if we are loading it.</param>
        /// <returns>The address for the user configuration.</returns>
        protected abstract Uri GetDefaultUserConfigAddress( bool saving );
        
        /// <summary>
        /// This method is called by <see cref="EnsureCurrentContextProfile"/> whenever the host needs an address to store the current context
        /// and there is no already registered context in the user profile.
        /// </summary>
        /// <param name="saving">True if we are saving the current context. False if we are loading it.</param>
        /// <returns>The display name and address for the context.</returns>
        protected abstract KeyValuePair<string, Uri> GetDefaultContextProfile( bool saving );

        #region System Management

        public virtual LoadResult LoadSystemConfig()
        {
            return DoRead( _ctx.ConfigManager.Extended.LoadSystemConfig, GetSystemConfigAddress() );
        }

        public virtual void SaveSystemConfig()
        {
            using( var sw = OpenWrite( GetSystemConfigAddress() ) )
            {
                _ctx.ConfigManager.Extended.SaveSystemConfig( sw );
            }
        }

        #endregion

        #region UserProfile Management.

        /// <summary>
        /// Finds or creates a <see cref="IUriHistory"/> for the current user profile.
        /// It is first the <see cref="ISystemConfiguration.CurrentUserProfile"/>, if it is null we take the first profile 
        /// among <see cref="ISystemConfiguration.UserProfiles"/> that is a file (disk-based), if there is no such registered profile,
        /// we take the first one (whathever kind of uri it is). Finally, if there is no profile at all, we create a new one
        /// with the user name and the return of the virtual <see cref="GetDefaultUserConfigAddress"/> method.
        /// </summary>
        /// <param name="saving">True if we are saving the configuration. False if we are loading it.</param>
        /// <returns>A <see cref="IUriHistory"/> that is the current user profile.</returns>
        public virtual IUriHistory EnsureCurrentUserProfile( bool saving )
        {
            IUriHistory currentProfile = _ctx.ConfigManager.SystemConfiguration.CurrentUserProfile;
            if( currentProfile == null ) currentProfile = _ctx.ConfigManager.SystemConfiguration.UserProfiles.FirstOrDefault( p => p.Address.IsFile );
            if( currentProfile == null ) currentProfile = _ctx.ConfigManager.SystemConfiguration.UserProfiles.FirstOrDefault();
            if( currentProfile == null )
            {
                currentProfile = _ctx.ConfigManager.SystemConfiguration.UserProfiles.FindOrCreate( GetDefaultUserConfigAddress( saving ) );
                currentProfile.DisplayName = Environment.UserName;
            }
            return currentProfile;
        }

        public virtual LoadResult LoadUserConfig()
        {
            return LoadUserConfig( EnsureCurrentUserProfile( false ).Address );
        }

        public virtual LoadResult LoadUserConfig( Uri address )
        {
            LoadResult r = DoRead( _ctx.ConfigManager.Extended.LoadUserConfig, address );
            if( !r.FileNotFound )
            {
                _ctx.ConfigManager.SystemConfiguration.CurrentUserProfile = _ctx.ConfigManager.SystemConfiguration.UserProfiles.FindOrCreate( address );
            }
            return r;
        }

        public virtual void SaveUserConfig()
        {
            SaveUserConfig( EnsureCurrentUserProfile( true ).Address, true );
        }

        public virtual void SaveUserConfig( Uri address, bool setAdressAsCurrent )
        {
            using( IStructuredWriter sw = OpenWrite( address ) )
            {
                _ctx.ConfigManager.Extended.SaveUserConfig( sw );
                if( setAdressAsCurrent )
                    _ctx.ConfigManager.SystemConfiguration.CurrentUserProfile = _ctx.ConfigManager.SystemConfiguration.UserProfiles.FindOrCreate( address );
            }
        }

        #endregion

        #region ContextProfile Management.

        public virtual IUriHistory EnsureCurrentContextProfile( bool saving )
        {
            IUriHistory currentProfile = _ctx.ConfigManager.UserConfiguration.CurrentContextProfile;
            if( currentProfile == null ) currentProfile = _ctx.ConfigManager.UserConfiguration.ContextProfiles.FirstOrDefault( p => p.Address.IsFile );
            if( currentProfile == null ) currentProfile = _ctx.ConfigManager.UserConfiguration.ContextProfiles.FirstOrDefault();
            if( currentProfile == null )
            {
                var p = GetDefaultContextProfile( saving );
                currentProfile = _ctx.ConfigManager.UserConfiguration.ContextProfiles.Find( p.Value );
                if( currentProfile == null )
                {
                    currentProfile = _ctx.ConfigManager.UserConfiguration.ContextProfiles.FindOrCreate( p.Value );
                    currentProfile.DisplayName = p.Key;
                }
            }
            return currentProfile;
        }

        public virtual LoadResult LoadContext()
        {
            return LoadContext( EnsureCurrentContextProfile( false ).Address, null );
        }

        public virtual LoadResult LoadContext( Assembly fallbackResourceAssembly, string fallbackResourcePath )
        {
            return LoadContext( EnsureCurrentContextProfile( false ).Address, fallbackResourceAssembly, fallbackResourcePath );
        }

        public virtual LoadResult LoadContext( Uri address, Assembly fallbackResourceAssembly = null, string fallbackResourcePath = null )
        {
            LoadResult r = DoRead( _ctx.LoadContext, address );
            if( !r.FileNotFound )
            {
                _ctx.ConfigManager.UserConfiguration.CurrentContextProfile = _ctx.ConfigManager.UserConfiguration.ContextProfiles.FindOrCreate( address );
            }
            else if( !String.IsNullOrEmpty( fallbackResourcePath ) )
            {
                using( Stream str = fallbackResourceAssembly.GetManifestResourceStream( fallbackResourcePath ) )
                {
                    if( str != null )
                    {
                        using( IStructuredReader sr = SimpleStructuredReader.CreateReader( str, Context ) )
                        {
                            r = DoRead( _ctx.LoadContext, sr );
                        }
                    }
                }
            }
            return r;
        }

        public void SaveContext()
        {
            SaveContext( EnsureCurrentContextProfile( true ).Address );
        }

        public void SaveContext( Uri address )
        {
            using( IStructuredWriter sw = OpenWrite( address ) )
            {
                _ctx.SaveContext( sw );
                _ctx.ConfigManager.UserConfiguration.CurrentContextProfile = _ctx.ConfigManager.UserConfiguration.ContextProfiles.FindOrCreate( address );
            }
        }

        #endregion

        protected virtual IStructuredWriter OpenWrite( Uri u )
        {
            if( u == null ) throw new ArgumentNullException( "u" );
         
            if( !u.IsFile ) throw new ArgumentException( "Only file:// protocol is currently supported." );
            string path = u.LocalPath;
            return SimpleStructuredWriter.CreateWriter( new FileStream( path, FileMode.Create ), _ctx );
        }

        protected virtual IStructuredReader OpenRead( Uri u, bool throwIfMissing )
        {
            if( u == null ) throw new ArgumentNullException( "u" );
            
            if( !u.IsFile ) throw new ArgumentException( "Only file:// protocol is currently supported." );
            string path = u.LocalPath;
            return SimpleStructuredReader.CreateReader( File.Exists( path ) ? new FileStream( path, FileMode.Open ) : null, _ctx, throwIfMissing );
        }

        protected virtual LoadResult DoRead( Func<IStructuredReader,IReadOnlyList<ISimpleErrorMessage>> reader, Uri u )
        {
            if( reader == null ) throw new ArgumentNullException( "reader" );           
            if( u == null ) throw new ArgumentNullException( "u" );

            using( var sr = OpenRead( u, false ) )
            {
                if( sr == null ) return new LoadResult( _ctx.LogCenter, reader.Method, true, null );
                return DoRead( reader, sr );
            }
        }

        protected virtual LoadResult DoRead( Func<IStructuredReader, IReadOnlyList<ISimpleErrorMessage>> reader, IStructuredReader structuredReader )
        {
            if( reader == null ) throw new ArgumentNullException( "reader" );
            if( structuredReader == null ) throw new ArgumentNullException( "structuredReader" );

            return new LoadResult( _ctx.LogCenter, reader.Method, false, reader( structuredReader ) );
        }

    }
}
