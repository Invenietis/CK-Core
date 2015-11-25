#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutput.DefaultConfig.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;
using System.Threading;

namespace CK.Monitoring
{
    public sealed partial class GrandOutput
    {
        static object _watcherLock = new object();
        static string _configPath = null;
        static DateTime _lastConfigFileWriteTime = FileUtil.MissingFileLastWriteTimeUtc;
        static LogFilter _fileWatcherMonitoringMinimalFilter = LogFilter.Release;

        /// <summary>
        /// Gets or sets the minimal filter that monitors created for the 
        /// GrandOutput itself will use.
        /// Defaults to <see cref="LogFilter.Release"/> (this should be changed only for debugging reasons).
        /// Caution: this applies only to the current AppDomain!
        /// </summary>
        static public LogFilter GrandOutputMinimalFilter
        {
            get { return _fileWatcherMonitoringMinimalFilter; }
            set { _fileWatcherMonitoringMinimalFilter = value; }
        }

        /// <summary>
        /// Ensures that the <see cref="Default"/> GrandOutput is created (see <see cref="EnsureActiveDefault"/>) and configured with default settings:
        /// only one one channel with its minimal filter sets to Debug with one file handler that writes .ckmon files in "<see cref="SystemActivityMonitor.RootLogPath"/>\GrandOutputDefault" directory.
        /// The <see cref="SystemActivityMonitor.RootLogPath"/> must be valid and if a GrandOutput.config file exists inside, it is loaded as the configuration.
        /// If it exists, it must be valid (otherwise an exception is thrown).
        /// Once loaded, the file is monitored and any change that occurs to it dynamically triggers a <see cref="SetConfiguration"/> with the new file.
        /// </summary>
        /// <param name="monitor">An optional monitor.</param>
        static public GrandOutput EnsureActiveDefaultWithDefaultSettings( IActivityMonitor monitor = null )
        {
            lock( _defaultLock )
            {
                if( _default == null )
                {
                    if( monitor == null ) monitor = new SystemActivityMonitor( true, "GrandOutput" ) { MinimalFilter = GrandOutputMinimalFilter };
                    using( monitor.OpenInfo().Send( "Attempting Default GrandOutput configuration." ) )
                    {
                        try
                        {
                            SystemActivityMonitor.AssertRootLogPathIsSet();
                            _configPath = SystemActivityMonitor.RootLogPath + "GrandOutput.config";
                            GrandOutputConfiguration def = CreateDefaultConfig();
                            if( !File.Exists( _configPath ) )
                            {
                                File.WriteAllText( _configPath, _defaultConfig );
                            }
                            if( !def.LoadFromFile( _configPath, monitor ) )
                            {
                                throw new CKException( "Unable to load Configuration file: '{0}'.", _configPath );
                            }
                            GrandOutput output = new GrandOutput();
                            if( !output.SetConfiguration( def, monitor ) )
                            {
                                throw new CKException( "Failed to set Configuration." );
                            }
                            StartMonitoring( monitor );
                            _default = output;
                            ActivityMonitor.AutoConfiguration += m => _default.Register( m );
                        }
                        catch( Exception ex )
                        {
                            monitor.Fatal().Send( ex );
                            throw;
                        }
                    }
                }
            }
            return _default;
        }

        const string _defaultConfig = 
@"<GrandOutputConfiguration>
    <Channel MinimalFilter=""Debug"">
        <Add Type=""BinaryFile"" Name=""All"" Path=""GrandOutputDefault"" MaxCountPerFile=""20000"" />
    </Channel>
</GrandOutputConfiguration>";

        static GrandOutputConfiguration CreateDefaultConfig()
        {
            GrandOutputConfiguration def = new GrandOutputConfiguration();
            Debug.Assert( def.SourceOverrideFilterApplicationMode == SourceFilterApplyMode.None );
            Debug.Assert( def.AppDomainDefaultFilter == null );
            var route = new RouteConfiguration();
            route.ConfigData = new GrandOutputChannelConfigData() { MinimalFilter = LogFilter.Debug };
            route.AddAction( new BinaryFileConfiguration( "All" ) { Path = "GrandOutputDefault" } );
            def.ChannelsConfiguration = route;
            return def;
        }

        static void StartMonitoring( IActivityMonitor monitor )
        {
            if( _watcher != null ) _watcher.Dispose();
            _watcher = new FileSystemWatcher();
            _watcher.Path = SystemActivityMonitor.RootLogPath;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Filter = "GrandOutput.config";
            _watcher.Changed += _watcher_Changed;
            _watcher.Deleted += _watcher_Changed;
            _watcher.Renamed += _watcher_Changed;
            _watcher.Error += _watcher_Error;
            _watcher.EnableRaisingEvents = true;
        }

        static void _watcher_Error( object sender, ErrorEventArgs e )
        {
            ActivityMonitor.CriticalErrorCollector.Add( e.GetException(), String.Format( "While monitoring GrandOutput.Default configuration file '{0}'.", _watcher.Path ) );
        }

        static void _watcher_Changed( object sender, FileSystemEventArgs unusedEventArgs )
        {
            if( _watcher == null ) return;
            ThreadPool.UnsafeQueueUserWorkItem( Reload, null );
        }
        static void Reload( object state )
        {
            if( _watcher == null ) return;
            // Quick and dirty trick to handle Renamed events
            // or too quick events.
            Thread.Sleep( 100 );
            var time = File.GetLastWriteTimeUtc( _configPath );
            if( time != _lastConfigFileWriteTime )
            {
                GrandOutputConfiguration def;
                lock( _watcherLock )
                {
                    time = File.GetLastWriteTimeUtc( _configPath );
                    if( time == _lastConfigFileWriteTime ) return;
                    _lastConfigFileWriteTime = time;
                }
                var monitor = new SystemActivityMonitor( true, "GrandOutput.Default.Reconfiguration" ) { MinimalFilter = GrandOutputMinimalFilter };
                try
                {
                    using( monitor.OpenInfo().Send( "AppDomain '{0}',  file '{1}' changed (change n°{2}).", AppDomain.CurrentDomain.FriendlyName, _configPath, _default.ConfigurationAttemptCount ) )
                    {
                        def = CreateDefaultConfig();
                        if( File.Exists( _configPath ) )
                        {
                            if( time == FileUtil.MissingFileLastWriteTimeUtc ) _lastConfigFileWriteTime = File.GetLastWriteTimeUtc( _configPath );
                            def.LoadFromFile( _configPath, monitor );
                        }
                        else
                        {
                            _lastConfigFileWriteTime = FileUtil.MissingFileLastWriteTimeUtc;
                            monitor.Trace().Send( "File missing: applying catch-all default configuration." );
                        }
                        if( !_default._channelHost.IsDisposed ) _default.SetConfiguration( def, monitor );
                    }
                }
                catch( Exception ex )
                {
                    monitor.Error().Send( ex );
                }
            }
        }
    }
}
