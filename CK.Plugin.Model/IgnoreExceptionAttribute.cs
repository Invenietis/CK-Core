#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\IgnoreExceptionAttribute.cs) is part of CiviKey. 
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
    /// Removes the catching of exceptions from methods (or events).
    /// This should not be used in production code: overhead of catching errors is very low and should be kept.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
    public class IgnoreExceptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="IgnoreExceptionAttribute"/>.
        /// </summary>
        public IgnoreExceptionAttribute()
        {
        }
    }

}
