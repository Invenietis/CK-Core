#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\ISubStructuredReader.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Storage
{
    /// <summary>
    /// Specialized <see cref="IStructuredReader"/>. Such subordinate readers
    /// are obtained through <see cref="IStructuredReader.OpenSubReader"/>.
    /// As specialized IStructuredReader, they must be disposed by the caller.
    /// </summary>
    public interface ISubStructuredReader : IStructuredReader
    {
        /// <summary>
        /// Gets the <see cref="IStructuredReader.DeserializationActions"/> property of the root
        /// reader. Actions added to this sequence are executed at the end of the global read operation.
        /// </summary>
        ActionSequence RootDeserializationActions { get; }

    }
}
