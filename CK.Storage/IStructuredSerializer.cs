#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\IStructuredSerializer.cs) is part of CiviKey. 
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
    /// This interface enables external implementation of the serialization to <see cref="IStructuredWriter"/> and deserialization from <see cref="IStructuredReader"/>
    /// for a given type.
    /// When available in the services provided by the <see cref="IServiceProvider"/> associated to the reader or the writer, structured object read/write
    /// operations are delegated to this implementation instead of relying on <see cref="IStructuredSerializable"/> interface that <typeparamref name="T"/>
    /// may implement.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object whose serialization/deserialization is implemented.
    /// It is the exact object type that will be handled (no fallbacks.
    /// </typeparam>
    /// <remarks>
    /// This interface enables objects implementation to be totally unaware of any persistence mechanism on one hand, and on the 
    /// other hand to hook existing serialization processes.
    /// </remarks>
    public interface IStructuredSerializer<T>
    {
        /// <summary>
        /// Reads or creates the object (if the one provided is null) from the given structured storage.
        /// The current Xml element is already opened and will be closed by the framework: this method must not skip any 
        /// unknown element nor read the current end element.
        /// </summary>
        /// <param name="o">Object to read. Null if the serializer has to create it.</param>
        /// <param name="sr">The reader from which the object is deserialized.</param>
        object ReadInlineContent( IStructuredReader sr, T o );

        /// <summary>
        /// Persists an object into the given <see cref="IStructuredWriter"/>.
        /// The current Xml element is already opened and will be closed by the framework: this method must not write the end of the current element.
        /// </summary>
        /// <param name="sw">The writer to which the object is serialized.</param>
        /// <param name="o">The object to write.</param>
        /// <remarks>You can still write attributes on the startElement</remarks>
        void WriteInlineContent( IStructuredWriter sw, T o );
    }
}
