#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\ServiceStoppedException.cs) is part of CiviKey. 
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
using System.Runtime.Serialization;

namespace CK.Plugin
{

    /// <summary>
    /// Exception raised whenever an event is raised by or a method is called on a stopped service. 
    /// </summary>
	[Serializable]
	public class ServiceStoppedException : Exception, ISerializable
	{
        /// <summary>
        /// Gets the service type name.
        /// </summary>
        public string ServiceTypeName { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="ServiceStoppedException"/>.
        /// </summary>
        /// <param name="serviceType">Type of the concerned service.</param>
		public ServiceStoppedException( Type serviceType )
		{
            ServiceTypeName = serviceType.AssemblyQualifiedName;
		}

        /// <summary>
        /// Initializes a new <see cref="ServiceStoppedException"/>.
        /// </summary>
        /// <param name="serviceType">Type of the concerned service.</param>
        /// <param name="message">Detailed message.</param>
        public ServiceStoppedException( Type serviceType, string message )
			: base( message )
		{
            ServiceTypeName = serviceType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Initializes a new <see cref="ServiceStoppedException"/> (serialization).
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
		protected ServiceStoppedException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
            ServiceTypeName = info.GetString( "ServiceTypeName" );
		}

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "ServiceTypeName", ServiceTypeName );
        }
    }
}
