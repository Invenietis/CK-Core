#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\IStructuredReaderBookmark.cs) is part of CiviKey. 
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

namespace CK.Storage
{
    /// <summary>
    /// Defines a bookmark inside a structured content.
    /// A bookmark encapsulates a content fragment in order to delay the actual read process.
    /// </summary>
    public interface IStructuredReaderBookmark
    {
        /// <summary>
        /// Restores the state of a reader.
        /// </summary>
        /// <param name="baseServiceProvider">The <see cref="IServiceProvider"/> to use to obtain ambiant services.</param>
        /// <returns>A ready-to-use <see cref="IStructuredReader"/> on the bookmark.</returns>
        IStructuredReader Restore( IServiceProvider baseServiceProvider );

        /// <summary>
        /// Writes the bookmark back into a <see cref="IStructuredWriter"/>.
        /// </summary>
        /// <param name="w">The <see cref="IStructuredWriter"/> to write to.</param>
        void WriteBack( IStructuredWriter w );

    }
}
