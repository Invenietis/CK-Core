#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\IMergeable.cs) is part of CiviKey. 
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
    /// Simple interface to support merging of information from external objects.
    /// </summary>
    public interface IMergeable
    {
        /// <summary>
        /// Attempts to merge this object with the given one.
        /// This method should not raise any exception. Instead, false should be returned. 
        /// If an exception is raised, callers should handle the exception and behaves as if the method returned false.
        /// </summary>
        /// <param name="source">Source object to merge into this one.</param>
        /// <param name="services">Optional services (can be null) that can be injected into the merge process.</param>
        /// <returns>True if the merge succeeded, false if the merge failed or is not possible.</returns>
        bool Merge( object source, IServiceProvider services = null );
    }
}
