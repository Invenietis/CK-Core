#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\UniqueId\INamedVersionedUniqueId.cs) is part of CiviKey. 
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
    /// Extends <see cref="IVersionedUniqueId"/> to associate a <see cref="P:PublicName"/> descriptor.
    /// </summary>
    public interface INamedVersionedUniqueId : IVersionedUniqueId
    {
        /// <summary>
        /// Gets the public name of this object. 
        /// It mmust never be null (defaults to <see cref="String.Empty"/>) and can be any string 
        /// in any culture (english US should be used as much a possible).
        /// </summary>
        string PublicName { get; }
    }

}
