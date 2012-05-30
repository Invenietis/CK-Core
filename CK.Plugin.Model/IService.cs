#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\IService.cs) is part of CiviKey. 
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
    /// This generic interface is automatically implemented for each <see cref="IDynamicService"/> and
    /// enables a plugin to manage service status.
    /// </summary>
    /// <typeparam name="T">The dynamic service interface.</typeparam>
    public interface IService<T> where T : IDynamicService
    {
        /// <summary>
        /// Gets the service itself. It is actually this object itself: <c>this</c> can be directly casted into 
        /// the interface.
        /// </summary>
        T Service { get; }

        /// <summary>
        /// Gets the current <see cref="RunningStatus"/> of the service.
        /// </summary>
        RunningStatus Status { get; }

        /// <summary>
        /// Fires whenever the <see cref="Status"/> changed.
        /// </summary>
        event EventHandler<ServiceStatusChangedEventArgs> ServiceStatusChanged;
    }
}
