#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\IServiceHost.cs) is part of CiviKey. 
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
    /// Defines the host for services.
    /// </summary>
    public interface IServiceHost
    {
        /// <summary>
        /// Gets a <see cref="ISimpleServiceHostConfiguration"/> that is always taken into account (one can not <see cref="Remove"/> it).
        /// Any change to it must be followed by a call to <see cref="ApplyConfiguration"/>.
        /// </summary>
        ISimpleServiceHostConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// Adds a configuration layer.
        /// The <see cref="ApplyConfiguration"/> must be called to actually update the 
        /// internal configuration.
        /// </summary>
        void Add( IServiceHostConfiguration configurator );

        /// <summary>
        /// Removes a configuration layer.
        /// The <see cref="ApplyConfiguration"/> must be called to actually update the 
        /// internal configuration.
        /// </summary>
        void Remove( IServiceHostConfiguration configurator );

        /// <summary>
        /// Applies the configuration: the <see cref="IServiceHostConfiguration"/> that have been <see cref="Add"/>ed are challenged
        /// for each intercepted method or event.
        /// </summary>
        void ApplyConfiguration();

        /// <summary>
        /// Gets the service implementation if it is available (it can be stopped).
        /// If <paramref name="interfaceType"/> is a wrapped <see cref="IService{T}"/> and the service is disabled, it is returned,
        /// but if <paramref name="interfaceType"/> is a mere interface and the service is disabled, null is returned.
        /// </summary>
        /// <param name="interfaceType">Type of the service (it can be a wrapped <see cref="IService{T}"/>).</param>
        /// <returns>The implementation or null if it is not available (disabled) and <paramref name="interfaceType"/> is a mere interface.</returns>
        object GetProxy( Type interfaceType );
        
        /// <summary>
        /// Gets the service implementation if it is available and starting, stopping or running (null will be returned
        /// if it is stopped or disabled).
        /// </summary>
        /// <param name="interfaceType">Type of the service.</param>
        /// <returns>The implementation or null if it is not available (disabled or stopped).</returns>
        object GetRunningProxy( Type interfaceType );

        /// <summary>
        /// Ensures that a proxy exists for the given interface and associates it to an implementation.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="currentImplementation">Implementation to use.</param>
        /// <returns>The proxy object.</returns>
        object InjectExternalService( Type interfaceType, object currentImplementation );

    
    }
}
