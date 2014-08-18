#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputHandlers\HandlerTypeAttribute.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Simple attribute that binds a <see cref="HandlerConfiguration"/> to the actual <see cref="HandlerBase"/> that will actually do the job.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class HandlerTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="HandlerTypeAttribute"/> with a type that must be a <see cref="HandlerBase"/> specialization.
        /// </summary>
        /// <param name="handlerType"></param>
        public HandlerTypeAttribute( Type handlerType )
        {
            HandlerType = handlerType;
        }

        /// <summary>
        /// Gets the type of the associated <see cref="HandlerBase"/>.
        /// </summary>
        public readonly Type HandlerType;
    }

}
