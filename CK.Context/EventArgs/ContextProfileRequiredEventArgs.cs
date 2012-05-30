#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\EventArgs\ContextProfileRequiredEventArgs.cs) is part of CiviKey. 
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

namespace CK.Context
{
    /// <summary>
    /// </summary>
    public class ContextProfileRequiredEventArgs : ContextEventArgs
    {
        public ContextProfileRequiredEventArgs( IContext ctx,  bool saving )
            : base( ctx )
        {
            IsSaving = saving;
        }

        public bool IsSaving { get; private set; }

        /// <summary>
        /// Gets or sets the display name for the default context.
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> of the default context to use.
        /// </summary>
        public Uri Address { get; set; }
    }
}
