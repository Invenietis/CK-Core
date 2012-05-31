#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Discoverer\IPluginConfigAccessorInfo.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// An editor plugin can edit data (configuration properties) of another plugin.
	/// This interface defines the association between the <see cref="Plugin"/> and the <see cref="Source"/> plugins.
    /// </summary>
    public interface IPluginConfigAccessorInfo : IDiscoveredInfo
    {
        /// <summary>
        /// Gets the <see cref="IPlugin"/> that edits the <see cref="Source"/>.
        /// </summary>
        IPluginInfo Plugin { get; }

        /// <summary>
        /// Gets the unique identifier of the plugin that <see cref="Plugin"/> claims to edit.
        /// </summary>
        Guid Source { get; }

        /// <summary>
        /// Gets the <see cref="IPluginInfo"/> that the <see cref="Plugin"/> claims to edit if it exists (null
        /// if the plugin has not been discovered).
        /// </summary>
        IPluginInfo EditedSource { get; }

        /// <summary>
        /// Gets the property name of the <see cref="Plugin"/> plugin that access the <see cref="Source"/> configuration.
        /// Null if no property exists.
        /// </summary>
        string ConfigurationPropertyName { get; }

        /// <summary>
        /// Gets if the <see cref="ConfigurationPropertyName"/> is not null (a property actually exists 
        /// on the plugin for the <see cref="Source"/> plugin), no error is associated to this object, 
        /// and <see cref="EditedSource"/> is not null.
        /// </summary>
        bool IsConfigurationPropertyValid { get; }
    }
}
