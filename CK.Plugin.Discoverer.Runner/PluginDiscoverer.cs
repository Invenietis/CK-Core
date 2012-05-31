#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\PluginDiscoverer.cs) is part of CiviKey. 
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
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using CK.Core;
using System.Configuration;
using System.Collections;

namespace CK.Plugin.Discoverer.Runner
{
    public sealed class PluginDiscoverer : MarshalByRefObject
	{
        //Collections used to insert items and to be converted 
        //into an IReadOnlyList after (for properties)
        //Assemblies
        Dictionary<string,PluginAssemblyInfo> _filesProcessed;
        Dictionary<string,PluginAssemblyInfo> _assembliesByName;
        //Plugins
        IDictionary<Guid, PluginInfo> _pluginsById;
        IList<PluginInfo> _existingPlugins;
        IList<PluginInfo> _oldPlugins;
        //Services
        IDictionary<string,ServiceInfo> _dicAllServices;
        IList<ServiceInfo> _services;
        IList<ServiceInfo> _notFoundServices;

		// Dynamic state : valid only while discovering.
		List<FileInfo> _currentFiles;

		public PluginDiscoverer()
		{
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ( o, args ) => { return Assembly.ReflectionOnlyLoad( args.Name ); };

			_filesProcessed = new Dictionary<string, PluginAssemblyInfo>();
            _assembliesByName = new Dictionary<string, PluginAssemblyInfo>();
            _pluginsById = new Dictionary<Guid, PluginInfo>();
            _existingPlugins = new List<PluginInfo>();
            _oldPlugins = new List<PluginInfo>();
            _dicAllServices = new Dictionary<string, ServiceInfo>();
            _notFoundServices = new List<ServiceInfo>();
            _services = new List<ServiceInfo>();
			_currentFiles = new List<FileInfo>();
        }

		public RunnerDataHolder Discover( DirectoryInfo dir, Predicate<FileInfo> filter )
		{
            List<FileInfo> files = new List<FileInfo>();
            foreach( FileInfo f in dir.GetFiles( "*.dll" ) )
                if( filter( f ) ) files.Add( f );
            return Discover( files );
		}

        /// <summary>
        /// For each FileInfo in _currentFiles, Process will try to create a new PluginAssemblyInfo
        /// based on the FileInfo (that is currently processed).
        /// After that, this method fill properties collections with IPluginInfo or IServiceInfo.
        /// </summary>
        public RunnerDataHolder Discover( IEnumerable<FileInfo> files )
		{
            // Transforms FileInfo into PluginAssemblyInfo.
            foreach( FileInfo f in files )
            {
                PluginAssemblyInfo a;
                string fName = f.FullName;
                if( !_filesProcessed.TryGetValue( fName, out a ) )
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.ReflectionOnlyLoadFrom( fName );
                        a = new PluginAssemblyInfo( assembly, f );
                        _assembliesByName.Add( assembly.FullName, a );
                    }
                    catch( Exception ex )
                    {
                        a = new PluginAssemblyInfo( fName );
                        a.AddErrorLine( ex.Message );
                    }
                    _filesProcessed.Add( fName, a );
                }
            }
            foreach( var e in _assembliesByName )
            {
                e.Value.LoadDependencies();
            }
            foreach( var e in _assembliesByName )
            {
                PluginAssemblyInfo a = e.Value;
                if( !a.HasError )
                {
                    Debug.Assert( a.ErrorMessage == null );
                    try
                    {
                        a.ReadAllTypes( RegisterServiceInfo, RegisterUseServiceInfo );
                    }
                    catch( Exception ex )
                    {
                        a.AddErrorLine( ex.Message );
                    }
                }
            }
            
            // Consider DynamicService without any definition assembly as an error.
            foreach( ServiceInfo serviceInfo in _dicAllServices.Values )
            {
                if( serviceInfo.IsDynamicService && serviceInfo.AssemblyInfo == null )
                {
                    serviceInfo.AddErrorLine( R.AssemblyNotFoundForDynamicService );
                }
            }

            // Fills _existingPlugins, updates _pluginsById (keep only the best version), removes old plugins from Assembly.Plugins collection.
            foreach( var e in _assembliesByName )
            {
                PluginAssemblyInfo a = e.Value;
                if( !a.HasError )
                {
                    List<PluginInfo> toRemove = new List<PluginInfo>();
                    foreach( PluginInfo pluginInfo in a.Plugins )
                    {
                        // Transfer any pluginInfo.Service.HasError to pluginInfo.HasError.
                        if( pluginInfo.Service != null && pluginInfo.Service.HasError )
                        {
                            pluginInfo.AddErrorLine( R.ImplementedServiceIsOnError );
                        }
                        // Errors on the ServiceReferences are propagated up to the plugin object
                        // only if the reference "Must Exist": references that are optional do not transfer errors.
                        foreach( var i in pluginInfo.ServiceReferences )
                        {
                            if( i.HasError && i.Requirements > RunningRequirement.OptionalTryStart )
                            {
                                pluginInfo.AddErrorLine( String.Format( "Dependency {0} is on error: {1}", i.PropertyName, i.ErrorMessage ) );
                            }
                        }

                        _existingPlugins.Add( pluginInfo );
                        PluginInfo dicPlugin;
                        if( _pluginsById.TryGetValue( pluginInfo.PluginId, out dicPlugin ) )
                        {
                            if( dicPlugin.Version >= pluginInfo.Version )
                            {
                                pluginInfo.IsOldVersion = true;
                                if( pluginInfo.Service != null )
                                    pluginInfo.Service.Implementations.Remove( pluginInfo );
                                _oldPlugins.Add( pluginInfo );
                                toRemove.Add( pluginInfo );
                            }
                            else if( dicPlugin.Version < pluginInfo.Version )
                            {
                                _pluginsById.Remove( dicPlugin.PluginId );
                                _pluginsById.Add( pluginInfo.PluginId, pluginInfo );
                                dicPlugin.IsOldVersion = true;
                                if( dicPlugin.Service != null )
                                    dicPlugin.Service.Implementations.Remove( dicPlugin );
                                _oldPlugins.Add( dicPlugin );
                                toRemove.Add( dicPlugin );
                            }
                        }
                        else
                        {
                            _pluginsById.Add( pluginInfo.PluginId, pluginInfo );
                        }
                    }
                    // We remove old plugins from assemblyInfo
                    foreach( PluginInfo oldPlugin in toRemove )
                        a.Plugins.Remove( oldPlugin );
                }
            }
            // Then we put the editors into edited plugins.
            foreach( PluginInfo plugin in _pluginsById.Values )
            {
                foreach( PluginConfigAccessorInfo editor in plugin.EditorsInfo )
                {
                    PluginInfo editedPlugin;
                    if( _pluginsById.TryGetValue( editor.Source, out editedPlugin ) )
                        editedPlugin.EditableBy.Add( editor );
                }
            }

            foreach( PluginAssemblyInfo assembly in _filesProcessed.Values ) assembly.NormalizeCollections();
            foreach( PluginInfo plugin in _oldPlugins ) plugin.NormalizeCollections();
            foreach( PluginInfo plugin in _existingPlugins ) plugin.NormalizeCollections();
            foreach( ServiceInfo service in _dicAllServices.Values ) service.NormalizeCollections();

            PluginAssemblyInfo[] allAssemblies = _filesProcessed.Values.ToArray();
            Array.Sort( allAssemblies );
            PluginInfo[] oldPlugins = _oldPlugins.ToArray();
            Array.Sort( oldPlugins );
            ServiceInfo[] notFoundServices = _notFoundServices.ToArray();
            Array.Sort( notFoundServices );

            return new RunnerDataHolder(
                allAssemblies, 
                oldPlugins, 
                notFoundServices 
            );
		}

        // When a IServiceInfo (ex : interface ICommonTimer) is found in an assembly.
        ServiceInfo RegisterServiceInfo( PluginAssemblyInfo a, Type t )
        {
            string assemblyQualifiedName = t.AssemblyQualifiedName;
            ServiceInfo serv;
            if( !_dicAllServices.TryGetValue( assemblyQualifiedName, out serv ) )
            {
                serv = new ServiceInfo( a, t );
                _dicAllServices.Add( assemblyQualifiedName, serv );
                _services.Add( serv );
            }
            else if( serv.AssemblyInfo == null )
            {
                serv.AssemblyInfo = a;
                _services.Add( serv );
                _notFoundServices.Remove( serv );
            }
            return serv;
        }

        /// <summary>
        /// Called when a reference to a service is found (ex: CommonTimerWindow needs ICommonTimer service).
        /// The resulting <see cref="ServiceRefInfo.Reference"/> is null 
        /// (if the type is a generic interface other than IService&lt;&gt;)
        /// OR
        /// (the type is not a IDynamicService and registerOnlyIDynamicService is true)
        /// </summary>
        ServiceRefInfo RegisterUseServiceInfo( Type propertyType, bool registerOnlyIDynamicService )
        {
            Debug.Assert( propertyType.IsInterface );
            bool isIDynamicService = false;
            bool isGeneric = false;
            bool isIServiceWrapper = false;
            ServiceInfo serv;
            if( propertyType.IsGenericType )
            {
                isGeneric = true;
                if( propertyType.GetGenericTypeDefinition().AssemblyQualifiedName == typeof( IService<> ).AssemblyQualifiedName )
                {
                    Type serviceType = propertyType.GetGenericArguments()[0];
                    Debug.Assert( IsIDynamicService( serviceType ), "Since IService<T> where T : IDynamicService." );
                    isIServiceWrapper = isIDynamicService = true;
                    serv = EnsureNotFoundService( serviceType );
                }
                else serv = null;
            }
            else
            {
                isIDynamicService = IsIDynamicService( propertyType );
                if( !registerOnlyIDynamicService || isIDynamicService )
                {
                    serv = EnsureNotFoundService( propertyType );
                }
                else serv = null;
            }
            return new ServiceRefInfo( serv, isIServiceWrapper, isGeneric, isIDynamicService );
        }
       
        private ServiceInfo EnsureNotFoundService( Type t )
        {
            ServiceInfo serv;
            if( !_dicAllServices.TryGetValue( t.AssemblyQualifiedName, out serv ) )
            {
                serv = new ServiceInfo( null, t );
                _dicAllServices.Add( t.AssemblyQualifiedName, serv );
                _notFoundServices.Add( serv );
            }
            return serv;
        }

        /// <summary>
        /// The Type.IsAssignableFrom method can not work between types loaded in ReflectionOnly and types loaded normally.
        /// Since we are referencing CK.Plugin.Model assembly, its types are not loaded in the ReflectionOnly space.
        /// This methods returns false if the type is the IDynamicService itself.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static internal bool IsIDynamicService( Type t )
        {
            return t.AssemblyQualifiedName != typeof( IDynamicService ).AssemblyQualifiedName && t.GetInterface( typeof( IDynamicService ).FullName ) != null;
        }
    }
}
