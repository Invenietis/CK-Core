#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\ServiceStatusChangedEventArgs.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Plugin
{
	/// <summary>
	/// Event argument when a service <see cref="RunningStatus">status</see> changed.
	/// This event is available on the generic <see cref="IService{T}"/>.<see cref="IService{T}.ServiceStatusChanged">ServiceStatusChanged</see>.
	/// </summary>
	public class ServiceStatusChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the previous status.
		/// </summary>
		public RunningStatus Previous { get; private set; }
		
		/// <summary>
		/// Gets the current status of the service.
		/// </summary>
		public RunningStatus Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="ServiceStatusChangedEventArgs"/>.
        /// </summary>
        /// <param name="previous">The previous running status.</param>
        /// <param name="current">The current running Status</param>
        /// <param name="allowErrorTransition">True if the next status is a valid next one (like <see cref="RunningStatus.Starting"/> to <see cref="RunningStatus.Started"/>). False otherwise.</param>
		public ServiceStatusChangedEventArgs( RunningStatus previous, RunningStatus current, bool allowErrorTransition )
		{
			Debug.Assert( previous.IsValidTransition( current, allowErrorTransition ) );
			Previous = previous;
			Current = current;
		}

	}

}
