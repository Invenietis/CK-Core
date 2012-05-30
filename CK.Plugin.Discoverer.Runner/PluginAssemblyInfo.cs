#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\PluginAssemblyInfo.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using CK.Core;
using CK.Plugin.Config;

namespace CK.Plugin.Discoverer.Runner
{
    [Serializable]
    public sealed class PluginAssemblyInfo : DiscoveredInfo, IComparable<PluginAssemblyInfo>
    {
        readonly string _fileName;
        readonly int _fileSize;
        readonly AssemblyName _assemblyName;
        List<PluginInfo> _plugins;
        List<ServiceInfo> _services;
        [NonSerialized]
        readonly Assembly  _assembly;

        #region Properties

        public string AssemblyFileName 
		{
			get { return _fileName; } 
		}

        public int AssemblyFileSize
        {
            get { return _fileSize; }
        }

        public AssemblyName AssemblyName 
		{
            get { return _assemblyName; } 
		}

		public bool HasPluginsOrServices
		{
			get { return _plugins != null && (_plugins.Any( p => !p.HasError ) || _services.Any( s => s.HasError )); } 
		}

		public List<PluginInfo> Plugins 
		{
			get { return _plugins; } 
		}

		public List<ServiceInfo> Services
		{
            get { return _services; }
        }

        #endregion

        internal PluginAssemblyInfo( Assembly a, FileInfo f )
        {
            _fileName = f.FullName;
            _assemblyName = a.GetName();
            _fileSize = (int)f.Length;
            _assembly = a;
            _plugins = new List<PluginInfo>();
            _services = new List<ServiceInfo>();
        }

        internal PluginAssemblyInfo( string fName )
        {
            _fileName = fName;
            _plugins = new List<PluginInfo>();
            _services = new List<ServiceInfo>();
        }

        internal void NormalizeCollections()
        {
            _plugins.Sort();
            _services.Sort();
        }

        internal bool LoadDependencies()
        {
            try
            {
                foreach( AssemblyName n in _assembly.GetReferencedAssemblies() )
                {
                    Assembly.ReflectionOnlyLoad( n.FullName );
                }
                return true;
            }
            catch( Exception ex )
            {
                AddErrorLine( ex.Message );
            }
            return false;
        }

        /// <summary>
        /// Read all Type in the assembly.
        /// </summary>
        /// <param name="RegisterServiceInfo">Will be called for each discovered service interface.</param>
        /// <param name="RegisterUseServiceInfo">Will be called for each discovered service reference.</param>
        internal void ReadAllTypes( Func<PluginAssemblyInfo,Type,ServiceInfo> RegisterServiceInfo,
                                    Func<Type,bool,ServiceRefInfo> RegisterUseServiceInfo ) 
        {
            Debug.Assert( _assembly != null );
            List<PluginInfo> plug = new List<PluginInfo>();
            List<ServiceInfo> serv = new List<ServiceInfo>();
            // We loop on each type we can find into the assembly.
            foreach( Type t in _assembly.GetExportedTypes() )
            {
                // If it implements the IPlugin interface.
                if( t.GetInterface( typeof( IPlugin ).FullName ) != null )
                {
                    PluginInfo info = null;
                    try
                    {
                        // And if it have the PluginAttribute.
                        foreach( CustomAttributeData attr in CustomAttributeData.GetCustomAttributes( t ) )
                        {
                            if( attr.Constructor.DeclaringType.FullName == "CK.Plugin.PluginAttribute" )
                            {
                                info = new PluginInfo( attr ) { AssemblyInfo = this, PluginFullName = t.FullName };
                                break;
                            }
                        }
                        if( info == null )
                        {
                            info = new PluginInfo( "Unable to find Plugin attribute." );
                        }
                    }
                    catch( Exception ex ) 
                    { 
                        info = new PluginInfo( ex.Message ); 
                    }
                    Debug.Assert( info != null, "At this point, info can not be null." );
                    if( info.ErrorMessage == null ) // If there are no errors
                    {
                        // Discover implemented Services.
                        try
                        {
                            ServiceInfo result = null;
                            foreach( Type i in t.GetInterfaces() )
                            {
                                if( i.IsGenericType && i.GetGenericTypeDefinition().AssemblyQualifiedName == typeof(IService<>).AssemblyQualifiedName )
                                {
                                    // Implementing the IService marker interface is an error.
                                    info.AddErrorLine( R.PluginImplementIService );
                                    break;
                                }
                                if( PluginDiscoverer.IsIDynamicService( i ) )
                                {
                                    if( result != null ) 
                                    {
                                        // Only one IService can be implemented at a time.
                                        info.AddErrorLine( R.MultipleServiceImplementation );
                                        result = null;
                                        break;
                                    }
                                    else
                                    {
                                        // We are on a IDynamicService, not a IService<>.
                                        ServiceRefInfo r = RegisterUseServiceInfo( i, false );
                                        Debug.Assert( !r.IsIServiceWrapped && !r.IsUnknownGenericInterface, "We tested this above." ); 
                                        Debug.Assert( r.Reference != null, "Since we are on a IDynamicService (tested above)." ); 
                                        result = r.Reference;
                                    }
                                }
                            }
                            if( result != null )
                                result.Implementations.Add( info );
                            info.Service = result;
                        }
                        catch( Exception ex )
                        {
                            info.AddErrorLine( ex.Message );
                        }

                        // Discover referenced Services and edited configuration.
                        try
                        {
                            // Process public properties to discover service references and edited configurations.
                            List<ServiceReferenceInfo> refs = info.ServiceReferences = new List<ServiceReferenceInfo>();
                            foreach( PropertyInfo p in t.GetProperties() )
                            {
                                try
                                {
                                    bool serviceRefHandled = false;
                                    bool editedConfHandled = false;
                                    foreach( CustomAttributeData attr in CustomAttributeData.GetCustomAttributes( p ) )
                                    {
                                        #region The [DynamicServiceAttribute] has the priority.
                                        if( attr.Constructor.DeclaringType.FullName == typeof(DynamicServiceAttribute).FullName )
                                        {
                                            serviceRefHandled = true;
                                            ServiceReferenceInfo dep;
                                            // Since we are in a [DynamicServiceAttribute], we expect a IDynamicService and reject any other generic interface than IService<>,
                                            // but we accept other (not IDynamicService) interfaces in order to capture the reference (hence the registerOnlyIDynamicService = false parameter below).
                                            ServiceRefInfo r = RegisterUseServiceInfo( p.PropertyType, false );
                                            dep = new ServiceReferenceInfo( info, p.Name, r, attr.NamedArguments );
                                            Debug.Assert( (r.Reference == null) == (r.IsUnknownGenericInterface) ); 
                                            // Handle errors.
                                            if( !p.CanWrite ) dep.AddErrorLine( R.NotWritableServiceType );
                                            if( r.IsUnknownGenericInterface ) dep.AddErrorLine( R.GenericServiceOtherThanIServiceNotSupported );
                                            if( !r.IsIDynamicService )
                                            {
                                                Debug.Assert( !PluginDiscoverer.IsIDynamicService( p.PropertyType ) );
                                                dep.AddErrorLine( R.DynamicServiceAttributeRequiresIDynamicService );
                                            }
                                            refs.Add( dep );
                                            break;
                                        }
                                        #endregion
                                        #region The [RequiredService] can reference non IDynamicService service.
                                        if( attr.Constructor.DeclaringType.FullName == typeof( RequiredServiceAttribute ).FullName )
                                        {
                                            RunningRequirement req = RunningRequirement.MustExistAndRun;
                                            // Since [RequiredServiceAttribute] can have Requires = false parameter. 
                                            if( attr.NamedArguments.Count == 1 && (bool)attr.NamedArguments[0].TypedValue.Value == false )
                                            {
                                                // If Requires == false, we consider it to be optional.
                                                req = RunningRequirement.Optional;
                                            }
                                            serviceRefHandled = true;
                                            ServiceRefInfo r = RegisterUseServiceInfo( p.PropertyType, false );
                                            ServiceReferenceInfo dep = new ServiceReferenceInfo( info, p.Name, r, req );
                                            if( !p.CanWrite ) dep.AddErrorLine( R.NotWritableServiceType );
                                            else if( r.IsUnknownGenericInterface ) dep.AddErrorLine( R.GenericServiceOtherThanIServiceNotSupported );
                                            refs.Add( dep );
                                            break;
                                        }
                                        #endregion
                                        #region The [EditedConfiguration] comes last...
                                        if( attr.Constructor.DeclaringType.FullName == typeof( ConfigurationAccessorAttribute ).FullName )
                                        {
                                            editedConfHandled = true;
                                            try
                                            {
                                                info.EditorsInfo.Add( new PluginConfigAccessorInfo( attr, info, false ) { ConfigurationPropertyName = p.Name } );
                                            }
                                            catch( Exception ex )
                                            {
                                                info.AddErrorLine( ex.Message );
                                            }
                                        }
                                        #endregion
                                    }
                                    // If the property has not been handled through its attributes, we detect potential services references: any public read/write 
                                    // property that expose a IDynamicService or a IService<> is transformed into an optional ServiceReferenceInfo.
                                    if( !serviceRefHandled && !editedConfHandled && p.CanWrite && p.CanRead )
                                    {
                                        if( p.PropertyType.IsInterface )
                                        {
                                            if( p.PropertyType.Name == "IPluginConfigAccessor" )
                                                info.EditorsInfo.Add( new PluginConfigAccessorInfo( null, info, true ) { ConfigurationPropertyName = p.Name } );
                                            else
                                            {
                                                // Here we only register services if they are IDynamicService.
                                                ServiceRefInfo r = RegisterUseServiceInfo( p.PropertyType, true );
                                                if( r.Reference != null )
                                                {
                                                    Debug.Assert( r.IsIDynamicService );
                                                    ServiceReferenceInfo dep;
                                                    dep = new ServiceReferenceInfo( info, p.Name, r, RunningRequirement.Optional );
                                                }
                                            }
                                        }
                                    }
                                }
                                catch( Exception ex )
                                {
                                    info.AddErrorLine( ex.Message );
                                }
                            }
                        }   
                        catch( Exception ex )
                        {
                            info.AddErrorLine( ex.Message );
                        }

                        plug.Add( info );
                    }
                }
                // Discover just a service
                else if( t.IsInterface && PluginDiscoverer.IsIDynamicService( t ) )
                {
                    ServiceInfo service = RegisterServiceInfo( this, t );
                    service.AssemblyQualifiedName = t.AssemblyQualifiedName;

                    // Get the Events that the service exposes
                    foreach( EventInfo e in CK.Reflection.ReflectionHelper.GetFlattenEvents( t ) )
                    {
                        service.EventsInfoCollection.Add( new SimpleEventInfo( e.Name ) );
                    }

                    // Get the Properties that the service exposes
                    foreach( PropertyInfo p in CK.Reflection.ReflectionHelper.GetFlattenProperties( t ) )
                    {
                        service.PropertiesInfoCollection.Add( new SimplePropertyInfo( p.Name, p.PropertyType.ToString() ) );
                    }

                    // Get the Methods that the service exposes
                    foreach( MethodInfo m in CK.Reflection.ReflectionHelper.GetFlattenMethods( t ) )
                    {
                        if( !m.IsSpecialName )
                        {
                            Type tR = m.ReturnType;
                            SimpleMethodInfo s = new SimpleMethodInfo( m.Name, tR.ToString() );
                            foreach( ParameterInfo p in m.GetParameters() )
                            {
                                s.Parameters.Add( new SimpleParameterInfo( p.Name, p.ParameterType.ToString() ) );
                            }
                            service.MethodsInfoCollection.Add( s );
                        }
                    }
                    serv.Add( service );
                }
            }
            // Fills _plugins and _services collections (properties Plugin and Services of this PluginAssemblyInfo) 
            // with local collections (Plugin = _plugins = plug & Services = _services = serv).
            _plugins = plug;
            _services = serv;
        }

        #region IComparable<PluginAssemblyInfo> Membres

        public int CompareTo( PluginAssemblyInfo other )
        {
            if( this == other ) return 0;
            //return _assembly.FullName.CompareTo( other._assembly.FullName ); // Utilise le fullname comme clé
            return this._fileName.CompareTo( other._fileName ); // Utilise le file path comme clé
        }

        #endregion
    }
}

