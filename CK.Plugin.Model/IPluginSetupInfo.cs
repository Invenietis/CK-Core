#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\IPluginSetupInfo.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;

namespace CK.Plugin
{
    /// <summary>
    /// 
    /// </summary>
    public class IPluginSetupInfo
    {
        /// <summary>
        /// Gets or sets an explicit message for the user when <see cref="IPlugin.Setup"/> fails.
        /// </summary>
        public string FailedUserMessage { get; set; }

        /// <summary>
        /// Gets or sets a message for the user when <see cref="IPlugin.Setup"/> fails.
        /// </summary>
        public string FailedDetailedMessage { get; set; }

        /// <summary>
        /// Gets or sets an optional exception.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Clears <see cref="FailedUserMessage"/>, <see cref="FailedDetailedMessage"/> and <see cref="Error"/>: they are all set to null.
        /// </summary>
        public void Clear()
        {
            FailedUserMessage = null;
            FailedDetailedMessage = null;
            Error = null;
        }

    }
}
