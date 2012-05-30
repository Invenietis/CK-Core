#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\IgnoreServiceStoppedAttribute.cs) is part of CiviKey. 
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
    /// Enables methods (or events) to be called (and fire) even when the service is stopped.
    /// This is a dangerous attribute that must be reserved for special uses.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
    public class IgnoreServiceStoppedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="IgnoreServiceStoppedAttribute"/>.
        /// </summary>
        public IgnoreServiceStoppedAttribute()
        {
        }
    }

}
