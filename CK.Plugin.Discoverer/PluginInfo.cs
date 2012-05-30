#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\PluginInfo.cs) is part of CiviKey. 
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using CK.Core;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CK.Plugin.Discoverer
{
    /// <summary>
    /// Defines the basic information of a plugin.
    /// </summary>
    internal sealed class PluginInfo : DiscoveredInfo, IPluginInfo, IComparable<PluginInfo>
    {
        #region Fields

        static Version _emptyVersion = new Version();
        Guid _pluginId;
        string _name;
        string _pluginFullName;
        string _desc;
        Uri _url;
        Version _version;
        Uri _iconUri;
        PluginAssemblyInfo _assemblyInfo;
        bool _isOldVersion;
        IServiceInfo _service;

        IReadOnlyList<string> _categories;
        IReadOnlyList<IServiceReferenceInfo> _serviceReferences;
        IReadOnlyList<IPluginConfigAccessorInfo> _editorsInfo;
        IReadOnlyList<IPluginConfigAccessorInfo> _editableBy;

        string[] _categoriesCollection;
        Dictionary<Runner.ServiceReferenceInfo, ServiceReferenceInfo> _dicServiceReferences;
        List<ServiceReferenceInfo> _servicesReferencesCollection;
        List<PluginConfigAccessorInfo> _editorsCollection;
        List<PluginConfigAccessorInfo> _editableByCollection;

        #endregion

        #region Properties

        public Guid PluginId
        {
            get { return _pluginId; }
        }

        public string PublicName
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _desc; }
        }

        public Version Version
        {
            get { return _version; }
        }

        public Uri RefUrl
        {
            get { return _url; }
        }

        public IReadOnlyList<string> Categories
        {
            get { return _categories; }
        }

        public Uri IconUri
        {
            get { return _iconUri; }
        }

        public string PluginFullName
        {
            get { return _pluginFullName; }
        }

        public IReadOnlyList<IPluginConfigAccessorInfo> EditorsInfo
        {
            get { return _editorsInfo; }
        }

        public IReadOnlyList<IPluginConfigAccessorInfo> EditableBy
        {
            get { return _editableBy; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public IReadOnlyList<IServiceReferenceInfo> ServiceReferences
        {
            get { return _serviceReferences; }
        }

        public IServiceInfo Service
        {
            get { return _service; }
        }

        public bool IsOldVersion
        {
            get { return _isOldVersion; }
            set { _isOldVersion = value; }
        }

        #endregion

        internal PluginInfo( PluginDiscoverer discoverer )
            : base( discoverer )
        {
        }

        internal void Initialize( PluginDiscoverer.Merger merger, Runner.PluginInfo r )
        {
            base.Initialize( r );
            
            _pluginId = r.PluginId;
            _name = r.PublicName;
            _pluginFullName = r.PluginFullName;
            _desc = r.Description;
            _url = r.RefUrl;
            _version = r.Version;
            _iconUri = r.IconUri;
            _isOldVersion = r.IsOldVersion;
            _assemblyInfo = merger.FindOrCreate( r.AssemblyInfo );

            Debug.Assert( r.Categories != null );
            _categoriesCollection = r.Categories;
            _categories = new ReadOnlyListOnIList<string>( _categoriesCollection );

            if( r.EditorsInfo != null )
            {
                _editorsCollection = new List<PluginConfigAccessorInfo>();
                foreach( Runner.PluginConfigAccessorInfo editor in r.EditorsInfo )
                    _editorsCollection.Add( merger.FindOrCreate( editor ) );
            }

            _servicesReferencesCollection = new List<ServiceReferenceInfo>();
            _dicServiceReferences = new Dictionary<Runner.ServiceReferenceInfo, ServiceReferenceInfo>();
            foreach( Runner.ServiceReferenceInfo service in r.ServiceReferences )
                _servicesReferencesCollection.Add( FindOrCreate( merger, service ) );

            if( r.Service != null )
            {
                _service = merger.FindOrCreate( r.Service );
            }
            _editableByCollection = new List<PluginConfigAccessorInfo>();
            foreach( Runner.PluginConfigAccessorInfo editor in r.EditableBy )
                _editableByCollection.Add( merger.FindOrCreate( editor ) );

            _serviceReferences = new ReadOnlyListOnIList<ServiceReferenceInfo>( _dicServiceReferences.Values.ToList() );
            _editorsInfo = new ReadOnlyListOnIList<PluginConfigAccessorInfo>( _editorsCollection );
            _editableBy = new ReadOnlyListOnIList<PluginConfigAccessorInfo>( _editableByCollection );
            _serviceReferences = new ReadOnlyListOnIList<ServiceReferenceInfo>( _dicServiceReferences.Values.ToList() );
            _editorsInfo = new ReadOnlyListOnIList<PluginConfigAccessorInfo>( _editorsCollection );
        }

        internal bool Merge( PluginDiscoverer.Merger merger, Runner.PluginInfo r )
        {
            bool hasChanged = false;

            if( _pluginId != r.PluginId )
            {
                _pluginId = r.PluginId;
                hasChanged = true;
            }
            if( _name != r.PublicName )
            {
                _name = r.PublicName;
                hasChanged = true;
            }
            if( _pluginFullName != r.PluginFullName )
            {
                _pluginFullName = r.PluginFullName;
                hasChanged = true;
            }
            if( _desc != r.Description )
            {
                _desc = r.Description;
                hasChanged = true;
            }
            if( _url != r.RefUrl )
            {
                _url = r.RefUrl;
                hasChanged = true;
            }
            if( _version != r.Version )
            {
                _version = r.Version;
                hasChanged = true;
            }
            if( _iconUri != r.IconUri )
            {
                _iconUri = r.IconUri;
                hasChanged = true;
            }
            if( _isOldVersion != r.IsOldVersion )
            {
                _isOldVersion = r.IsOldVersion;
                hasChanged = true;
            }
            
            Debug.Assert( _categories != null && r.Categories != null, "Already initialized." );
            if( !_categories.SequenceEqual( r.Categories, StringComparer.Ordinal ) )
            {
                _categoriesCollection = r.Categories;
                _categories = new ReadOnlyListOnIList<string>( _categoriesCollection );
                hasChanged = true;
            }
            
            PluginAssemblyInfo newAI = merger.FindOrCreate( r.AssemblyInfo );
            if( _assemblyInfo != newAI )
            {
                _assemblyInfo = newAI;
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _servicesReferencesCollection, r.ServiceReferences, ( s ) => { return FindOrCreate( merger, s ); }, null ) )
            {
                hasChanged = true;
            }

            // r.Service can be null, some checks have to be made:
            if( r.Service == null )
            {
                if( _service != null )
                {
                    _service = null;
                    hasChanged = true;
                }
            }
            else
            {
                ServiceInfo s = merger.FindOrCreate( r.Service );
                if( _service != s )
                {
                    _service = s;
                    hasChanged = true;
                }
            }

            if ( PluginDiscoverer.Merger.GenericMergeLists( _editorsCollection, r.EditorsInfo, merger.FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            if( PluginDiscoverer.Merger.GenericMergeLists( _editableByCollection, r.EditableBy, merger.FindOrCreate, null ) )
            {
                hasChanged = true;
            }

            return Merge( r, hasChanged );
        }

        internal PluginInfo Clone()
        {
            return (PluginInfo)MemberwiseClone();
        }

        internal ServiceReferenceInfo FindOrCreate( PluginDiscoverer.Merger merger, Runner.ServiceReferenceInfo serviceRef )
        {
            ServiceReferenceInfo f;
            if( !_dicServiceReferences.TryGetValue( serviceRef, out f ) )
            {
                f = new ServiceReferenceInfo( Discoverer );
                _dicServiceReferences.Add( serviceRef, f );
                f.Initialize( merger, serviceRef );
            }
            else
            {
                if( f.LastChangedVersion != Discoverer.CurrentVersion )
                    f.Merge( merger, serviceRef );
            }
            return f;
        }

        public override string ToString()
        {
            return _name;
        }

        int IComparable<PluginInfo>.CompareTo( PluginInfo other )
        {
            return CompareTo( other );
        }

        public int CompareTo( IPluginInfo other )
        {
            if( this == other ) return 0;
            int cmp = _pluginId.CompareTo( other.PluginId );
            if( cmp == 0 ) cmp = _version.CompareTo( other.Version );
            return cmp;
        }

        Guid IUniqueId.UniqueId
        {
            get { return _pluginId; }
        }
    }
}
