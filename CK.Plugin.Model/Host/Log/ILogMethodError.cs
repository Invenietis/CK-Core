#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\Log\ILogMethodError.cs) is part of CiviKey. 
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
using System.Reflection;

namespace CK.Plugin
{
    /// <summary>
    /// Log event emitted whenever an error occured during a method call.
    /// </summary>
    public interface ILogMethodError : ILogInterceptionEntry, ILogErrorCaught
    {
        /// <summary>
        /// The method that raised the error (it is also the <see cref="ILogErrorCulprit.Culprit"/>).
        /// </summary>
        MethodInfo Method { get; }

        /// <summary>
        /// Logged method entry if it exists (null otherwise).
        /// </summary>
        ILogMethodEntry MethodEntry { get; }

    }
}
