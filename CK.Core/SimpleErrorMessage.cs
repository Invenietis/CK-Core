#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\SimpleErrorMessage.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Basic implementation of <see cref="ISimpleErrorMessage"/>.
    /// </summary>
    public class SimpleErrorMessage : ISimpleErrorMessage
    {
        /// <summary>
        /// Initializes a new <see cref="SimpleErrorMessage"/> with 
        /// a null <see cref="ErrorMessage"/>.
        /// </summary>
        public SimpleErrorMessage()
        {
        }

        /// <summary>
        /// Gets or sets whether this error message 
        /// should be considered only as a warning.
        /// </summary>
        public bool IsWarning { get; set;  }

        /// <summary>
        /// Gets or sets an error message. Can be null.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
