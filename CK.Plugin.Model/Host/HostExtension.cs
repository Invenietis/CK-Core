#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\HostExtension.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Defines the host extension methods.
    /// </summary>
    public static class HostExtension
    {
        /// <summary>
        /// Gets the <see cref="IService{T}"/> service proxy if it is available (it may be stopped but null will be returned
        /// if it is disabled).
        /// </summary>
        /// <typeparam name="T">Type of the service (interface marked with <see cref="IDynamicService"/>)</typeparam>
        /// <returns>The service or null if not available (disabled).</returns>
        public static IService<T> GetProxy<T>( this IServiceHost host ) where T : IDynamicService
        {
            return (IService<T>)host.GetProxy( typeof( T ) );
        }

        /// <summary>
        /// Gets the <see cref="IService{T}"/> service proxy if it is available and starting, stopping or running (null will be returned
        /// if it is stopped or disabled).
        /// </summary>
        /// <typeparam name="T">Type of the service (interface marked with <see cref="IDynamicService"/>)</typeparam>
        /// <returns>The service or null if not available (disabled or stopped).</returns>
        public static IService<T> GetRunningProxy<T>( this IServiceHost host ) where T : IDynamicService
        {
            return (IService<T>)host.GetRunningProxy( typeof( T ) );
        }

        /// <summary>
        /// Checks whether a plugin is running or not.
        /// </summary>
        /// <param name="key">Plugin identifier.</param>
        /// <returns>True if the plugin is loaded and is currently running.</returns>
        public static bool IsPluginRunning( this IPluginHost host, Guid key )
        {
            IPluginProxy p = host.FindLoadedPlugin( key, false );
            return p != null && host.IsPluginRunning( p.PluginKey );
        }

    }
}
