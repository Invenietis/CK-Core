#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\StandardReadStatus.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace CK.Storage
{

    /// <summary>
    /// Defines the result of a read operation.
    /// </summary>
    [Flags]
    public enum StandardReadStatus
    {
        /// <summary>
        /// Set when a null object has been read.
        /// </summary>
        NullData = 1,

        /// <summary>
        /// Set when a simple typed object (not null) has been read.
        /// </summary>
        SimpleTypeData = 2,

        /// <summary>
        /// Set when an <see cref="IXmlSerializable"/> object has been read.
        /// </summary>
        XmlSerializable = 4,

        /// <summary>
        /// Set when a serialized object has been read.
        /// </summary>
        BinaryObject = 8,

        /// <summary>
        /// Set when an <see cref="IStructuredSerializable"/> object has been read.
        /// </summary>
        Structured = 16,

        /// <summary>
        /// Set when a type has not been resolved. The read value is set to the default for the expected type (null for object, 0 for enum).
        /// </summary>
        IgnoredType = 32,

        /// <summary>
        /// Set when a general Xml error occured such as a missing end tag or any malformed xml. 
        /// </summary>
        ErrorXmlRead = 128,

        /// <summary>
        /// Set when an error occured during the read of a data element.
        /// </summary>
        ErrorWhileReadingElementObject = 256,

        /// <summary>
        /// Set when the 'type' required attribute is missing.
        /// </summary>
        ErrorTypeAttributeMissing = 512,

        /// <summary>
        /// Set when the type="xxx" attribute references an unknown type.
        /// </summary>
        ErrorUnknownTypeAttribute = 1024,

        /// <summary>
        /// Defines the greatest error code for this read status.
        /// </summary>
        LastReadError = 1024,

        /// <summary>
        /// Mask that covers all the errors.
        /// </summary>
        ErrorMask = ErrorXmlRead | ErrorTypeAttributeMissing | ErrorWhileReadingElementObject | ErrorUnknownTypeAttribute,
    }

}
