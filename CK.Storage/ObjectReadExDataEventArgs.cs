#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\ObjectReadExDataEventArgs.cs) is part of CiviKey. 
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

namespace CK.Storage
{
    /// <summary>
    /// Provides data for <see cref="IStructuredReader.ObjectReadExData"/> event.
    /// </summary>
    public class ObjectReadExDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the object read.
        /// </summary>
        public readonly object Obj;

        /// <summary>
        /// Gets the structured reader. The reader is postionned on
        /// the element.
        /// </summary>
        public readonly IStructuredReader Reader;

        /// <summary>
        /// Gets or sets whether the extra element has been read.
        /// It must be set to true as soon as the <see cref="IStructuredReader.Xml"/> reader
        /// has been forwarded.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ObjectWriteExDataEventArgs"/>.
        /// </summary>
        /// <param name="r">Structured reader.</param>
        /// <param name="o">Object read.</param>
        public ObjectReadExDataEventArgs( IStructuredReader r, object o )
        {
            Obj = o;
            Reader = r;
        }

    }

}
