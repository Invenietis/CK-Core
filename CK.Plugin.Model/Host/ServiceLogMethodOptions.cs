#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\ServiceLogMethodOptions.cs) is part of CiviKey. 
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
    /// Bit flags that describes the way a method is intercepted.
    /// </summary>
    [Flags]
    public enum ServiceLogMethodOptions
    {
        /// <summary>
        /// "Naked mode". Nothing is logged (even exceptions). 
        /// </summary>
        None = 0,

        /// <summary>
        /// Log the exception that the method may throw.
        /// </summary>
        LogError = 1,

        /// <summary>
        /// Log the beginning of the call to the method.
        /// </summary>
        Enter = 2,

        /// <summary>
        /// Log the parameters of the call.
        /// </summary>
        LogParameters = 4,

        /// <summary>
        /// Log the caller method.
        /// </summary>
        LogCaller = 8,

        /// <summary>
        /// Log the end of the call to the method.
        /// </summary>
        Leave = 16,
        
        /// <summary>
        /// Log the return value of the method.
        /// </summary>
        LogReturnValue = 32,
      
        /// <summary>
        /// Log when the method is called. (Info type)
        /// </summary>
        CreateEntryMask = Leave | Enter
    }

}
