#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\IStructuredSerializable.cs) is part of CiviKey. 
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
using System.Xml;
using CK.Core;

namespace CK.Storage
{

    /// <summary>
    /// Provides custom serialization to <see cref="IStructuredWriter"/> and deserialization from <see cref="IStructuredReader"/>.
    /// </summary>
    public interface IStructuredSerializable
    {
        /// <summary>
        /// Restores this object from the given structured storage.
        /// The current Xml element is already opened and will be closed by the framework: this method must not skip any 
        /// unknown element nor read the current end element.
        /// </summary>
        /// <param name="sr">The reader from which the object is deserialized.</param>
        void ReadContent( IStructuredReader sr );
        
        /// <summary>
        /// Persists an object into the given <see cref="IStructuredWriter"/>.
        /// The current Xml element is already opened and will be closed by the framework: this method must not write the end of the current element.
        /// </summary>
        /// <param name="sw">The writer to which the object is serialized.</param>
        void WriteContent( IStructuredWriter sw );
    }
}
