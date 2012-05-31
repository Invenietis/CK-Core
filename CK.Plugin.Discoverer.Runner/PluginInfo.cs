#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\PluginInfo.cs) is part of CiviKey. 
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
using CK.Core;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CK.Plugin.Discoverer.Runner
{
	/// <summary>
	/// Defines the basic information of a plugin.
	/// </summary>
    [Serializable]
    public sealed class PluginInfo : DiscoveredInfo, IComparable<PluginInfo>
	{
		Guid _pluginId;
		string _name;
        string _pluginFullName;
		string _desc;
		Uri _url;
		Version _version;
		Uri _iconUri;
		static Version _emptyVersion = new Version();
        PluginAssemblyInfo _assemblyInfo;

		string[] _categories;
        List<ServiceReferenceInfo> _serviceReferences;
        ServiceInfo _service;
        List<PluginConfigAccessorInfo> _editors;
        List<PluginConfigAccessorInfo> _editableBy;

        #region Properties

        public Guid PluginId
        {
            get { return _pluginId; }
            set { _pluginId = value; }
        }

        public string PublicName
        {
            get { return _name; }
            set { _name = value != null ? value : String.Empty; }
        }

        public string Description
        {
            get { return _desc; }
            set { _desc = value != null ? value : String.Empty; }
        }

        public Version Version
        {
            get { return _version; }
            set { _version = value != null ? value : _emptyVersion; }
        }

        public Uri RefUrl
        {
            get { return _url; }
            set { _url = value; }
        }

        public string[] Categories
        {
            get { return _categories; }
        }

        public Uri IconUri
        {
            get { return _iconUri; }
            set { _iconUri = value; }
        }

        public string PluginFullName
        {
            get { return _pluginFullName; }
            set { _pluginFullName = value; }
        }

        public List<PluginConfigAccessorInfo> EditorsInfo
        {
            get { return _editors; }
            set { _editors = value; }
        }

        public List<PluginConfigAccessorInfo> EditableBy
        {
            get { return _editableBy; }
            set { _editableBy = value; }
        }

        public PluginAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
            set { _assemblyInfo = value; }
        }

        public List<ServiceReferenceInfo> ServiceReferences
        {
            get { return _serviceReferences; }
            set { _serviceReferences = value; }
        }

        public ServiceInfo Service
        {
            get { return _service; }
            set { _service = value; }
        }

        public bool IsOldVersion { get; set; }

        #endregion

        /// <summary>
        /// Initializes a plugin info with an error.
        /// </summary>
        internal PluginInfo( string errorMessage )
        {
            AddErrorLine( errorMessage );
        }

        /// <summary>
		/// Initializes a new PluginInfo based on the <see cref="PluginAttribute"/> configuration.
		/// </summary>
		internal PluginInfo( CustomAttributeData attribute )
		{
            try
            {
                _categories = Util.EmptyStringArray;
                _editors = new List<PluginConfigAccessorInfo>();
                _editableBy = new List<PluginConfigAccessorInfo>();
                _pluginId = new Guid( (string)attribute.ConstructorArguments[0].Value );
                foreach( CustomAttributeNamedArgument a in attribute.NamedArguments )
                {
                    switch( a.MemberInfo.Name )
                    {
                        case "PublicName": _name = (string)a.TypedValue.Value; break;
                        case "Description": _desc = (string)a.TypedValue.Value; break;
                        case "Version": _version = new Version( (string)a.TypedValue.Value ); break;
                        case "RefUrl": _url = a.TypedValue.Value != null ? new Uri( (string)a.TypedValue.Value ) : null; break;
                        case "IconUri": _iconUri = a.TypedValue.Value != null ? new Uri( (string)a.TypedValue.Value, UriKind.RelativeOrAbsolute ) : null; break;
                        case "Categories": _categories = ReadStringArray( a.TypedValue.Value ); break;
                    }
                }
            }
            catch( Exception ex )
            {
                AddErrorLine( ex.Message );
            }
		}

        internal void NormalizeCollections()
        {
            Debug.Assert( _categories.IsSortedStrict( StringComparer.Ordinal.Compare ) );
            _serviceReferences.Sort();
            _editableBy.Sort();
        }

        static internal string[] ReadStringArray( object values )
        {
            ReadOnlyCollection<CustomAttributeTypedArgument> v = (ReadOnlyCollection<CustomAttributeTypedArgument>)values;
            return v.Select( a => (string)a.Value ).Distinct( StringComparer.Ordinal ).OrderBy( Util.FuncIdentity, StringComparer.Ordinal ).ToArray();
        }

        #region IComparable<PluginInfo> Membres

        public int CompareTo( PluginInfo other )
        {
            if( this == other ) return 0;
            int cmp = _pluginId.CompareTo( other.PluginId );
            if( cmp == 0 ) cmp = _version.CompareTo( other.Version );
            return cmp;
        }

        #endregion
    }
}
