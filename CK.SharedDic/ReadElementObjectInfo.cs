#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\ReadElementObjectInfo.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Storage;

namespace CK.SharedDic
{
    /// <summary>
    /// Encapsulates the result of a read operation.
    /// </summary>
    public sealed class ReadElementObjectInfo : ISimpleErrorMessage
    {
        /// <summary>
        /// Defines the result of a an element read from a Xml stream.
        /// </summary>
        [Flags]
        public enum ReadStatus
        {
            /// <summary>
            /// Set when a null object has been read.
            /// </summary>
            NullData = StandardReadStatus.NullData,

            /// <summary>
            /// Set when a simple typed object (not null) has been read.
            /// </summary>
            SimpleTypeData = StandardReadStatus.SimpleTypeData,

            /// <summary>
            /// Set when an <see cref="IXmlSerializable"/> object has been read.
            /// </summary>
            XmlSerializable = StandardReadStatus.XmlSerializable,

            /// <summary>
            /// Set when a serialized object has been read.
            /// </summary>
            BinaryObject = StandardReadStatus.BinaryObject,

            /// <summary>
            /// Set when a general Xml error occured such as a missing end tag or any malformed xml. 
            /// </summary>
            ErrorXmlRead = StandardReadStatus.ErrorXmlRead,

            /// <summary>
            /// Set when an error occured during the read of a data element.
            /// </summary>
            ErrorWhileReadingElementObject = StandardReadStatus.ErrorWhileReadingElementObject,

            /// <summary>
            /// Set when the required type="xxx" attribute is missing.
            /// </summary>
            ErrorTypeAttributeMissing = StandardReadStatus.ErrorTypeAttributeMissing,

            /// <summary>
            /// Set when the type="xxx" attribute references an unknown type.
            /// </summary>
            ErrorUnknownTypeAttribute = StandardReadStatus.ErrorUnknownTypeAttribute,

            /// <summary>
            /// Set when the required key="key" attribute is missing.
            /// </summary>
            ErrorKeyAttributeMissing = StandardReadStatus.LastReadError << 1,

            /// <summary>
            /// Set when reading multiple objects when an unknown object element is skipped. 
            /// </summary>
            ErrorUnknownObjectElement = StandardReadStatus.LastReadError << 2,

            /// <summary>
            /// Set when the required guid="..." attribute is missing.
            /// </summary>
            ErrorGuidAttributeMissing = StandardReadStatus.LastReadError << 3,

            /// <summary>
            /// Set when the version="..." attribute is invalid.
            /// </summary>
            ErrorVersionAttributeInvalid = StandardReadStatus.LastReadError << 4,

            /// <summary>
            /// Mask that covers all the errors.
            /// </summary>
            ErrorMask = ErrorXmlRead | ErrorWhileReadingElementObject | ErrorTypeAttributeMissing | ErrorUnknownTypeAttribute | ErrorKeyAttributeMissing | ErrorUnknownObjectElement | ErrorGuidAttributeMissing | ErrorVersionAttributeInvalid
        }

        /// <summary>
        /// Gets the read status.
        /// </summary>
        public ReadStatus Status { get; private set; }

        /// <summary>
        /// Gets the name of the key read. Null if an error occured.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the object read from the stream. Null if an error occured.
        /// </summary>
        public object ReadObject { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the read operation failed.
        /// </summary>
        public bool HasError
        {
            get { return ErrorMessage != null; }
        }

        /// <summary>
        /// Since an error does not stop the reading process (element is skipped), it 
        /// is more a warning than an error.
        /// </summary>
        bool ISimpleErrorMessage.IsWarning
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the error message if an error occured. Null otherwise.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the line where the error occured. Defaults to -1.
        /// This information is available only if the stream supports this notion (text xml stream).
        /// </summary>
        public int ErrorLine { get; private set; }

        /// <summary>
        /// Gets the column where the error occured. Defaults to -1.
        /// This information is available only if the stream supports this notion (text xml stream).
        /// </summary>
        public int ErrorColumn { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadElementObjectInfo"/> class for a successful read operation.
        /// </summary>
        /// <param name="s">The <see cref="Status"/> of the read operation.</param>
        /// <param name="k">The <see cref="Key"/> of the read operation.</param>
        /// <param name="o">The successfully <see cref="ReadObject"/>.</param>
        public ReadElementObjectInfo( ReadStatus s, string k, object o )
        {
            Status = s;
            Key = k;
            ReadObject = o;
            ErrorMessage = null;
            ErrorLine = ErrorColumn = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadElementObjectInfo"/> class when an error occured.
        /// </summary>
        /// <param name="s">The <see cref="Status"/> of the read operation.</param>
        /// <param name="r">The xml stream.</param>
        /// <param name="errorMessage">Error message. Must not be null nor empty.</param>
        internal ReadElementObjectInfo( ReadStatus s, XmlReader r, string errorMessage )
        {
            Debug.Assert( (s & ReadStatus.ErrorMask) != 0, "The status must be on error." );
            Debug.Assert( errorMessage != null && errorMessage.Length > 0, "Error message must be set." );
            Status = s;
            Key = null;
            ReadObject = null;
            ErrorMessage = errorMessage;

            // r is a XmlTextReaderImpl (Inherits from XmlReader) and not a XmlTextReader 
            XmlTextReader textReader = r as XmlTextReader;

            if( textReader != null )
            {
                ErrorLine = textReader.LineNumber;
                ErrorColumn = textReader.LinePosition;
            }
            else ErrorLine = ErrorColumn = -1;
        }

    }

}
