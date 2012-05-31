#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\PluginAttribute.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Tags class that must implement <see cref="IPlugin"/> and may implement one <see cref="IDynamicService"/>.
    /// The only required property is the <see cref="Id"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
    public class PluginAttribute : Attribute
    {
        Guid _pluginId;
        string _description;
        Version _version;
        string[] _categories;

        /// <summary>
        /// Initializes a new <see cref="PluginAttribute"/> with its <see cref="Id"/>.
        /// </summary>
        /// <param name="pluginIdentifier">Identifier of the plugin.</param>
        public PluginAttribute( string pluginIdentifier )
        {
            _pluginId = new Guid( pluginIdentifier );
            _description = string.Empty;
            PublicName = GetType().Name;
            _version = Util.EmptyVersion;
        }

		/// <summary>
		/// Gets the unique identifier of the plugin.
		/// </summary>
		public Guid Id
		{
            get { return _pluginId; }
        }

        /// <summary>
        /// Gets or sets the public name of the plugin. Can be any string in any culture.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the description of the plugin.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value != null ? value : String.Empty; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Version"/> of the plugin. It is a string
        /// with at least the first two of the "Major.Minor.Build.Revision" standard version.
        /// Defaults to <see cref="Util.EmptyVersion"/>.
        /// </summary>
        public string Version
        {
            get { return _version.ToString(); }
            set { _version = value != null ? new Version(value) : Util.EmptyVersion; }
        }

        /// <summary>
        /// Gets or sets an optional url that describes the plugin. Can be null.
        /// </summary>
        public string RefUrl { get; set; }

        /// <summary>
        /// Gets or sets an optional url where we can find an Icon attached to the plugin. Can be null.
        /// </summary>
        public string IconUri { get; set; }

        /// <summary>
        /// Gets or sets an optional list of categories, used to sort the plugin list by theme.
        /// Can also be used to define if this plugin must appear in the "Public" or "Advanced" configuration panel.
        /// Will never be null (an empty array will be returned instead of null).
        /// </summary>
        public string[] Categories
        {
            get { return _categories ?? CK.Core.Util.EmptyStringArray; }
            set { _categories = value; }
        }

    }
}
