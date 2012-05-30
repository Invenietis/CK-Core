#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\PluginAssemblyInfo.cs) is part of CiviKey. 
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
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using CK.Core;

namespace CK.Plugin.Discoverer
{
    internal sealed class PluginAssemblyInfo : DiscoveredInfo, IAssemblyInfo, IComparable<PluginAssemblyInfo>
    {
        string _fileName;
        int _fileSize;
        AssemblyName _assemblyName;
        IReadOnlyList<IPluginInfo> _plugins;
        IReadOnlyList<IServiceInfo> _services;
        List<PluginInfo> _pluginsCollection;
        List<ServiceInfo> _servicesCollection;

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
			get { return _plugins != null && (_plugins.Count > 0 || _services.Count > 0); } 
		}

		public IReadOnlyList<IPluginInfo> Plugins 
		{
			get { return _plugins; } 
		}

		public IReadOnlyList<IServiceInfo> Services
		{
			get { return _services; } 
		}

        internal PluginAssemblyInfo( PluginDiscoverer discoverer )
            : base( discoverer )
        {
            _pluginsCollection = new List<PluginInfo>();
            _servicesCollection = new List<ServiceInfo>();
        }

        internal void Initialize( PluginDiscoverer.Merger merger, Runner.PluginAssemblyInfo r )
        {
            base.Initialize( r );
            _fileName = r.AssemblyFileName;
            _assemblyName = r.AssemblyName;
            _fileSize = r.AssemblyFileSize;

            foreach( Runner.PluginInfo plugin in r.Plugins )
                _pluginsCollection.Add( merger.FindOrCreate( plugin ) );

            foreach( Runner.ServiceInfo service in r.Services )
                _servicesCollection.Add( merger.FindOrCreate( service ) );

            _plugins = new ReadOnlyListOnIList<PluginInfo>( _pluginsCollection );
            _services = new ReadOnlyListOnIList<ServiceInfo>( _servicesCollection );
        }

        internal bool Merge( PluginDiscoverer.Merger merger, Runner.PluginAssemblyInfo r )
        {
            bool hasChanged = false;

            if( _fileName != r.AssemblyFileName ) 
            { 
                _fileName = r.AssemblyFileName;  
                hasChanged = true; 
            }
            // AssemblyName does not override Equals.
            // We use the Fullname This is perfect for strongly signed assembly, not necessary 
            // for unsigned ones.
            // This is why we added the file size to the AssemblyInfo: file size gives
            // us a way to detect file change even if its name (typically through its version) is not updated.
            bool rHasAssemblyName = r.AssemblyName != null;
            if( (_assemblyName != null) != rHasAssemblyName || (rHasAssemblyName && _assemblyName.FullName != r.AssemblyName.FullName) )
            {
                _assemblyName = r.AssemblyName;
                hasChanged = true;
            }

            if( _fileSize != r.AssemblyFileSize )
            {
                _fileSize = r.AssemblyFileSize;
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _pluginsCollection, r.Plugins, merger.FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _servicesCollection, r.Services, merger.FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            return Merge( r, hasChanged );
        }

        int IComparable<PluginAssemblyInfo>.CompareTo( PluginAssemblyInfo other )
        {
            return CompareTo( other );
        }

        public int CompareTo( IAssemblyInfo other )
        {
            if( this == other ) return 0;
            return _fileName.CompareTo( other.AssemblyFileName );
        }

    }
}
